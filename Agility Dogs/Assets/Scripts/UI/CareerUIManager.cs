using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Services;

namespace AgilityDogs.UI
{
    /// <summary>
    /// CareerUIManager - Handles career mode UI screens
    /// Breeding screen, training camp, show selection, and progression
    /// </summary>
    public class CareerUIManager : MonoBehaviour
    {
        public static CareerUIManager Instance { get; private set; }

        [Header("Career Panels")]
        [SerializeField] private GameObject careerHubPanel;
        [SerializeField] private GameObject breedingPanel;
        [SerializeField] private GameObject puppySelectionPanel;
        [SerializeField] private GameObject trainingCampPanel;
        [SerializeField] private GameObject showSelectionPanel;
        [SerializeField] private GameObject showResultsPanel;
        [SerializeField] private GameObject westminsterPanel;

        [Header("Career Hub")]
        [SerializeField] private TextMeshProUGUI careerPhaseText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private Slider xpProgressSlider;
        [SerializeField] private TextMeshProUGUI wingsText;
        [SerializeField] private TextMeshProUGUI puppyNameText;
        [SerializeField] private TextMeshProUGUI puppyBreedText;
        [SerializeField] private Image puppyPortrait;
        [SerializeField] private TextMeshProUGUI puppyStatsText;
        [SerializeField] private Button startShowButton;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button breedingButton;

        [Header("Breeding Panel")]
        [SerializeField] private Transform puppyLitterContainer;
        [SerializeField] private GameObject puppyCardPrefab;
        [SerializeField] private TextMeshProUGUI breedingInstructionsText;
        [SerializeField] private Button generatePuppiesButton;
        [SerializeField] private Button selectPuppyButton;
        [SerializeField] private TMP_InputField puppyNameInput;

        [Header("Training Camp")]
        [SerializeField] private TextMeshProUGUI trainingPuppyNameText;
        [SerializeField] private Image trainingPuppyPortrait;
        [SerializeField] private Slider[] skillSliders; // For each TrainingSkill
        [SerializeField] private TextMeshProUGUI[] skillTexts;
        [SerializeField] private Button startTrainingRunButton;
        [SerializeField] private Button backToHubButton;
        [SerializeField] private TextMeshProUGUI trainingInstructionsText;

        [Header("Show Selection")]
        [SerializeField] private Transform showTiersContainer;
        [SerializeField] private GameObject showTierCardPrefab;
        [SerializeField] private TextMeshProUGUI showSelectionTitle;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private Button enterShowButton;

        [Header("Show Results")]
        [SerializeField] private TextMeshProUGUI showResultTitle;
        [SerializeField] private TextMeshProUGUI showResultText;
        [SerializeField] private TextMeshProUGUI placementText;
        [SerializeField] private TextMeshProUGUI xpAwardedText;
        [SerializeField] private TextMeshProUGUI progressionText;
        [SerializeField] private Button nextShowButton;
        [SerializeField] private Button returnToHubButton;
        [SerializeField] private ParticleSystem celebrationParticles;

        [Header("Westminster")]
        [SerializeField] private TextMeshProUGUI westminsterTitleText;
        [SerializeField] private TextMeshProUGUI westminsterSubtitleText;
        [SerializeField] private TextMeshProUGUI westminsterRequirementsText;
        [SerializeField] private Button enterWestminsterButton;
        [SerializeField] private TextMeshProUGUI westminsterQualificationText;

        // State
        private CareerPhase currentPhase = CareerPhase.Breeding;
        private PuppyData selectedPuppyForBreeding;
        private List<PuppyData> currentLitter = new List<PuppyData>();
        private ShowTier selectedShowTier = ShowTier.Local;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupButtons();
            HideAllPanels();

            // Subscribe to service events
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Setup

