using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Data;
using AgilityDogs.Gameplay;

namespace AgilityDogs.Services
{
    /// <summary>
    /// GameManager - Core game state management
    /// Coordinates with GameModeManager for mode-specific flows
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Scene References")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameplayScene = "SampleScene";

        // Mode configuration
        private GameMode currentGameMode = GameMode.None;
        private bool isPaused = false;

        // Current run configuration
        private CourseDefinition currentCourse;
        private HandlerData currentHandler;
        private BreedData currentDog;
        private bool isTrainingMode = false;
        private bool isCareerMode = false;

        // Events
        public event Action<GameState, GameState> OnStateChanged;
        public event Action<GameMode> OnGameModeChanged;
        public event Action<bool> OnPauseStateChanged;

        // Properties
        public GameState CurrentState => currentState;
        public GameMode CurrentGameMode => currentGameMode;
        public bool IsPaused => isPaused;
        public bool IsTrainingMode => isTrainingMode;
        public bool IsCareerMode => isCareerMode;
        public CourseDefinition CurrentCourse => currentCourse;
        public HandlerData CurrentHandler => currentHandler;
        public BreedData CurrentDog => currentDog;

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
            // Subscribe to events
            GameEvents.OnRunCompleted += HandleRunCompleted;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        #region State Management

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previous = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State changed: {previous} -> {newState}");

