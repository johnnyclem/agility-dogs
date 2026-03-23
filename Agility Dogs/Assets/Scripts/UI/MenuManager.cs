using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;

namespace AgilityDogs.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Opening Sequence")]
        [SerializeField] private OpeningSequence openingSequence;
        [SerializeField] private bool waitForOpeningSequence = true;

        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject modeSelectPanel;
        [SerializeField] private GameObject teamSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject pausePanel;

        [Header("Transitions")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Main Menu")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Mode Select")]
        [SerializeField] private Button trainingModeButton;
        [SerializeField] private Button exhibitionModeButton;
        [SerializeField] private Button careerModeButton;
        [SerializeField] private TextMeshProUGUI modeDescriptionText;
        [SerializeField] private Image modePreviewImage;
        [SerializeField] private Sprite trainingSprite;
        [SerializeField] private Sprite exhibitionSprite;
        [SerializeField] private Sprite careerSprite;

        [Header("Team Select")]
        [SerializeField] private Transform handlerListContainer;
        [SerializeField] private GameObject handlerEntryPrefab;
        [SerializeField] private Transform dogListContainer;
        [SerializeField] private GameObject dogEntryPrefab;
        [SerializeField] private Image selectedHandlerPortrait;
        [SerializeField] private Image selectedDogPortrait;
        [SerializeField] private TextMeshProUGUI selectedHandlerName;
        [SerializeField] private TextMeshProUGUI selectedDogName;
        [SerializeField] private TextMeshProUGUI selectedDogBreed;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Button backToModeSelectButton;

        [Header("Settings")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider voiceVolumeSlider;
        [SerializeField] private Slider crowdVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle colorblindModeToggle;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Button resetSettingsButton;
        [SerializeField] private Button applySettingsButton;
        [SerializeField] private Button closeSettingsButton;

        [Header("Results")]
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI resultFaultsText;
        [SerializeField] private TextMeshProUGUI resultScoreText;
        [SerializeField] private TextMeshProUGUI resultPositionText;
        [SerializeField] private TextMeshProUGUI personalBestText;
        [SerializeField] private TextMeshProUGUI courseRecordText;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextCourseButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private ParticleSystem resultParticles;

        [Header("Pause Menu")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button quitToMenuButton;

        [Header("Data References")]
        [SerializeField] private HandlerData[] availableHandlers;
        [SerializeField] private BreedData[] availableDogs;
        [SerializeField] private CourseDefinition[] availableCourses;

        // Selection state
        private GameMode selectedMode = GameMode.Training;
        private int selectedHandlerIndex = 0;
        private int selectedDogIndex = 0;
        private int selectedCourseIndex = 0;

        // References
        private GameManager gameManager;

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();

            // Find opening sequence if not assigned
            if (openingSequence == null)
            {
                openingSequence = FindObjectOfType<OpeningSequence>();
            }

            SetupButtons();
            SetupSliders();
            SetupToggles();

            // Set version text
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }

            // Check if we should wait for opening sequence
            if (waitForOpeningSequence && openingSequence != null)
            {
                // Hide all panels until opening sequence completes
                HideAllPanels();
                if (menuCanvasGroup != null)
                {
                    menuCanvasGroup.alpha = 0f;
                }
            }
            else
            {
                // Show main menu immediately
                ShowMainMenu();
                HideAllPanels();
            }

            // Subscribe to events
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        #region Opening Sequence Integration

        /// <summary>
        /// Called by OpeningSequence when the opening cinematic completes.
        /// </summary>
        public void OnOpeningSequenceComplete()
        {
            Debug.Log("[MenuManager] Opening sequence completed, showing main menu");

            // Show main menu with transition
            StartCoroutine(ShowMainMenuWithTransition());
        }

        private IEnumerator ShowMainMenuWithTransition()
        {
            // Use TransitionManager if available
            if (TransitionManager.Instance != null && menuCanvasGroup != null)
            {
                menuCanvasGroup.gameObject.SetActive(true);
                TransitionManager.Instance.Fade(menuCanvasGroup, 1f, transitionDuration, () =>
                {
                    ShowMainMenu();
                    TransitionManager.Instance.PlayTransitionSound();
                });
            }
            else
            {
                // Fallback to manual fade
                if (menuCanvasGroup != null)
                {
                    menuCanvasGroup.gameObject.SetActive(true);
                    yield return StartCoroutine(FadeCanvasGroup(menuCanvasGroup, 0f, 1f, transitionDuration));
                }
                ShowMainMenu();
            }

            yield break;
        }

        #endregion

        private void SetupButtons()
        {
            // Main Menu
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ShowModeSelect();
                });
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ShowSettings();
                });
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    QuitGame();
                });
            }

            // Mode Select
            if (trainingModeButton != null)
            {
                trainingModeButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    SelectMode(GameMode.Training);
                });
            }
            if (exhibitionModeButton != null)
            {
                exhibitionModeButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    SelectMode(GameMode.Exhibition);
                });
            }
            if (careerModeButton != null)
            {
                careerModeButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    SelectMode(GameMode.Career);
                });
            }

            // Team Select
            if (startRunButton != null)
            {
                startRunButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    StartSelectedRun();
                });
            }
            if (backToModeSelectButton != null)
            {
                backToModeSelectButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ShowModeSelect();
                });
            }

            // Settings
            if (resetSettingsButton != null)
            {
                resetSettingsButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ResetSettings();
                });
            }
            if (applySettingsButton != null)
            {
                applySettingsButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ApplySettings();
                });
            }
            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    CloseSettings();
                });
            }

            // Results
            if (replayButton != null)
            {
                replayButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    StartReplay();
                });
            }
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    RetryCourse();
                });
            }
            if (nextCourseButton != null)
            {
                nextCourseButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    NextCourse();
                });
            }
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ShowMainMenu();
                });
            }

            // Pause
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ResumeGame();
                });
            }
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    RestartGame();
                });
            }
            if (pauseSettingsButton != null)
            {
                pauseSettingsButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    ShowSettings();
                });
            }
            if (quitToMenuButton != null)
            {
                quitToMenuButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSound();
                    QuitToMenu();
                });
            }
        }

        private void SetupSliders()
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 1f);
                voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
            }

            if (crowdVolumeSlider != null)
            {
                crowdVolumeSlider.value = PlayerPrefs.GetFloat("CrowdVolume", 1f);
                crowdVolumeSlider.onValueChanged.AddListener(SetCrowdVolume);
            }
        }

        private void SetupToggles()
        {
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            if (colorblindModeToggle != null)
            {
                colorblindModeToggle.isOn = PlayerPrefs.GetInt("ColorblindMode", 0) == 1;
                colorblindModeToggle.onValueChanged.AddListener(SetColorblindMode);
            }

            if (subtitlesToggle != null)
            {
                subtitlesToggle.isOn = PlayerPrefs.GetInt("Subtitles", 1) == 1;
                subtitlesToggle.onValueChanged.AddListener(SetSubtitles);
            }
        }

        #region Panel Management

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
            if (teamSelectPanel != null) teamSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }

        private void ShowModeSelect()
        {
            HideAllPanels();
            if (modeSelectPanel != null) modeSelectPanel.SetActive(true);
            UpdateModeDescription(selectedMode);
        }

        private void ShowTeamSelect()
        {
            HideAllPanels();
            if (teamSelectPanel != null) teamSelectPanel.SetActive(true);
            PopulateTeamLists();
            UpdateTeamPreview();
        }

        private void ShowSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void ShowResults(RunResult result, float time, int faults, float score, int position)
        {
            HideAllPanels();
            if (resultsPanel != null) resultsPanel.SetActive(true);

            if (resultTitleText != null)
            {
                resultTitleText.text = GetResultTitle(result);
                resultTitleText.color = GetResultColor(result);
            }

            if (resultTimeText != null)
            {
                resultTimeText.text = FormatTime(time);
            }

            if (resultFaultsText != null)
            {
                resultFaultsText.text = faults.ToString();
                resultFaultsText.color = faults > 0 ? Color.red : Color.green;
            }

            if (resultScoreText != null)
            {
                resultScoreText.text = score.ToString("F1");
            }

            if (resultPositionText != null)
            {
                resultPositionText.text = GetOrdinalSuffix(position);
            }

            // Check for personal best
            float personalBest = PlayerPrefs.GetFloat("PersonalBest", float.MaxValue);
            if (time < personalBest)
            {
                PlayerPrefs.SetFloat("PersonalBest", time);
                if (personalBestText != null)
                {
                    personalBestText.text = "NEW PERSONAL BEST!";
                    personalBestText.gameObject.SetActive(true);
                }

                if (resultParticles != null)
                {
                    resultParticles.Play();
                }
            }
            else
            {
                if (personalBestText != null)
                {
                    personalBestText.gameObject.SetActive(false);
                }
            }

            // Update button visibility based on result
            if (nextCourseButton != null)
            {
                nextCourseButton.gameObject.SetActive(result == RunResult.Qualified);
            }
        }

        public void ShowPauseMenu()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        #endregion

        #region Mode Selection

        private void SelectMode(GameMode mode)
        {
            selectedMode = mode;
            UpdateModeDescription(mode);
            ShowTeamSelect();
        }

        private void UpdateModeDescription(GameMode mode)
        {
            if (modeDescriptionText == null) return;

            string description = "";
            switch (mode)
            {
                case GameMode.Training:
                    description = "Practice courses at your own pace. No pressure, no competition. Perfect for learning!";
                    break;
                case GameMode.Exhibition:
                    description = "Run exhibition courses and compete for the best times. Show off your skills!";
                    break;
                case GameMode.Career:
                    description = "Build your career as an agility handler. Earn experience, unlock new dogs and venues!";
                    break;
            }
            modeDescriptionText.text = description;

            // Update preview image
            if (modePreviewImage != null)
            {
                switch (mode)
                {
                    case GameMode.Training:
                        modePreviewImage.sprite = trainingSprite;
                        break;
                    case GameMode.Exhibition:
                        modePreviewImage.sprite = exhibitionSprite;
                        break;
                    case GameMode.Career:
                        modePreviewImage.sprite = careerSprite;
                        break;
                }
            }
        }

        #endregion

        #region Team Selection

        private void PopulateTeamLists()
        {
            // Populate handlers
            if (handlerListContainer != null && handlerEntryPrefab != null && availableHandlers != null)
            {
                // Clear existing entries
                foreach (Transform child in handlerListContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create new entries
                for (int i = 0; i < availableHandlers.Length; i++)
                {
                    HandlerData handler = availableHandlers[i];
                    GameObject entry = Instantiate(handlerEntryPrefab, handlerListContainer);

                    TextMeshProUGUI nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                    if (nameText != null)
                    {
                        nameText.text = handler.displayName;
                    }

                    Image portrait = entry.transform.Find("Portrait")?.GetComponent<Image>();
                    if (portrait != null && handler.portrait != null)
                    {
                        portrait.sprite = handler.portrait;
                    }

                    Button button = entry.GetComponent<Button>();
                    int index = i; // Capture for lambda
                    if (button != null)
                    {
                        button.onClick.AddListener(() => SelectHandler(index));
                    }
                }
            }

            // Populate dogs
            if (dogListContainer != null && dogEntryPrefab != null && availableDogs != null)
            {
                // Clear existing entries
                foreach (Transform child in dogListContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create new entries
                for (int i = 0; i < availableDogs.Length; i++)
                {
                    BreedData dog = availableDogs[i];
                    GameObject entry = Instantiate(dogEntryPrefab, dogListContainer);

                    TextMeshProUGUI nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                    if (nameText != null)
                    {
                        nameText.text = dog.displayName;
                    }

                    Image portrait = entry.transform.Find("Portrait")?.GetComponent<Image>();
                    if (portrait != null && dog.portrait != null)
                    {
                        portrait.sprite = dog.portrait;
                    }

                    Button button = entry.GetComponent<Button>();
                    int index = i; // Capture for lambda
                    if (button != null)
                    {
                        button.onClick.AddListener(() => SelectDog(index));
                    }
                }
            }
        }

        private void SelectHandler(int index)
        {
            selectedHandlerIndex = index;
            UpdateTeamPreview();
        }

        private void SelectDog(int index)
        {
            selectedDogIndex = index;
            UpdateTeamPreview();
        }

        private void UpdateTeamPreview()
        {
            // Update handler preview
            if (availableHandlers != null && selectedHandlerIndex < availableHandlers.Length)
            {
                HandlerData handler = availableHandlers[selectedHandlerIndex];

                if (selectedHandlerPortrait != null && handler.portrait != null)
                {
                    selectedHandlerPortrait.sprite = handler.portrait;
                }

                if (selectedHandlerName != null)
                {
                    selectedHandlerName.text = handler.displayName;
                }
            }

            // Update dog preview
            if (availableDogs != null && selectedDogIndex < availableDogs.Length)
            {
                BreedData dog = availableDogs[selectedDogIndex];

                if (selectedDogPortrait != null && dog.portrait != null)
                {
                    selectedDogPortrait.sprite = dog.portrait;
                }

                if (selectedDogName != null)
                {
                    selectedDogName.text = dog.displayName;
                }

                if (selectedDogBreed != null)
                {
                    selectedDogBreed.text = dog.breedName;
                }
            }
        }

        private void StartSelectedRun()
        {
            // Set up the selected configuration
            // In a full implementation, this would pass the selection to the GameManager
            if (gameManager != null)
            {
                // For now, just start the game
                gameManager.StartGame();
            }
        }

        #endregion

        #region Settings

        private void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat("MusicVolume", volume);
            // Apply to audio system
        }

        private void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat("SFXVolume", volume);
            // Apply to audio system
        }

        private void SetVoiceVolume(float volume)
        {
            PlayerPrefs.SetFloat("VoiceVolume", volume);
            // Apply to audio system
        }

        private void SetCrowdVolume(float volume)
        {
            PlayerPrefs.SetFloat("CrowdVolume", volume);
            // Apply to audio system
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void SetColorblindMode(bool enabled)
        {
            PlayerPrefs.SetInt("ColorblindMode", enabled ? 1 : 0);
            // Apply colorblind mode settings
        }

        private void SetSubtitles(bool enabled)
        {
            PlayerPrefs.SetInt("Subtitles", enabled ? 1 : 0);
        }

        private void ResetSettings()
        {
            if (musicVolumeSlider != null) musicVolumeSlider.value = 1f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (voiceVolumeSlider != null) voiceVolumeSlider.value = 1f;
            if (crowdVolumeSlider != null) crowdVolumeSlider.value = 1f;
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            if (colorblindModeToggle != null) colorblindModeToggle.isOn = false;
            if (subtitlesToggle != null) subtitlesToggle.isOn = true;
        }

        private void ApplySettings()
        {
            PlayerPrefs.Save();
            CloseSettings();
        }

        #endregion

        #region Results

        private void StartReplay()
        {
            // Start replay of the last run
            if (gameManager != null)
            {
                gameManager.StartReplay();
            }
        }

        private void RetryCourse()
        {
            // Retry the current course
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        private void NextCourse()
        {
            // Move to next course
            selectedCourseIndex++;
            if (selectedCourseIndex >= availableCourses.Length)
            {
                selectedCourseIndex = 0;
            }

            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }

        #endregion

        #region Pause Menu

        private void ResumeGame()
        {
            HidePauseMenu();
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
        }

        private void RestartGame()
        {
            HidePauseMenu();
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        private void QuitToMenu()
        {
            HidePauseMenu();
            ShowMainMenu();
            if (gameManager != null)
            {
                gameManager.QuitToMenu();
            }
        }

        #endregion

        #region General

        private void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.Pause:
                    ShowPauseMenu();
                    break;
                default:
                    HidePauseMenu();
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int decimals = Mathf.FloorToInt((time % 1f) * 100f);
            return $"{minutes:00}:{seconds:00}.{decimals:00}";
        }

        private string GetResultTitle(RunResult result)
        {
            switch (result)
            {
                case RunResult.Qualified: return "QUALIFIED!";
                case RunResult.NonQualified: return "NOT QUALIFIED";
                case RunResult.Elimination: return "ELIMINATED";
                case RunResult.TimeFaultOnly: return "TIME FAULTS";
                default: return "RUN COMPLETE";
            }
        }

        private Color GetResultColor(RunResult result)
        {
            switch (result)
            {
                case RunResult.Qualified: return Color.green;
                case RunResult.NonQualified: return Color.yellow;
                case RunResult.Elimination: return Color.red;
                case RunResult.TimeFaultOnly: return new Color(1f, 0.5f, 0f);
                default: return Color.white;
            }
        }

        private string GetOrdinalSuffix(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = transitionCurve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }

        private void PlayButtonClickSound()
        {
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.PlayClickSound();
            }
        }

        #endregion
    }

    public enum GameMode
    {
        Training,
        Exhibition,
        Career
    }
}
