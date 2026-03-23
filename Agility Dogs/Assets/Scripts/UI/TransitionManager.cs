using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AgilityDogs.UI
{
    /// <summary>
    /// Manages UI transitions and animations for menu screens.
    /// Provides fade, slide, scale, and other transition effects.
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }

        [Header("Default Settings")]
        [SerializeField] private float defaultTransitionDuration = 0.3f;
        [SerializeField] private AnimationCurve defaultCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip transitionSound;
        [SerializeField] private AudioClip menuClickSound;
        [SerializeField] private AudioClip menuHoverSound;
        [SerializeField] private float soundVolume = 0.5f;

        [Header("Screen Wipe")]
        [SerializeField] private Image screenWipeImage;
        [SerializeField] private Color screenWipeColor = Color.black;

        // Active transitions
        private int activeTransitions = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup audio source
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D
            }
        }

        #region Public API

        /// <summary>
        /// Fade a CanvasGroup in or out.
        /// </summary>
        public Coroutine Fade(CanvasGroup canvasGroup, float targetAlpha, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(FadeCoroutine(canvasGroup, canvasGroup.alpha, targetAlpha, duration, onComplete));
        }

        /// <summary>
        /// Fade a CanvasGroup from one alpha to another.
        /// </summary>
        public Coroutine Fade(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(FadeCoroutine(canvasGroup, startAlpha, endAlpha, duration, onComplete));
        }

        /// <summary>
        /// Fade an Image in or out.
        /// </summary>
        public Coroutine Fade(Image image, float targetAlpha, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(FadeImageCoroutine(image, image.color.a, targetAlpha, duration, onComplete));
        }

        /// <summary>
        /// Slide a RectTransform to a target position.
        /// </summary>
        public Coroutine Slide(RectTransform rectTransform, Vector2 targetPosition, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(SlideCoroutine(rectTransform, rectTransform.anchoredPosition, targetPosition, duration, onComplete));
        }

        /// <summary>
        /// Slide a RectTransform from one position to another.
        /// </summary>
        public Coroutine Slide(RectTransform rectTransform, Vector2 startPosition, Vector2 endPosition, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(SlideCoroutine(rectTransform, startPosition, endPosition, duration, onComplete));
        }

        /// <summary>
        /// Scale a RectTransform.
        /// </summary>
        public Coroutine Scale(RectTransform rectTransform, Vector3 targetScale, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(ScaleCoroutine(rectTransform, rectTransform.localScale, targetScale, duration, onComplete));
        }

        /// <summary>
        /// Scale a RectTransform from one scale to another.
        /// </summary>
        public Coroutine Scale(RectTransform rectTransform, Vector3 startScale, Vector3 endScale, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            return StartCoroutine(ScaleCoroutine(rectTransform, startScale, endScale, duration, onComplete));
        }

        /// <summary>
        /// Perform a screen wipe transition.
        /// </summary>
        public Coroutine ScreenWipe(Color targetColor, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = defaultTransitionDuration;
            if (screenWipeImage == null) return null;

            return StartCoroutine(ScreenWipeCoroutine(targetColor, duration, onComplete));
        }

        /// <summary>
        /// Play a transition sound effect.
        /// </summary>
        public void PlayTransitionSound()
        {
            if (audioSource != null && transitionSound != null)
            {
                audioSource.PlayOneShot(transitionSound, soundVolume);
            }
        }

        /// <summary>
        /// Play a menu click sound.
        /// </summary>
        public void PlayClickSound()
        {
            if (audioSource != null && menuClickSound != null)
            {
                audioSource.PlayOneShot(menuClickSound, soundVolume);
            }
        }

        /// <summary>
        /// Play a menu hover sound.
        /// </summary>
        public void PlayHoverSound()
        {
            if (audioSource != null && menuHoverSound != null)
            {
                audioSource.PlayOneShot(menuHoverSound, soundVolume * 0.5f);
            }
        }

        /// <summary>
        /// Check if any transitions are currently active.
        /// </summary>
        public bool IsTransitioning => activeTransitions > 0;

        /// <summary>
        /// Cancel all active transitions.
        /// </summary>
        public void CancelAllTransitions()
        {
            StopAllCoroutines();
            activeTransitions = 0;
        }

        #endregion

        #region Transition Coroutines

        private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, Action onComplete)
        {
            if (canvasGroup == null) yield break;

            activeTransitions++;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = defaultCurve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator FadeImageCoroutine(Image image, float startAlpha, float endAlpha, float duration, Action onComplete)
        {
            if (image == null) yield break;

            activeTransitions++;
            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = defaultCurve.Evaluate(t);
                color.a = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                image.color = color;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator SlideCoroutine(RectTransform rectTransform, Vector2 startPosition, Vector2 endPosition, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = defaultCurve.Evaluate(t);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, curveValue);
                yield return null;
            }

            rectTransform.anchoredPosition = endPosition;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator ScaleCoroutine(RectTransform rectTransform, Vector3 startScale, Vector3 endScale, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = defaultCurve.Evaluate(t);
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                yield return null;
            }

            rectTransform.localScale = endScale;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator ScreenWipeCoroutine(Color targetColor, float duration, Action onComplete)
        {
            if (screenWipeImage == null) yield break;

            activeTransitions++;
            screenWipeImage.gameObject.SetActive(true);
            screenWipeImage.color = screenWipeColor;

            // Fade in
            float halfDuration = duration / 2f;
            yield return StartCoroutine(FadeImageCoroutine(screenWipeImage, 0f, 1f, halfDuration, null));

            // Wait a frame
            yield return null;

            // Fade out
            yield return StartCoroutine(FadeImageCoroutine(screenWipeImage, 1f, 0f, halfDuration, null));

            screenWipeImage.gameObject.SetActive(false);
            activeTransitions--;
            onComplete?.Invoke();
        }

        #endregion

        #region Animation Presets

        /// <summary>
        /// Pop-in animation (scale from 0 to 1 with overshoot).
        /// </summary>
        public Coroutine PopIn(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            return StartCoroutine(PopInCoroutine(rectTransform, duration, onComplete));
        }

        /// <summary>
        /// Pop-out animation (scale from 1 to 0).
        /// </summary>
        public Coroutine PopOut(RectTransform rectTransform, float duration = 0.2f, Action onComplete = null)
        {
            return StartCoroutine(PopOutCoroutine(rectTransform, duration, onComplete));
        }

        /// <summary>
        /// Bounce animation.
        /// </summary>
        public Coroutine Bounce(RectTransform rectTransform, float amount = 0.1f, float duration = 0.4f, Action onComplete = null)
        {
            return StartCoroutine(BounceCoroutine(rectTransform, amount, duration, onComplete));
        }

        /// <summary>
        /// Shake animation.
        /// </summary>
        public Coroutine Shake(RectTransform rectTransform, float amount = 5f, float duration = 0.3f, Action onComplete = null)
        {
            return StartCoroutine(ShakeCoroutine(rectTransform, amount, duration, onComplete));
        }

        private IEnumerator PopInCoroutine(RectTransform rectTransform, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;
            float overshoot = 1.2f;

            rectTransform.localScale = startScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Custom easing with overshoot
                float scale;
                if (t < 0.7f)
                {
                    scale = Mathf.Lerp(0f, overshoot, t / 0.7f);
                }
                else
                {
                    scale = Mathf.Lerp(overshoot, 1f, (t - 0.7f) / 0.3f);
                }
                
                rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            rectTransform.localScale = endScale;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator PopOutCoroutine(RectTransform rectTransform, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            Vector3 startScale = rectTransform.localScale;
            Vector3 endScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = defaultCurve.Evaluate(t);
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                yield return null;
            }

            rectTransform.localScale = endScale;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator BounceCoroutine(RectTransform rectTransform, float amount, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            Vector3 originalScale = rectTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Sine wave bounce
                float bounce = Mathf.Sin(t * Mathf.PI * 2f) * amount * (1f - t);
                rectTransform.localScale = originalScale + Vector3.one * bounce;
                
                yield return null;
            }

            rectTransform.localScale = originalScale;
            activeTransitions--;
            onComplete?.Invoke();
        }

        private IEnumerator ShakeCoroutine(RectTransform rectTransform, float amount, float duration, Action onComplete)
        {
            if (rectTransform == null) yield break;

            activeTransitions++;
            Vector3 originalPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Random shake that decreases over time
                float shakeX = UnityEngine.Random.Range(-1f, 1f) * amount * (1f - t);
                float shakeY = UnityEngine.Random.Range(-1f, 1f) * amount * (1f - t);
                rectTransform.anchoredPosition = originalPosition + new Vector3(shakeX, shakeY, 0f);
                
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
            activeTransitions--;
            onComplete?.Invoke();
        }

        #endregion
    }
}