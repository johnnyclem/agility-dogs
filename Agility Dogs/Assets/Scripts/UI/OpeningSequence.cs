using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using AgilityDogs.Core;

namespace AgilityDogs.UI
{
    /// <summary>
    /// Manages the opening cinematic sequence that plays when the game starts.
    /// Includes studio logo, game title reveal, and gameplay montage.
    /// </summary>
    public class OpeningSequence : MonoBehaviour
    {
        [Header("Sequence Elements")]
        [SerializeField] private CanvasGroup openingCanvasGroup;
        [SerializeField] private GameObject studioLogoPanel;
        [SerializeField] private Image studioLogoImage;
        [SerializeField] private GameObject gameTitlePanel;
        [SerializeField] private TextMeshProUGUI gameTitleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private GameObject gameplayMontagePanel;
        [SerializeField] private RawImage montageVideoImage;
        [SerializeField] private VideoPlayer montageVideoPlayer;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI skipButtonText;

        [Header("Timing")]
        [SerializeField] private float logoFadeInDuration = 1f;
        [SerializeField] private float logoHoldDuration = 2f;
        [SerializeField] private float logoFadeOutDuration = 0.5f;
        [SerializeField] private float titleFadeInDuration = 1.5f;
        [SerializeField] private float titleHoldDuration = 3f;
        [SerializeField] private float titleFadeOutDuration = 0.5f;
        [SerializeField] private float montageFadeInDuration = 0.5f;
        [SerializeField] private float montageFadeOutDuration = 1f;
        [SerializeField] private float skipButtonDelay = 2f;

        [Header("Visual Effects")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private ParticleSystem titleParticles;
        [SerializeField] private ParticleSystem logoParticles;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip studioLogoSound;
        [SerializeField] private AudioClip titleRevealSound;
        [SerializeField] private AudioClip montageMusic;
        [SerializeField] private AudioClip skipSound;

        [Header("Gameplay Montage")]
        [SerializeField] private Texture[] montageTextures;
        [SerializeField] private float textureDisplayDuration = 2f;
        [SerializeField] private RawImage[] montageImageSlots;

        [Header("Settings")]
        [SerializeField] private bool playOpeningSequence = true;
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private string skipPromptText = "Press ANY KEY or CLICK to skip";
        [SerializeField] private string skipCountdownText = "Starting in {0}...";

        // State
        private bool isPlaying = false;
        private bool hasBeenSkipped = false;
        private float skipTimer = 0f;
        private Coroutine currentSequence;

        // References
        private MenuManager menuManager;

        private void Start()
        {
            menuManager = FindObjectOfType<MenuManager>();

            // Setup skip button
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipSequence);
                skipButton.gameObject.SetActive(false);
            }

            // Setup audio source
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D
            }

