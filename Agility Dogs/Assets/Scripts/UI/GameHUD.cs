using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject hudPanel;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI timerDecimalText;
        [SerializeField] private Image timerBackground;
        [SerializeField] private Color normalTimeColor = Color.white;
        [SerializeField] private Color fastTimeColor = Color.green;
        [SerializeField] private Color slowTimeColor = Color.red;

        [Header("Split Times")]
        [SerializeField] private Transform splitTimesContainer;
        [SerializeField] private GameObject splitTimeEntryPrefab;
        [SerializeField] private int maxSplitTimeDisplay = 5;
        private List<SplitTimeEntry> splitTimeEntries = new List<SplitTimeEntry>();

        [Header("Fault Counter")]
        [SerializeField] private TextMeshProUGUI faultCountText;
        [SerializeField] private Image faultIcon;
        [SerializeField] private GameObject faultPanel;
        [SerializeField] private Color noFaultColor = Color.green;
        [SerializeField] private Color hasFaultColor = Color.red;
        [SerializeField] private AnimationFaultAnimation faultPulseAnimation;

        [Header("Score/Position")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private GameObject scorePanel;

        [Header("Course Progress")]
        [SerializeField] private Slider courseProgressSlider;
        [SerializeField] private TextMeshProUGUI obstacleCountText;
        [SerializeField] private Image currentObstacleIcon;
        [SerializeField] private Image nextObstacleIcon;
        [SerializeField] private GameObject courseProgressPanel;

        [Header("Command Feedback")]
        [SerializeField] private TextMeshProUGUI commandFeedbackText;
        [SerializeField] private Image commandFeedbackIcon;
        [SerializeField] private float commandFeedbackDuration = 1f;
        [SerializeField] private CanvasGroup commandFeedbackCanvasGroup;
        private float commandFeedbackTimer;

        [Header("Handler Influence")]
        [SerializeField] private Image pathInfluenceIndicator;
        [SerializeField] private float pathInfluenceScale = 50f;
        [SerializeField] private Color positiveInfluenceColor = Color.cyan;
        [SerializeField] private Color negativeInfluenceColor = Color.magenta;

        [Header("Countdown Overlay")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Animation countdownAnimation;
        [SerializeField] private AudioClip countdownBeep;

        [Header("Run Complete Celebration")]
        [SerializeField] private GameObject celebrationPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI resultFaultsText;
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private Animation celebrationAnimation;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button continueButton;

        [Header("Fault Notification")]
        [SerializeField] private GameObject faultNotificationPanel;
        [SerializeField] private TextMeshProUGUI faultNotificationText;
        [SerializeField] private Image faultNotificationIcon;
        [SerializeField] private float faultNotificationDuration = 2f;
        [SerializeField] private Animation faultNotificationAnimation;
        private float faultNotificationTimer;

        [Header("Split Time Highlight")]
        [SerializeField] private GameObject splitHighlightPanel;
        [SerializeField] private TextMeshProUGUI splitHighlightText;
        [SerializeField] private TextMeshProUGUI splitHighlightDiffText;
        [SerializeField] private Image splitHighlightBorder;
        [SerializeField] private float splitHighlightDuration = 1.5f;
        [SerializeField] private Color personalBestColor = Color.yellow;
        [SerializeField] private Color goodSplitColor = Color.green;
        [SerializeField] private Color badSplitColor = Color.red;
        private float splitHighlightTimer;

        [Header("Dog State")]
        [SerializeField] private Image dogStateIcon;
        [SerializeField] private TextMeshProUGUI dogStateText;
        [SerializeField] private Sprite[] dogStateSprites;
        [SerializeField] private GameObject dogStatePanel;

        [Header("Gesture Indicator")]
        [SerializeField] private Image gestureIndicator;
        [SerializeField] private TextMeshProUGUI gestureText;
        [SerializeField] private float gestureIndicatorDuration = 0.5f;
        private float gestureIndicatorTimer;

        // References
        private GameManager gameManager;
        private AgilityScoringService scoringService;
        private float currentRunTime;
        private int currentFaults;
        private int currentObstaclesCompleted;
        private int totalObstacles;
        private float personalBestTime = float.MaxValue;
        private List<float> personalBestSplits = new List<float>();

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            scoringService = FindObjectOfType<AgilityScoringService>();

            // Subscribe to events
            SubscribeToEvents();

            // Hide panels initially
            HideAllPanels();
            ShowHUD();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnCountdownTick += HandleCountdownTick;
            GameEvents.OnCommandIssued += HandleCommandIssued;
            GameEvents.OnHandlerPathInfluence += HandlePathInfluence;
            GameEvents.OnHandlerGesture += HandleGesture;
            GameEvents.OnDogRecovery += HandleDogRecovery;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTime;
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnCountdownTick -= HandleCountdownTick;
            GameEvents.OnCommandIssued -= HandleCommandIssued;
            GameEvents.OnHandlerPathInfluence -= HandlePathInfluence;
            GameEvents.OnHandlerGesture -= HandleGesture;
            GameEvents.OnDogRecovery -= HandleDogRecovery;
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState == GameState.Gameplay)
            {
                UpdateTimer();
            }

            UpdateCommandFeedback();
            UpdateFaultNotification();
            UpdateSplitHighlight();
            UpdateGestureIndicator();
        }

        #region Event Handlers

        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Countdown:
                    ShowCountdown();
                    break;
                case GameState.Gameplay:
                    HideCountdown();
                    ShowHUD();
                    break;
                case GameState.RunComplete:
                    // Run completion is handled by OnRunCompleted
                    break;
                case GameState.Results:
                    // Results screen is separate
                    break;
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            currentObstaclesCompleted++;
            UpdateCourseProgress();

            if (!clean)
            {
                ShowFaultNotification("Contact Zone Missed!");
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            currentFaults++;
            UpdateFaultCounter();

            string faultMessage = GetFaultMessage(fault);
            ShowFaultNotification(faultMessage);

            // Animate fault counter
            if (faultPulseAnimation != null)
            {
                faultPulseAnimation.TriggerAnimation();
            }
        }

        private void HandleSplitTime(float splitTime)
        {
            AddSplitTime(splitTime);

            // Check if it's a personal best
            if (splitTime < personalBestTime)
            {
                personalBestTime = splitTime;
                ShowSplitHighlight(splitTime, 0f, true);
            }
            else
            {
                float diff = splitTime - personalBestTime;
                ShowSplitHighlight(splitTime, diff, false);
            }
        }

        private void HandleRunStarted()
        {
            currentRunTime = 0f;
            currentFaults = 0;
            currentObstaclesCompleted = 0;

            UpdateTimer();
            UpdateFaultCounter();
            UpdateCourseProgress();
            ClearSplitTimes();
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            ShowCelebration(result, time, faults);
        }

        private void HandleCountdownTick()
        {
            // Countdown is handled by the countdown panel
        }

        private void HandleCommandIssued(HandlerCommand command)
        {
            ShowCommandFeedback(command);
        }

        private void HandlePathInfluence(float influence)
        {
            UpdatePathInfluenceIndicator(influence);
        }

        private void HandleGesture(GestureType gesture, Vector3 direction)
        {
            ShowGestureIndicator(gesture);
        }

        private void HandleDogRecovery(RecoveryReason reason)
        {
            if (reason != RecoveryReason.None)
            {
                string message = GetRecoveryMessage(reason);
                ShowFaultNotification(message);
            }
        }

        #endregion

        #region Update Methods

        private void UpdateTimer()
        {
            if (timerText == null) return;

            currentRunTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(currentRunTime / 60f);
            int seconds = Mathf.FloorToInt(currentRunTime % 60f);
            int decimals = Mathf.FloorToInt((currentRunTime % 1f) * 100f);

            timerText.text = $"{minutes:00}:{seconds:00}";
            if (timerDecimalText != null)
            {
                timerDecimalText.text = $".{decimals:00}";
            }

            // Color based on pace (simplified - would compare against par time)
            if (timerBackground != null)
            {
                // This is a simplified check - real implementation would compare against course par time
                timerBackground.color = normalTimeColor;
            }
        }

        private void UpdateFaultCounter()
        {
            if (faultCountText != null)
            {
                faultCountText.text = currentFaults.ToString();
            }

            if (faultIcon != null)
            {
                faultIcon.color = currentFaults > 0 ? hasFaultColor : noFaultColor;
            }
        }

        private void UpdateCourseProgress()
        {
            if (courseProgressSlider != null)
            {
                float progress = totalObstacles > 0 ? (float)currentObstaclesCompleted / totalObstacles : 0f;
                courseProgressSlider.value = progress;
            }

            if (obstacleCountText != null)
            {
                obstacleCountText.text = $"{currentObstaclesCompleted}/{totalObstacles}";
            }
        }

        private void UpdateCommandFeedback()
        {
            if (commandFeedbackCanvasGroup == null) return;

            if (commandFeedbackTimer > 0)
            {
                commandFeedbackTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(commandFeedbackTimer / commandFeedbackDuration);
                commandFeedbackCanvasGroup.alpha = alpha;

                // Scale effect
                float scale = 1f + (1f - alpha) * 0.3f;
                commandFeedbackText.transform.localScale = Vector3.one * scale;
            }
            else
            {
                commandFeedbackCanvasGroup.alpha = 0f;
            }
        }

        private void UpdateFaultNotification()
        {
            if (faultNotificationTimer > 0)
            {
                faultNotificationTimer -= Time.deltaTime;
                if (faultNotificationTimer <= 0)
                {
                    faultNotificationPanel.SetActive(false);
                }
            }
        }

        private void UpdateSplitHighlight()
        {
            if (splitHighlightTimer > 0)
            {
                splitHighlightTimer -= Time.deltaTime;
                if (splitHighlightTimer <= 0)
                {
                    splitHighlightPanel.SetActive(false);
                }
            }
        }

        private void UpdateGestureIndicator()
        {
            if (gestureIndicatorTimer > 0)
            {
                gestureIndicatorTimer -= Time.deltaTime;
                if (gestureIndicatorTimer <= 0)
                {
                    gestureIndicator.enabled = false;
                    gestureText.enabled = false;
                }
            }
        }

        private void UpdatePathInfluenceIndicator(float influence)
        {
            if (pathInfluenceIndicator == null) return;

            pathInfluenceIndicator.enabled = Mathf.Abs(influence) > 0.1f;

            if (Mathf.Abs(influence) > 0.1f)
            {
                Vector2 anchoredPos = pathInfluenceIndicator.rectTransform.anchoredPosition;
                anchoredPos.x = influence * pathInfluenceScale;
                pathInfluenceIndicator.rectTransform.anchoredPosition = anchoredPos;

                pathInfluenceIndicator.color = influence > 0 ? positiveInfluenceColor : negativeInfluenceColor;
            }
        }

        #endregion

        #region Display Methods

        private void HideAllPanels()
        {
            if (countdownPanel != null) countdownPanel.SetActive(false);
            if (celebrationPanel != null) celebrationPanel.SetActive(false);
            if (faultNotificationPanel != null) faultNotificationPanel.SetActive(false);
            if (splitHighlightPanel != null) splitHighlightPanel.SetActive(false);
        }

        private void ShowHUD()
        {
            if (hudPanel != null) hudPanel.SetActive(true);
        }

        private void ShowCountdown()
        {
            if (countdownPanel != null) countdownPanel.SetActive(true);
        }

        private void HideCountdown()
        {
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }

        private void ShowCelebration(RunResult result, float time, int faults)
        {
            if (celebrationPanel == null) return;

            celebrationPanel.SetActive(true);

            // Set result text
            if (resultText != null)
            {
                resultText.text = GetResultText(result);
                resultText.color = GetResultColor(result);
            }

            // Set time
            if (resultTimeText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                int decimals = Mathf.FloorToInt((time % 1f) * 100f);
                resultTimeText.text = $"Time: {minutes:00}:{seconds:00}.{decimals:00}";
            }

            // Set faults
            if (resultFaultsText != null)
            {
                resultFaultsText.text = $"Faults: {faults}";
            }

            // Play celebration effects
            if (celebrationParticles != null && result == RunResult.Qualified)
            {
                celebrationParticles.Play();
            }

            if (celebrationAnimation != null)
            {
                celebrationAnimation.Play();
            }
        }

        private void ShowFaultNotification(string message)
        {
            if (faultNotificationPanel == null) return;

            faultNotificationPanel.SetActive(true);
            faultNotificationTimer = faultNotificationDuration;

            if (faultNotificationText != null)
            {
                faultNotificationText.text = message;
            }

            if (faultNotificationAnimation != null)
            {
                faultNotificationAnimation.Rewind();
                faultNotificationAnimation.Play();
            }
        }

        private void ShowSplitHighlight(float time, float diff, bool isPersonalBest)
        {
            if (splitHighlightPanel == null) return;

            splitHighlightPanel.SetActive(true);
            splitHighlightTimer = splitHighlightDuration;

            if (splitHighlightText != null)
            {
                splitHighlightText.text = FormatTime(time);
            }

            if (splitHighlightDiffText != null)
            {
                if (isPersonalBest)
                {
                    splitHighlightDiffText.text = "NEW BEST!";
                    splitHighlightDiffText.color = personalBestColor;
                }
                else
                {
                    string sign = diff >= 0 ? "+" : "-";
                    splitHighlightDiffText.text = $"{sign}{FormatTime(Mathf.Abs(diff))}";
                    splitHighlightDiffText.color = diff <= 0 ? goodSplitColor : badSplitColor;
                }
            }

            if (splitHighlightBorder != null)
            {
                splitHighlightBorder.color = isPersonalBest ? personalBestColor : 
                    (diff <= 0 ? goodSplitColor : badSplitColor);
            }
        }

        private void ShowCommandFeedback(HandlerCommand command)
        {
            if (commandFeedbackText == null) return;

            commandFeedbackText.text = GetCommandDisplayName(command);
            commandFeedbackTimer = commandFeedbackDuration;

            if (commandFeedbackCanvasGroup != null)
            {
                commandFeedbackCanvasGroup.alpha = 1f;
            }
        }

        private void ShowGestureIndicator(GestureType gesture)
        {
            if (gestureIndicator == null || gestureText == null) return;

            gestureIndicator.enabled = true;
            gestureText.enabled = true;
            gestureText.text = GetGestureDisplayName(gesture);
            gestureIndicatorTimer = gestureIndicatorDuration;
        }

        private void AddSplitTime(float time)
        {
            // Create new entry
            SplitTimeEntry entry = new SplitTimeEntry
            {
                time = time,
                splitNumber = splitTimeEntries.Count + 1
            };

            splitTimeEntries.Add(entry);

            // Update display
            UpdateSplitTimeDisplay();
        }

        private void UpdateSplitTimeDisplay()
        {
            if (splitTimesContainer == null || splitTimeEntryPrefab == null) return;

            // Clear existing entries
            foreach (Transform child in splitTimesContainer)
            {
                Destroy(child.gameObject);
            }

            // Show most recent splits
            int startIndex = Mathf.Max(0, splitTimeEntries.Count - maxSplitTimeDisplay);
            for (int i = startIndex; i < splitTimeEntries.Count; i++)
            {
                GameObject entryObj = Instantiate(splitTimeEntryPrefab, splitTimesContainer);
                TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();

                if (entryText != null)
                {
                    SplitTimeEntry entry = splitTimeEntries[i];
                    entryText.text = $"Split {entry.splitNumber}: {FormatTime(entry.time)}";
                }
            }
        }

        private void ClearSplitTimes()
        {
            splitTimeEntries.Clear();
            UpdateSplitTimeDisplay();
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

        private string GetFaultMessage(FaultType fault)
        {
            switch (fault)
            {
                case FaultType.MissedContact: return "Missed Contact Zone!";
                case FaultType.WrongCourse: return "Wrong Course!";
                case FaultType.Refusal: return "Refusal!";
                case FaultType.RunOut: return "Run Out!";
                case FaultType.TimeFault: return "Time Fault!";
                case FaultType.KnockedBar: return "Knocked Bar!";
                case FaultType.OffCourse: return "Off Course!";
                default: return "Fault!";
            }
        }

        private string GetRecoveryMessage(RecoveryReason reason)
        {
            switch (reason)
            {
                case RecoveryReason.MissedCommand: return "Recovering...";
                case RecoveryReason.WrongObstacle: return "Wrong Obstacle!";
                case RecoveryReason.Stuck: return "Stuck!";
                case RecoveryReason.HandlerTooFar: return "Come Back!";
                default: return "Recovering...";
            }
        }

        private string GetResultText(RunResult result)
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
                case RunResult.TimeFaultOnly: return new Color(1f, 0.5f, 0f); // Orange
                default: return Color.white;
            }
        }

        private string GetCommandDisplayName(HandlerCommand command)
        {
            switch (command)
            {
                case HandlerCommand.Jump: return "JUMP!";
                case HandlerCommand.Tunnel: return "TUNNEL!";
                case HandlerCommand.Weave: return "WEAVE!";
                case HandlerCommand.Table: return "TABLE!";
                case HandlerCommand.Go: return "GO!";
                case HandlerCommand.Here: return "HERE!";
                case HandlerCommand.Out: return "OUT!";
                case HandlerCommand.ComeBye: return "COME BYE!";
                case HandlerCommand.Away: return "AWAY!";
                case HandlerCommand.Left: return "LEFT!";
                case HandlerCommand.Right: return "RIGHT!";
                default: return command.ToString().ToUpper();
            }
        }

        private string GetGestureDisplayName(GestureType gesture)
        {
            switch (gesture)
            {
                case GestureType.Point: return "POINT";
                case GestureType.PointLeft: return "POINT LEFT";
                case GestureType.PointRight: return "POINT RIGHT";
                case GestureType.Beckon: return "BECKON";
                case GestureType.Wave: return "WAVE";
                case GestureType.Stop: return "STOP";
                default: return "";
            }
        }

        #endregion

        #region Public Methods

        public void SetTotalObstacles(int total)
        {
            totalObstacles = total;
            UpdateCourseProgress();
        }

        public void SetPersonalBest(float time)
        {
            personalBestTime = time;
        }

        public void SetPersonalBestSplit(int index, float time)
        {
            while (personalBestSplits.Count <= index)
            {
                personalBestSplits.Add(float.MaxValue);
            }
            personalBestSplits[index] = time;
        }

        public void UpdateCountdown(int countdown)
        {
            if (countdownText != null)
            {
                countdownText.text = countdown > 0 ? countdown.ToString() : "GO!";
            }
        }

        #endregion
    }

    [System.Serializable]
    public class SplitTimeEntry
    {
        public float time;
        public int splitNumber;
    }

    public class AnimationFaultAnimation : MonoBehaviour
    {
        public void TriggerAnimation()
        {
            // Trigger animation - would be implemented with Animation component
            Animation anim = GetComponent<Animation>();
            if (anim != null)
            {
                anim.Rewind();
                anim.Play();
            }
        }
    }
}