        private void SetupButtons()
        {
            // Career Hub buttons
            if (startShowButton != null)
                startShowButton.onClick.AddListener(() => ShowShowSelection());
            if (trainingButton != null)
                trainingButton.onClick.AddListener(() => ShowTrainingCamp());
            if (breedingButton != null)
                breedingButton.onClick.AddListener(() => ShowBreeding());

            // Breeding buttons
            if (generatePuppiesButton != null)
                generatePuppiesButton.onClick.AddListener(() => GenerateNewPuppies());
            if (selectPuppyButton != null)
                selectPuppyButton.onClick.AddListener(() => ConfirmPuppySelection());

            // Training buttons
            if (startTrainingRunButton != null)
                startTrainingRunButton.onClick.AddListener(() => StartTrainingRun());
            if (backToHubButton != null)
                backToHubButton.onClick.AddListener(() => ShowCareerHub());

            // Show selection buttons
            if (enterShowButton != null)
                enterShowButton.onClick.AddListener(() => EnterSelectedShow());

            // Show results buttons
            if (nextShowButton != null)
                nextShowButton.onClick.AddListener(() => GameModeManager.Instance?.ConfirmCareerResultsAndAdvance());
            if (returnToHubButton != null)
                returnToHubButton.onClick.AddListener(() => ShowCareerHub());

            // Westminster buttons
            if (enterWestminsterButton != null)
                enterWestminsterButton.onClick.AddListener(() => EnterWestminster());
        }

        private void SubscribeToEvents()
        {
            var gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.OnCareerPhaseChanged += HandleCareerPhaseChanged;
                gameModeManager.OnShowTierChanged += HandleShowTierChanged;
                gameModeManager.OnWestminsterReached += HandleWestminsterReached;
            }

            var breedingService = DogBreedingService.Instance;
            if (breedingService != null)
            {
                breedingService.OnPuppySelected += HandlePuppySelected;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.OnCareerPhaseChanged -= HandleCareerPhaseChanged;
                gameModeManager.OnShowTierChanged -= HandleShowTierChanged;
                gameModeManager.OnWestminsterReached -= HandleWestminsterReached;
            }

            var breedingService = DogBreedingService.Instance;
            if (breedingService != null)
            {
                breedingService.OnPuppySelected -= HandlePuppySelected;
            }
        }

        #endregion

        #region Panel Management

        private void HideAllPanels()
        {
            if (careerHubPanel != null) careerHubPanel.SetActive(false);
            if (breedingPanel != null) breedingPanel.SetActive(false);
            if (puppySelectionPanel != null) puppySelectionPanel.SetActive(false);
            if (trainingCampPanel != null) trainingCampPanel.SetActive(false);
            if (showSelectionPanel != null) showSelectionPanel.SetActive(false);
            if (showResultsPanel != null) showResultsPanel.SetActive(false);
            if (westminsterPanel != null) westminsterPanel.SetActive(false);
        }

        public void ShowCareerHub()
        {
            HideAllPanels();
            if (careerHubPanel != null) careerHubPanel.SetActive(true);

            currentPhase = CareerPhase.Training; // Default after breeding
            UpdateCareerHub();
        }

        public void ShowBreeding()
        {
            HideAllPanels();
            if (breedingPanel != null) breedingPanel.SetActive(true);

            currentPhase = CareerPhase.Breeding;
            UpdateBreedingPanel();
        }

        public void ShowTrainingCamp()
        {
            HideAllPanels();
            if (trainingCampPanel != null) trainingCampPanel.SetActive(true);

            currentPhase = CareerPhase.Training;
            UpdateTrainingCamp();
        }

        public void ShowShowSelection()
        {
            HideAllPanels();
            if (showSelectionPanel != null) showSelectionPanel.SetActive(true);

            UpdateShowSelection();
        }

        public void ShowShowResults(ShowResult result, int placement, int xpAwarded, bool canAdvance)
        {
            HideAllPanels();
            if (showResultsPanel != null) showResultsPanel.SetActive(true);

            UpdateShowResults(result, placement, xpAwarded, canAdvance);
        }

        public void ShowWestminster()
        {
            HideAllPanels();
            if (westminsterPanel != null) westminsterPanel.SetActive(true);

            UpdateWestminsterPanel();
        }

        #endregion

        #region Career Hub Updates

