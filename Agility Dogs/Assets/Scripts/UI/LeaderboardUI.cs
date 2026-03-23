using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Services;

namespace AgilityDogs.UI
{
    /// <summary>
    /// UI component for displaying leaderboard entries.
    /// Supports filtering by category and displaying ghost run information.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LeaderboardService leaderboardService;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject entryPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown categoryDropdown;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text playerRankText;
        [SerializeField] private TMP_Text playerBestText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button loadOnlineButton;
        
        [Header("Settings")]
        [SerializeField] private int displayLimit = 10;
        [SerializeField] private bool showGhosts = true;
        [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.2f, 1f);
        
        [Header("Ghost Button")]
        [SerializeField] private Button playGhostButton;
        [SerializeField] private TMP_Text ghostInfoText;
        
        private string currentLeaderboardId;
        private List<LeaderboardEntry> currentEntries = new List<LeaderboardEntry>();
        private List<GameObject> entryObjects = new List<GameObject>();
        
        private void Start()
        {
            if (leaderboardService == null)
            {
                leaderboardService = LeaderboardService.Instance;
            }
            
            InitializeUI();
            SetupListeners();
            SubscribeToEvents();
        }
        
        private void InitializeUI()
        {
            // Initialize category dropdown
            if (categoryDropdown != null && leaderboardService != null)
            {
                categoryDropdown.ClearOptions();
                
                var categories = leaderboardService.Categories;
                List<string> options = new List<string>();
                
                foreach (var category in categories)
                {
                    options.Add(category.name);
                }
                
                categoryDropdown.AddOptions(options);
                
                // Set default selection
                if (options.Count > 0)
                {
                    categoryDropdown.value = 0;
                    OnCategoryChanged(0);
                }
            }
            
            UpdatePlayerInfo();
        }
        
