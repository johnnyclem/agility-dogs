using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Handler;

namespace AgilityDogs.Gameplay.Replay
{
    public class ReplayManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DogAgentController dog;
        [SerializeField] private HandlerController handler;
        [SerializeField] private Transform dogTransform;
        [SerializeField] private Transform handlerTransform;
        
        [Header("Recording")]
        [SerializeField] private bool recordOnStart = false;
        [SerializeField] private float recordingInterval = 0.05f; // 20 fps
        
        [Header("Playback")]
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private float playbackSpeed = 1f;
        [SerializeField] private bool loopPlayback = false;
        
        [Header("Slow Motion")]
        [SerializeField] private float faultSlowMoSpeed = 0.2f;
        [SerializeField] private float faultSlowMoDuration = 1.5f;
        [SerializeField] private float splitSlowMoSpeed = 0.3f;
        [SerializeField] private float splitSlowMoDuration = 0.8f;
        [SerializeField] private AnimationCurve slowMoBlendCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Highlight Selection")]
        [SerializeField] private float highlightLeadTime = 2f;
        [SerializeField] private float highlightTrailTime = 1f;
        [SerializeField] private int maxHighlights = 5;
        
        [Header("Post-Run Review")]
        [SerializeField] private bool enableReviewMode = true;
        [SerializeField] private float scrubSpeed = 0.5f;
        
        [Header("Output")]
        [SerializeField] private ReplayData currentReplayData;
        
        private bool isRecording;
        private bool isPlaying;
        private float recordingTimer;
        private float playbackTime;
        private int currentFrameIndex;
        private int currentEventIndex;
        private NavMeshAgent dogNavMeshAgent;
        
        private List<ReplayFrame> recordedFrames = new List<ReplayFrame>();
        private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
        
        // Slow motion state
        private bool isSlowMoActive;
        private float slowMoTimer;
        private float slowMoTargetSpeed;
        private float slowMoDuration;
        private float normalPlaybackSpeed;
        
        // Highlight state
        private List<ReplayHighlight> highlights = new List<ReplayHighlight>();
        private int currentHighlightIndex;
        
        // Review state
        private bool isReviewMode;
        private float reviewScrubSpeed;
        private bool isPaused;
        
        private void Start()
        {
            if (dog == null) dog = FindObjectOfType<DogAgentController>();
            if (handler == null) handler = FindObjectOfType<HandlerController>();
            
            if (dog != null)
            {
                dogTransform = dog.transform;
                dogNavMeshAgent = dog.GetComponent<NavMeshAgent>();
            }
            if (handler != null) handlerTransform = handler.transform;
            
            if (recordOnStart)
            {
                StartRecording();
            }
            
            if (playOnStart && currentReplayData != null)
            {
                StartPlayback(currentReplayData);
            }
        }
        
        private void Update()
        {
            if (isRecording)
            {
                UpdateRecording();
            }
            
            if (isPlaying && !isPaused)
            {
                UpdatePlayback();
                UpdateSlowMotion();
            }
            
            if (isReviewMode && isPlaying)
            {
                UpdateReviewMode();
            }
        }
        
        #region Recording
        
        public void StartRecording()
        {
            if (isPlaying)
            {
                StopPlayback();
            }
            
            recordedFrames.Clear();
            recordedEvents.Clear();
            isRecording = true;
            recordingTimer = 0f;
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Recording started");
        }
        
        public void StopRecording()
        {
            if (!isRecording) return;
            
            isRecording = false;
            UnsubscribeFromEvents();
            
            // Create ReplayData asset
            if (currentReplayData == null)
            {
                currentReplayData = ScriptableObject.CreateInstance<ReplayData>();
            }
            
            currentReplayData.Clear();
            currentReplayData.frames = new List<ReplayFrame>(recordedFrames);
            currentReplayData.events = new List<ReplayEvent>(recordedEvents);
            
            if (dog != null && dog.Breed != null)
            {
                currentReplayData.breedName = dog.Breed.displayName;
            }
            
            Debug.Log($"Recording stopped. Captured {recordedFrames.Count} frames and {recordedEvents.Count} events.");
        }
        
