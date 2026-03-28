using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Demo
{
    public class DemoModeController : MonoBehaviour
    {
        public static DemoModeController Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float idleThreshold = 5f;
        [SerializeField] private float segmentTransitionDelay = 1.5f;
        [SerializeField] private float brandingDisplayDuration = 4f;
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float linePaddingDuration = 0.3f;

        [Header("Loop")]
        [SerializeField] private bool loopDemo = true;
        [SerializeField] private float loopRestartDelay = 3f;

        [Header("References")]
        [SerializeField] private DemoUI demoUI;

        private float lastInputTime;
        private bool isDemoActive;
        private bool isDemoPlaying;
        private Coroutine demoCoroutine;
        private List<DemoSegment> segments;
        private int fadeToken;

        public bool IsDemoActive => isDemoActive;
        public bool IsDemoPlaying => isDemoPlaying;

        public event Action OnDemoStarted;
        public event Action OnDemoEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (demoUI == null)
                demoUI = GetComponent<DemoUI>() ?? FindObjectOfType<DemoUI>();

            segments = DemoDialogueContent.BuildSegments();
            lastInputTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (isDemoActive)
            {
                if (AnyInputDetected())
                {
                    StopDemo();
                    return;
                }
            }
            else if (!isDemoPlaying)
            {
                if (Time.unscaledTime - lastInputTime >= idleThreshold)
                {
                    StartDemo();
                }
            }
        }

        private void OnEnable()
        {
            lastInputTime = Time.unscaledTime;
        }

        #region Public API

        public void NotifyActivity()
        {
            lastInputTime = Time.unscaledTime;
            if (isDemoActive)
            {
                StopDemo();
            }
        }

        public void StartDemo()
        {
            if (isDemoPlaying) return;
            isDemoPlaying = true;
            isDemoActive = true;
            demoCoroutine = StartCoroutine(DemoSequence());
            OnDemoStarted?.Invoke();
        }

        public void StopDemo()
        {
            if (!isDemoPlaying) return;

            if (demoCoroutine != null)
            {
                StopCoroutine(demoCoroutine);
                demoCoroutine = null;
            }

            isDemoPlaying = false;
            isDemoActive = false;

            int token = ++fadeToken;

            if (demoUI != null)
            {
                demoUI.CompleteTyping();
                demoUI.FadeOut(fadeDuration, () =>
                {
                    if (fadeToken == token)
                    {
                        lastInputTime = Time.unscaledTime;
                    }
                });
            }
            else
            {
                lastInputTime = Time.unscaledTime;
            }

            OnDemoEnded?.Invoke();
        }

        #endregion

        #region Demo Sequence

        IEnumerator DemoSequence()
        {
            if (demoUI == null) yield break;

            yield return demoUI.FadeIn(fadeDuration);

            for (int pass = 0; loopDemo || pass == 0; pass++)
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    if (!isDemoActive) yield break;

                    var segment = segments[i];
                    yield return PlaySegment(segment);

                    if (!isDemoActive) yield break;

                    if (i < segments.Count - 1)
                    {
                        demoUI.HideDialogue();
                        demoUI.HideSegmentHeader();
                        yield return new WaitForSecondsRealtime(segmentTransitionDelay);
                    }
                }

                if (loopDemo && isDemoActive)
                {
                    demoUI.HideDialogue();
                    demoUI.HideSegmentHeader();
                    yield return new WaitForSecondsRealtime(loopRestartDelay);
                }
            }
        }

        IEnumerator PlaySegment(DemoSegment segment)
        {
            if (!isDemoActive) yield break;

            if (segment.showBranding && !string.IsNullOrEmpty(segment.brandingTitle))
            {
                demoUI.ShowBranding(segment.brandingTitle, segment.brandingSubtitle);
                yield return new WaitForSecondsRealtime(brandingDisplayDuration);
                demoUI.HideBranding();
                yield return new WaitForSecondsRealtime(0.5f);

                if (!isDemoActive) yield break;
            }

            if (!string.IsNullOrEmpty(segment.subtitle))
            {
                demoUI.ShowSegmentHeader(segment.segmentTitle, segment.subtitle);
            }

            for (int i = 0; i < segment.lines.Count; i++)
            {
                if (!isDemoActive) yield break;

                var line = segment.lines[i];

                if (line.delayBefore > 0f)
                    yield return new WaitForSecondsRealtime(line.delayBefore);

                if (!isDemoActive) yield break;

                demoUI.ShowDialogue(line.speakerName, line.speakerType, line.text, line.duration);

                float typewriterEstimate = line.text.Length / 40f;
                float displayWait = Mathf.Max(typewriterEstimate, line.duration) + linePaddingDuration;

                yield return new WaitForSecondsRealtime(displayWait);

                if (!isDemoActive) yield break;
            }
        }

        #endregion

        #region Input Detection

        bool AnyInputDetected()
        {
            if (Input.anyKeyDown) return true;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                return true;

            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                return true;

            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown($"joystick {i} button 0"))
                    return true;
            }

            return false;
        }

        #endregion

        private void OnDestroy()
        {
            if (this == Instance) Instance = null;
        }
    }
}
