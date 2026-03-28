using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    /// <summary>
    /// ShowManager - Manages dog show competitions from local to Westminster
    /// Handles show progression, opponent generation, and results
    /// </summary>
    public class ShowManager : MonoBehaviour
    {
        public static ShowManager Instance { get; private set; }

        [Header("Show Configuration")]
        [SerializeField] private int competitorsPerShow = 8;
        [SerializeField] private float opponentSkillVariance = 0.2f;

        [Header("Tier Progression Requirements")]
        [SerializeField] private int winsForCounty = 2;
        [SerializeField] private int winsForRegional = 4;
        [SerializeField] private int winsForState = 6;
        [SerializeField] private int winsForNational = 8;
        [SerializeField] private int winsForWestminster = 12;

        [Header("Westminster Requirements")]
        [SerializeField] private float westminsterMinSkill = 0.8f;
        [SerializeField] private int westminsterMinCompetitions = 20;

        // Current show state
        private ShowTier currentTier = ShowTier.Local;
        private int totalWins = 0;
        private int tierWins = 0;
        private int totalCompetitions = 0;
        private List<CompetitorData> currentCompetitors = new List<CompetitorData>();
        private CompetitorData playerCompetitor;

        // Show history
        private List<ShowResult> showHistory = new List<ShowResult>();
        private Dictionary<ShowTier, int> winsByTier = new Dictionary<ShowTier, int>();

        // Events
        public event Action<ShowTier> OnShowStarted;
        public event Action<ShowResult, int> OnShowCompleted; // Result, placement
        public event Action<ShowTier> OnTierUnlocked;
        public event Action OnWestminsterQualified;
        public event Action<ShowResult> OnWestminsterCompleted;

        // Properties
        public ShowTier CurrentTier => currentTier;
        public int TotalWins => totalWins;
        public int TotalCompetitions => totalCompetitions;
        public bool IsWestminsterQualified => CanEnterWestminster();
        public List<ShowResult> ShowHistory => showHistory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeTierTracking();
        }

        private void Start()
        {
            GameEvents.OnRunCompleted += HandleRunCompleted;
            LoadShowProgress();
        }

        private void OnDestroy()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
        }

        #region Show Management

        /// <summary>
        /// Start a show at the specified tier
        /// </summary>
        public void StartShow(ShowTier tier)
        {
            currentTier = tier;
            tierWins = winsByTier.ContainsKey(tier) ? winsByTier[tier] : 0;

            Debug.Log($"[ShowManager] Starting {tier} show");

            // Generate competitors
            GenerateCompetitors(tier);

            // Create player competitor
            CreatePlayerCompetitor();

            OnShowStarted?.Invoke(tier);

            // The actual run will be triggered by the gameplay system
        }

        /// <summary>
        /// Start the next available show for the player
        /// </summary>
        public void StartNextShow()
        {
            ShowTier nextTier = DetermineEligibleTier();
            StartShow(nextTier);
        }

        /// <summary>
        /// Determine which tier the player is eligible for
        /// </summary>
        public ShowTier DetermineEligibleTier()
        {
            var careerService = CareerProgressionService.Instance;
            if (careerService == null) return ShowTier.Local;

            int level = careerService.CurrentLevel;

            // Progressive unlocks based on level and wins
            if (level >= 25 && CanEnterWestminster())
            {
                return ShowTier.Westminster;
            }
            if (level >= 20 && tierWins >= winsForNational)
            {
                return ShowTier.National;
            }
            if (level >= 15 && tierWins >= winsForState)
            {
                return ShowTier.State;
            }
            if (level >= 10 && tierWins >= winsForRegional)
            {
                return ShowTier.Regional;
            }
            if (level >= 5 && tierWins >= winsForCounty)
            {
                return ShowTier.County;
            }

            return ShowTier.Local;
        }

        #endregion

        #region Competitor Generation

        /// <summary>
        /// Generate AI competitors for the show
        /// </summary>
        private void GenerateCompetitors(ShowTier tier)
        {
            currentCompetitors.Clear();

            float baseSkill = GetTierBaseSkill(tier);

            for (int i = 0; i < competitorsPerShow - 1; i++) // -1 for player
            {
                CompetitorData competitor = GenerateCompetitor(tier, baseSkill);
                currentCompetitors.Add(competitor);
            }

            Debug.Log($"[ShowManager] Generated {currentCompetitors.Count} competitors for {tier} show");
        }

        /// <summary>
        /// Generate a single competitor
        /// </summary>
        private CompetitorData GenerateCompetitor(ShowTier tier, float baseSkill)
        {
            float skill = baseSkill + UnityEngine.Random.Range(-opponentSkillVariance, opponentSkillVariance);
            skill = Mathf.Clamp01(skill);

            return new CompetitorData
            {
                competitorId = Guid.NewGuid().ToString(),
                competitorName = GenerateCompetitorName(),
                dogName = GenerateDogName(),
                skill = skill,
                tier = tier,
                isPlayer = false
            };
        }

        /// <summary>
        /// Create the player's competitor entry
        /// </summary>
        private void CreatePlayerCompetitor()
        {
            var breedingService = DogBreedingService.Instance;
            var careerService = CareerProgressionService.Instance;

            PuppyData puppy = breedingService?.GetSelectedPuppy();
            float playerSkill = 0.5f; // Default

            if (puppy != null)
            {
                playerSkill = puppy.baseStats.GetOverallRating();
                // Add training bonus
                playerSkill += puppy.TrainingProgressPercent * 0.3f;
            }

            playerSkill = Mathf.Clamp01(playerSkill);

            playerCompetitor = new CompetitorData
            {
                competitorId = "PLAYER",
                competitorName = "You",
                dogName = puppy?.puppyName ?? "Your Dog",
                skill = playerSkill,
                tier = currentTier,
                isPlayer = true
            };

            currentCompetitors.Add(playerCompetitor);
        }

        #endregion

        #region Show Results

        /// <summary>
        /// Process the show results after a run
        /// </summary>
        public ShowResult ProcessShowResult(RunResult runResult, float time, int faults)
        {
            // Calculate player placement based on performance
            int placement = CalculatePlacement(runResult, time, faults);

            ShowResult showResult;
            if (placement == 1)
                showResult = ShowResult.FirstPlace;
            else if (placement == 2)
                showResult = ShowResult.SecondPlace;
            else if (placement == 3)
                showResult = ShowResult.ThirdPlace;
            else if (placement <= competitorsPerShow)
                showResult = ShowResult.HonorableMention;
            else
                showResult = ShowResult.DidNotPlace;

            // Special case: Best in Show for exceptional performance
            if (placement == 1 && runResult == RunResult.Qualified && faults == 0)
            {
                float timeBonus = EvaluateTimePerformance(time);
                if (timeBonus > 0.9f)
                {
                    showResult = ShowResult.BestInShow;
                }
            }

            // Update stats
            UpdateShowStats(showResult, placement);

            // Check for tier progression
            CheckTierProgression(showResult);

            // Fire events
            OnShowCompleted?.Invoke(showResult, placement);

            // Special handling for Westminster
            if (currentTier == ShowTier.Westminster)
            {
                OnWestminsterCompleted?.Invoke(showResult);
            }

            Debug.Log($"[ShowManager] Show result: {showResult} (Placement: {placement}/{competitorsPerShow})");

            return showResult;
        }

        /// <summary>
        /// Calculate placement based on performance
        /// </summary>
        private int CalculatePlacement(RunResult runResult, float time, int faults)
        {
            // Calculate player score
            float playerScore = CalculateScore(runResult, time, faults);

            // Compare against competitors
            int placement = 1;
            foreach (var competitor in currentCompetitors)
            {
                if (competitor.isPlayer) continue;

                // Simulate competitor performance
                float competitorScore = SimulateCompetitorScore(competitor);

                if (competitorScore > playerScore)
                {
                    placement++;
                }
            }

            return Mathf.Min(placement, competitorsPerShow);
        }

        /// <summary>
        /// Calculate a score from run performance
        /// </summary>
        private float CalculateScore(RunResult runResult, float time, int faults)
        {
            float score = 0f;

            // Base score from result
            score += runResult switch
            {
                RunResult.Qualified => 100f,
                RunResult.TimeFaultOnly => 60f,
                RunResult.NonQualified => 30f,
                RunResult.Elimination => 0f,
                _ => 0f
            };

            // Penalty for faults
            score -= faults * 10f;

            // Time bonus (lower is better)
            float timeBonus = Mathf.Max(0, 1f - (time / 60f)) * 50f;
            score += timeBonus;

            return Mathf.Max(0, score);
        }

        /// <summary>
        /// Simulate a competitor's score based on their skill
        /// </summary>
        private float SimulateCompetitorScore(CompetitorData competitor)
        {
            // Base score from skill
            float baseScore = competitor.skill * 100f;

            // Add randomness
            float variance = UnityEngine.Random.Range(-20f, 20f);

            return Mathf.Max(0, baseScore + variance);
        }

        /// <summary>
        /// Evaluate time performance (0-1)
        /// </summary>
        private float EvaluateTimePerformance(float time)
        {
            // Lower time = better performance
            float parTime = 45f; // Standard course time
            return Mathf.Clamp01(1f - (time / parTime));
        }

        #endregion

        #region Tier Progression

        /// <summary>
        /// Check if player should advance to next tier
        /// </summary>
        private void CheckTierProgression(ShowResult result)
        {
            if (result != ShowResult.FirstPlace && result != ShowResult.BestInShow)
                return;

            tierWins++;
            totalWins++;
            winsByTier[currentTier] = tierWins;

            Debug.Log($"[ShowManager] Win at {currentTier}! Total tier wins: {tierWins}, Total wins: {totalWins}");

            // Check for tier unlock
            ShowTier? nextTier = GetNextTier(currentTier);
            if (nextTier.HasValue && HasRequiredWins(nextTier.Value))
            {
                Debug.Log($"[ShowManager] Unlocked {nextTier.Value}!");
                OnTierUnlocked?.Invoke(nextTier.Value);

                // Special check for Westminster
                if (nextTier.Value == ShowTier.Westminster)
                {
                    OnWestminsterQualified?.Invoke();
                }
            }

            SaveShowProgress();
        }

        /// <summary>
        /// Get the next tier after the given tier
        /// </summary>
        private ShowTier? GetNextTier(ShowTier current)
        {
            return current switch
            {
                ShowTier.Local => ShowTier.County,
                ShowTier.County => ShowTier.Regional,
                ShowTier.Regional => ShowTier.State,
                ShowTier.State => ShowTier.National,
                ShowTier.National => ShowTier.Westminster,
                _ => null
            };
        }

        /// <summary>
        /// Check if player has required wins for a tier
        /// </summary>
        private bool HasRequiredWins(ShowTier tier)
        {
            int requiredWins = tier switch
            {
                ShowTier.County => winsForCounty,
                ShowTier.Regional => winsForRegional,
                ShowTier.State => winsForState,
                ShowTier.National => winsForNational,
                ShowTier.Westminster => winsForWestminster,
                _ => 0
            };

            return totalWins >= requiredWins;
        }

        /// <summary>
        /// Check if player can enter Westminster
        /// </summary>
        public bool CanEnterWestminster()
        {
            var careerService = CareerProgressionService.Instance;
            var breedingService = DogBreedingService.Instance;

            // Check total wins
            if (totalWins < winsForWestminster) return false;

            // Check career level
            if (careerService != null && careerService.CurrentLevel < 25) return false;

            // Check dog skill level
            PuppyData puppy = breedingService?.GetSelectedPuppy();
            if (puppy != null && puppy.baseStats.GetOverallRating() < westminsterMinSkill) return false;

            // Check competition count
            if (totalCompetitions < westminsterMinCompetitions) return false;

            return true;
        }

        public int GetWinsAtTier(ShowTier tier)
        {
            return winsByTier.TryGetValue(tier, out int wins) ? wins : 0;
        }

        #endregion

        #region Stats & Tracking

        private void UpdateShowStats(ShowResult result, int placement)
        {
            totalCompetitions++;
            showHistory.Add(result);

            // Award career progression
            var careerService = CareerProgressionService.Instance;
            if (careerService != null)
            {
                int xpReward = GetXPReward(result);
                careerService.AddXP(xpReward, $"Show: {currentTier} - {result}");
            }
        }

        private int GetXPReward(ShowResult result)
        {
            return result switch
            {
                ShowResult.BestInShow => 500,
                ShowResult.FirstPlace => 300,
                ShowResult.SecondPlace => 200,
                ShowResult.ThirdPlace => 150,
                ShowResult.HonorableMention => 100,
                ShowResult.DidNotPlace => 50,
                _ => 0
            };
        }

        public float GetTierBaseSkill(ShowTier tier)
        {
            return tier switch
            {
                ShowTier.Local => 0.3f,
                ShowTier.County => 0.4f,
                ShowTier.Regional => 0.5f,
                ShowTier.State => 0.6f,
                ShowTier.National => 0.7f,
                ShowTier.Westminster => 0.85f,
                _ => 0.3f
            };
        }

        private void InitializeTierTracking()
        {
            foreach (ShowTier tier in Enum.GetValues(typeof(ShowTier)))
            {
                winsByTier[tier] = 0;
            }
        }

        #endregion

        #region Competitor Names

        private string GenerateCompetitorName()
        {
            string[] firstNames = { "John", "Jane", "Mike", "Sarah", "Alex", "Chris", "Jordan", "Taylor", "Morgan", "Casey" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Moore" };

            return firstNames[UnityEngine.Random.Range(0, firstNames.Length)] + " " +
                   lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
        }

        private string GenerateDogName()
        {
            string[] names = { "Max", "Bella", "Charlie", "Lucy", "Cooper", "Daisy", "Rocky", "Luna", "Bear", "Sadie",
                              "Duke", "Molly", "Tucker", "Sadie", "Bear", "Zoey", "Oliver", "Penny", "Jack", "Ruby" };

            return names[UnityEngine.Random.Range(0, names.Length)];
        }

        #endregion

        #region Persistence

        private void SaveShowProgress()
        {
            PlayerPrefs.SetInt("TotalWins", totalWins);
            PlayerPrefs.SetInt("TotalCompetitions", totalCompetitions);

            foreach (var kvp in winsByTier)
            {
                PlayerPrefs.SetInt($"TierWins_{kvp.Key}", kvp.Value);
            }

            PlayerPrefs.Save();
        }

        private void LoadShowProgress()
        {
            totalWins = PlayerPrefs.GetInt("TotalWins", 0);
            totalCompetitions = PlayerPrefs.GetInt("TotalCompetitions", 0);

            foreach (ShowTier tier in Enum.GetValues(typeof(ShowTier)))
            {
                winsByTier[tier] = PlayerPrefs.GetInt($"TierWins_{tier}", 0);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            if (GameModeManager.Instance?.IsCareerMode == true)
            {
                ShowResult showResult = ProcessShowResult(result, time, faults);
                GameModeManager.Instance.ShowCareerResults(showResult, result == RunResult.Qualified);
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Competitor data for show opponents
    /// </summary>
    [Serializable]
    public class CompetitorData
    {
        public string competitorId;
        public string competitorName;
        public string dogName;
        public float skill; // 0-1 skill level
        public ShowTier tier;
        public bool isPlayer;
        public bool isBye;
    }

    #endregion
}
