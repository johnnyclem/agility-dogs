using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Services;

namespace AgilityDogs.Demo
{
    public class DemoUI : MonoBehaviour
    {
        public static DemoUI Instance { get; private set; }

        [Header("Canvas")]
        [SerializeField] private Canvas demoCanvas;
        [SerializeField] private CanvasGroup rootCanvasGroup;

        [Header("Branding")]
        [SerializeField] private GameObject brandingPanel;
        [SerializeField] private TextMeshProUGUI brandingTitleText;
        [SerializeField] private TextMeshProUGUI brandingSubtitleText;
        [SerializeField] private float brandingFadeDuration = 1.5f;

        [Header("Dialogue")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject speakerIndicator;

        [Header("Segment Header")]
        [SerializeField] private GameObject segmentHeaderPanel;
        [SerializeField] private TextMeshProUGUI segmentTitleText;
        [SerializeField] private TextMeshProUGUI segmentSubtitleText;

        [Header("Colors")]
        [SerializeField] private Color arthurColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color buckColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private Color paColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color coachColor = new Color(0.4f, 0.9f, 0.4f);
        [SerializeField] private Color narratorColor = new Color(1f, 1f, 1f);

        [Header("Typewriter")]
        [SerializeField] private float typewriterSpeed = 40f;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float backgroundAlpha = 0.85f;

        private Coroutine typewriterCoroutine;
        private bool isTyping;
        private string fullText;

        public bool IsVisible { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateUIIfNeeded();
            HideImmediate();
        }

        void CreateUIIfNeeded()
        {
            if (demoCanvas != null) return;

            var canvasObj = new GameObject("DemoCanvas");
            canvasObj.transform.SetParent(transform, false);
            demoCanvas = canvasObj.AddComponent<Canvas>();
            demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            demoCanvas.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var rootObj = new GameObject("Root");
            rootObj.transform.SetParent(canvasObj.transform, false);
            var rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootCanvasGroup = rootObj.AddComponent<CanvasGroup>();
            rootCanvasGroup.alpha = 0f;

            backgroundImage = rootObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.02f, 0.02f, 0.06f, backgroundAlpha);

            CreateBrandingPanel(rootObj.transform);
            CreateDialoguePanel(rootObj.transform);
            CreateSegmentHeader(rootObj.transform);
        }

        void CreateBrandingPanel(Transform parent)
        {
            brandingPanel = new GameObject("BrandingPanel");
            brandingPanel.transform.SetParent(parent, false);
            var rect = brandingPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            brandingTitleText = CreateText(brandingPanel.transform, "Title",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                new Vector2(900, 120), 64, Color.white, TextAlignmentOptions.Center,
                FontStyles.Bold);

            brandingSubtitleText = CreateText(brandingPanel.transform, "Subtitle",
                new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f),
                new Vector2(800, 60), 28, new Color(0.8f, 0.8f, 0.9f), TextAlignmentOptions.Center);

            brandingPanel.SetActive(false);
        }

        void CreateDialoguePanel(Transform parent)
        {
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(parent, false);
            var panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.3f);
            panelRect.offsetMin = new Vector2(40, 20);
            panelRect.offsetMax = new Vector2(-40, -20);

            var bg = dialoguePanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

            var speakerRow = new GameObject("SpeakerRow");
            speakerRow.transform.SetParent(dialoguePanel.transform, false);
            var sRect = speakerRow.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0.65f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.offsetMin = new Vector2(20, 0);
            sRect.offsetMax = new Vector2(-20, -5);

            speakerNameText = CreateText(speakerRow.transform, "SpeakerName",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(600, 40), 22, Color.white, TextAlignmentOptions.Left,
                FontStyles.Bold);
            speakerNameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            speakerNameText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            speakerNameText.rectTransform.pivot = new Vector2(0f, 0.5f);
            speakerNameText.rectTransform.offsetMin = new Vector2(0, -20);
            speakerNameText.rectTransform.offsetMax = new Vector2(0, 20);

            dialogueText = CreateText(dialoguePanel.transform, "DialogueText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), 20, new Color(0.9f, 0.9f, 0.95f), TextAlignmentOptions.Left,
                FontStyles.Normal, true);
            var dRect = dialogueText.rectTransform;
            dRect.anchorMin = new Vector2(0f, 0.02f);
            dRect.anchorMax = new Vector2(1f, 0.6f);
            dRect.offsetMin = new Vector2(25, 5);
            dRect.offsetMax = new Vector2(-25, -5);

