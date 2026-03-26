using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Services;

namespace AgilityDogs.UI
{
    /// <summary>
    /// CutsceneUI - Handles cutscene playback during campaign mode
    /// Displays dialogue, character portraits, and handles user input
    /// </summary>
    public class CutsceneUI : MonoBehaviour
    {
        public static CutsceneUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject cutsceneCanvas;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image continueIcon;
        [SerializeField] private Image fadeOverlay;

        [Header("Settings")]
        [SerializeField] private float typeWriterSpeed = 30f;
        [SerializeField] private float autoAdvanceDelay = 0.5f;

        // State
        private bool isPlaying = false;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private CutsceneData currentCutscene;
        private int currentLineIndex = 0;

        // Events
        public event Action OnCutsceneComplete;

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
            SubscribeToEvents();
            HideCutscene();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            var campaignService = CampaignService.Instance;
            if (campaignService != null)
            {
                campaignService.OnCutsceneStarted += HandleCutsceneStarted;
                campaignService.OnCutsceneEnded += HandleCutsceneEnded;
                campaignService.OnDialogueLine += HandleDialogueLine;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var campaignService = CampaignService.Instance;
            if (campaignService != null)
            {
                campaignService.OnCutsceneStarted -= HandleCutsceneStarted;
                campaignService.OnCutsceneEnded -= HandleCutsceneEnded;
                campaignService.OnDialogueLine -= HandleDialogueLine;
            }
        }

        private void Update()
        {
            if (!isPlaying) return;

            // Handle input
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (isTyping)
                {
                    // Skip typing animation
                    CompleteTyping();
                }
                else
                {
                    // Advance to next line
                    AdvanceDialogue();
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Show the cutscene UI
        /// </summary>
        public void ShowCutscene()
        {
            if (cutsceneCanvas != null)
            {
                cutsceneCanvas.SetActive(true);
            }
            StartFadeIn();
        }

        /// <summary>
        /// Hide the cutscene UI
        /// </summary>
        public void HideCutscene()
        {
            if (cutsceneCanvas != null)
            {
                cutsceneCanvas.SetActive(false);
            }
            isPlaying = false;
        }

        /// <summary>
        /// Play a specific cutscene
        /// </summary>
        public void PlayCutscene(string cutsceneId)
        {
            CampaignService.Instance?.PlayCutscene(cutsceneId);
        }

        /// <summary>
        /// Skip the current cutscene
        /// </summary>
        public void SkipCutscene()
        {
            if (currentCutscene != null)
            {
                HideCutscene();
                CampaignService.Instance?.EndCampaign();
                OnCutsceneComplete?.Invoke();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleCutsceneStarted(CutsceneData cutscene)
        {
            currentCutscene = cutscene;
            currentLineIndex = 0;
            isPlaying = true;
            ShowCutscene();

            Debug.Log($"[CutsceneUI] Playing cutscene: {cutscene.title ?? cutscene.cutsceneId}");
        }

        private void HandleCutsceneEnded()
        {
            HideCutscene();
            currentCutscene = null;
            isPlaying = false;
            OnCutsceneComplete?.Invoke();
        }

        private void HandleDialogueLine(DialogueLine line)
        {
            DisplayDialogue(line);
        }

        #endregion

        #region Dialogue Display

        private void DisplayDialogue(DialogueLine line)
        {
            // Clear previous
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // Get speaker ID for character lookup (fall back to speakerName if speakerId not set)
            string speakerId = !string.IsNullOrEmpty(line.speakerId) ? line.speakerId : line.speakerName;

            // Get character data
            var character = CampaignService.Instance?.GetCharacter(speakerId);

            // Update speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = character?.characterName ?? line.speakerName ?? "Unknown";
            }

            // Update portrait with emotion-aware selection
            if (portraitImage != null)
            {
                Sprite portrait = null;
                if (character != null && line.showPortrait)
                {
                    // Use emotion-aware portrait if available
                    portrait = character.GetPortraitForEmotion(line.emotion);
                }

                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            // Update emotion indicator if available
            UpdateEmotionIndicator(line.emotion);

            // Start typewriter effect
            typingCoroutine = StartCoroutine(TypeText(line.dialogueText));
        }

        private void UpdateEmotionIndicator(string emotion)
        {
            // Could implement emotion-based visual effects here
            // For now, we'll leave it as a placeholder for future enhancement
        }

            // Update speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName ?? "";
            }

            // Update portrait
            if (portraitImage != null)
            {
                var character = CampaignService.Instance?.GetCharacter(line.speakerName);
                if (character?.portrait != null && line.showPortrait)
                {
                    portraitImage.sprite = character.portrait;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            // Hide continue icon during typing
            if (continueIcon != null)
            {
                continueIcon.gameObject.SetActive(false);
            }

            // Start typewriter effect
            typingCoroutine = StartCoroutine(TypeText(line.dialogueText));
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(1f / typeWriterSpeed);
            }

            isTyping = false;
            OnTypingComplete();
        }

        private void CompleteTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            isTyping = false;
            dialogueText.text = currentCutscene?.dialogueLines[currentLineIndex].dialogueText ?? "";
            OnTypingComplete();
        }

        private void OnTypingComplete()
        {
            // Show continue icon
            if (continueIcon != null)
            {
                continueIcon.gameObject.SetActive(true);
            }

            // Auto-advance after delay
            StartCoroutine(AutoAdvance());
        }

        private IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(autoAdvanceDelay);

            if (!isTyping && continueIcon != null && continueIcon.gameObject.activeSelf)
            {
                AdvanceDialogue();
            }
        }

        private void AdvanceDialogue()
        {
            if (currentCutscene == null) return;

            currentLineIndex++;

            if (currentLineIndex < currentCutscene.dialogueLines.Count)
            {
                // Show next line
                DisplayDialogue(currentCutscene.dialogueLines[currentLineIndex]);
            }
            else
            {
                // Cutscene complete
                HandleCutsceneEnded();
            }
        }

        #endregion

        #region Fade Effects

        private void StartFadeIn()
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.color = Color.black;
                fadeOverlay.gameObject.SetActive(true);
                StartCoroutine(FadeOverlay(1f, 0f, 0.5f));
            }
        }

        private void StartFadeOut(System.Action onComplete = null)
        {
            if (fadeOverlay != null)
            {
                StartCoroutine(FadeOverlay(0f, 1f, 0.5f, onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator FadeOverlay(float from, float to, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;
            Color color = fadeOverlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                fadeOverlay.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            fadeOverlay.color = new Color(color.r, color.g, color.b, to);
            
            if (to == 0f)
            {
                fadeOverlay.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        #endregion
    }
}