        public void SaveReplayData(string path)
        {
            if (currentReplayData == null) return;
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(currentReplayData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Replay data saved to {path}");
            #endif
        }
        
        private void UpdateRecording()
        {
            recordingTimer += Time.deltaTime;
            
            if (recordingTimer >= recordingInterval)
            {
                recordingTimer = 0f;
                RecordFrame();
            }
        }
        
        private void RecordFrame()
        {
            if (dogTransform == null || handlerTransform == null) return;
            
            ReplayFrame frame = new ReplayFrame
            {
                timestamp = Time.time,
                dogPosition = dogTransform.position,
                dogRotation = dogTransform.rotation,
                handlerPosition = handlerTransform.position,
                handlerRotation = handlerTransform.rotation,
                dogState = dog != null ? dog.CurrentState : DogState.Idle,
                isHandlerSprinting = handler != null && handler.IsSprinting
            };
            
            recordedFrames.Add(frame);
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnCommandIssued += RecordCommandIssued;
            GameEvents.OnFaultCommitted += RecordFaultCommitted;
            GameEvents.OnObstacleCompleted += RecordObstacleCompleted;
            GameEvents.OnSplitTimeRecorded += RecordSplitTime;
            GameEvents.OnRunStarted += RecordRunStarted;
            GameEvents.OnRunCompleted += RecordRunCompleted;
            GameEvents.OnCountdownTick += RecordCountdownTick;
            GameEvents.OnCourseLoaded += RecordCourseLoaded;
            GameEvents.OnGameStateChanged += RecordGameStateChanged;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCommandIssued -= RecordCommandIssued;
            GameEvents.OnFaultCommitted -= RecordFaultCommitted;
            GameEvents.OnObstacleCompleted -= RecordObstacleCompleted;
            GameEvents.OnSplitTimeRecorded -= RecordSplitTime;
            GameEvents.OnRunStarted -= RecordRunStarted;
            GameEvents.OnRunCompleted -= RecordRunCompleted;
            GameEvents.OnCountdownTick -= RecordCountdownTick;
            GameEvents.OnCourseLoaded -= RecordCourseLoaded;
            GameEvents.OnGameStateChanged -= RecordGameStateChanged;
        }
        
        private void RecordEvent(ReplayEventType type, string data = "")
        {
            ReplayEvent replayEvent = new ReplayEvent
            {
                timestamp = Time.time,
                eventType = type,
                data = data
            };
            recordedEvents.Add(replayEvent);
        }
        
        private void RecordCommandIssued(HandlerCommand command)
        {
            RecordEvent(ReplayEventType.CommandIssued, command.ToString());
        }
        
        private void RecordFaultCommitted(FaultType fault, string obstacleName)
        {
            RecordEvent(ReplayEventType.FaultCommitted, $"{fault.ToString()}|{obstacleName}");
            
            // Trigger slow motion on fault
            if (isPlaying)
            {
                TriggerSlowMotion(faultSlowMoSpeed, faultSlowMoDuration);
            }
        }
        
        private void RecordSplitTime(float time)
        {
            RecordEvent(ReplayEventType.SplitTimeRecorded, time.ToString());
            
            // Trigger brief slow motion on personal best splits
            if (isPlaying && IsPersonalBestSplit(time))
            {
                TriggerSlowMotion(splitSlowMoSpeed, splitSlowMoDuration);
            }
        }
        
        private bool IsPersonalBestSplit(float splitTime)
        {
            // Check if this is a personal best split
            // This is simplified - would compare against stored personal bests
            if (currentReplayData != null && currentReplayData.events.Count > 0)
            {
                var previousSplits = currentReplayData.events
                    .Where(e => e.eventType == ReplayEventType.SplitTimeRecorded)
                    .Select(e => float.Parse(e.data))
                    .ToList();
                    
                if (previousSplits.Count == 0 || splitTime < previousSplits.Min())
                {
                    return true;
                }
            }
            return false;
        }
        
        private void RecordObstacleCompleted(ObstacleType type, bool clean)
        {
            RecordEvent(ReplayEventType.ObstacleCompleted, $"{type.ToString()}|{clean}");
        }
        
        private void RecordRunStarted()
        {
            RecordEvent(ReplayEventType.RunStarted);
        }
        
        private void RecordRunCompleted(RunResult result, float time, int faults)
        {
            RecordEvent(ReplayEventType.RunCompleted, $"{result.ToString()}|{time}|{faults}");
            
            if (currentReplayData != null)
            {
                currentReplayData.runResult = result;
                currentReplayData.runTime = time;
                currentReplayData.faultCount = faults;
            }
        }
        
        private void RecordCountdownTick()
        {
            RecordEvent(ReplayEventType.CountdownTick);
        }
        
        private void RecordCourseLoaded()
        {
            RecordEvent(ReplayEventType.CourseLoaded);
        }
        
        private void RecordGameStateChanged(GameState from, GameState to)
        {
            RecordEvent(ReplayEventType.GameStateChanged, $"{from.ToString()}|{to.ToString()}");
        }
        
        #endregion
        
        #region Playback
        
        public void StartPlayback(ReplayData replayData)
        {
            if (isRecording)
            {
                StopRecording();
            }
            
            currentReplayData = replayData;
            if (currentReplayData == null || currentReplayData.frames.Count == 0)
            {
                Debug.LogError("No replay data to play back");
                return;
            }
            
            isPlaying = true;
            playbackTime = 0f;
            currentFrameIndex = 0;
            currentEventIndex = 0;
            
            // Disable agent movement during playback
            if (dogNavMeshAgent != null)
            {
                dogNavMeshAgent.enabled = false;
            }
            
            Debug.Log($"Playback started. Duration: {currentReplayData.GetDuration()}s");
        }
        
        public void StopPlayback()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            
            // Re-enable agent movement
            if (dogNavMeshAgent != null)
            {
                dogNavMeshAgent.enabled = true;
            }
            
            Debug.Log("Playback stopped");
        }
        
        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = Mathf.Max(0.1f, speed);
        }
        
        private void UpdatePlayback()
        {
            playbackTime += Time.deltaTime * playbackSpeed;
            
            // Process events that occur before current playback time
            while (currentEventIndex < currentReplayData.events.Count &&
                   currentReplayData.events[currentEventIndex].timestamp <= playbackTime)
            {
                ProcessEvent(currentReplayData.events[currentEventIndex]);
                currentEventIndex++;
            }
            
            // Find the two frames to interpolate between
            while (currentFrameIndex < currentReplayData.frames.Count - 1 &&
                   currentReplayData.frames[currentFrameIndex + 1].timestamp <= playbackTime)
            {
                currentFrameIndex++;
            }
            
            if (currentFrameIndex >= currentReplayData.frames.Count - 1)
            {
                // Reached end of replay
                if (loopPlayback)
                {
                    playbackTime = 0f;
                    currentFrameIndex = 0;
                    currentEventIndex = 0;
                }
                else
                {
                    StopPlayback();
                }
                return;
            }
            
            // Interpolate between frames
            ReplayFrame currentFrame = currentReplayData.frames[currentFrameIndex];
            ReplayFrame nextFrame = currentReplayData.frames[currentFrameIndex + 1];
            
            float t = (playbackTime - currentFrame.timestamp) / (nextFrame.timestamp - currentFrame.timestamp);
            t = Mathf.Clamp01(t);
            
            if (dogTransform != null)
            {
                dogTransform.position = Vector3.Lerp(currentFrame.dogPosition, nextFrame.dogPosition, t);
                dogTransform.rotation = Quaternion.Slerp(currentFrame.dogRotation, nextFrame.dogRotation, t);
            }
            
            if (handlerTransform != null)
            {
                handlerTransform.position = Vector3.Lerp(currentFrame.handlerPosition, nextFrame.handlerPosition, t);
                handlerTransform.rotation = Quaternion.Slerp(currentFrame.handlerRotation, nextFrame.handlerRotation, t);
            }
        }
        
        private void ProcessEvent(ReplayEvent replayEvent)
        {
            // For now, just log events. In the future, we could trigger visual effects or UI.
            Debug.Log($"[Replay] {replayEvent.timestamp:F2}s: {replayEvent.eventType} {replayEvent.data}");
            
            // Trigger highlights for important events
            switch (replayEvent.eventType)
            {
                case ReplayEventType.FaultCommitted:
                case ReplayEventType.ObstacleCompleted:
                case ReplayEventType.SplitTimeRecorded:
                    AddHighlight(replayEvent.timestamp, replayEvent.eventType);
                    break;
            }
        }
        
        #endregion
        
        #region Slow Motion
        
        public void TriggerSlowMotion(float speed, float duration)
        {
            if (isSlowMoActive) return;
            
            isSlowMoActive = true;
            slowMoTimer = 0f;
            slowMoTargetSpeed = speed;
            slowMoDuration = duration;
            normalPlaybackSpeed = playbackSpeed;
        }
        
        private void UpdateSlowMotion()
        {
            if (!isSlowMoActive) return;
            
            slowMoTimer += Time.deltaTime;
            
            if (slowMoTimer >= slowMoDuration)
            {
                // End slow motion
                isSlowMoActive = false;
                playbackSpeed = normalPlaybackSpeed;
                return;
            }
            
            // Calculate blend using curve
            float progress = slowMoTimer / slowMoDuration;
            float blend = slowMoBlendCurve.Evaluate(progress);
            
            // Blend from slow-mo speed back to normal speed
            playbackSpeed = Mathf.Lerp(slowMoTargetSpeed, normalPlaybackSpeed, blend);
        }
        
        #endregion
        
        #region Highlight Selection
        
        private void AddHighlight(float timestamp, ReplayEventType eventType)
        {
            // Check if we already have a highlight near this timestamp
            float minHighlightSpacing = 0.5f;
            foreach (var existing in highlights)
            {
                if (Mathf.Abs(existing.timestamp - timestamp) < minHighlightSpacing)
                {
                    return; // Too close to existing highlight
                }
            }
            
            ReplayHighlight highlight = new ReplayHighlight
            {
                timestamp = timestamp,
                eventType = eventType,
                startTime = Mathf.Max(0f, timestamp - highlightLeadTime),
                endTime = timestamp + highlightTrailTime
            };
            
            highlights.Add(highlight);
            
            // Limit number of highlights
            if (highlights.Count > maxHighlights)
            {
                // Remove oldest highlight
                highlights.RemoveAt(0);
            }
            
            Debug.Log($"Added highlight at {timestamp:F2}s for {eventType}");
        }
        
        public List<ReplayHighlight> GetHighlights()
        {
            return highlights;
        }
        
        public void PlayHighlight(int index)
        {
            if (index < 0 || index >= highlights.Count) return;
            
            ReplayHighlight highlight = highlights[index];
            currentHighlightIndex = index;
            
            // Seek to highlight start time
            SeekToTime(highlight.startTime);
            
            Debug.Log($"Playing highlight {index + 1}/{highlights.Count} starting at {highlight.startTime:F2}s");
        }
        
        public void PlayNextHighlight()
        {
            if (highlights.Count == 0) return;
            
            currentHighlightIndex = (currentHighlightIndex + 1) % highlights.Count;
            PlayHighlight(currentHighlightIndex);
        }
        
        public void PlayPreviousHighlight()
        {
            if (highlights.Count == 0) return;
            
            currentHighlightIndex = (currentHighlightIndex - 1 + highlights.Count) % highlights.Count;
            PlayHighlight(currentHighlightIndex);
        }
        
        public void GenerateHighlights()
        {
            if (currentReplayData == null) return;
            
            highlights.Clear();
            
            // Generate highlights from recorded events
            foreach (var replayEvent in currentReplayData.events)
            {
                switch (replayEvent.eventType)
                {
                    case ReplayEventType.FaultCommitted:
                    case ReplayEventType.ObstacleCompleted:
                    case ReplayEventType.SplitTimeRecorded:
                        AddHighlight(replayEvent.timestamp, replayEvent.eventType);
                        break;
                }
            }
            
            Debug.Log($"Generated {highlights.Count} highlights");
        }
        
        #endregion
        
        #region Post-Run Review
        
        public void EnterReviewMode()
        {
            if (currentReplayData == null || currentReplayData.frames.Count == 0)
            {
                Debug.LogError("No replay data available for review");
                return;
            }
            
            isReviewMode = true;
            isPaused = true;
            reviewScrubSpeed = scrubSpeed;
            
            // Generate highlights
            GenerateHighlights();
            
            Debug.Log("Entered review mode. Use scrub controls to navigate.");
        }
        
        public void ExitReviewMode()
        {
            isReviewMode = false;
            isPaused = false;
        }
        
        private void UpdateReviewMode()
        {
            if (!isReviewMode || isPaused) return;
            
            // Auto-play highlights if configured
            if (currentHighlightIndex < highlights.Count)
            {
                ReplayHighlight highlight = highlights[currentHighlightIndex];
                
                // Check if we've passed the end of the current highlight
                if (playbackTime >= highlight.endTime)
                {
                    PlayNextHighlight();
                }
            }
        }
        
        public void TogglePause()
        {
            isPaused = !isPaused;
            Debug.Log(isPaused ? "Paused" : "Resumed");
        }
        
        public void ScrubForward()
        {
            if (!isReviewMode) return;
            
            float newTime = playbackTime + reviewScrubSpeed;
            SeekToTime(newTime);
        }
        
        public void ScrubBackward()
        {
            if (!isReviewMode) return;
            
            float newTime = playbackTime - reviewScrubSpeed;
            SeekToTime(Mathf.Max(0f, newTime));
        }
        
        public void SeekToTime(float time)
        {
            if (currentReplayData == null) return;
            
            playbackTime = Mathf.Clamp(time, 0f, currentReplayData.GetDuration());
            
            // Reset frame and event indices
            currentFrameIndex = 0;
            currentEventIndex = 0;
            
            // Find correct frame index
            while (currentFrameIndex < currentReplayData.frames.Count - 1 &&
                   currentReplayData.frames[currentFrameIndex + 1].timestamp <= playbackTime)
            {
                currentFrameIndex++;
            }
            
            // Find correct event index
            while (currentEventIndex < currentReplayData.events.Count &&
                   currentReplayData.events[currentEventIndex].timestamp <= playbackTime)
            {
                currentEventIndex++;
            }
            
            // Update positions immediately
            UpdatePlaybackPosition();
        }
        
        private void UpdatePlaybackPosition()
        {
            if (currentReplayData == null || currentFrameIndex >= currentReplayData.frames.Count - 1) return;
            
            ReplayFrame currentFrame = currentReplayData.frames[currentFrameIndex];
            ReplayFrame nextFrame = currentReplayData.frames[currentFrameIndex + 1];
            
            float t = (playbackTime - currentFrame.timestamp) / (nextFrame.timestamp - currentFrame.timestamp);
            t = Mathf.Clamp01(t);
            
            if (dogTransform != null)
            {
                dogTransform.position = Vector3.Lerp(currentFrame.dogPosition, nextFrame.dogPosition, t);
                dogTransform.rotation = Quaternion.Slerp(currentFrame.dogRotation, nextFrame.dogRotation, t);
            }
            
            if (handlerTransform != null)
            {
                handlerTransform.position = Vector3.Lerp(currentFrame.handlerPosition, nextFrame.handlerPosition, t);
                handlerTransform.rotation = Quaternion.Slerp(currentFrame.handlerRotation, nextFrame.handlerRotation, t);
            }
        }
        
        public float GetPlaybackProgress()
        {
            if (currentReplayData == null || currentReplayData.GetDuration() <= 0f) return 0f;
            return playbackTime / currentReplayData.GetDuration();
        }
        
        public float GetPlaybackTime()
        {
            return playbackTime;
        }
        
        public float GetDuration()
        {
            return currentReplayData != null ? currentReplayData.GetDuration() : 0f;
        }
        
        public ReplayData CurrentReplayData => currentReplayData;
        
        public bool IsInReviewMode()
        {
            return isReviewMode;
        }
        
        public bool IsPaused()
        {
            return isPaused;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (isRecording)
            {
                StopRecording();
            }
            
            if (isPlaying)
            {
                StopPlayback();
            }
        }
    }
}