            dialoguePanel.SetActive(false);
        }

        void CreateSegmentHeader(Transform parent)
        {
            segmentHeaderPanel = new GameObject("SegmentHeader");
            segmentHeaderPanel.transform.SetParent(parent, false);
            var rect = segmentHeaderPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.85f);
            rect.anchorMax = new Vector2(0.5f, 0.95f);
            rect.sizeDelta = new Vector2(600, 0);

            segmentTitleText = CreateText(segmentHeaderPanel.transform, "SegmentTitle",
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f),
                new Vector2(600, 40), 28, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center,
                FontStyles.Bold);
            var tRect = segmentTitleText.rectTransform;
            tRect.anchorMin = new Vector2(0f, 0.55f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.sizeDelta = Vector2.zero;

            segmentSubtitleText = CreateText(segmentHeaderPanel.transform, "SegmentSubtitle",
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
                new Vector2(600, 30), 18, new Color(0.7f, 0.7f, 0.8f), TextAlignmentOptions.Center);
            var sRect = segmentSubtitleText.rectTransform;
            sRect.anchorMin = new Vector2(0f, 0f);
            sRect.anchorMax = new Vector2(1f, 0.45f);
            sRect.sizeDelta = Vector2.zero;

            segmentHeaderPanel.SetActive(false);
        }

        TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment,
            FontStyles style = FontStyles.Normal, bool wordWrap = false)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.enableWordWrapping = wordWrap;
            return text;
        }

        #region Public API

        public void Show()
        {
            IsVisible = true;
            if (demoCanvas != null) demoCanvas.gameObject.SetActive(true);
            if (rootCanvasGroup != null) rootCanvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            IsVisible = false;
            if (rootCanvasGroup != null) rootCanvasGroup.alpha = 0f;
            if (brandingPanel != null) brandingPanel.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (segmentHeaderPanel != null) segmentHeaderPanel.SetActive(false);
            if (demoCanvas != null) demoCanvas.gameObject.SetActive(false);
        }

        public Coroutine FadeIn(float duration = 0.5f)
        {
            Show();
            return StartCoroutine(FadeCanvasGroup(rootCanvasGroup, 0f, 1f, duration));
        }

        public Coroutine FadeOut(float duration = 0.5f, Action onComplete = null)
        {
            return StartCoroutine(FadeOutRoutine(duration, onComplete));
        }

        IEnumerator FadeOutRoutine(float duration, Action onComplete)
        {
            yield return StartCoroutine(FadeCanvasGroup(rootCanvasGroup, 1f, 0f, duration));
            HideImmediate();
            onComplete?.Invoke();
        }

        public void ShowBranding(string title, string subtitle)
        {
            HideDialogue();
            HideSegmentHeader();
            if (brandingPanel != null) brandingPanel.SetActive(true);
            if (brandingTitleText != null) brandingTitleText.text = title;
            if (brandingSubtitleText != null) brandingSubtitleText.text = subtitle;
        }

        public void HideBranding()
        {
            if (brandingPanel != null) brandingPanel.SetActive(false);
        }

        public void ShowSegmentHeader(string title, string subtitle)
        {
            if (segmentHeaderPanel != null) segmentHeaderPanel.SetActive(true);
            if (segmentTitleText != null) segmentTitleText.text = title;
            if (segmentSubtitleText != null) segmentSubtitleText.text = subtitle;
        }

        public void HideSegmentHeader()
        {
            if (segmentHeaderPanel != null) segmentHeaderPanel.SetActive(false);
        }

        public void ShowDialogue(string speakerName, CommentatorType speakerType, string text, float duration)
        {
            HideBranding();
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName;
                speakerNameText.color = GetSpeakerColor(speakerName);
            }

            if (dialogueText != null)
            {
                if (typewriterCoroutine != null)
                    StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypeText(text));
            }
        }

        public void HideDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void CompleteTyping()
        {
            if (isTyping && dialogueText != null && fullText != null)
            {
                if (typewriterCoroutine != null)
                    StopCoroutine(typewriterCoroutine);
                dialogueText.text = fullText;
                isTyping = false;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (this == Instance) Instance = null;
        }

        #region Private

        Color GetSpeakerColor(string name)
        {
            if (name == null) return narratorColor;
            if (name.Contains("Arthur")) return arthurColor;
            if (name.Contains("Buck")) return buckColor;
            if (name.Contains("PA") || name.Contains("Announcer")) return paColor;
            if (name.Contains("Coach") || name.Contains("Sarah")) return coachColor;
            return narratorColor;
        }

        IEnumerator TypeText(string text)
        {
            isTyping = true;
            fullText = text;
            dialogueText.text = "";

            foreach (char c in text)
            {
                if (!isTyping) yield break;
                dialogueText.text += c;
                yield return new WaitForSeconds(1f / typewriterSpeed);
            }

            isTyping = false;
        }

        IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            group.alpha = to;
        }

        #endregion
    }
}
