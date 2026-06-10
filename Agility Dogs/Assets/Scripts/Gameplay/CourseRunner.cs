using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Services;

namespace AgilityDogs.Gameplay
{
    public class CourseRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandlerController handler;
        [SerializeField] private DogAgentController dog;
        [SerializeField] private AgilityScoringService scoringService;

        [Header("Course")]
        [SerializeField] private CourseDefinition currentCourse;

        [Header("Countdown")]
        [SerializeField] private float countdownDuration = 5f;
        [SerializeField] private int countdownStart = 3;

        private ObstacleBase[] courseObstacles;
        private int currentObstacleOrder = 0;
        private ObstacleBase expectedObstacle;
        private bool isRunActive;

        public bool IsRunActive => isRunActive;
        public CourseDefinition CurrentCourse => currentCourse;

        private void OnEnable()
        {
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompletedWithReference += HandleObstacleCompletedWithReference;
        }

        private void OnDisable()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompletedWithReference -= HandleObstacleCompletedWithReference;
        }

        public void LoadCourse(CourseDefinition course)
        {
            // A redundant reload of the course mid-run would reset sequence
            // tracking and corrupt the active run.
            if (isRunActive && course == currentCourse) return;

            // Validate course obstacle types
            if (course != null && course.obstacleSequence != null)
            {
                foreach (ObstacleData obstacleData in course.obstacleSequence)
                {
                    if (obstacleData != null)
                    {
                        bool validForCourse = false;
                        foreach (CourseType validType in obstacleData.validCourseTypes)
                        {
                            if (validType == course.courseType)
                            {
                                validForCourse = true;
                                break;
                            }
                        }
                        if (!validForCourse)
                        {
                            Debug.LogWarning($"Obstacle {obstacleData.obstacleName} (type {obstacleData.obstacleType}) is not valid for course type {course.courseType}");
                        }
                    }
                }
            }

            currentCourse = course;
            scoringService.SetCourse(course);
            currentObstacleOrder = 0;

            if (course != null && course.obstacleSequence != null && course.obstacleSequence.Length > 0)
            {
                // Build the course layout from data; returns obstacles already
                // in sequence order.
                courseObstacles = CourseLayoutBuilder.BuildCourse(course);
            }
            else
            {
                Debug.LogWarning($"[CourseRunner] Course '{course?.courseName}' has no obstacle sequence; falling back to scene obstacles.");
                courseObstacles = FindObjectsOfType<ObstacleBase>();
                OrderObstaclesBySequence();
            }

            GameEvents.RaiseCourseLoaded();
        }

        public void StartCountdown()
        {
            GameManager.Instance.StartCountdown();
            StartCoroutine(CountdownSequence());
        }

        private IEnumerator CountdownSequence()
        {
            handler.SetEnabled(false);

            for (int i = countdownStart; i > 0; i--)
            {
                GameEvents.RaiseCountdownTick();
                yield return new WaitForSeconds(1f);
            }

            StartRun();
        }

        private void StartRun()
        {
            isRunActive = true;
            handler.SetEnabled(true);
            GameManager.Instance.BeginRun();
            currentObstacleOrder = 0;
            expectedObstacle = null;

            if (courseObstacles != null)
            {
                foreach (var obs in courseObstacles)
                {
                    obs.ResetObstacle();
                }
            }

            // Send the dog to the first obstacle. Without this the dog has no
            // target until it stumbles onto an obstacle by accident.
            AdvanceToNextObstacle();
        }

        public void CompleteRun()
        {
            if (!isRunActive) return;
            isRunActive = false;
            handler.SetEnabled(false);

            // Scoring service finalizes the result and notifies GameManager,
            // which raises the single OnRunCompleted event.
            scoringService.CompleteRun(scoringService.EvaluateRunResult());
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            isRunActive = false;
            handler.SetEnabled(false);
        }

        private void HandleObstacleCompletedWithReference(ObstacleBase obstacle, bool clean)
        {
            if (!isRunActive || obstacle == null) return;

            if (expectedObstacle != null && obstacle != expectedObstacle)
            {
                // Wrong course: dog took a different obstacle. Record the fault
                // but keep expecting the correct obstacle instead of skipping it.
                GameEvents.RaiseFaultCommitted(FaultType.WrongCourse, obstacle.ObstacleType.ToString());

                if (dog != null)
                {
                    dog.SetTargetObstacle(expectedObstacle);
                }
                return;
            }

            expectedObstacle = null;
            AdvanceToNextObstacle();
        }

        public void AdvanceToNextObstacle()
        {
            if (courseObstacles == null || currentObstacleOrder >= courseObstacles.Length)
            {
                CompleteRun();
                return;
            }

            var nextObstacle = courseObstacles[currentObstacleOrder];
            if (nextObstacle != null)
            {
                dog.SetTargetObstacle(nextObstacle);
                expectedObstacle = nextObstacle;
            }
            currentObstacleOrder++;
        }

        public void RestartCourse()
        {
            isRunActive = false;
            StopAllCoroutines();
            currentObstacleOrder = 0;
            scoringService.SetCourse(currentCourse);

            if (courseObstacles != null)
            {
                foreach (var obs in courseObstacles)
                {
                    obs.ResetObstacle();
                }
            }
        }

        public void SetHandlerAndDog(HandlerController newHandler, DogAgentController newDog)
        {
            handler = newHandler;
            dog = newDog;

            if (dog != null && handler != null)
            {
                dog.SetHandler(handler.transform);
            }
        }

        public void SetScoringService(AgilityScoringService service)
        {
            scoringService = service;
        }

        public ObstacleBase GetCurrentObstacle()
        {
            if (courseObstacles == null || currentObstacleOrder <= 0 ||
                currentObstacleOrder > courseObstacles.Length)
                return null;

            return courseObstacles[currentObstacleOrder - 1];
        }

        public int GetRemainingObstacles()
        {
            if (courseObstacles == null) return 0;
            return courseObstacles.Length - currentObstacleOrder;
        }

        private void OrderObstaclesBySequence()
        {
            if (courseObstacles == null || currentCourse == null || currentCourse.obstacleSequence == null)
                return;

            // Create a mapping from ObstacleData to its index in the course sequence
            Dictionary<ObstacleData, int> sequenceIndices = new Dictionary<ObstacleData, int>();
            for (int i = 0; i < currentCourse.obstacleSequence.Length; i++)
            {
                ObstacleData data = currentCourse.obstacleSequence[i];
                if (data != null && !sequenceIndices.ContainsKey(data))
                {
                    sequenceIndices[data] = i;
                }
            }

            // Sort obstacles based on their obstacleData's index in the sequence
            System.Array.Sort(courseObstacles, (a, b) =>
            {
                int indexA = (a.ObstacleData != null && sequenceIndices.TryGetValue(a.ObstacleData, out int idxA)) ? idxA : int.MaxValue;
                int indexB = (b.ObstacleData != null && sequenceIndices.TryGetValue(b.ObstacleData, out int idxB)) ? idxB : int.MaxValue;
                return indexA.CompareTo(indexB);
            });
        }

        private void OnDrawGizmos()
        {
            if (courseObstacles == null) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < courseObstacles.Length - 1; i++)
            {
                if (courseObstacles[i] != null && courseObstacles[i + 1] != null)
                {
                    Gizmos.DrawLine(courseObstacles[i].transform.position, courseObstacles[i + 1].transform.position);
                }
            }
        }
    }
}
