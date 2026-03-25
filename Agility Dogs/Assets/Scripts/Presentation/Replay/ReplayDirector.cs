using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Presentation.Replay
{
    public class ReplayDirector : MonoBehaviour
    {
        public static ReplayDirector Instance { get; private set; }

        [Header("Replay Settings")]
        [SerializeField] private float slowMotionFactor = 0.25f;
        [SerializeField] private float highlightDuration = 3f;
        [SerializeField] private float replayLengthBeforeFault = 2f;
        [SerializeField] private float replayLengthAfterFault = 1f;

        [Header("Camera Angles")]
        [SerializeField] private CameraAngle[] replayCameraAngles;
        [SerializeField] private float cameraTransitionDuration = 0.5f;

        [Header("Highlights")]
        [SerializeField] private int maxHighlights = 5;
        [SerializeField] private float personalBestThreshold = 0.5f;

        private ReplayManager replayManager;
        private Camera mainCamera;
        private bool isReplaying;
        private int currentCameraIndex;
        private List<HighlightClip> highlights = new List<HighlightClip>();

        public bool IsReplaying => isReplaying;

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
            replayManager = FindObjectOfType<ReplayManager>();
            mainCamera = Camera.main;

            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
        }

        private void OnDestroy()
        {
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTime;
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            if (fault == FaultType.None || isReplaying) return;

            StartCoroutine(ReplayFaultMoment(fault, obstacleName));
        }

        private void HandleSplitTime(float splitTime)
        {
            if (!isReplaying && replayManager != null)
            {
                float personalBest = PlayerPrefs.GetFloat("PersonalBestSplit_" + GetCurrentObstacleIndex(), float.MaxValue);
                if (splitTime < personalBest - personalBestThreshold)
                {
                    StartCoroutine(ReplayPersonalBest(splitTime));
                }
            }
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            if (result == RunResult.Qualified && faults == 0)
            {
                GenerateHighlightReel();
            }
        }

        private int GetCurrentObstacleIndex()
        {
            return 0;
        }

        private IEnumerator ReplayFaultMoment(FaultType fault, string obstacleName)
        {
            if (replayManager == null) yield break;

            isReplaying = true;
            float originalTimeScale = Time.timeScale;
            Time.timeScale = slowMotionFactor;

            replayManager.BeginReplaySegment(replayLengthBeforeFault);

            yield return new WaitForSeconds(highlightDuration);

            Time.timeScale = originalTimeScale;
            replayManager.EndReplaySegment();
            isReplaying = false;
        }

        private IEnumerator ReplayPersonalBest(float splitTime)
        {
            if (replayManager == null) yield break;

            isReplaying = true;
            float originalTimeScale = Time.timeScale;
            Time.timeScale = slowMotionFactor;

            replayManager.BeginReplaySegment(1.5f);

            yield return new WaitForSeconds(2f);

            Time.timeScale = originalTimeScale;
            replayManager.EndReplaySegment();
            isReplaying = false;
        }

        public void EnterReplayMode()
        {
            if (replayManager == null) return;

            isReplaying = true;
            replayManager.EnterReviewMode();
            currentCameraIndex = 0;

            if (replayCameraAngles != null && replayCameraAngles.Length > 0)
            {
                TransitionToCamera(currentCameraIndex);
            }
        }

        public void ExitReplayMode()
        {
            isReplaying = false;
            if (replayManager != null)
            {
                replayManager.ExitReviewMode();
            }

            Time.timeScale = 1f;
        }

        public void NextCamera()
        {
            if (replayCameraAngles == null || replayCameraAngles.Length == 0) return;

            currentCameraIndex = (currentCameraIndex + 1) % replayCameraAngles.Length;
            TransitionToCamera(currentCameraIndex);
        }

        public void PreviousCamera()
        {
            if (replayCameraAngles == null || replayCameraAngles.Length == 0) return;

            currentCameraIndex--;
            if (currentCameraIndex < 0) currentCameraIndex = replayCameraAngles.Length - 1;
            TransitionToCamera(currentCameraIndex);
        }

        private void TransitionToCamera(int index)
        {
            if (replayCameraAngles == null || index < 0 || index >= replayCameraAngles.Length) return;

            var angle = replayCameraAngles[index];
            if (angle.targetTransform != null && mainCamera != null)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionCameraCoroutine(angle));
            }
        }

        private IEnumerator TransitionCameraCoroutine(CameraAngle angle)
        {
            float elapsed = 0f;
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            while (elapsed < cameraTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / cameraTransitionDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                mainCamera.transform.position = Vector3.Lerp(startPos, angle.targetTransform.position, smoothT);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, angle.targetTransform.rotation, smoothT);

                yield return null;
            }

            mainCamera.transform.position = angle.targetTransform.position;
            mainCamera.transform.rotation = angle.targetTransform.rotation;
        }

        public void ScrubToTime(float time)
        {
            if (replayManager != null)
            {
                replayManager.ScrubToTime(time);
            }
        }

        public void SetPlaybackSpeed(float speed)
        {
            if (replayManager != null)
            {
                replayManager.SetPlaybackSpeed(speed);
            }
        }

        private void GenerateHighlightReel()
        {
            highlights.Clear();

            if (replayManager == null) return;

            var segments = replayManager.GetRecordedSegments();
            foreach (var segment in segments)
            {
                if (segment.isFault || segment.isPersonalBest)
                {
                    highlights.Add(segment);
                    if (highlights.Count >= maxHighlights) break;
                }
            }
        }

        public HighlightClip[] GetHighlights()
        {
            return highlights.ToArray();
        }

        public void PlayHighlight(int index)
        {
            if (index < 0 || index >= highlights.Count) return;

            var highlight = highlights[index];
            ScrubToTime(highlight.timestamp);
            SetPlaybackSpeed(1f);
        }

        [System.Serializable]
        public class CameraAngle
        {
            public string name;
            public Transform targetTransform;
            public float fieldOfView = 60f;
        }

        [System.Serializable]
        public class HighlightClip
        {
            public float timestamp;
            public float duration;
            public bool isFault;
            public bool isPersonalBest;
            public string description;
        }
    }
}