            // Start opening sequence
            if (playOpeningSequence)
            {
                StartOpeningSequence();
            }
            else
            {
                SkipToMainMenu();
            }
        }

        private void Update()
        {
            if (!isPlaying) return;

            // Handle skip input
            if (allowSkip && !hasBeenSkipped)
            {
                HandleSkipInput();
            }

            // Update skip button text
            UpdateSkipButton();
        }

        #region Public Methods

        /// <summary>
        /// Start the opening sequence from the beginning.
        /// </summary>
        public void StartOpeningSequence()
        {
            if (isPlaying) return;

            isPlaying = true;
            hasBeenSkipped = false;

            // Hide all panels initially
            HideAllPanels();

            // Show opening canvas
            if (openingCanvasGroup != null)
            {
                openingCanvasGroup.alpha = 1f;
                openingCanvasGroup.gameObject.SetActive(true);
            }

            // Start the sequence
            currentSequence = StartCoroutine(PlayOpeningSequence());

            // Play opening music
            if (audioSource != null && montageMusic != null)
            {
                audioSource.clip = montageMusic;
                audioSource.loop = true;
                audioSource.volume = 0.3f;
                audioSource.Play();
            }

            Debug.Log("[OpeningSequence] Opening sequence started");
        }

        /// <summary>
        /// Skip the opening sequence and go to main menu.
        /// </summary>
        public void SkipSequence()
        {
            if (hasBeenSkipped) return;

            hasBeenSkipped = true;

            // Play skip sound
            if (audioSource != null && skipSound != null)
            {
                audioSource.PlayOneShot(skipSound);
            }

            // Stop current sequence
            if (currentSequence != null)
            {
                StopCoroutine(currentSequence);
            }

            // Fade out music
            if (audioSource != null)
            {
                StartCoroutine(FadeOutMusic(0.5f));
            }

            // Skip to main menu
            SkipToMainMenu();

            Debug.Log("[OpeningSequence] Opening sequence skipped");
        }

        /// <summary>
        /// Check if the opening sequence is currently playing.
        /// </summary>
        public bool IsPlaying => isPlaying;

        #endregion

        #region Sequence Coroutines

        private IEnumerator PlayOpeningSequence()
        {
            // Phase 1: Studio Logo
            yield return StartCoroutine(ShowStudioLogo());

            // Phase 2: Game Title
            yield return StartCoroutine(ShowGameTitle());

            // Phase 3: Gameplay Montage
            yield return StartCoroutine(ShowGameplayMontage());

            // Phase 4: Transition to Main Menu
            yield return StartCoroutine(TransitionToMainMenu());
        }

        private IEnumerator ShowStudioLogo()
        {
            if (studioLogoPanel == null) yield break;

            Debug.Log("[OpeningSequence] Showing studio logo");

            // Show logo panel
            studioLogoPanel.SetActive(true);

            // Fade in logo
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(studioLogoPanel), 0f, 1f, logoFadeInDuration));

            // Play logo sound
            if (audioSource != null && studioLogoSound != null)
            {
                audioSource.PlayOneShot(studioLogoSound);
            }

            // Play logo particles
            if (logoParticles != null)
            {
                logoParticles.Play();
            }

            // Show skip button after delay
            StartCoroutine(ShowSkipButtonAfterDelay(skipButtonDelay));

            // Hold logo
            yield return new WaitForSeconds(logoHoldDuration);

            // Fade out logo
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(studioLogoPanel), 1f, 0f, logoFadeOutDuration));

            // Hide logo panel
            studioLogoPanel.SetActive(false);
        }

        private IEnumerator ShowGameTitle()
        {
            if (gameTitlePanel == null) yield break;

            Debug.Log("[OpeningSequence] Showing game title");

            // Show title panel
            gameTitlePanel.SetActive(true);

            // Fade in title
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(gameTitlePanel), 0f, 1f, titleFadeInDuration));

            // Play title sound
            if (audioSource != null && titleRevealSound != null)
            {
                audioSource.PlayOneShot(titleRevealSound);
            }

            // Play title particles
            if (titleParticles != null)
            {
                titleParticles.Play();
            }

            // Hold title
            yield return new WaitForSeconds(titleHoldDuration);

            // Fade out title
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(gameTitlePanel), 1f, 0f, titleFadeOutDuration));

            // Hide title panel
            gameTitlePanel.SetActive(false);
        }

        private IEnumerator ShowGameplayMontage()
        {
            if (gameplayMontagePanel == null) yield break;

            Debug.Log("[OpeningSequence] Showing gameplay montage");

            // Show montage panel
            gameplayMontagePanel.SetActive(true);

            // Fade in montage
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(gameplayMontagePanel), 0f, 1f, montageFadeInDuration));

            // Play montage video or show textures
            if (montageVideoPlayer != null && montageVideoImage != null)
            {
                // Play video montage
                montageVideoPlayer.Play();
                
                // Wait for video to finish or skip
                while (montageVideoPlayer.isPlaying && !hasBeenSkipped)
                {
                    yield return null;
                }
            }
            else if (montageTextures != null && montageTextures.Length > 0)
            {
                // Show texture montage
                yield return StartCoroutine(PlayTextureMontage());
            }

            // Fade out montage
            yield return StartCoroutine(FadeCanvasGroup(GetCanvasGroup(gameplayMontagePanel), 1f, 0f, montageFadeOutDuration));

            // Hide montage panel
            gameplayMontagePanel.SetActive(false);
        }

        private IEnumerator PlayTextureMontage()
        {
            if (montageTextures.Length == 0) yield break;

            // Use montage image slots if available, otherwise use montageVideoImage
            RawImage targetImage = montageVideoImage;
            if (montageImageSlots != null && montageImageSlots.Length > 0)
            {
                targetImage = montageImageSlots[0];
            }

            if (targetImage == null) yield break;

            // Cycle through textures
            for (int i = 0; i < montageTextures.Length && !hasBeenSkipped; i++)
            {
                targetImage.texture = montageTextures[i];
                targetImage.color = Color.white;

                // Fade in texture
                yield return StartCoroutine(FadeImage(targetImage, 0f, 1f, 0.5f));

                // Hold texture
                yield return new WaitForSeconds(textureDisplayDuration);

                // Fade out texture
                yield return StartCoroutine(FadeImage(targetImage, 1f, 0f, 0.5f));
            }
        }

        private IEnumerator TransitionToMainMenu()
        {
            Debug.Log("[OpeningSequence] Transitioning to main menu");

            // Fade out opening canvas
            if (openingCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(openingCanvasGroup, 1f, 0f, 1f));
            }

            // Hide opening canvas
            if (openingCanvasGroup != null)
            {
                openingCanvasGroup.gameObject.SetActive(false);
            }

            // Complete sequence
            CompleteSequence();
        }

        #endregion

        #region Helper Methods

        private void HideAllPanels()
        {
            if (studioLogoPanel != null) studioLogoPanel.SetActive(false);
            if (gameTitlePanel != null) gameTitlePanel.SetActive(false);
            if (gameplayMontagePanel != null) gameplayMontagePanel.SetActive(false);
        }

        private void SkipToMainMenu()
        {
            // Hide all panels
            HideAllPanels();

            // Hide opening canvas
            if (openingCanvasGroup != null)
            {
                openingCanvasGroup.alpha = 0f;
                openingCanvasGroup.gameObject.SetActive(false);
            }

            // Complete sequence
            CompleteSequence();
        }

        private void CompleteSequence()
        {
            isPlaying = false;

            // Notify menu manager
            if (menuManager != null)
            {
                menuManager.OnOpeningSequenceComplete();
            }

            Debug.Log("[OpeningSequence] Opening sequence completed");
        }

        private void HandleSkipInput()
        {
            // Check for any key or mouse input
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                SkipSequence();
            }
        }

        private void UpdateSkipButton()
        {
            if (skipButton == null || !skipButton.gameObject.activeSelf) return;

            if (skipButtonText != null)
            {
                skipButtonText.text = skipPromptText;
            }
        }

        private IEnumerator ShowSkipButtonAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (skipButton != null && !hasBeenSkipped)
            {
                skipButton.gameObject.SetActive(true);
            }
        }

        private CanvasGroup GetCanvasGroup(GameObject obj)
        {
            if (obj == null) return null;

            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration && !hasBeenSkipped)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = fadeCurve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }

        private IEnumerator FadeImage(RawImage image, float startAlpha, float endAlpha, float duration)
        {
            if (image == null) yield break;

            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < duration && !hasBeenSkipped)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = fadeCurve.Evaluate(t);
                color.a = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                image.color = color;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            if (audioSource == null) yield break;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        #endregion
    }
}