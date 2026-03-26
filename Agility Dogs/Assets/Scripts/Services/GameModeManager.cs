using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;
using AgilityDogs.UI;
using AgilityDogs.Gameplay;

namespace AgilityDogs.Services
{
    /// <summary>
    /// GameModeManager - Central routing for all game modes
    /// Handles Quick Play, Training, and Career mode transitions
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "StartMenu";
        [SerializeField] private string gameplayScene = "SampleScene";
        [SerializeField] private string trainingScene = "TrainingScene";
        [SerializeField] private string careerHubScene = "CareerScene";

        [Header("Quick Play Defaults")]
        [SerializeField] private CourseDefinition quickPlayCourse;
        [SerializeField] private HandlerData quickPlayHandler;
        [SerializeField] private BreedData quickPlayDog;

        [Header("Training Defaults")]
        [SerializeField] private CourseDefinition trainingCourse;
        [SerializeField] private HandlerData trainingHandler;
        [SerializeField] private BreedData trainingDog;

        // Current mode state
        private GameMode currentMode = GameMode.None;
        private CareerPhase currentCareerPhase = CareerPhase.Breeding;
        private ShowTier currentShowTier = ShowTier.Local;

        // Mode-specific configuration
        private CourseDefinition selectedCourse;
        private HandlerData selectedHandler;
        private BreedData selectedDog;
        private bool isTrainingMode = false;
        private bool isCareerMode = false;

        // Career progression callbacks
        public event Action<CareerPhase> OnCareerPhaseChanged;
        public event Action<ShowTier> OnShowTierChanged;
        public event Action OnWestminsterReached;

        // Properties
        public GameMode CurrentMode => currentMode;
        public CareerPhase CurrentCareerPhase => currentCareerPhase;
        public ShowTier CurrentShowTier => currentShowTier;
        public CourseDefinition SelectedCourse => selectedCourse;
        public HandlerData SelectedHandler => selectedHandler;
        public BreedData SelectedDog => selectedDog;
        public bool IsTrainingMode => isTrainingMode;
        public bool IsCareerMode => isCareerMode;
        public bool IsCampaignMode => currentMode == GameMode.Campaign;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Subscribe to game events
            GameEvents.OnRunCompleted += HandleRunCompleted;
            
