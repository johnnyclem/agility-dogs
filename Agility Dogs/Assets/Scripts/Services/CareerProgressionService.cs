using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    public class CareerProgressionService : MonoBehaviour
    {
        public static CareerProgressionService Instance { get; private set; }

        [Header("XP Configuration")]
        [SerializeField] private int baseXPPerLevel = 100;
        [SerializeField] private float xpMultiplierPerLevel = 1.15f;
        [SerializeField] private int maxLevel = 50;

        [Header("XP Sources")]
        [SerializeField] private int xpPerObstacleClean = 10;
        [SerializeField] private int xpPerObstaclePerfect = 25;
        [SerializeField] private int xpPerQualifiedRun = 100;
        [SerializeField] private int xpPerPersonalBest = 150;
        [SerializeField] private int xpPerWin = 200;
        [SerializeField] private int xpPerFirstPlace = 300;

        [Header("Unlock Costs")]
        [SerializeField] private int handlerUnlockBaseCost = 500;
        [SerializeField] private int dogUnlockBaseCost = 500;
        [SerializeField] private int venueUnlockBaseCost = 1000;

        // Player progression state
        private int currentXP;
        private int currentLevel;
        private int totalXP;
        private int careerWings;
        private List<Achievement> achievements = new List<Achievement>();
        private List<string> unlockedHandlers = new List<string>();
        private List<string> unlockedDogs = new List<string>();
        private List<string> unlockedVenues = new List<string>();
        private Dictionary<string, int> breedXP = new Dictionary<string, int>();
        private Dictionary<string, int> handlerXP = new Dictionary<string, int>();

        // Events
        public event Action<int, int> OnXPChanged; // currentXP, xpToNextLevel
        public event Action<int> OnLevelUp; // newLevel
        public event Action<Achievement> OnAchievementUnlocked;
        public event Action<string> OnHandlerUnlocked;
        public event Action<string> OnDogUnlocked;
        public event Action<string> OnVenueUnlocked;
        public event Action<int> OnWingsChanged;

        // Properties
        public int CurrentXP => currentXP;
        public int CurrentLevel => currentLevel;
        public int TotalXP => totalXP;
        public int CareerWings => careerWings;
        public int XPToNextLevel => GetXPForLevel(currentLevel + 1);
        public IReadOnlyList<Achievement> Achievements => achievements;
        public IReadOnlyList<string> UnlockedHandlers => unlockedHandlers;
        public IReadOnlyList<string> UnlockedDogs => unlockedDogs;
        public IReadOnlyList<string> UnlockedVenues => unlockedVenues;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgressionData();
            InitializeDefaultUnlocks();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            SaveProgressionData();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnPersonalBestRecorded += HandlePersonalBest;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnPersonalBestRecorded -= HandlePersonalBest;
        }

        private void InitializeDefaultUnlocks()
        {
            // Unlock starter content if not already unlocked
            if (unlockedHandlers.Count == 0)
            {
                unlockedHandlers.Add("Starter");
            }
            if (unlockedDogs.Count == 0)
            {
                unlockedDogs.Add("BorderCollie");
            }
            if (unlockedVenues.Count == 0)
            {
                unlockedVenues.Add("LocalPark");
            }
        }

        #region XP System

        public int GetXPForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.RoundToInt(baseXPPerLevel * Mathf.Pow(xpMultiplierPerLevel, level - 2));
        }

        public float GetLevelProgress()
        {
            int xpForCurrentLevel = GetXPForLevel(currentLevel);
            int xpForNextLevel = GetXPForLevel(currentLevel + 1);
            int xpNeededForLevel = xpForNextLevel - xpForCurrentLevel;
            int xpIntoLevel = currentXP - xpForCurrentLevel;
            return (float)xpIntoLevel / xpNeededForLevel;
        }

        public void AddXP(int amount, string source = "")
        {
            if (amount <= 0) return;

            int previousLevel = currentLevel;
            currentXP += amount;
            totalXP += amount;

            // Check for level up
            while (currentLevel < maxLevel && currentXP >= GetXPForLevel(currentLevel + 1))
            {
                currentLevel++;
                OnLevelUp?.Invoke(currentLevel);
                CheckLevelBasedAchievements();
            }

            OnXPChanged?.Invoke(currentXP, XPToNextLevel);
            SaveProgressionData();

            Debug.Log($"[CareerProgression] +{amount} XP from {source}. Total: {currentXP}, Level: {currentLevel}");
        }

        public int CalculateRunXP(float time, int faults, RunResult result, bool isPersonalBest, string breed, string handler)
        {
            int xp = 0;

            // Base XP for completing a run
            xp += 25;

            // XP based on result
            switch (result)
            {
                case RunResult.Qualified:
                    xp += xpPerQualifiedRun;
                    break;
                case RunResult.NonQualified:
                    xp += xpPerQualifiedRun / 2;
                    break;
            }

            // Bonus for personal best
            if (isPersonalBest)
            {
                xp += xpPerPersonalBest;
            }

            // Fault penalty (reduced XP)
            xp -= faults * 15;
            xp = Mathf.Max(xp, 10); // Minimum XP per run

            // Add breed and handler XP
            AddBreedXP(breed, xp / 2);
            AddHandlerXP(handler, xp / 2);

            return xp;
        }

        #endregion

        #region Currency System

        public void AddWings(int amount)
        {
            if (amount <= 0) return;
            careerWings += amount;
            OnWingsChanged?.Invoke(careerWings);
            SaveProgressionData();
        }

        public bool SpendWings(int amount)
        {
            if (careerWings < amount) return false;
            careerWings -= amount;
            OnWingsChanged?.Invoke(careerWings);
            SaveProgressionData();
            return true;
        }

        #endregion

        #region Unlock System

        public bool CanUnlockHandler(string handlerId, HandlerData data)
        {
            if (unlockedHandlers.Contains(handlerId)) return false;
            int cost = GetHandlerUnlockCost(data);
            return careerWings >= cost;
        }

        public bool TryUnlockHandler(string handlerId, HandlerData data)
        {
            if (unlockedHandlers.Contains(handlerId)) return false;

            int cost = GetHandlerUnlockCost(data);
            if (!SpendWings(cost)) return false;

            unlockedHandlers.Add(handlerId);
            OnHandlerUnlocked?.Invoke(handlerId);
            CheckCollectionAchievements();
            SaveProgressionData();

            Debug.Log($"[CareerProgression] Unlocked handler: {handlerId}");
            return true;
        }

        public int GetHandlerUnlockCost(HandlerData data)
        {
            if (data == null) return handlerUnlockBaseCost;
            // Cost scales with handler stats
            int statTotal = data.speedStat + data.commandPrecisionStat + 
                           data.courseReadingStat + data.pressureHandlingStat;
            return handlerUnlockBaseCost + (statTotal * 25);
        }

        public bool CanUnlockDog(string dogId, BreedData data)
        {
            if (unlockedDogs.Contains(dogId)) return false;
            int cost = GetDogUnlockCost(data);
            return careerWings >= cost;
        }

        public bool TryUnlockDog(string dogId, BreedData data)
        {
            if (unlockedDogs.Contains(dogId)) return false;

            int cost = GetDogUnlockCost(data);
            if (!SpendWings(cost)) return false;

            unlockedDogs.Add(dogId);
            OnDogUnlocked?.Invoke(dogId);
            CheckCollectionAchievements();
            SaveProgressionData();

            Debug.Log($"[CareerProgression] Unlocked dog breed: {dogId}");
            return true;
        }

        public int GetDogUnlockCost(BreedData data)
        {
            if (data == null) return dogUnlockBaseCost;
            // Cost scales with breed stats
            int cost = dogUnlockBaseCost;
            cost += Mathf.RoundToInt((data.maxSpeed - 5f) * 50f);
            cost += Mathf.RoundToInt(data.jumpPower * 100f);
            return cost;
        }

        public bool CanUnlockVenue(string venueId, VenueData data)
        {
            if (unlockedVenues.Contains(venueId)) return false;
            int cost = GetVenueUnlockCost(data);
            return careerWings >= cost;
        }

        public bool TryUnlockVenue(string venueId, VenueData data)
        {
            if (unlockedVenues.Contains(venueId)) return false;

            int cost = GetVenueUnlockCost(data);
            if (!SpendWings(cost)) return false;

            unlockedVenues.Add(venueId);
            OnVenueUnlocked?.Invoke(venueId);
            CheckCollectionAchievements();
            SaveProgressionData();

            Debug.Log($"[CareerProgression] Unlocked venue: {venueId}");
            return true;
        }

        public int GetVenueUnlockCost(VenueData data)
        {
            if (data == null) return venueUnlockBaseCost;
            // Cost scales with venue difficulty and prestige
            return venueUnlockBaseCost + (data.difficultyRating * 100) + (data.prestigeLevel * 200);
        }

        public bool IsHandlerUnlocked(string handlerId)
        {
            return unlockedHandlers.Contains(handlerId);
        }

        public bool IsDogUnlocked(string dogId)
        {
            return unlockedDogs.Contains(dogId);
        }

        public bool IsVenueUnlocked(string venueId)
        {
            return unlockedVenues.Contains(venueId);
        }

        #endregion

        #region Breed & Handler Mastery

        private void AddBreedXP(string breedId, int amount)
        {
            if (string.IsNullOrEmpty(breedId)) return;

            if (!breedXP.ContainsKey(breedId))
                breedXP[breedId] = 0;

            breedXP[breedId] += amount;
            CheckBreedMastery(breedId);
        }

        private void AddHandlerXP(string handlerId, int amount)
        {
            if (string.IsNullOrEmpty(handlerId)) return;

            if (!handlerXP.ContainsKey(handlerId))
                handlerXP[handlerId] = 0;

            handlerXP[handlerId] += amount;
            CheckHandlerMastery(handlerId);
        }

        public int GetBreedXP(string breedId)
        {
            return breedXP.TryGetValue(breedId, out int xp) ? xp : 0;
        }

        public int GetHandlerXP(string handlerId)
        {
            return handlerXP.TryGetValue(handlerId, out int xp) ? xp : 0;
        }

        public int GetBreedLevel(string breedId)
        {
            return CalculateLevelFromXP(GetBreedXP(breedId), 50, 200);
        }

        public int GetHandlerLevel(string handlerId)
        {
            return CalculateLevelFromXP(GetHandlerXP(handlerId), 50, 200);
        }

        private int CalculateLevelFromXP(int xp, int baseXP, float multiplier)
        {
            int level = 1;
            float xpRequired = baseXP;
            while (xp >= xpRequired && level < 10)
            {
                xp -= Mathf.RoundToInt(xpRequired);
                level++;
                xpRequired *= multiplier;
            }
            return level;
        }

        private void CheckBreedMastery(string breedId)
        {
            int breedLevel = GetBreedLevel(breedId);
            if (breedLevel >= 10)
            {
                UnlockAchievement($"BreedMaster_{breedId}", $"Master Trainer: {breedId}");
            }
        }

        private void CheckHandlerMastery(string handlerId)
        {
            int handlerLevel = GetHandlerLevel(handlerId);
            if (handlerLevel >= 10)
            {
                UnlockAchievement($"HandlerMaster_{handlerId}", $"Handler Expert: {handlerId}");
            }
        }

        #endregion

        #region Achievement System

        public void UnlockAchievement(string achievementId, string displayName, int wingsReward = 50)
        {
            if (achievements.Any(a => a.Id == achievementId)) return;

            Achievement achievement = new Achievement
            {
                Id = achievementId,
                DisplayName = displayName,
                UnlockedAt = DateTime.Now,
                WingsReward = wingsReward
            };

            achievements.Add(achievement);
            AddWings(wingsReward);
            OnAchievementUnlocked?.Invoke(achievement);

            Debug.Log($"[CareerProgression] Achievement unlocked: {displayName} (+{wingsReward} Wings)");
            SaveProgressionData();
        }

        public bool HasAchievement(string achievementId)
        {
            return achievements.Any(a => a.Id == achievementId);
        }

        public int GetAchievementCount()
        {
            return achievements.Count;
        }

        private void CheckLevelBasedAchievements()
        {
            if (currentLevel >= 5) UnlockAchievement("Level5", "Rising Star");
            if (currentLevel >= 10) UnlockAchievement("Level10", "Seasoned Competitor");
            if (currentLevel >= 20) UnlockAchievement("Level20", "Elite Handler");
            if (currentLevel >= 30) UnlockAchievement("Level30", "Champion");
            if (currentLevel >= 50) UnlockAchievement("Level50", "Grand Master");
        }

        private void CheckCollectionAchievements()
        {
            if (unlockedHandlers.Count >= 3) UnlockAchievement("Unlock3Handlers", "Handler Collector");
            if (unlockedHandlers.Count >= 5) UnlockAchievement("Unlock5Handlers", "Handler Enthusiast");
            if (unlockedDogs.Count >= 3) UnlockAchievement("Unlock3Dogs", "Dog Lover");
            if (unlockedDogs.Count >= 5) UnlockAchievement("Unlock5Dogs", "Canine Connoisseur");
            if (unlockedVenues.Count >= 3) UnlockAchievement("Unlock3Venues", "World Traveler");
        }

        private void CheckRunAchievements(RunResult result, int faults, float time)
        {
            if (result == RunResult.Qualified)
            {
                UnlockAchievement("FirstQualified", "Qualified Run");
            }

            if (faults == 0 && result == RunResult.Qualified)
            {
                UnlockAchievement("CleanRun", "Flawless Run");
            }

            if (result == RunResult.Elimination)
            {
                UnlockAchievement("FirstElimination", "Back to Training");
            }
        }

        #endregion

        #region Event Handlers

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            int xp = clean ? xpPerObstaclePerfect : xpPerObstacleClean;
            AddXP(xp, $"Obstacle: {type}");
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            // Run XP is calculated elsewhere and added via CalculateRunXP
            CheckRunAchievements(result, faults, time);
        }

        private void HandlePersonalBest(string splitName, float time)
        {
            AddXP(xpPerPersonalBest, $"Personal Best: {splitName}");
        }

        #endregion

        #region Save/Load

        private void SaveProgressionData()
        {
            CareerProgressionData data = new CareerProgressionData
            {
                currentXP = currentXP,
                currentLevel = currentLevel,
                totalXP = totalXP,
                careerWings = careerWings,
                unlockedHandlers = new List<string>(unlockedHandlers),
                unlockedDogs = new List<string>(unlockedDogs),
                unlockedVenues = new List<string>(unlockedVenues),
                breedXP = new Dictionary<string, int>(breedXP),
                handlerXP = new Dictionary<string, int>(handlerXP),
                achievements = new List<Achievement>(achievements)
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();
            System.IO.File.WriteAllText(path, json);
        }

        private void LoadProgressionData()
        {
            string path = GetSavePath();
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                CareerProgressionData data = JsonUtility.FromJson<CareerProgressionData>(json);

                currentXP = data.currentXP;
                currentLevel = data.currentLevel;
                totalXP = data.totalXP;
                careerWings = data.careerWings;
                unlockedHandlers = data.unlockedHandlers ?? new List<string>();
                unlockedDogs = data.unlockedDogs ?? new List<string>();
                unlockedVenues = data.unlockedVenues ?? new List<string>();
                breedXP = data.breedXP ?? new Dictionary<string, int>();
                handlerXP = data.handlerXP ?? new Dictionary<string, int>();
                achievements = data.achievements ?? new List<Achievement>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerProgression] Failed to load progression data: {e.Message}");
            }
        }

        private string GetSavePath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "career_progression.json");
        }

        #endregion

        #region Public API

        public int GetTotalObstaclesCompleted()
        {
            // This would track from a stats service in production
            return totalXP / xpPerObstacleClean;
        }

        public int GetTotalRunsCompleted()
        {
            // This would track from a stats service in production
            return totalXP / 100;
        }

        public CareerStats GetCareerStats()
        {
            return new CareerStats
            {
                level = currentLevel,
                totalXP = totalXP,
                wings = careerWings,
                handlersUnlocked = unlockedHandlers.Count,
                dogsUnlocked = unlockedDogs.Count,
                venuesUnlocked = unlockedVenues.Count,
                achievementsUnlocked = achievements.Count
            };
        }

        #endregion
    }

    [Serializable]
    public class Achievement
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public DateTime UnlockedAt;
        public int WingsReward;
    }

    [Serializable]
    public class CareerStats
    {
        public int level;
        public int totalXP;
        public int wings;
        public int handlersUnlocked;
        public int dogsUnlocked;
        public int venuesUnlocked;
        public int achievementsUnlocked;
    }

    [Serializable]
    public class CareerProgressionData
    {
        public int currentXP;
        public int currentLevel;
        public int totalXP;
        public int careerWings;
        public List<string> unlockedHandlers;
        public List<string> unlockedDogs;
        public List<string> unlockedVenues;
        public Dictionary<string, int> breedXP;
        public Dictionary<string, int> handlerXP;
        public List<Achievement> achievements;
    }
}
