using System.Collections.Generic;
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
            
            if (isPlaying)
            {
                UpdatePlayback();
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
        }
        
        private void RecordObstacleCompleted(ObstacleType type, bool clean)
        {
            RecordEvent(ReplayEventType.ObstacleCompleted, $"{type.ToString()}|{clean}");
        }
        
        private void RecordSplitTime(float time)
        {
            RecordEvent(ReplayEventType.SplitTimeRecorded, time.ToString());
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