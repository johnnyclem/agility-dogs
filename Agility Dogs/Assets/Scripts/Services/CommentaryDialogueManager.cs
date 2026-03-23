using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Obstacles;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Commentary Dialogue Manager - Manages dynamic banter between Arthur and Buck
    /// Implements state-based triggering with anti-repetition logic
    /// </summary>
    public class CommentaryDialogueManager : MonoBehaviour
    {
        [Header("Dialogue Data")]
        [SerializeField] private CommentaryDialogueData dialogueData;
        
        [Header("Service References")]
        [SerializeField] private ElevenLabsService elevenLabsService;
        [SerializeField] private AudioManager audioManager;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource arthurSource;
        [SerializeField] private AudioSource buckSource;
        
        [Header("Dialogue Settings")]
        [SerializeField] private bool enableDialogue = true;
        [SerializeField] private float dialogueInterval = 5f;
        [SerializeField] private bool allowInterruptions = false;
        [SerializeField] private float interruptionCooldown = 10f;
        
        [Header("Banter Settings")]
        [SerializeField] private float banterChance = 0.3f;
        [SerializeField] private bool enableBanter = true;
        
        // State tracking
        private CommentaryState currentState = CommentaryState.MatchIntro;
        private CommentaryState previousState = CommentaryState.General;
        private bool isPlayingDialogue = false;
        private float lastDialogueTime;
        private float lastInterruptionTime;
        private Coroutine dialogueCoroutine;
        
        // Anti-repetition tracking
        private Dictionary<AnnouncerType, Queue<string>> recentLines = new Dictionary<AnnouncerType, Queue<string>>();
        private Dictionary<string, float> lineCooldowns = new Dictionary<string, float>();
        private const int RecentLinesMax = 8;
        
        // Dialogue queue
        private Queue<DialogueTask> dialogueQueue = new Queue<DialogueTask>();
        private bool isProcessingQueue = false;
        
        // Events
        public event Action<string, AnnouncerType> OnDialogueStarted;
        public event Action<string, AnnouncerType> OnDialogueCompleted;
        public event Action<CommentaryState> OnStateChanged;
        
        // Properties
        public CommentaryState CurrentState => currentState;
        public bool IsPlayingDialogue => isPlayingDialogue;
        
        /// <summary>
        /// Dialogue task for queuing
        /// </summary>
        private class DialogueTask
        {
            public AnnouncerType announcerType;
            public DialogueLine line;
            public CommentaryState state;
            public float timestamp;
            public bool isBanter;
        }
        
        private void Awake()
        {
            // Initialize recent lines tracking
            recentLines[AnnouncerType.Main] = new Queue<string>();
            recentLines[AnnouncerType.Color] = new Queue<string>();
            recentLines[AnnouncerType.PA] = new Queue<string>();
            
            // Initialize dialogue data
            if (dialogueData != null)
            {
                dialogueData.Initialize();
            }
        }
        
        private void Start()
        {
            SubscribeToEvents();
            
            // Start with match intro
            if (enableDialogue)
            {
                StartCoroutine(InitialDialogueDelay());
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
        }
        
        private void SubscribeToEvents()
        {
            // Game state events
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            
            // Obstacle events
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            
            // Custom commentary triggers
            CommentaryManager.OnCommentaryTrigger += HandleCommentaryTrigger;
        }
        
        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            
            CommentaryManager.OnCommentaryTrigger -= HandleCommentaryTrigger;
        }
        
        /// <summary>
        /// Initial delay before first dialogue
        /// </summary>
        private IEnumerator InitialDialogueDelay()
        {
            yield return new WaitForSeconds(2f);
            
            // Play match intro
            TriggerState(CommentaryState.MatchIntro);
        }
        
        #region State Management
        
        /// <summary>
        /// Trigger a commentary state
        /// </summary>
        public void TriggerState(CommentaryState state, bool forceNow = false)
        {
            if (!enableDialogue) return;
            if (state == currentState && !forceNow) return;
            
            previousState = currentState;
            currentState = state;
            
            OnStateChanged?.Invoke(state);
            
            // Queue dialogue for this state
            QueueStateDialogue(state);
        }
        
        /// <summary>
        /// Queue dialogue lines for a specific state
        /// </summary>
        private void QueueStateDialogue(CommentaryState state)
        {
            if (dialogueData == null) return;
            
            // Get lines for Arthur (Main Announcer)
            var arthurLines = GetLinesForState(AnnouncerType.Main, state);
            if (arthurLines != null && arthurLines.Length > 0)
            {
                var selectedLine = SelectWeightedRandomLine(arthurLines, AnnouncerType.Main);
                if (selectedLine != null)
                {
                    QueueDialogue(AnnouncerType.Main, selectedLine, state);
                }
            }
            
            // Possibly add Buck's commentary (Color Commentator)
            if (enableBanter && UnityEngine.Random.value < banterChance)
            {
                var buckLines = GetLinesForState(AnnouncerType.Color, state);
                if (buckLines != null && buckLines.Length > 0)
                {
                    var selectedLine = SelectWeightedRandomLine(buckLines, AnnouncerType.Color);
                    if (selectedLine != null)
                    {
                        QueueDialogue(AnnouncerType.Color, selectedLine, state, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get lines for an announcer and state
        /// </summary>
        private DialogueLine[] GetLinesForState(AnnouncerType announcerType, CommentaryState state)
        {
            if (dialogueData == null) return null;
            
            var announcerData = dialogueData.GetAnnouncerData(announcerType);
            if (announcerData == null) return null;
            
            var collection = announcerData.GetLinesForState(state);
            return collection?.lines;
        }
        
        #endregion
        
        #region Dialogue Selection
        
        /// <summary>
        /// Select a weighted random line, avoiding repetition
        /// </summary>
        private DialogueLine SelectWeightedRandomLine(DialogueLine[] lines, AnnouncerType announcerType)
        {
            if (lines == null || lines.Length == 0) return null;
            
            // Filter out lines on cooldown
            var availableLines = lines.Where(line => !IsLineOnCooldown(line.text)).ToList();
            
            if (availableLines.Count == 0)
            {
                // All lines on cooldown, return null to wait
                return null;
            }
            
            // Weighted random selection
            float totalWeight = availableLines.Sum(l => l.weight);
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var line in availableLines)
            {
                currentWeight += line.weight;
                if (randomValue <= currentWeight)
                {
                    return line;
                }
            }
            
            return availableLines.Last();
        }
        
        /// <summary>
        /// Check if a line is on cooldown
        /// </summary>
        private bool IsLineOnCooldown(string lineText)
        {
            if (lineCooldowns.TryGetValue(lineText, out float cooldownEnd))
            {
                return Time.time < cooldownEnd;
            }
            return false;
        }
        
        /// <summary>
        /// Set a line on cooldown
        /// </summary>
        private void SetLineCooldown(string lineText, float cooldown)
        {
            lineCooldowns[lineText] = Time.time + cooldown;
        }
        
        /// <summary>
        /// Add line to recent history
        /// </summary>
        private void AddToRecentLines(AnnouncerType announcerType, string lineText)
        {
            if (!recentLines.ContainsKey(announcerType))
            {
                recentLines[announcerType] = new Queue<string>();
            }
            
            var queue = recentLines[announcerType];
            queue.Enqueue(lineText);
            
            // Remove oldest if over limit
            while (queue.Count > RecentLinesMax)
            {
                queue.Dequeue();
            }
        }
        
        /// <summary>
        /// Check if line was recently used
        /// </summary>
        private bool WasRecentlyUsed(AnnouncerType announcerType, string lineText)
        {
            if (!recentLines.ContainsKey(announcerType))
                return false;
                
            return recentLines[announcerType].Contains(lineText);
        }
        
        #endregion
        
        #region Dialogue Queue & Playback
        
        /// <summary>
        /// Queue a dialogue line for playback
        /// </summary>
        private void QueueDialogue(AnnouncerType announcerType, DialogueLine line, CommentaryState state, bool isBanter = false)
        {
            if (line == null) return;
            
            var task = new DialogueTask
            {
                announcerType = announcerType,
                line = line,
                state = state,
                timestamp = Time.time,
                isBanter = isBanter
            };
            
            dialogueQueue.Enqueue(task);
            
            if (!isProcessingQueue)
            {
                ProcessDialogueQueue();
            }
        }
        
        /// <summary>
        /// Process the dialogue queue
        /// </summary>
        private void ProcessDialogueQueue()
        {
            if (isProcessingQueue || dialogueQueue.Count == 0)
                return;
            
            isProcessingQueue = true;
            
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
            
            dialogueCoroutine = StartCoroutine(ProcessDialogueCoroutine());
        }
        
        /// <summary>
        /// Coroutine to process dialogue
        /// </summary>
        private IEnumerator ProcessDialogueCoroutine()
        {
            while (dialogueQueue.Count > 0)
            {
                var task = dialogueQueue.Dequeue();
                
                // Check if we should wait (dialogue interval)
                float timeSinceLast = Time.time - lastDialogueTime;
                if (timeSinceLast < dialogueInterval && !task.isBanter)
                {
                    yield return new WaitForSeconds(dialogueInterval - timeSinceLast);
                }
                
                // Play the dialogue
                yield return StartCoroutine(PlayDialogueLine(task));
                
                // Wait between lines
                yield return new WaitForSeconds(0.5f);
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
            
            // Get announcer data
            var announcerData = dialogueData?.GetAnnouncerData(task.announcerType);
            string voiceId = announcerData?.elevenLabsVoiceId ?? "default";
            
            // Fire event
            OnDialogueStarted?.Invoke(task.line.text, task.announcerType);
            
            // Get appropriate audio source
            AudioSource source = GetAudioSource(task.announcerType);
            
            // Track the line
            AddToRecentLines(task.announcerType, task.line.text);
            SetLineCooldown(task.line.text, task.line.cooldown);
            
            // Generate and play audio via ElevenLabs
            AudioClip audioClip = null;
            bool audioLoaded = false;
            
            switch (task.announcerType)
            {
                case AnnouncerType.Main: // Arthur
                    yield return elevenLabsService.SpeakAsMainAnnouncer(task.line.text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                case AnnouncerType.Color: // Buck
                    yield return elevenLabsService.SpeakAsColorCommentator(task.line.text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                default:
                    yield return elevenLabsService.SpeakAsMainAnnouncer(task.line.text, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
            }
            
            // Wait for audio to load
            while (!audioLoaded) yield return null;
            
            // Play the audio
            if (audioClip != null && source != null)
            {
                elevenLabsService.PlayAudioClip(audioClip, source);
                
                // Wait for audio to finish
                yield return new WaitForSeconds(audioClip.length + 0.2f);
            }
            
            isPlayingDialogue = false;
            OnDialogueCompleted?.Invoke(task.line.text, task.announcerType);
        }
        
        /// <summary>
        /// Get audio source for announcer type
        /// </summary>
        private AudioSource GetAudioSource(AnnouncerType announcerType)
        {
            switch (announcerType)
            {
                case AnnouncerType.Main: return arthurSource;
                case AnnouncerType.Color: return buckSource;
                default: return arthurSource;
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
                    // Countdown state - maybe add countdown-specific lines
                    break;
                case GameState.Results:
                    TriggerState(CommentaryState.FinishLine);
                    break;
            }
        }
        
        private void HandleRunStarted()
        {
            TriggerState(CommentaryState.MatchIntro, true);
        }
        
        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            TriggerState(CommentaryState.FinishLine);
        }
        
        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            // Update state based on obstacle type
            switch (type)
            {
                case ObstacleType.WeavePoles:
                    TriggerState(CommentaryState.WeavePoles);
                    break;
                case ObstacleType.AFrame:
                case ObstacleType.DogWalk:
                    TriggerState(CommentaryState.ContactObstacles);
                    break;
                case ObstacleType.Tunnel:
                    TriggerState(CommentaryState.Tunnel);
                    break;
                case ObstacleType.Teeter:
                    TriggerState(CommentaryState.TeeterTotter);
                    break;
                case ObstacleType.BarJump:
                case ObstacleType.TireJump:
                case ObstacleType.BroadJump:
                case ObstacleType.WallJump:
                case ObstacleType.DoubleJump:
                case ObstacleType.TripleJump:
                case ObstacleType.PanelJump:
                case ObstacleType.LongJump:
                case ObstacleType.SpreadJump:
                    TriggerState(CommentaryState.Jumps);
                    break;
                default:
                    TriggerState(CommentaryState.General);
                    break;
            }
            
            // If fault, trigger mistake state
            if (!clean)
            {
                TriggerState(CommentaryState.Mistakes);
            }
        }
        
        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            TriggerState(CommentaryState.Mistakes);
        }
        
        private void HandleCommentaryTrigger(string category, string text)
        {
            // Handle custom commentary triggers from other systems
            // Could map categories to states here
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force trigger a specific dialogue line
        /// </summary>
        public void ForceDialogue(AnnouncerType announcerType, string text)
        {
            var line = new DialogueLine
            {
                text = text,
                weight = 10f,
                cooldown = 0f
            };
            
            QueueDialogue(announcerType, line, currentState);
        }
        
        /// <summary>
        /// Trigger banter between announcers
        /// </summary>
        public void TriggerBanter()
        {
            if (!enableBanter) return;
            
            // Get random state for banter
            var states = new[]
            {
                CommentaryState.MatchIntro,
                CommentaryState.WeavePoles,
                CommentaryState.ContactObstacles,
                CommentaryState.Tunnel,
                CommentaryState.TeeterTotter,
                CommentaryState.Jumps,
                CommentaryState.Mistakes,
                CommentaryState.FinishLine
            };
            
            var randomState = states[UnityEngine.Random.Range(0, states.Length)];
            TriggerState(randomState);
        }
        
        /// <summary>
        /// Clear all cooldowns and history
        /// </summary>
        public void ResetDialogueHistory()
        {
            lineCooldowns.Clear();
            recentLines[AnnouncerType.Main].Clear();
            recentLines[AnnouncerType.Color].Clear();
            recentLines[AnnouncerType.PA].Clear();
            dialogueQueue.Clear();
        }
        
        /// <summary>
        /// Get current dialogue status
        /// </summary>
        public DialogueStatus GetStatus()
        {
            return new DialogueStatus
            {
                currentState = currentState,
                isPlayingDialogue = isPlayingDialogue,
                queueCount = dialogueQueue.Count,
                arthurRecentCount = recentLines[AnnouncerType.Main]?.Count ?? 0,
                buckRecentCount = recentLines[AnnouncerType.Color]?.Count ?? 0,
                totalCooldowns = lineCooldowns.Count
            };
        }
        
        /// <summary>
        /// Enable or disable dialogue
        /// </summary>
        public void SetDialogueEnabled(bool enabled)
        {
            enableDialogue = enabled;
            
            if (!enabled)
            {
                dialogueQueue.Clear();
                isPlayingDialogue = false;
                
                if (dialogueCoroutine != null)
                {
                    StopCoroutine(dialogueCoroutine);
                }
            }
        }
        
        #endregion
        
        #if UNITY_EDITOR
        [ContextMenu("Test Arthur Line")]
        private void TestArthurLine()
        {
            ForceDialogue(AnnouncerType.Main, "This is a test line from Arthur.");
        }
        
        [ContextMenu("Test Buck Line")]
        private void TestBuckLine()
        {
            ForceDialogue(AnnouncerType.Color, "This is a test line from Buck. I'm ignorant and confident!");
        }
        
        [ContextMenu("Test Banter")]
        private void TestBanter()
        {
            TriggerBanter();
        }
        #endif
    }
    
    /// <summary>
    /// Dialogue status data structure
    /// </summary>
    [Serializable]
    public struct DialogueStatus
    {
        public CommentaryState currentState;
        public bool isPlayingDialogue;
        public int queueCount;
        public int arthurRecentCount;
        public int buckRecentCount;
        public int totalCooldowns;
    }
}