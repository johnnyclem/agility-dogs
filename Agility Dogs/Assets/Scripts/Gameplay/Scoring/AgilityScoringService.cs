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

                // Exceeding maximum course time ends the run as a non-qualifying result.
                if (currentCourse != null && runTimer >= currentCourse.maximumTime)
                {
                    CompleteRun(RunResult.NonQualified);
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

            if (splitTimes != null && currentObstacleIndex < splitTimes.Length)
            {
                splitTimes[currentObstacleIndex] = runTimer;
                GameEvents.RaiseSplitTime(runTimer);
            }

            currentObstacleIndex++;

            // Run completion is owned by CourseRunner, which tracks the actual
            // course sequence. Scoring only records splits and faults.
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

            // Time faults accrue only for time spent over the standard course time.
            float standardTime = currentCourse != null ? currentCourse.standardTime : 45f;
            float overTime = Mathf.Max(0f, runTimer - standardTime);
            if (overTime > 0f)
            {
                totalFaults += Mathf.CeilToInt(overTime / faultPenaltySeconds);
            }

            // Record course personal best on qualifying runs.
            if (result == RunResult.Qualified && currentCourse != null &&
                GetFinalScore() < currentCourse.bestTime)
            {
                currentCourse.bestTime = GetFinalScore();
            }

            // GameManager is the single place that raises OnRunCompleted, so the
            // event cannot fire twice for one run (scoring + course runner paths).
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteRun(result, runTimer, totalFaults);
            }
            else
            {
                GameEvents.RaiseRunCompleted(result, runTimer, totalFaults);
            }
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
            if (currentCourse == null || splitTimes == null || splitTimes.Length == 0) return 0f;
            if (obstacleIndex < 0 || obstacleIndex >= splitTimes.Length) return 0f;

            float best = currentCourse.bestTime;
            // No comparison available until a personal best has been recorded.
            if (best <= 0f || best == float.MaxValue) return 0f;

            float current = obstacleIndex > 0 ? splitTimes[obstacleIndex] - splitTimes[obstacleIndex - 1] : splitTimes[obstacleIndex];
            return current - (best / splitTimes.Length);
        }

        public bool IsPersonalBest()
        {
            if (currentCourse == null) return false;
            return GetFinalScore() < currentCourse.bestTime;
        }

        public RunResult EvaluateRunResult()
        {
            float standardTime = currentCourse != null ? currentCourse.standardTime : 45f;
            float maximumTime = currentCourse != null ? currentCourse.maximumTime : 60f;

            if (runTimer >= maximumTime) return RunResult.NonQualified;
            if (GetFinalScore() > maximumTime) return RunResult.NonQualified;
            if (faultCount == 0 && runTimer <= standardTime) return RunResult.Qualified;

            // Finished under maximum time but with faults and/or over standard time.
            return RunResult.TimeFaultOnly;
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