        private void SetupListeners()
        {
            if (categoryDropdown != null)
            {
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            }
            
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshLeaderboard);
            }
            
            if (loadOnlineButton != null)
            {
                loadOnlineButton.onClick.AddListener(LoadOnlineLeaderboard);
            }
            
            if (playGhostButton != null)
            {
                playGhostButton.onClick.AddListener(PlayBestGhost);
            }
        }
        
        private void SubscribeToEvents()
        {
            if (leaderboardService != null)
            {
                leaderboardService.OnLeaderboardLoaded += OnLeaderboardLoaded;
                leaderboardService.OnScoreSubmitted += OnScoreSubmitted;
                leaderboardService.OnGhostRunSaved += OnGhostRunSaved;
                leaderboardService.OnLeaderboardError += OnLeaderboardError;
            }
        }
        
        private void OnDestroy()
        {
            if (leaderboardService != null)
            {
                leaderboardService.OnLeaderboardLoaded -= OnLeaderboardLoaded;
                leaderboardService.OnScoreSubmitted -= OnScoreSubmitted;
                leaderboardService.OnGhostRunSaved -= OnGhostRunSaved;
                leaderboardService.OnLeaderboardError -= OnLeaderboardError;
            }
        }
        
        #region Public Methods
        
        /// <summary>
        /// Display leaderboard for a specific course.
        /// </summary>
        public void ShowLeaderboard(string courseId, string courseName = null)
        {
            currentLeaderboardId = courseId;
            
            if (titleText != null)
            {
                titleText.text = courseName ?? courseId;
            }
            
            RefreshLeaderboard();
        }
        
        /// <summary>
        /// Display leaderboard for a specific category.
        /// </summary>
        public void ShowLeaderboardByCategory(string leaderboardId)
        {
            currentLeaderboardId = leaderboardId;
            RefreshLeaderboard();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnCategoryChanged(int index)
        {
            if (leaderboardService == null || leaderboardService.Categories.Length <= index) return;
            
            var category = leaderboardService.Categories[index];
            
            if (currentLeaderboardId != null)
            {
                // Replace the category part of the leaderboard ID
                string courseId = GetCourseIdFromLeaderboardId(currentLeaderboardId);
                currentLeaderboardId = $"{category.id}_{courseId}";
                RefreshLeaderboard();
            }
        }
        
        private void OnLeaderboardLoaded(string leaderboardId, List<LeaderboardEntry> entries)
        {
            if (leaderboardId == currentLeaderboardId)
            {
                DisplayEntries(entries);
            }
        }
        
        private void OnScoreSubmitted(string leaderboardId, bool isPersonalBest)
        {
            if (isPersonalBest)
            {
                Debug.Log("New personal best!");
                // Could show a celebration animation here
            }
            
            RefreshLeaderboard();
        }
        
        private void OnGhostRunSaved(string leaderboardId, GhostRunData ghostData)
        {
            if (leaderboardId == currentLeaderboardId)
            {
                UpdateGhostInfo(ghostData);
            }
        }
        
        private void OnLeaderboardError(string error)
        {
            Debug.LogError($"Leaderboard error: {error}");
            
            if (titleText != null)
            {
                titleText.text = $"Error: {error}";
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void RefreshLeaderboard()
        {
            if (leaderboardService == null || string.IsNullOrEmpty(currentLeaderboardId)) return;
            
            currentEntries = leaderboardService.GetLeaderboard(currentLeaderboardId, displayLimit);
            DisplayEntries(currentEntries);
            UpdatePlayerInfo();
            UpdateGhostDisplay();
        }
        
        private void DisplayEntries(List<LeaderboardEntry> entries)
        {
            ClearEntries();
            
            if (entryPrefab == null || contentParent == null)
            {
                Debug.LogWarning("Entry prefab or content parent not assigned");
                return;
            }
            
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entryObj = Instantiate(entryPrefab, contentParent);
                entryObj.SetActive(true);
                
                // Configure entry UI
                var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Configure(entry, i + 1, localPlayerColor);
                }
                else
                {
                    // Fallback: try to configure with basic components
                    ConfigureEntryFallback(entryObj, entry, i + 1);
                }
                
                entryObjects.Add(entryObj);
            }
        }
        
        private void ConfigureEntryFallback(GameObject entryObj, LeaderboardEntry entry, int rank)
        {
            // Try to find and set text components
            var texts = entryObj.GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
            {
                if (text.name.Contains("Rank"))
                    text.text = rank.ToString();
                else if (text.name.Contains("Name"))
                    text.text = entry.playerName;
                else if (text.name.Contains("Score"))
                    text.text = FormatScore(entry.score);
                else if (text.name.Contains("Secondary"))
                    text.text = FormatSecondaryValue(entry.secondaryValue);
            }
            
            // Highlight local player
            if (entry.isLocalPlayer)
            {
                var image = entryObj.GetComponent<Image>();
                if (image != null)
                {
                    image.color = localPlayerColor;
                }
            }
        }
        
        private void ClearEntries()
        {
            foreach (var obj in entryObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            entryObjects.Clear();
        }
        
        private void UpdatePlayerInfo()
        {
            if (leaderboardService == null || string.IsNullOrEmpty(currentLeaderboardId)) return;
            
            int rank = leaderboardService.GetPlayerRank(currentLeaderboardId);
            var bestScore = leaderboardService.GetPlayerBestScore(currentLeaderboardId);
            
            if (playerRankText != null)
            {
                playerRankText.text = rank > 0 ? $"Rank: #{rank}" : "Not ranked";
            }
            
            if (playerBestText != null)
            {
                playerBestText.text = bestScore != null 
                    ? $"Best: {FormatScore(bestScore.score)}" 
                    : "No best score";
            }
        }
        
        private void UpdateGhostDisplay()
        {
            if (ghostInfoText == null || playGhostButton == null) return;
            
            if (!showGhosts || leaderboardService == null || string.IsNullOrEmpty(currentLeaderboardId))
            {
                playGhostButton.gameObject.SetActive(false);
                ghostInfoText.gameObject.SetActive(false);
                return;
            }
            
            var ghost = leaderboardService.LoadBestGhostRun(currentLeaderboardId);
            if (ghost != null)
            {
                playGhostButton.gameObject.SetActive(true);
                ghostInfoText.gameObject.SetActive(true);
                ghostInfoText.text = $"Ghost: {ghost.finalTime:F2}s ({ghost.totalFaults} faults)";
            }
            else
            {
                playGhostButton.gameObject.SetActive(false);
                ghostInfoText.text = "No ghost run available";
            }
        }
        
        private void UpdateGhostInfo(GhostRunData ghostData)
        {
            if (ghostInfoText != null && ghostData != null)
            {
                ghostInfoText.text = $"Ghost: {ghostData.finalTime:F2}s ({ghostData.totalFaults} faults)";
            }
        }
        
        #endregion
        
        #region Actions
        
        private void LoadOnlineLeaderboard()
        {
            if (leaderboardService != null && !string.IsNullOrEmpty(currentLeaderboardId))
            {
                leaderboardService.LoadOnlineLeaderboard(currentLeaderboardId, displayLimit);
            }
        }
        
        private void PlayBestGhost()
        {
            if (leaderboardService == null || string.IsNullOrEmpty(currentLeaderboardId)) return;
            
            var ghost = leaderboardService.LoadBestGhostRun(currentLeaderboardId);
            if (ghost != null)
            {
                // Find the ReplayManager and start ghost playback
                var replayManager = FindObjectOfType<Gameplay.Replay.ReplayManager>();
                if (replayManager != null)
                {
                    // Convert GhostRunData to ReplayData
                    var replayData = ConvertGhostToReplayData(ghost);
                    // replayManager.StartPlayback(replayData);
                    Debug.Log($"Playing ghost run: {ghost.finalTime:F2}s");
                }
            }
        }
        
        private Gameplay.Replay.ReplayData ConvertGhostToReplayData(GhostRunData ghost)
        {
            var replayData = ScriptableObject.CreateInstance<Gameplay.Replay.ReplayData>();
            
            // Convert ghost frames to replay frames
            foreach (var ghostFrame in ghost.frames)
            {
                var replayFrame = new Gameplay.Replay.ReplayFrame
                {
                    timestamp = ghostFrame.time,
                    dogPosition = ghostFrame.dogPosition.ToVector3(),
                    dogRotation = ghostFrame.dogRotation.ToQuaternion(),
                    handlerPosition = ghostFrame.handlerPosition.ToVector3(),
                    handlerRotation = Quaternion.identity // Ghost doesn't store handler rotation
                };
                replayData.frames.Add(replayFrame);
            }
            
            replayData.runTime = ghost.finalTime;
            replayData.faultCount = ghost.totalFaults;
            
            return replayData;
        }
        
        #endregion
        
        #region Helpers
        
        private string GetCourseIdFromLeaderboardId(string leaderboardId)
        {
            // Leaderboard IDs are formatted as "category_courseId"
            int underscoreIndex = leaderboardId.IndexOf('_');
            if (underscoreIndex > 0 && underscoreIndex < leaderboardId.Length - 1)
            {
                return leaderboardId.Substring(underscoreIndex + 1);
            }
            return leaderboardId;
        }
        
        private string FormatScore(float score)
        {
            // Determine format based on the score type
            if (score < 100f) // Likely time in seconds
            {
                return $"{score:F2}s";
            }
            else if (score < 1000f) // Likely points
            {
                return $"{score:F0}";
            }
            else
            {
                return score.ToString("N0");
            }
        }
        
        private string FormatSecondaryValue(float value)
        {
            if (value < 100f)
            {
                return $"{value:F2}";
            }
            return value.ToString("F0");
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI component for a single leaderboard entry.
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text secondaryText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject ghostIndicator;
        
        public void Configure(LeaderboardEntry entry, int rank, Color localPlayerColor)
        {
            if (rankText != null) rankText.text = rank.ToString();
            if (playerNameText != null) playerNameText.text = entry.playerName;
            if (scoreText != null) scoreText.text = FormatScore(entry.score);
            if (secondaryText != null) secondaryText.text = FormatSecondary(entry.secondaryValue);
            
            if (backgroundImage != null && entry.isLocalPlayer)
            {
                backgroundImage.color = localPlayerColor;
            }
            
            if (ghostIndicator != null)
            {
                ghostIndicator.SetActive(entry.hasGhostData);
            }
        }
        
        private string FormatScore(float score)
        {
            if (score < 100f) return $"{score:F2}s";
            return score.ToString("N0");
        }
        
        private string FormatSecondary(float value)
        {
            if (value < 100f) return $"{value:F2}";
            return value.ToString("F0");
        }
    }
}