using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Best In Show Dialogue Manager
    /// Uses "bag of clips" pool pattern for Arthur and Buck's banter
    /// AAA anti-repetition: deplete pool, refill when empty, avoid consecutive repeats
    /// </summary>
    public class BestInShowDialogueManager : MonoBehaviour
    {
        [Header("Dialogue Data")]
        [SerializeField] private BestInShowDialogue dialogueData;
        
        [Header("Service References")]
        [SerializeField] private ElevenLabsService elevenLabsService;
        [SerializeField] private AudioManager audioManager;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource arthurSource;
        [SerializeField] private AudioSource buckSource;
        
        [Header("Playback Settings")]
        [SerializeField] private bool enableDialogue = true;
        [SerializeField] private float minDialogueInterval = 3f;
        [SerializeField] private float maxDialogueInterval = 12f;
        [SerializeField] private bool allowBuckInterruptions = false;
        
        [Header("Voice IDs (ElevenLabs)")]
        [SerializeField] private string arthurVoiceId = "ErXwobaYiN019PkySvjV"; // Example ID
        [SerializeField] private string buckVoiceId = "VR6AewLTigWG4xSOukaG"; // Example ID
        
        // State tracking
        private CommentaryState currentState = CommentaryState.General;
        private bool isPlayingDialogue = false;
        private float lastDialogueTime;
        private Coroutine currentDialogueCoroutine;
        
        // Queue for dialogue tasks
        private Queue<DialogueTask> dialogueQueue = new Queue<DialogueTask>();
        private bool isProcessingQueue = false;
        
        // Events
        public event Action<string, AnnouncerType, CommentaryState> OnDialoguePlayed;
        public event Action<CommentaryState> OnStateChanged;
        
        /// <summary>
        /// Internal dialogue task
        /// </summary>
        private class DialogueTask
        {
            public DialogueLineEntry line;
            public CommentaryState state;
            public float delay;
            public bool isPriority;
        }
        
        private void Awake()
        {
            // Initialize dialogue data
            if (dialogueData != null)
            {
                dialogueData.InitializeAllPools();
                Debug.Log($"[BestInShow] Initialized with {dialogueData.GetTotalLineCount()} total dialogue lines");
            }
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
            }
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
        }
        
        #region State Management
        
        /// <summary>
        /// Trigger dialogue for a specific state
        /// </summary>
        public void TriggerState(CommentaryState state, bool immediate = false)
        {
            if (!enableDialogue || dialogueData == null) return;
            
            CommentaryState previousState = currentState;
            currentState = state;
            
            OnStateChanged?.Invoke(state);
            
            Debug.Log($"[BestInShow] State changed: {previousState} -> {state}");
            
            // Get the pool for this state
            var statePool = dialogueData.GetPoolForState(state);
            if (statePool == null) return;
            
            // Queue Arthur's line
            QueueArthurLine(state, statePool, immediate);
            
            // Possibly queue Buck's line (with delay)
            if (UnityEngine.Random.value < statePool.buckChance)
            {
                QueueBuckLine(state, statePool, statePool.buckDelay);
            }
        }
        
        /// <summary>
        /// Queue Arthur's dialogue line
        /// </summary>
        private void QueueArthurLine(CommentaryState state, CommentaryStatePool pool, bool immediate)
        {
            var line = pool.arthurPool?.GetNextLine();
            if (line == null || string.IsNullOrEmpty(line.text)) return;
            
            dialogueQueue.Enqueue(new DialogueTask
            {
                line = line,
                state = state,
                delay = immediate ? 0f : pool.arthurDelay,
                isPriority = immediate
            });
            
            if (!isProcessingQueue)
            {
                ProcessQueue();
            }
        }
        
        /// <summary>
        /// Queue Buck's dialogue line
        /// </summary>
        private void QueueBuckLine(CommentaryState state, CommentaryStatePool pool, float delay)
        {
            var line = pool.buckPool?.GetNextLine();
            if (line == null || string.IsNullOrEmpty(line.text)) return;
            
            dialogueQueue.Enqueue(new DialogueTask
            {
                line = line,
                state = state,
                delay = delay,
                isPriority = false
            });
            
            if (!isProcessingQueue)
            {
                ProcessQueue();
            }
        }
        
        #endregion
        
        #region Queue Processing
        
        /// <summary>
        /// Process the dialogue queue
        /// </summary>
        private void ProcessQueue()
        {
            if (isProcessingQueue || dialogueQueue.Count == 0) return;
            
            isProcessingQueue = true;
            
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
            }
            
            currentDialogueCoroutine = StartCoroutine(ProcessQueueCoroutine());
        }
        
        /// <summary>
        /// Coroutine to process dialogue queue
        /// </summary>
        private IEnumerator ProcessQueueCoroutine()
        {
            while (dialogueQueue.Count > 0)
            {
                var task = dialogueQueue.Dequeue();
                
                // Apply delay
                if (task.delay > 0)
                {
                    yield return new WaitForSeconds(task.delay);
                }
                
                // Check minimum interval (unless priority)
                if (!task.isPriority)
                {
                    float timeSinceLast = Time.time - lastDialogueTime;
                    if (timeSinceLast < minDialogueInterval)
                    {
                        yield return new WaitForSeconds(minDialogueInterval - timeSinceLast);
                    }
                }
                
                // Play the dialogue line
                yield return StartCoroutine(PlayDialogueLine(task));
                
                // Small gap between lines
                yield return new WaitForSeconds(0.3f);
            }
            
            isProcessingQueue = false;
        }
        
        /// <summary>
        /// Play a single dialogue line
        /// </summary>
        private IEnumerator PlayDialogueLine(DialogueTask task)
        {
            if (task.line == null || string.IsNullOrEmpty(task.line.text))
                yield break;
            
            isPlayingDialogue = true;
            lastDialogueTime = Time.time;
            
            string text = task.line.text;
            AnnouncerType announcerType = task.line.announcerType;
            
            Debug.Log($"[BestInShow] {announcerType}: \"{text}\"");
            
            // Get audio source
            AudioSource source = GetAudioSource(announcerType);
            if (source == null)
            {
                Debug.LogWarning($"[BestInShow] No audio source for {announcerType}");
                yield break;
            }
            
            // Get voice ID
            string voiceId = !string.IsNullOrEmpty(task.line.voiceIdOverride) 
                ? task.line.voiceIdOverride 
                : GetDefaultVoiceId(announcerType);
            
            // Generate audio via ElevenLabs
            AudioClip audioClip = null;
            bool audioLoaded = false;
            
            // Use the appropriate TTS method based on announcer type
            switch (announcerType)
            {
                case AnnouncerType.Main: // Arthur
                    yield return elevenLabsService.SpeakAsMainAnnouncer(text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                case AnnouncerType.Color: // Buck
                    yield return elevenLabsService.SpeakAsColorCommentator(text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                default:
                    yield return elevenLabsService.SpeakAsMainAnnouncer(text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
            }
            
            // Wait for audio to load
            float timeout = 5f;
            float elapsed = 0f;
            while (!audioLoaded && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Play audio
            if (audioClip != null)
            {
                source.PlayOneShot(audioClip);
                
                // Wait for audio to finish
                yield return new WaitForSeconds(audioClip.length + 0.1f);
            }
            else
            {
                Debug.LogWarning($"[BestInShow] Failed to generate audio for: {text}");
            }
            
            isPlayingDialogue = false;
            
            // Fire event
            OnDialoguePlayed?.Invoke(text, announcerType, task.state);
        }
        
        /// <summary>
        /// Get audio source for announcer type
        /// </summary>
        private AudioSource GetAudioSource(AnnouncerType type)
        {
            switch (type)
            {
                case AnnouncerType.Main: return arthurSource;
                case AnnouncerType.Color: return buckSource;
                default: return arthurSource;
            }
        }
        
        /// <summary>
        /// Get default voice ID for announcer type
        /// </summary>
        private string GetDefaultVoiceId(AnnouncerType type)
        {
            switch (type)
            {
                case AnnouncerType.Main: return arthurVoiceId;
                case AnnouncerType.Color: return buckVoiceId;
                default: return arthurVoiceId;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Gameplay:
                    TriggerState(CommentaryState.MatchIntro);
                    break;
                case GameState.Countdown:
                    // Could add countdown-specific lines
                    break;
                case GameState.Results:
                    TriggerState(CommentaryState.FinishLine);
                    break;
            }
        }
        
        private void HandleRunStarted()
        {
            TriggerState(CommentaryState.MatchIntro, immediate: true);
        }
        
        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            TriggerState(CommentaryState.FinishLine, immediate: true);
        }
        
        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            // Map obstacle types to commentary states
            CommentaryState newState = type switch
            {
                ObstacleType.WeavePoles => CommentaryState.WeavePoles,
                ObstacleType.AFrame or ObstacleType.DogWalk => CommentaryState.ContactObstacles,
                ObstacleType.Tunnel => CommentaryState.Tunnel,
                ObstacleType.Teeter => CommentaryState.TeeterTotter,
                ObstacleType.BarJump or ObstacleType.TireJump or ObstacleType.BroadJump 
                    or ObstacleType.WallJump or ObstacleType.DoubleJump or ObstacleType.TripleJump
                    or ObstacleType.PanelJump or ObstacleType.LongJump or ObstacleType.SpreadJump 
                    => CommentaryState.Jumps,
                _ => CommentaryState.General
            };
            
            // If fault, trigger mistake state instead
            if (!clean)
            {
                newState = CommentaryState.Mistakes;
            }
            
            TriggerState(newState);
        }
        
        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            TriggerState(CommentaryState.Mistakes);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force play a specific line
        /// </summary>
        public void ForcePlayLine(AnnouncerType announcerType, string text)
        {
            var line = new DialogueLineEntry(text, announcerType);
            
            dialogueQueue.Enqueue(new DialogueTask
            {
                line = line,
                state = currentState,
                delay = 0f,
                isPriority = true
            });
            
            if (!isProcessingQueue)
            {
                ProcessQueue();
            }
        }
        
        /// <summary>
        /// Reset all pools (refill all bags)
        /// </summary>
        public void ResetAllPools()
        {
            dialogueData?.ResetAllPools();
            Debug.Log("[BestInShow] Reset all dialogue pools");
        }
        
        /// <summary>
        /// Enable or disable dialogue system
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableDialogue = enabled;
            
            if (!enabled)
            {
                dialogueQueue.Clear();
                isProcessingQueue = false;
                
                if (currentDialogueCoroutine != null)
                {
                    StopCoroutine(currentDialogueCoroutine);
                }
            }
        }
        
        /// <summary>
        /// Get current status
        /// </summary>
        public BestInShowStatus GetStatus()
        {
            return new BestInShowStatus
            {
                currentState = currentState,
                isPlayingDialogue = isPlayingDialogue,
                queueCount = dialogueQueue.Count,
                isProcessingQueue = isProcessingQueue
            };
        }
        
        #endregion
        
        #if UNITY_EDITOR
        [ContextMenu("Test Arthur Line")]
        private void TestArthurLine()
        {
            ForcePlayLine(AnnouncerType.Main, "This is a test line from Arthur Pendelton.");
        }
        
        [ContextMenu("Test Buck Line")]
        private void TestBuckLine()
        {
            ForcePlayLine(AnnouncerType.Color, "Arthur, how much do you think that dog owes in taxes?");
        }
        
        [ContextMenu("Test Match Intro")]
        private void TestMatchIntro()
        {
            TriggerState(CommentaryState.MatchIntro, immediate: true);
        }
        
        [ContextMenu("Test Weave Poles")]
        private void TestWeavePoles()
        {
            TriggerState(CommentaryState.WeavePoles, immediate: true);
        }
        
        [ContextMenu("Test Mistakes")]
        private void TestMistakes()
        {
            TriggerState(CommentaryState.Mistakes, immediate: true);
        }
        #endif
    }
    
    /// <summary>
    /// Status data structure
    /// </summary>
    [Serializable]
    public struct BestInShowStatus
    {
        public CommentaryState currentState;
        public bool isPlayingDialogue;
        public int queueCount;
        public bool isProcessingQueue;
    }
}