        private void UpdateCareerHub()
        {
            var gameModeManager = GameModeManager.Instance;
            var careerService = CareerProgressionService.Instance;
            var breedingService = DogBreedingService.Instance;

            // Update phase text
            if (careerPhaseText != null)
            {
                careerPhaseText.text = GetPhaseDisplayName(currentPhase);
            }

            // Update level
            if (currentLevelText != null && careerService != null)
            {
                currentLevelText.text = $"Level {careerService.CurrentLevel}";
            }

            // Update XP
            if (xpProgressSlider != null && careerService != null)
            {
                xpProgressSlider.value = careerService.GetLevelProgress();
            }

            // Update wings
            if (wingsText != null && careerService != null)
            {
                wingsText.text = $"Wings: {careerService.CareerWings}";
            }

            // Update puppy info
            PuppyData puppy = breedingService?.GetSelectedPuppy();
            if (puppy != null)
            {
                if (puppyNameText != null)
                    puppyNameText.text = puppy.puppyName;
                if (puppyBreedText != null)
                    puppyBreedText.text = puppy.breedData?.displayName ?? "Unknown";
                if (puppyStatsText != null)
                    puppyStatsText.text = FormatPuppyStats(puppy);
            }

            // Show/hide buttons based on phase
            if (breedingButton != null)
                breedingButton.gameObject.SetActive(currentPhase == CareerPhase.Breeding);
        }

        private string FormatPuppyStats(PuppyData puppy)
        {
            if (puppy?.baseStats == null) return "No stats";

            PuppyStats stats = puppy.baseStats;
            return $"Speed: {stats.speed:F1} | Agility: {stats.agility:F1}\n" +
                   $"Jump: {stats.jumpPower:F1} | Focus: {stats.focus:F1}\n" +
                   $"Training: {puppy.TrainingProgressPercent:P0}";
        }

        #endregion

        #region Breeding Updates

        private void UpdateBreedingPanel()
        {
            if (breedingInstructionsText != null)
            {
                breedingInstructionsText.text = "Select a breed and generate a puppy to begin your career!";
            }
        }

        public void GenerateNewPuppies()
        {
            var breedingService = DogBreedingService.Instance;
            if (breedingService == null) return;

            // Generate a litter of 3 puppies
            currentLitter = breedingService.GeneratePuppyLitter(3);

            // Populate the litter container
            PopulatePuppyLitter();
        }