            OnStateChanged?.Invoke(previous, newState);
            GameEvents.RaiseGameStateChanged(previous, newState);
        }

        #endregion

        #region Game Mode Methods

        /// <summary>
        /// Start Quick Play mode - jumps straight to competition
        /// </summary>
        public void StartQuickPlay()
        {
            Debug.Log("[GameManager] Starting Quick Play mode");
            
            currentGameMode = GameMode.QuickPlay;
            isTrainingMode = false;
            isCareerMode = false;

            OnGameModeChanged?.Invoke(currentGameMode);

            // Use default course if none set
            if (currentCourse == null)
            {
                LoadDefaultCourse();
            }

            StartCountdown();
        }

        /// <summary>
        /// Start Training mode - practice course
        /// </summary>
        public void StartTraining(CourseDefinition course = null, HandlerData handler = null, BreedData dog = null)
        {
            Debug.Log("[GameManager] Starting Training mode");
            
            currentGameMode = GameMode.Training;
            isTrainingMode = true;
            isCareerMode = false;

            currentCourse = course ?? currentCourse;
            currentHandler = handler ?? currentHandler;
            currentDog = dog ?? currentDog;

            OnGameModeChanged?.Invoke(currentGameMode);

            // Training mode uses the gameplay scene
            StartCountdown();
        }

        /// <summary>
        /// Start Career mode - full career progression
        /// </summary>
        public void StartCareer(CareerPhase startPhase = CareerPhase.Breeding)
        {
            Debug.Log($"[GameManager] Starting Career mode at phase: {startPhase}");
            
            currentGameMode = GameMode.Career;
            isTrainingMode = false;
            isCareerMode = true;

            OnGameModeChanged?.Invoke(currentGameMode);

            // Delegate to GameModeManager for career routing
            var gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.StartCareer(startPhase);
            }
            else
            {
                // Fallback - start with breeding phase
                SetState(GameState.Career);
            }
        }

        /// <summary>
        /// Set the current course for gameplay
        /// </summary>
        public void SetCourse(CourseDefinition course)
        {
            currentCourse = course;
        }

        /// <summary>
        /// Set the current team (handler and dog)
        /// </summary>
        public void SetTeam(HandlerData handler, BreedData dog)
        {
            currentHandler = handler;
            currentDog = dog;
        }

        #endregion

        #region Run Flow

        public void StartCountdown()
        {
            Debug.Log("[GameManager] Starting countdown");
            SetState(GameState.Countdown);
        }

        public void BeginRun()
        {
            Debug.Log("[GameManager] Beginning run");
            SetState(GameState.Gameplay);
            GameEvents.RaiseRunStarted();
        }

        public void CompleteRun(RunResult result, float time, int faults)
        {
            Debug.Log($"[GameManager] Run completed: {result}, Time: {time}, Faults: {faults}");
            SetState(GameState.RunComplete);
            GameEvents.RaiseRunCompleted(result, time, faults);
        }

        public void ShowResults()
        {
            Debug.Log("[GameManager] Showing results");
            SetState(GameState.Results);
        }

        public void ReturnToMenu()
        {
            Debug.Log("[GameManager] Returning to menu");
            
            // Reset mode
            currentGameMode = GameMode.None;
            isTrainingMode = false;
            isCareerMode = false;
            
            SetState(GameState.MainMenu);
            
            // Load menu scene
            SceneManager.LoadScene(mainMenuScene);
        }

        #endregion

        #region Menu Methods (Called by MenuManager)

        public void StartGame()
        {
            // Default to Quick Play if no specific mode set
            if (currentGameMode == GameMode.None)
            {
                StartQuickPlay();
            }
            else
            {
                StartCountdown();
            }
        }

        public void StartReplay()
        {
            Debug.Log("[GameManager] Starting replay");
            SetState(GameState.Replay);
        }

        public void RestartGame()
        {
            Debug.Log("[GameManager] Restarting game");
            
            // Keep current mode, just restart the run
            if (currentGameMode == GameMode.Career)
            {
                // In career mode, might go back to hub instead
                var gameModeManager = GameModeManager.Instance;
                if (gameModeManager != null)
                {
                    gameModeManager.ReturnToCareerHub();
                }
            }
            else
            {
                BeginRun();
            }
        }

        public void ResumeGame()
        {
            Debug.Log("[GameManager] Resuming game");
            isPaused = false;
            SetState(GameState.Gameplay);
            OnPauseStateChanged?.Invoke(false);
        }

        public void PauseGame()
        {
            Debug.Log("[GameManager] Pausing game");
            isPaused = true;
            SetState(GameState.Pause);
            OnPauseStateChanged?.Invoke(true);
        }

        public void QuitToMenu()
        {
            ReturnToMenu();
        }

        #endregion

        #region Scene Management

        private void LoadDefaultCourse()
        {
            // Try to load a default course from resources
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
            if (courses.Length > 0)
            {
                currentCourse = courses[0];
                Debug.Log($"[GameManager] Loaded default course: {currentCourse.name}");
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameManager] Scene loaded: {scene.name}");

            // If gameplay scene loaded, configure it
            if (scene.name == gameplayScene || scene.name == "SampleScene")
            {
                ConfigureGameplayScene();
            }
        }

        private void ConfigureGameplayScene()
        {
            Debug.Log("[GameManager] Configuring gameplay scene");

            // Find and configure the course runner
            var courseRunner = FindObjectOfType<CourseRunner>();
            if (courseRunner != null && currentCourse != null)
            {
                courseRunner.LoadCourse(currentCourse);
            }

            // Find and configure the dog
            var dogController = FindObjectOfType<Gameplay.Dog.DogAgentController>();
            if (dogController != null && currentDog != null)
            {
                // Dog configuration is handled via inspector references
                // This could be expanded to dynamically set the breed
            }

            // Mode-specific configuration
            if (isTrainingMode)
            {
                ConfigureTrainingMode();
            }
            else if (isCareerMode)
            {
                ConfigureCareerMode();
            }
        }

        private void ConfigureTrainingMode()
        {
            Debug.Log("[GameManager] Configuring training mode settings");
            
            // Training mode adjustments:
            // - Timer can be optional/hidden
            // - Unlimited retries
            // - Training aids visible
            // - Fault penalties reduced or disabled
            
            // This would be expanded based on specific training features
        }

        private void ConfigureCareerMode()
        {
            Debug.Log("[GameManager] Configuring career mode settings");
            
            // Career mode settings:
            // - Full competition rules
            // - Career progression tracking
            // - Show-specific rules based on tier
        }

        #endregion

        #region Event Handlers

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            Debug.Log($"[GameManager] Run completed event received: {result}");

            // Transition to results
            ShowResults();

            // In career mode, delegate to ShowManager
            if (isCareerMode)
            {
                var showManager = ShowManager.Instance;
                if (showManager != null)
                {
                    showManager.ProcessShowResult(result, time, faults);
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Check if gameplay is currently active
        /// </summary>
        public bool IsGameplayActive()
        {
            return currentState == GameState.Gameplay && !isPaused;
        }

        /// <summary>
        /// Get formatted game mode name
        /// </summary>
        public string GetGameModeName()
        {
            return currentGameMode switch
            {
                GameMode.QuickPlay => "Quick Play",
                GameMode.Training => "Training",
                GameMode.Career => "Career",
                _ => "Unknown"
            };
        }

        #endregion
    }
}