            // Auto-load defaults if not set
            LoadDefaultData();
        }
        
        private void LoadDefaultData()
        {
            // Load breeds if not assigned
            if (quickPlayDog == null || trainingDog == null)
            {
                var breeds = Resources.LoadAll<BreedData>("Data/Breeds");
                if (breeds.Length > 0)
                {
                    if (quickPlayDog == null) quickPlayDog = breeds[0];
                    if (trainingDog == null) trainingDog = breeds[0];
                    Debug.Log($"[GameModeManager] Auto-loaded breeds: {breeds.Length}");
                }
            }
            
            // Load handlers if not assigned
            if (quickPlayHandler == null || trainingHandler == null)
            {
                var handlers = Resources.LoadAll<HandlerData>("Data/Handlers");
                if (handlers.Length > 0)
                {
                    if (quickPlayHandler == null) quickPlayHandler = handlers[0];
                    if (trainingHandler == null) trainingHandler = handlers[0];
                    Debug.Log($"[GameModeManager] Auto-loaded handlers: {handlers.Length}");
                }
            }
            
            // Load courses if not assigned
            if (quickPlayCourse == null || trainingCourse == null)
            {
                var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
                if (courses.Length > 0)
                {
                    if (quickPlayCourse == null) quickPlayCourse = courses[0];
                    if (trainingCourse == null) trainingCourse = courses[0];
                    Debug.Log($"[GameModeManager] Auto-loaded courses: {courses.Length}");
                }
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
        }

        #region Mode Entry Points

        /// <summary>
        /// Start Quick Play mode - jumps straight to a competition with default settings
        /// </summary>
        public void StartQuickPlay()
        {
            Debug.Log("[GameModeManager] Starting Quick Play mode");
            
            currentMode = GameMode.QuickPlay;
            isTrainingMode = false;
            isCareerMode = false;

            // Use default quick play settings or last used
            selectedCourse = quickPlayCourse;
            selectedHandler = quickPlayHandler;
            selectedDog = quickPlayDog;

            // If no defaults set, use any available
            if (selectedHandler == null || selectedDog == null)
            {
                LoadDefaultTeam();
            }

            // Raise event and load gameplay
            GameEvents.RaiseGameStateChanged(GameState.MainMenu, GameState.CourseLoad);
            StartCoroutine(LoadGameplayScene());
        }

        /// <summary>
        /// Start Training mode - practice course with your dog
        /// </summary>
        public void StartTraining(HandlerData handler = null, BreedData dog = null, CourseDefinition course = null)
        {
            Debug.Log("[GameModeManager] Starting Training mode");
            
            currentMode = GameMode.Training;
            isTrainingMode = true;
            isCareerMode = false;

            // Use provided or default training settings
            selectedHandler = handler ?? trainingHandler;
            selectedDog = dog ?? trainingDog;
            selectedCourse = course ?? trainingCourse;

            // If no defaults set, use any available
            if (selectedHandler == null || selectedDog == null)
            {
                LoadDefaultTeam();
            }

            // Raise event and load gameplay
            GameEvents.RaiseGameStateChanged(GameState.MainMenu, GameState.CourseLoad);
            StartCoroutine(LoadGameplayScene());
        }

        /// <summary>
        /// Start Career mode - breeding, training, local shows, Westminster
        /// </summary>
        public void StartCareer(CareerPhase startPhase = CareerPhase.Breeding)
        {
            Debug.Log($"[GameModeManager] Starting Career mode at phase: {startPhase}");
            
            currentMode = GameMode.Career;
            isTrainingMode = false;
            isCareerMode = true;
            currentCareerPhase = startPhase;

            // Load career progression data
            LoadCareerProgress();

            // Raise event
            GameEvents.RaiseGameStateChanged(GameState.MainMenu, GameState.Career);

            // Route to appropriate phase
            RouteCareerPhase(startPhase);
        }

        /// <summary>
        /// Start Campaign/Story mode - narrative-driven career with story beats and cutscenes
        /// </summary>
        public void StartCampaign()
        {
            Debug.Log("[GameModeManager] Starting Campaign mode");
            
            currentMode = GameMode.Campaign;
            isTrainingMode = false;
            isCareerMode = true;

            // Start the campaign service
            var campaignService = CampaignService.Instance;
            if (campaignService != null)
            {
                if (!campaignService.IsCampaignActive)
                {
                    campaignService.StartCampaign();
                }
            }

            // Also initialize career data
            LoadCareerProgress();

            // Raise event
            GameEvents.RaiseGameStateChanged(GameState.MainMenu, GameState.Career);

            // Show career hub with campaign chapter info
            CareerUIManager.Instance?.ShowCareerHub();
        }

        #endregion

        #region Career Phase Management

        /// <summary>
        /// Route to the appropriate scene/flow for the current career phase
        /// </summary>
        public void RouteCareerPhase(CareerPhase phase)
        {
            currentCareerPhase = phase;
            OnCareerPhaseChanged?.Invoke(phase);

            Debug.Log($"[GameModeManager] Routing to career phase: {phase}");

            switch (phase)
            {
                case CareerPhase.Breeding:
                    ShowBreedingScreen();
                    break;

                case CareerPhase.Training:
                    ShowTrainingCamp();
                    break;

                case CareerPhase.LocalShows:
                case CareerPhase.RegionalShows:
                case CareerPhase.NationalShows:
                    ShowCareerShowSelection();
                    break;

                case CareerPhase.Westminster:
                    CareerUIManager.Instance?.ShowWestminster();
                    break;
            }
        }

        /// <summary>
        /// Advance to the next career phase
        /// </summary>
        public void AdvanceCareerPhase()
        {
            CareerPhase nextPhase = currentCareerPhase switch
            {
                CareerPhase.Breeding => CareerPhase.Training,
                CareerPhase.Training => CareerPhase.LocalShows,
                CareerPhase.LocalShows => CareerPhase.RegionalShows,
                CareerPhase.RegionalShows => CareerPhase.NationalShows,
                CareerPhase.NationalShows => CareerPhase.Westminster,
                CareerPhase.Westminster => CareerPhase.Westminster, // Stay at top
                _ => currentCareerPhase
            };

            Debug.Log($"[GameModeManager] Advancing career: {currentCareerPhase} -> {nextPhase}");
            RouteCareerPhase(nextPhase);
            SaveCareerProgress();
        }

        /// <summary>
        /// Enter a specific show tier for competition
        /// </summary>
        public void EnterShowTier(ShowTier tier)
        {
            currentShowTier = tier;
            OnShowTierChanged?.Invoke(tier);

            Debug.Log($"[GameModeManager] Entering show tier: {tier}");

            // Load appropriate course based on tier
            selectedCourse = GetCourseForShowTier(tier);

            // Use career dog and handler
            LoadCareerTeam();

            // Start the show
            StartCoroutine(LoadGameplayScene());
        }

        /// <summary>
        /// Enter the Westminster Agility Kings competition
        /// </summary>
        public void EnterWestminster()
        {
            Debug.Log("[GameModeManager] Entering Westminster Agility Kings!");
            
            currentShowTier = ShowTier.Westminster;
            OnShowTierChanged?.Invoke(ShowTier.Westminster);
            OnWestminsterReached?.Invoke();

            // Load Westminster course (final championship course)
            selectedCourse = GetWestminsterCourse();
            LoadCareerTeam();

            // Start the championship
            StartCoroutine(LoadGameplayScene());
        }

        #endregion

        #region Career Screens

        /// <summary>
        /// Show the breeding/puppy selection screen
        /// </summary>
        private void ShowBreedingScreen()
        {
            Debug.Log("[GameModeManager] Showing breeding screen");
            CareerUIManager.Instance?.ShowBreeding();
        }

        /// <summary>
        /// Show the training camp screen
        /// </summary>
        private void ShowTrainingCamp()
        {
            Debug.Log("[GameModeManager] Showing training camp");
            CareerUIManager.Instance?.ShowTrainingCamp();
        }

        /// <summary>
        /// Show the career hub (main career screen)
        /// </summary>
        public void ShowCareerHub()
        {
            Debug.Log("[GameModeManager] Showing career hub");
            CareerUIManager.Instance?.ShowCareerHub();
        }

        /// <summary>
        /// Show the show selection screen
        /// </summary>
        public void ShowCareerShowSelection()
        {
            Debug.Log("[GameModeManager] Showing show selection");
            CareerUIManager.Instance?.ShowShowSelection();
        }

        /// <summary>
        /// Show competition results and career progression
        /// </summary>
        public void ShowCareerResults(ShowResult result, bool qualified)
        {
            Debug.Log($"[GameModeManager] Career show result: {result}, Qualified: {qualified}");

            // Award career progression based on result
            AwardCareerProgress(result);

            // Check if player qualifies to advance to next phase
            bool canAdvance = ShouldAdvancePhase(result);

            // Determine current tier for display
            ShowTier currentTier = currentShowTier;

            // Show results screen via CareerUIManager
            CareerUIManager.Instance?.ShowShowResults(result, 0, GetXPReward(result), canAdvance);
        }

        /// <summary>
        /// Advance to next career phase after viewing results
        /// Called when player clicks "Continue" or "Next" on results screen
        /// </summary>
        public void ConfirmCareerResultsAndAdvance()
        {
            if (ShouldAdvancePhase(GetLastShowResult()))
            {
                AdvanceCareerPhase();
            }
            else
            {
                ShowCareerShowSelection();
            }
        }

        private ShowResult GetLastShowResult()
        {
            return ShowManager.Instance?.ShowHistory.LastOrDefault() ?? ShowResult.DidNotPlace;
        }

        #endregion

        #region Team Management

        /// <summary>
        /// Set the current team (handler and dog) for gameplay
        /// </summary>
        public void SetTeam(HandlerData handler, BreedData dog, CourseDefinition course = null)
        {
            selectedHandler = handler;
            selectedDog = dog;
            if (course != null)
            {
                selectedCourse = course;
            }
        }

        /// <summary>
        /// Set the current course
        /// </summary>
        public void SetCourse(CourseDefinition course)
        {
            selectedCourse = course;
        }

        /// <summary>
        /// Load default team from available data
        /// </summary>
        private void LoadDefaultTeam()
        {
            // Try to find handlers and dogs in the scene or resources
            if (selectedHandler == null)
            {
                var handlers = Resources.LoadAll<HandlerData>("Data/Handlers");
                if (handlers.Length > 0)
                {
                    selectedHandler = handlers[0];
                }
            }

            if (selectedDog == null)
            {
                var dogs = Resources.LoadAll<BreedData>("Data/Breeds");
                if (dogs.Length > 0)
                {
                    selectedDog = dogs[0];
                }
            }
        }

        /// <summary>
        /// Load career-specific team (player's bred dog and handler)
        /// </summary>
        private void LoadCareerTeam()
        {
            // Load from career save data
            var careerData = CareerProgressionService.Instance;
            if (careerData != null)
            {
                // Get the player's current dog from career data
                // For now, use default - will be expanded with breeding system
                LoadDefaultTeam();
            }
        }

        #endregion

        #region Course Selection

        /// <summary>
        /// Get appropriate course for a show tier
        /// </summary>
        private CourseDefinition GetCourseForShowTier(ShowTier tier)
        {
            // Load courses based on tier difficulty
            string tierName = tier switch
            {
                ShowTier.Local => "LocalPark",
                ShowTier.County => "CountyFair",
                ShowTier.Regional => "RegionalChamp",
                ShowTier.State => "StateChamp",
                ShowTier.National => "NationalChamp",
                ShowTier.Westminster => "Westminster",
                _ => "LocalPark"
            };

            // Try to load from resources
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
            foreach (var course in courses)
            {
                if (course.name.Contains(tierName))
                {
                    return course;
                }
            }

            // Fallback to any available course
            if (courses.Length > 0)
            {
                return courses[0];
            }

            return null;
        }

        /// <summary>
        /// Get the Westminster championship course
        /// </summary>
        private CourseDefinition GetWestminsterCourse()
        {
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
            foreach (var course in courses)
            {
                if (course.name.Contains("Westminster") || course.name.Contains("Championship"))
                {
                    return course;
                }
            }

            // Fallback
            return GetCourseForShowTier(ShowTier.Westminster);
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// Load the gameplay scene with current configuration
        /// </summary>
        private IEnumerator LoadGameplayScene()
        {
            Debug.Log($"[GameModeManager] Loading gameplay scene for mode: {currentMode}");

            // Show loading screen or transition
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.FadeOut(0.5f);
                yield return new WaitForSeconds(0.5f);
            }

            // Determine which scene to load based on mode
            string sceneToLoad = gameplayScene; // Default
            if (isTrainingMode)
            {
                sceneToLoad = trainingScene;
            }
            else if (isCareerMode)
            {
                sceneToLoad = careerHubScene;
            }
            
            Debug.Log($"[GameModeManager] Loading scene: {sceneToLoad}");

            // Load gameplay scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Configure gameplay based on mode
            ConfigureGameplay();

            // Fade in
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.FadeIn(0.5f);
            }

            // Start countdown
            yield return new WaitForSeconds(1f);
            GameManager.Instance?.StartCountdown();
        }

        /// <summary>
        /// Configure gameplay settings based on current mode
        /// </summary>
        private void ConfigureGameplay()
        {
            Debug.Log($"[GameModeManager] Configuring gameplay: Mode={currentMode}, Training={isTrainingMode}, Career={isCareerMode}");

            // Apply mode-specific settings
            if (isTrainingMode)
            {
                // Training mode settings
                // - No timer pressure (optional)
                // - Unlimited retries
                // - Training aids visible
                // - No scoring/ranking
            }
            else if (isCareerMode)
            {
                // Career mode settings
                // - Full competition rules
                // - Career progression tracking
                // - Show-specific rules based on tier
            }
            else
            {
                // Quick Play settings
                // - Standard competition rules
                // - Quick restart option
            }

            // Set up the course if available
            if (selectedCourse != null)
            {
                var courseRunner = FindObjectOfType<CourseRunner>();
                if (courseRunner != null)
                {
                    courseRunner.LoadCourse(selectedCourse);
                }
            }
        }

        /// <summary>
        /// Return to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameModeManager] Returning to main menu");
            
            currentMode = GameMode.None;
            isTrainingMode = false;
            isCareerMode = false;

            GameEvents.RaiseGameStateChanged(GameState.Gameplay, GameState.MainMenu);
            SceneManager.LoadScene(mainMenuScene);
        }

        /// <summary>
        /// Return to career hub (for career mode)
        /// </summary>
        public void ReturnToCareerHub()
        {
            if (currentMode == GameMode.Career)
            {
                Debug.Log("[GameModeManager] Returning to career hub");
                RouteCareerPhase(currentCareerPhase);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            Debug.Log($"[GameModeManager] Run completed: {result}, Time: {time}, Faults: {faults}");

            if (currentMode == GameMode.Career)
            {
                // Convert run result to show result
                ShowResult showResult = ConvertToShowResult(result);
                bool qualified = result == RunResult.Qualified;

                // Award career progression
                AwardCareerProgress(showResult);

                // Show career results
                ShowCareerResults(showResult, qualified);
            }
            else if (currentMode == GameMode.Training)
            {
                // Training mode - no progression, just practice
                Debug.Log("[GameModeManager] Training run complete - no career progression");
            }
        }

        private ShowResult ConvertToShowResult(RunResult runResult)
        {
            return runResult switch
            {
                RunResult.Qualified => ShowResult.FirstPlace, // Simplified - would need placement logic
                RunResult.NonQualified => ShowResult.DidNotPlace,
                RunResult.Elimination => ShowResult.DidNotPlace,
                RunResult.TimeFaultOnly => ShowResult.HonorableMention,
                _ => ShowResult.DidNotPlace
            };
        }

        private void AwardCareerProgress(ShowResult result)
        {
            var careerService = CareerProgressionService.Instance;
            if (careerService != null)
            {
                // Award XP and wings based on result
                int xpReward = result switch
                {
                    ShowResult.BestInShow => 500,
                    ShowResult.FirstPlace => 300,
                    ShowResult.SecondPlace => 200,
                    ShowResult.ThirdPlace => 150,
                    ShowResult.HonorableMention => 100,
                    ShowResult.DidNotPlace => 50,
                    _ => 0
                };

                careerService.AddXP(xpReward, $"Show Result: {result}");
            }
        }

        public int GetXPReward(ShowResult result)
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

        private bool ShouldAdvancePhase(ShowResult result)
        {
            if (result != ShowResult.FirstPlace && result != ShowResult.BestInShow)
                return false;

            var showManager = ShowManager.Instance;
            if (showManager == null)
                return false;

            return showManager.TotalWins switch
            {
                >= 12 => currentShowTier != ShowTier.Westminster,
                >= 8 => currentShowTier == ShowTier.National,
                >= 6 => currentShowTier == ShowTier.State,
                >= 4 => currentShowTier == ShowTier.Regional,
                >= 2 => currentShowTier == ShowTier.County,
                _ => currentShowTier == ShowTier.Local
            };
        }

        #endregion

        #region Career Progress Persistence

        private void SaveCareerProgress()
        {
            PlayerPrefs.SetInt("CareerPhase", (int)currentCareerPhase);
            PlayerPrefs.SetInt("ShowTier", (int)currentShowTier);
            PlayerPrefs.Save();
        }

        private void LoadCareerProgress()
        {
            currentCareerPhase = (CareerPhase)PlayerPrefs.GetInt("CareerPhase", (int)CareerPhase.Breeding);
            currentShowTier = (ShowTier)PlayerPrefs.GetInt("ShowTier", (int)ShowTier.Local);
        }

        #endregion
    }
}