        private void PopulatePuppyLitter()
        {
            if (puppyLitterContainer == null || puppyCardPrefab == null) return;

            // Clear existing
            foreach (Transform child in puppyLitterContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new cards
            foreach (PuppyData puppy in currentLitter)
            {
                GameObject card = Instantiate(puppyCardPrefab, puppyLitterContainer);
                SetupPuppyCard(card, puppy);
            }
        }

        private void SetupPuppyCard(GameObject card, PuppyData puppy)
        {
            // Set puppy name
            var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{puppy.puppyName}\n{puppy.breedData?.displayName}";
            }

            // Set traits
            var traitsText = card.transform.Find("TraitsText")?.GetComponent<TextMeshProUGUI>();
            if (traitsText != null)
            {
                traitsText.text = string.Join(", ", puppy.traits);
            }

            // Set stats
            var statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
            if (statsText != null && puppy.baseStats != null)
            {
                statsText.text = $"Overall: {puppy.baseStats.GetOverallRating():F1}";
            }

            // Set select button
            var selectButton = card.GetComponent<Button>();
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectPuppyFromLitter(puppy));
            }

            // Set portrait if available
            var portrait = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null && puppy.breedData?.portrait != null)
            {
                portrait.sprite = puppy.breedData.portrait;
            }
        }

        private void SelectPuppyFromLitter(PuppyData puppy)
        {
            selectedPuppyForBreeding = puppy;

            // Update name input
            if (puppyNameInput != null)
            {
                puppyNameInput.text = puppy.puppyName;
            }

            // Enable select button
            if (selectPuppyButton != null)
            {
                selectPuppyButton.interactable = true;
            }
        }

        public void ConfirmPuppySelection()
        {
            if (selectedPuppyForBreeding == null) return;

            // Apply custom name if entered
            if (puppyNameInput != null && !string.IsNullOrEmpty(puppyNameInput.text))
            {
                DogBreedingService.Instance?.SetPuppyName(selectedPuppyForBreeding, puppyNameInput.text);
            }

            // Select the puppy (this sets it but doesn't fire career advancement events yet)
            DogBreedingService.Instance?.SelectPuppy(selectedPuppyForBreeding);

            // Advance to training phase - this will trigger HandleCareerPhaseChanged -> ShowTrainingCamp
            var gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.AdvanceCareerPhase();
            }
            else
            {
                // Fallback if GameModeManager not available
                ShowTrainingCamp();
            }
        }

        #endregion

        #region Training Camp Updates

        private void UpdateTrainingCamp()
        {
            var breedingService = DogBreedingService.Instance;
            PuppyData puppy = breedingService?.GetSelectedPuppy();

            if (puppy == null)
            {
                if (trainingInstructionsText != null)
                    trainingInstructionsText.text = "No puppy selected. Return to breeding.";
                return;
            }

            // Update puppy info
            if (trainingPuppyNameText != null)
                trainingPuppyNameText.text = puppy.puppyName;

            // Update skill displays
            UpdateTrainingSkills(puppy);

            if (trainingInstructionsText != null)
                trainingInstructionsText.text = "Complete training runs to improve your puppy's skills!";
        }

        private void UpdateTrainingSkills(PuppyData puppy)
        {
            if (skillSliders == null || skillTexts == null) return;

            TrainingSkill[] skills = (TrainingSkill[])System.Enum.GetValues(typeof(TrainingSkill));

            for (int i = 0; i < Mathf.Min(skills.Length, skillSliders.Length); i++)
            {
                int skillLevel = puppy.trainingProgress.ContainsKey(skills[i]) ? puppy.trainingProgress[skills[i]] : 0;
                float normalizedLevel = skillLevel / 100f;

                if (skillSliders[i] != null)
                    skillSliders[i].value = normalizedLevel;

                if (skillTexts[i] != null)
                    skillTexts[i].text = $"{skills[i]}: {skillLevel}%";
            }
        }

        public void StartTrainingRun()
        {
            var gameModeManager = GameModeManager.Instance;
            var breedingService = DogBreedingService.Instance;

            if (gameModeManager != null && breedingService?.GetSelectedPuppy() != null)
            {
                // Set the training course and start
                gameModeManager.StartTraining();
            }
        }

        #endregion

        #region Show Selection Updates

        private void UpdateShowSelection()
        {
            var showManager = ShowManager.Instance;
            var careerService = CareerProgressionService.Instance;
            var breedingService = DogBreedingService.Instance;

            // Update title
            if (showSelectionTitle != null)
            {
                showSelectionTitle.text = "Select a Competition";
            }

            // Update player stats
            if (playerStatsText != null)
            {
                PuppyData puppy = breedingService?.GetSelectedPuppy();
                string stats = $"Level: {careerService?.CurrentLevel ?? 1}\n";
                stats += $"Total Wins: {showManager?.TotalWins ?? 0}\n";
                stats += $"Dog: {puppy?.puppyName ?? "None"}";
                playerStatsText.text = stats;
            }

            // Populate show tiers
            PopulateShowTiers();
        }

        private void PopulateShowTiers()
        {
            if (showTiersContainer == null || showTierCardPrefab == null) return;

            // Clear existing
            foreach (Transform child in showTiersContainer)
            {
                Destroy(child.gameObject);
            }

            var showManager = ShowManager.Instance;
            if (showManager == null) return;

            // Create tier cards
            ShowTier[] tiers = { ShowTier.Local, ShowTier.County, ShowTier.Regional, ShowTier.State, ShowTier.National, ShowTier.Westminster };

            foreach (ShowTier tier in tiers)
            {
                GameObject card = Instantiate(showTierCardPrefab, showTiersContainer);
                SetupShowTierCard(card, tier, showManager);
            }
        }

        private void SetupShowTierCard(GameObject card, ShowTier tier, ShowManager showManager)
        {
            // Set tier name
            var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = GetTierDisplayName(tier);
            }

            // Set tier info
            var infoText = card.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
            if (infoText != null)
            {
                string info = tier switch
                {
                    ShowTier.Local => "Begin your journey",
                    ShowTier.County => "County fairs and small shows",
                    ShowTier.Regional => "Regional championships",
                    ShowTier.State => "State-level competition",
                    ShowTier.National => "The best in the nation",
                    ShowTier.Westminster => "AGILITY KINGS!",
                    _ => ""
                };
                infoText.text = info;
            }

            // Set select button
            var selectButton = card.GetComponent<Button>();
            if (selectButton != null)
            {
                // Check if tier is unlocked
                bool canEnter = CanEnterTier(tier, showManager);
                selectButton.interactable = canEnter;

                if (canEnter)
                {
                    selectButton.onClick.AddListener(() => SelectShowTier(tier));
                }
            }

            // Set locked overlay if needed
            var lockedOverlay = card.transform.Find("LockedOverlay");
            if (lockedOverlay != null)
            {
                lockedOverlay.gameObject.SetActive(!CanEnterTier(tier, showManager));
            }
        }

        private bool CanEnterTier(ShowTier tier, ShowManager showManager)
        {
            return tier switch
            {
                ShowTier.Local => true, // Always available
                ShowTier.County => showManager.TotalWins >= 2,
                ShowTier.Regional => showManager.TotalWins >= 4,
                ShowTier.State => showManager.TotalWins >= 6,
                ShowTier.National => showManager.TotalWins >= 8,
                ShowTier.Westminster => showManager.CanEnterWestminster(),
                _ => false
            };
        }

        private void SelectShowTier(ShowTier tier)
        {
            selectedShowTier = tier;

            // Enable enter button
            if (enterShowButton != null)
            {
                enterShowButton.interactable = true;
            }
        }

        public void EnterSelectedShow()
        {
            var gameModeManager = GameModeManager.Instance;
            var showManager = ShowManager.Instance;

            if (gameModeManager != null && showManager != null)
            {
                gameModeManager.EnterShowTier(selectedShowTier);
            }
        }

        #endregion

        #region Show Results Updates

        private void UpdateShowResults(ShowResult result, int placement, int xpAwarded, bool canAdvance)
        {
            // Update result title
            if (showResultTitle != null)
            {
                showResultTitle.text = GetResultTitle(result);
                showResultTitle.color = GetResultColor(result);
            }

            // Update result text
            if (showResultText != null)
            {
                showResultText.text = GetResultDescription(result);
            }

            // Update placement
            if (placementText != null)
            {
                placementText.text = $"Placed {GetOrdinalSuffix(placement)}";
            }

            // Update XP
            if (xpAwardedText != null)
            {
                xpAwardedText.text = $"+{xpAwarded} XP";
            }

            // Update progression text
            if (progressionText != null)
            {
                if (canAdvance)
                {
                    progressionText.text = "Qualification achieved! Ready for next tier.";
                    progressionText.color = Color.green;
                }
                else
                {
                    progressionText.text = "Keep training and try again!";
                    progressionText.color = Color.yellow;
                }
            }

            // Play celebration for good results
            if (celebrationParticles != null && (result == ShowResult.FirstPlace || result == ShowResult.BestInShow))
            {
                celebrationParticles.Play();
            }
        }

        #endregion

        #region Westminster Updates

        private void UpdateWestminsterPanel()
        {
            var showManager = ShowManager.Instance;
            var breedingService = DogBreedingService.Instance;

            // Update title
            if (westminsterTitleText != null)
            {
                westminsterTitleText.text = "THE WESTMINSTER DOG SHOW";
            }

            if (westminsterSubtitleText != null)
            {
                westminsterSubtitleText.text = "Agility Kings Championship";
            }

            // Check qualification
            bool qualified = showManager?.CanEnterWestminster() ?? false;

            if (westminsterQualificationText != null)
            {
                westminsterQualificationText.text = qualified ? "QUALIFIED!" : "Not yet qualified";
                westminsterQualificationText.color = qualified ? Color.green : Color.red;
            }

            if (enterWestminsterButton != null)
            {
                enterWestminsterButton.interactable = qualified;
            }

            // Show requirements if not qualified
            if (westminsterRequirementsText != null && !qualified)
            {
                PuppyData puppy = breedingService?.GetSelectedPuppy();
                string reqs = "Requirements:\n";
                reqs += $"• Total Wins: {showManager?.TotalWins ?? 0}/12\n";
                reqs += $"• Career Level: 25+\n";
                reqs += $"• Dog Skill: 0.8+\n";
                reqs += $"• Competitions: {showManager?.TotalCompetitions ?? 0}/20";
                westminsterRequirementsText.text = reqs;
            }
        }

        public void EnterWestminster()
        {
            var gameModeManager = GameModeManager.Instance;
            gameModeManager?.EnterWestminster();
        }

        #endregion

        #region Event Handlers

        private void HandleCareerPhaseChanged(CareerPhase phase)
        {
            currentPhase = phase;

            switch (phase)
            {
                case CareerPhase.Breeding:
                    ShowBreeding();
                    break;
                case CareerPhase.Training:
                    ShowTrainingCamp();
                    break;
                case CareerPhase.LocalShows:
                case CareerPhase.RegionalShows:
                case CareerPhase.NationalShows:
                    ShowShowSelection();
                    break;
                case CareerPhase.Westminster:
                    ShowWestminster();
                    break;
            }
        }

        private void HandleShowTierChanged(ShowTier tier)
        {
            selectedShowTier = tier;
        }

        private void HandleWestminsterReached()
        {
            ShowWestminster();
        }

        private void HandlePuppySelected(PuppyData puppy)
        {
            // Only show career hub if we're not in breeding phase transitioning to training
            // The breeding confirmation handles phase advancement separately
            if (currentPhase != CareerPhase.Breeding)
            {
                ShowCareerHub();
            }
        }

        #endregion

        #region Helper Methods

        private string GetPhaseDisplayName(CareerPhase phase)
        {
            return phase switch
            {
                CareerPhase.Breeding => "Breeding",
                CareerPhase.Training => "Training Camp",
                CareerPhase.LocalShows => "Local Shows",
                CareerPhase.RegionalShows => "Regional Championships",
                CareerPhase.NationalShows => "National Championships",
                CareerPhase.Westminster => "WESTMINSTER",
                _ => "Unknown"
            };
        }

        private string GetTierDisplayName(ShowTier tier)
        {
            return tier switch
            {
                ShowTier.Local => "Local Park Show",
                ShowTier.County => "County Fair",
                ShowTier.Regional => "Regional Championship",
                ShowTier.State => "State Championship",
                ShowTier.National => "National Championship",
                ShowTier.Westminster => "WESTMINSTER AGILITY KINGS",
                _ => tier.ToString()
            };
        }

        private string GetResultTitle(ShowResult result)
        {
            return result switch
            {
                ShowResult.BestInShow => "BEST IN SHOW!",
                ShowResult.FirstPlace => "FIRST PLACE!",
                ShowResult.SecondPlace => "SECOND PLACE",
                ShowResult.ThirdPlace => "THIRD PLACE",
                ShowResult.HonorableMention => "HONORABLE MENTION",
                ShowResult.DidNotPlace => "DID NOT PLACE",
                _ => "RUN COMPLETE"
            };
        }

        private Color GetResultColor(ShowResult result)
        {
            return result switch
            {
                ShowResult.BestInShow => new Color(1f, 0.84f, 0f), // Gold
                ShowResult.FirstPlace => Color.green,
                ShowResult.SecondPlace => new Color(0.75f, 0.75f, 0.75f), // Silver
                ShowResult.ThirdPlace => new Color(0.8f, 0.5f, 0.2f), // Bronze
                ShowResult.HonorableMention => Color.yellow,
                ShowResult.DidNotPlace => Color.red,
                _ => Color.white
            };
        }

        private string GetResultDescription(ShowResult result)
        {
            return result switch
            {
                ShowResult.BestInShow => "Your dog was the star of the show!",
                ShowResult.FirstPlace => "An outstanding performance!",
                ShowResult.SecondPlace => "Great job, almost perfect!",
                ShowResult.ThirdPlace => "Good effort, keep training!",
                ShowResult.HonorableMention => "You made it into the rankings!",
                ShowResult.DidNotPlace => "Better luck next time!",
                _ => "Run completed."
            };
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

        #endregion
    }
}
