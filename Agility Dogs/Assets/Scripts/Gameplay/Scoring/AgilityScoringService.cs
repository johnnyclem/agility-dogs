using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Services;

namespace AgilityDogs.Gameplay.Scoring
{
    public class AgilityScoringService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CourseDefinition currentCourse;
        [SerializeField] private DogAgentController dog;

        [Header("Scoring")]
        [SerializeField] private int faultPenaltySeconds = 5;

        private float runTimer;
        private int faultCount;
        private float[] splitTimes;
        private int currentObstacleIndex;
        private bool isTimerRunning;
        private List<FaultRecord> faultHistory = new List<FaultRecord>();

        public float CurrentTime => runTimer;
        public int FaultCount => faultCount;
        public float[] SplitTimes => splitTimes;
        public bool IsRunning => isTimerRunning;
        public List<FaultRecord> FaultHistory => faultHistory;
        public int CurrentObstacleIndex => currentObstacleIndex;

        public struct FaultRecord
        {
            public FaultType type;
            public string obstacleName;
            public float time;
            public int obstacleIndex;
        }

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                runTimer += Time.deltaTime;

                if (currentCourse != null && runTimer >= currentCourse.maximumTime)
                {
                    CompleteRun(RunResult.TimeFaultOnly);
                }
            }
        }

        private void HandleRunStarted()
        {
            ResetScoring();
            isTimerRunning = true;

            if (currentCourse != null && currentCourse.obstacleSequence != null)
            {
                splitTimes = new float[currentCourse.obstacleSequence.Length];
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (!isTimerRunning) return;

            if (currentObstacleIndex < splitTimes.Length)
            {
                splitTimes[currentObstacleIndex] = runTimer;
                GameEvents.RaiseSplitTime(runTimer);
            }

            currentObstacleIndex++;

            if (currentCourse != null && currentObstacleIndex >= currentCourse.obstacleSequence.Length)
            {
                CompleteRun(RunResult.Qualified);
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            faultCount++;
            faultHistory.Add(new FaultRecord
            {
                type = fault,
                obstacleName = obstacleName,
                time = runTimer,
                obstacleIndex = currentObstacleIndex
            });
        }

        public void CompleteRun(RunResult result)
        {
            if (!isTimerRunning) return;
            isTimerRunning = false;

            int totalFaults = faultCount;

            if (result == RunResult.Qualified && totalFaults > 0)
            {
                float faultTime = totalFaults * faultPenaltySeconds;
                if (currentCourse != null && runTimer + faultTime > currentCourse.maximumTime)
                {
                    result = RunResult.TimeFaultOnly;
                }
            }

            if (result == RunResult.TimeFaultOnly)
            {
                totalFaults += Mathf.CeilToInt((runTimer - (currentCourse?.standardTime ?? 45f)) / faultPenaltySeconds);
            }

            GameEvents.RaiseRunCompleted(result, runTimer, totalFaults);
        }

        public void Eliminate()
        {
            CompleteRun(RunResult.Elimination);
        }

        public float GetFinalScore()
        {
            if (currentCourse == null) return runTimer;
            return runTimer + (faultCount * faultPenaltySeconds);
        }

        public float GetSplitTime(int obstacleIndex)
        {
            if (splitTimes == null || obstacleIndex < 0 || obstacleIndex >= splitTimes.Length)
                return 0f;
            return splitTimes[obstacleIndex];
        }

        public float GetSplitDelta(int obstacleIndex)
        {
            if (currentCourse == null || splitTimes == null) return 0f;
            float best = currentCourse.bestTime;
            float current = obstacleIndex > 0 ? splitTimes[obstacleIndex] - splitTimes[obstacleIndex - 1] : splitTimes[obstacleIndex];
            return best > 0 ? current - (best / splitTimes.Length) : 0f;
        }

        public bool IsPersonalBest()
        {
            if (currentCourse == null) return false;
            return GetFinalScore() < currentCourse.bestTime;
        }

        public RunResult EvaluateRunResult()
        {
            if (faultCount == 0 && IsPersonalBest()) return RunResult.Qualified;
            if (faultCount > 0 && GetFinalScore() < currentCourse?.maximumTime) return RunResult.Qualified;
            if (GetFinalScore() > currentCourse?.maximumTime) return RunResult.TimeFaultOnly;
            return RunResult.Qualified;
        }

        public void SetCourse(CourseDefinition course)
        {
            currentCourse = course;
        }

        public void SetDog(DogAgentController dogController)
        {
            dog = dogController;
        }

        private void ResetScoring()
        {
            runTimer = 0f;
            faultCount = 0;
            currentObstacleIndex = 0;
            faultHistory.Clear();

            if (currentCourse != null && currentCourse.obstacleSequence != null)
            {
                splitTimes = new float[currentCourse.obstacleSequence.Length];
            }
            else
            {
                splitTimes = new float[0];
            }
        }
    }
}
