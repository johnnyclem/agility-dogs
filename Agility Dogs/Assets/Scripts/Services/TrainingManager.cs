using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    public class TrainingManager : MonoBehaviour
    {
        public static TrainingManager Instance { get; private set; }

        [Header("Training Drills")]
        [SerializeField] private List<TrainingDrillDefinition> allDrills = new List<TrainingDrillDefinition>();

        [Header("Current Session")]
        [SerializeField] private TrainingDrillDefinition currentDrill;
        [SerializeField] private int currentRepetition;
        [SerializeField] private bool isTrainingActive;

        // Session tracking
        private TrainingSessionData currentSessionData;
        private Dictionary<string, TrainingSessionData> drillProgress = new Dictionary<string, TrainingSessionData>();
        private List<TrainingRunResult> currentRunHistory = new List<TrainingRunResult>();

        // Events
        public event Action<TrainingDrillDefinition> OnDrillStarted;
        public event Action<TrainingDrillDefinition, int, int> OnRepetitionCompleted; // drill, rep, totalReps
        public event Action<TrainingDrillDefinition, TrainingSessionData> OnDrillCompleted;
        public event Action<TrainingDrillDefinition> OnDrillUnlocked;
        public event Action<string> OnDrillFailed; // failure message
        public event Action<bool, float, int> OnTrainingRunComplete; // clean, time, faults

        // Properties
        public bool IsTrainingActive => isTrainingActive;
        public TrainingDrillDefinition CurrentDrill => currentDrill;
        public int CurrentRepetition => currentRepetition;
        public int TotalRepetitions => currentDrill?.repetitions ?? 0;
        public float SessionProgress => TotalRepetitions > 0 ? (float)currentRepetition / TotalRepetitions : 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgressData();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            SaveProgressData();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnFaultCommitted += HandleFault;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnFaultCommitted -= HandleFault;
        }

        #region Public API

        public List<TrainingDrillDefinition> GetAvailableDrills()
        {
            return allDrills.Where(d => d.isUnlockedByDefault || IsDrillUnlocked(d.drillId)).ToList();
        }

        public List<TrainingDrillDefinition> GetDrillsByType(TrainingDrillType type)
        {
            return GetAvailableDrills().Where(d => d.drillType == type).ToList();
        }

        public List<TrainingDrillDefinition> GetDrillsByDifficulty(int minDifficulty, int maxDifficulty)
        {
            return GetAvailableDrills().Where(d => d.difficulty >= minDifficulty && d.difficulty <= maxDifficulty).ToList();
        }

        public TrainingSessionData GetDrillProgress(string drillId)
        {
            return drillProgress.TryGetValue(drillId, out var data) ? data : null;
        }

        public int GetDrillStars(string drillId)
        {
            return GetDrillProgress(drillId)?.totalStars ?? 0;
        }

        public bool IsDrillUnlocked(string drillId)
        {
            var drill = allDrills.FirstOrDefault(d => d.drillId == drillId);
            if (drill == null) return false;
            if (drill.isUnlockedByDefault) return true;

            // Check if drill is unlocked through completion of prerequisites
            // This could be expanded with more complex unlock logic
            return GetDrillProgress(drillId)?.isCompleted ?? false;
        }

        public bool CanStartDrill(TrainingDrillDefinition drill)
        {
            if (drill == null) return false;
            if (isTrainingActive) return false;
            return IsDrillUnlocked(drill.drillId);
        }

        public bool StartDrill(TrainingDrillDefinition drill)
        {
            if (!CanStartDrill(drill)) return false;

            currentDrill = drill;
            currentRepetition = 0;
            isTrainingActive = true;
            currentRunHistory.Clear();

            // Initialize or get existing session data
            if (!drillProgress.ContainsKey(drill.drillId))
            {
                drillProgress[drill.drillId] = new TrainingSessionData
                {
                    drillId = drill.drillId,
                    completedRuns = 0,
                    cleanRuns = 0,
                    bestTime = float.MaxValue,
                    bestCleanTime = float.MaxValue,
                    lastPlayed = DateTime.Now,
                    isCompleted = false,
                    totalStars = 0
                };
            }
            currentSessionData = drillProgress[drill.drillId];
            currentSessionData.lastPlayed = DateTime.Now;

            OnDrillStarted?.Invoke(drill);
            Debug.Log($"[Training] Started drill: {drill.displayName}");

            StartNextRepetition();
            return true;
        }

        public void StopDrill()
        {
            if (!isTrainingActive) return;

            CompleteDrillEarly();
        }

        public void PauseTraining()
        {
            // Could implement pause logic here
        }

        public void ResumeTraining()
        {
            // Could implement resume logic here
        }

        public List<TrainingRunResult> GetCurrentRunHistory()
        {
            return new List<TrainingRunResult>(currentRunHistory);
        }

        #endregion

        #region Drill Execution

        private void StartNextRepetition()
        {
            currentRepetition++;
            Debug.Log($"[Training] Starting repetition {currentRepetition}/{currentDrill.repetitions}");

            // Here you would set up the course/scene for this specific repetition
            // - Load obstacle sequence
            // - Reset dog/handler positions
            // - Apply any drill-specific modifiers (slow motion, etc.)
            // - Start countdown

            if (currentDrill.slowMotionEnabled)
            {
                Time.timeScale = currentDrill.slowMotionSpeed;
            }
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            if (!isTrainingActive) return;

            bool clean = faults <= currentDrill.maxFaults && result == RunResult.Qualified;
            float targetTime = currentDrill.targetTime;

            var runResult = new TrainingRunResult
            {
                repetition = currentRepetition,
                time = time,
                faults = faults,
                result = result,
                isClean = clean,
                meetsTimeTarget = time <= targetTime,
                timestamp = DateTime.Now
            };
            currentRunHistory.Add(runResult);

            // Update session data
            currentSessionData.completedRuns++;
            if (clean) currentSessionData.cleanRuns++;
            if (time < currentSessionData.bestTime) currentSessionData.bestTime = time;
            if (clean && time < currentSessionData.bestCleanTime) currentSessionData.bestCleanTime = time;

            OnTrainingRunComplete?.Invoke(clean, time, faults);
            OnRepetitionCompleted?.Invoke(currentDrill, currentRepetition, currentDrill.repetitions);

            // Reset time scale if modified
            if (currentDrill.slowMotionEnabled)
            {
                Time.timeScale = 1f;
            }

            // Check if drill is complete
            if (currentRepetition >= currentDrill.repetitions)
            {
                CompleteDrill();
            }
            else
            {
                // Auto-reset if enabled and fault occurred
                if (currentDrill.autoResetOnFault && faults > 0)
                {
                    Invoke(nameof(StartNextRepetition), currentDrill.resetDelay);
                }
                else
                {
                    StartNextRepetition();
                }
            }
        }

        private void HandleFault(ObstacleType obstacleType, string faultType)
        {
            // Could implement drill-specific fault handling here
        }

        private void CompleteDrill()
        {
            isTrainingActive = false;

            // Calculate stars
            int stars = CalculateStars();
            currentSessionData.totalStars = Mathf.Max(currentSessionData.totalStars, stars);
            currentSessionData.isCompleted = stars > 0;

            // Grant rewards
            if (stars > 0 && CareerProgressionService.Instance != null)
            {
                CareerProgressionService.Instance.AddXP(currentDrill.xpReward * stars, $"Training: {currentDrill.displayName}");
                CareerProgressionService.Instance.AddWings(currentDrill.wingsReward * stars);

                // Unlock skills if available
                if (currentDrill.unlockableSkillIds != null && SkillTreeService.Instance != null)
                {
                    foreach (string skillId in currentDrill.unlockableSkillIds)
                    {
                        SkillTreeService.Instance.GrantBonusSkillPoints(1, $"Completed {currentDrill.displayName}");
                    }
                }
            }

            OnDrillCompleted?.Invoke(currentDrill, currentSessionData);
            SaveProgressData();

            Debug.Log($"[Training] Drill completed: {currentDrill.displayName} - {stars} stars");
        }

        private void CompleteDrillEarly()
        {
            isTrainingActive = false;

            if (currentRepetition > 0)
            {
                // Grant partial rewards
                float completionRate = (float)currentRepetition / currentDrill.repetitions;
                int partialXP = Mathf.RoundToInt(currentDrill.xpReward * completionRate * 0.5f);

                if (CareerProgressionService.Instance != null && partialXP > 0)
                {
                    CareerProgressionService.Instance.AddXP(partialXP, $"Training (partial): {currentDrill.displayName}");
                }
            }

            currentDrill = null;
            Debug.Log("[Training] Drill stopped early");
        }

        private int CalculateStars()
        {
            if (currentRunHistory.Count == 0) return 0;

            int stars = 0;

            // Star 1: Complete all repetitions
            if (currentRunHistory.Count >= currentDrill.repetitions)
                stars = 1;

            // Star 2: Meet clean rate requirement
            int cleanCount = currentRunHistory.Count(r => r.isClean);
            float cleanRate = (float)cleanCount / currentRunHistory.Count;
            if (cleanRate >= currentDrill.minimumCleanRate)
                stars = 2;

            // Star 3: Beat target time with clean runs
            var cleanRuns = currentRunHistory.Where(r => r.isClean && r.meetsTimeTarget).ToList();
            if (cleanRuns.Count >= Mathf.CeilToInt(currentDrill.repetitions * 0.5f))
                stars = 3;

            return stars;
        }

        #endregion

        #region Persistence

        private void SaveProgressData()
        {
            var data = new TrainingProgressSaveData
            {
                drillProgress = new Dictionary<string, TrainingSessionData>(drillProgress)
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();
            System.IO.File.WriteAllText(path, json);
        }

        private void LoadProgressData()
        {
            string path = GetSavePath();
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = JsonUtility.FromJson<TrainingProgressSaveData>(json);
                drillProgress = data.drillProgress ?? new Dictionary<string, TrainingSessionData>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Training] Failed to load progress data: {e.Message}");
            }
        }

        private string GetSavePath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "training_progress.json");
        }

        #endregion
    }

    [Serializable]
    public class TrainingRunResult
    {
        public int repetition;
        public float time;
        public int faults;
        public RunResult result;
        public bool isClean;
        public bool meetsTimeTarget;
        public DateTime timestamp;
    }

    [Serializable]
    public class TrainingProgressSaveData
    {
        public Dictionary<string, TrainingSessionData> drillProgress;
    }
}
