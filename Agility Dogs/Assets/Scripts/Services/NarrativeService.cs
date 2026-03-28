using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// NarrativeService - Central hub for all dialogue, narration, and storytelling
    /// Integrates story dialogue, competition commentary, tutorials, and dynamic narration
    /// </summary>
    public class NarrativeService : MonoBehaviour
    {
        public static NarrativeService Instance { get; private set; }

        [Header("Narrative Components")]
        [SerializeField] private StoryNarrator storyNarrator;
        [SerializeField] private CompetitionCommentator competitionCommentator;
        [SerializeField] private TutorialNarrator tutorialNarrator;
        [SerializeField] private DynamicNarrator dynamicNarrator;

        [Header("Display Settings")]
        [SerializeField] private float defaultDialogueSpeed = 30f;
        [SerializeField] private bool skipEnabled = true;
        [SerializeField] private bool autoAdvance = true;
        [SerializeField] private float autoAdvanceDelay = 2f;

        // State
        private NarrativeMode currentMode = NarrativeMode.Idle;
        private Queue<NarrativeEvent> eventQueue = new Queue<NarrativeEvent>();
        private bool isProcessingEvent = false;
        private NarrativeEvent currentEvent;
        private NarrativeContext currentContext;

        // Events
        public event Action<NarrativeEvent> OnEventStarted;
        public event Action<NarrativeEvent> OnEventCompleted;
        public event Action<DialogueData> OnDialogueStarted;
        public event Action<DialogueData> OnDialogueCompleted;
        public event Action OnNarrativePaused;
        public event Action OnNarrativeResumed;

        // Properties
        public NarrativeMode CurrentMode => currentMode;
        public bool IsNarrating => isProcessingEvent;
        public NarrativeContext Context => currentContext;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeComponents();
        }

        private void Start()
        {
            InitializeContext();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization

        private void InitializeComponents()
        {
            if (storyNarrator == null) storyNarrator = gameObject.AddComponent<StoryNarrator>();
            if (competitionCommentator == null) competitionCommentator = gameObject.AddComponent<CompetitionCommentator>();
            if (tutorialNarrator == null) tutorialNarrator = gameObject.AddComponent<TutorialNarrator>();
            if (dynamicNarrator == null) dynamicNarrator = gameObject.AddComponent<DynamicNarrator>();
        }

        private void InitializeContext()
        {
            currentContext = new NarrativeContext
            {
                currentChapter = 1,
                currentCharacter = "",
                currentScene = "",
                isStoryActive = false,
                tutorialCompleted = new HashSet<string>(),
                dialogueHistory = new List<DialogueRecord>()
            };
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;

            var campaignService = CampaignService.Instance;
            if (campaignService != null)
            {
                campaignService.OnChapterUnlocked += HandleChapterUnlocked;
                campaignService.OnCutsceneStarted += HandleCutsceneStarted;
                campaignService.OnCutsceneEnded += HandleCutsceneEnded;
            }

            var showManager = ShowManager.Instance;
            if (showManager != null)
            {
                showManager.OnShowStarted += HandleShowStarted;
                showManager.OnShowCompleted += HandleShowCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;

            var campaignService = CampaignService.Instance;
            if (campaignService != null)
            {
                campaignService.OnChapterUnlocked -= HandleChapterUnlocked;
                campaignService.OnCutsceneStarted -= HandleCutsceneStarted;
                campaignService.OnCutsceneEnded -= HandleCutsceneEnded;
            }

            var showManager = ShowManager.Instance;
            if (showManager != null)
            {
                showManager.OnShowStarted -= HandleShowStarted;
                showManager.OnShowCompleted -= HandleShowCompleted;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Queue a narrative event for display
        /// </summary>
        public void QueueEvent(NarrativeEvent narrativeEvent)
        {
            eventQueue.Enqueue(narrativeEvent);
            if (!isProcessingEvent)
            {
                ProcessNextEvent();
            }
        }

        /// <summary>
        /// Play immediate dialogue (bypasses queue)
        /// </summary>
        public void PlayImmediate(DialogueData dialogue)
        {
            StartCoroutine(PlayDialogueImmediate(dialogue));
        }

        /// <summary>
        /// Skip current dialogue
        /// </summary>
        public void SkipCurrent()
        {
            if (skipEnabled && isProcessingEvent)
            {
                storyNarrator?.CompleteCurrentLine();
                dynamicNarrator?.CompleteCurrentLine();
            }
        }

        /// <summary>
        /// Pause all narrative
        /// </summary>
        public void Pause()
        {
            Time.timeScale = 0;
            OnNarrativePaused?.Invoke();
        }

        /// <summary>
        /// Resume narrative
        /// </summary>
        public void Resume()
        {
            Time.timeScale = 1;
            OnNarrativeResumed?.Invoke();
        }

        /// <summary>
        /// Clear all queued events
        /// </summary>
        public void ClearQueue()
        {
            eventQueue.Clear();
        }

        /// <summary>
        /// Set narrative context for dynamic dialogue
        /// </summary>
        public void SetContext(NarrativeContext context)
        {
            currentContext = context;
        }

        /// <summary>
        /// Trigger a story event dialogue
        /// </summary>
        public void TriggerStoryDialogue(string eventId)
        {
            var dialogue = storyNarrator?.GetDialogueForEvent(eventId);
            if (dialogue != null)
            {
                QueueEvent(new NarrativeEvent
                {
                    type = NarrativeEventType.Story,
                    dialogue = dialogue,
                    priority = 100,
                    eventId = eventId
                });
            }
        }

        /// <summary>
        /// Trigger competition commentary
        /// </summary>
        public void TriggerCompetitionCommentary(CommentaryState state)
        {
            var dialogue = competitionCommentator?.GetCommentaryForState(state);
            if (dialogue != null)
            {
                QueueEvent(new NarrativeEvent
                {
                    type = NarrativeEventType.Commentary,
                    dialogue = dialogue,
                    priority = 50
                });
            }
        }

        /// <summary>
        /// Trigger tutorial hint
        /// </summary>
        public void TriggerTutorialHint(string tutorialId)
        {
            if (currentContext.tutorialCompleted.Contains(tutorialId)) return;

            var dialogue = tutorialNarrator?.GetTutorialDialogue(tutorialId);
            if (dialogue != null)
            {
                QueueEvent(new NarrativeEvent
                {
                    type = NarrativeEventType.Tutorial,
                    dialogue = dialogue,
                    priority = 80,
                    eventId = tutorialId
                });
            }
        }

        /// <summary>
        /// Trigger dynamic narration based on game state
        /// </summary>
        public void TriggerDynamicNarration(NarrativeTrigger trigger)
        {
            var dialogue = dynamicNarrator?.GetNarration(trigger);
            if (dialogue != null)
            {
                QueueEvent(new NarrativeEvent
                {
                    type = NarrativeEventType.Dynamic,
                    dialogue = dialogue,
                    priority = 60
                });
            }
        }

        /// <summary>
        /// Mark a tutorial as completed
        /// </summary>
        public void CompleteTutorial(string tutorialId)
        {
            currentContext.tutorialCompleted.Add(tutorialId);
        }

        #endregion

        #region Event Processing

        private void ProcessNextEvent()
        {
            if (eventQueue.Count == 0)
            {
                isProcessingEvent = false;
                currentEvent = null;
                return;
            }

            isProcessingEvent = true;
            currentEvent = eventQueue.Dequeue();
            currentEvent.startTime = Time.time;

            OnEventStarted?.Invoke(currentEvent);

            switch (currentEvent.type)
            {
                case NarrativeEventType.Story:
                    storyNarrator?.PlayNarration(currentEvent.dialogue, currentEvent.priority);
                    break;
                case NarrativeEventType.Commentary:
                    competitionCommentator?.PlayCommentary(currentEvent.dialogue);
                    break;
                case NarrativeEventType.Tutorial:
                    tutorialNarrator?.PlayTutorial(currentEvent.dialogue);
                    break;
                case NarrativeEventType.Dynamic:
                    dynamicNarrator?.PlayNarration(currentEvent.dialogue);
                    break;
            }

            if (currentEvent.dialogue != null)
            {
                float duration = currentEvent.dialogue.lines.Count * currentEvent.dialogue.averageLineDuration;
                StartCoroutine(AutoCompleteEvent(currentEvent, duration));
            }
        }

        private IEnumerator AutoCompleteEvent(NarrativeEvent evt, float delay)
        {
            yield return new WaitForSeconds(delay);
            NotifyEventComplete(evt);
        }

        private IEnumerator PlayDialogueImmediate(DialogueData dialogue)
        {
            isProcessingEvent = true;
            OnDialogueStarted?.Invoke(dialogue);

            // Wait for dialogue to complete
            yield return new WaitForSeconds(dialogue.lines.Count * dialogue.averageLineDuration);

            OnDialogueCompleted?.Invoke(dialogue);
            isProcessingEvent = false;
        }

        /// <summary>
        /// Called when current event completes
        /// </summary>
        public void NotifyEventComplete(NarrativeEvent evt)
        {
            if (currentEvent == evt)
            {
                OnEventCompleted?.Invoke(evt);
                ProcessNextEvent();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRunStarted()
        {
            // Show match intro commentary
            TriggerCompetitionCommentary(CommentaryState.MatchIntro);
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            var state = result == RunResult.Qualified ? CommentaryState.FinishLine : CommentaryState.Mistakes;
            TriggerCompetitionCommentary(state);

            // Dynamic narration based on result
            if (result == RunResult.Qualified && faults == 0)
            {
                TriggerDynamicNarration(new NarrativeTrigger
                {
                    type = TriggerType.PerfectRun,
                    value = time
                });
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            CommentaryState state = type switch
            {
                ObstacleType.WeavePoles => CommentaryState.WeavePoles,
                ObstacleType.Tunnel => CommentaryState.Tunnel,
                ObstacleType.AFrame or ObstacleType.DogWalk => CommentaryState.ContactObstacles,
                ObstacleType.Teeter => CommentaryState.TeeterTotter,
                ObstacleType.BarJump or ObstacleType.TireJump or ObstacleType.BroadJump => CommentaryState.Jumps,
                _ => CommentaryState.General
            };

            TriggerCompetitionCommentary(state);

            // Tutorial triggers for first-timers
            if (!currentContext.tutorialCompleted.Contains($"first_{type}"))
            {
                TriggerTutorialHint($"first_{type}");
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            TriggerCompetitionCommentary(CommentaryState.Mistakes);

            // Dynamic narration for specific faults
            TriggerDynamicNarration(new NarrativeTrigger
            {
                type = TriggerType.Fault,
                fault = fault,
                obstacle = obstacleName
            });
        }

        private void HandleChapterUnlocked(int chapter)
        {
            currentContext.currentChapter = chapter;
            currentContext.isStoryActive = true;

            // Trigger chapter introduction
            TriggerStoryDialogue($"chapter_{chapter}_intro");
        }

        private void HandleCutsceneStarted(CutsceneData cutscene)
        {
            currentMode = NarrativeMode.Cutscene;
            ClearQueue(); // Clear other events during cutscene
        }

        private void HandleCutsceneEnded()
        {
            currentMode = NarrativeMode.Idle;
        }

        private void HandleShowStarted(ShowTier tier)
        {
            TriggerDynamicNarration(new NarrativeTrigger
            {
                type = TriggerType.ShowStart
            });
        }

        private void HandleShowCompleted(ShowResult result, int placement)
        {
            TriggerDynamicNarration(new NarrativeTrigger
            {
                type = TriggerType.ShowEnd,
                result = result
            });
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Narrative event types
    /// </summary>
    public enum NarrativeEventType
    {
        Story,          // Cutscene/story dialogue
        Commentary,     // Competition commentary
        Tutorial,       // Tutorial hints
        Dynamic         // Dynamic narration based on state
    }

    /// <summary>
    /// Narrative mode
    /// </summary>
    public enum NarrativeMode
    {
        Idle,
        Cutscene,
        Competition,
        Tutorial
    }

    /// <summary>
    /// Narrative event for queue
    /// </summary>
    [Serializable]
    public class NarrativeEvent
    {
        public NarrativeEventType type;
        public DialogueData dialogue;
        public int priority;
        public string eventId;
        public float startTime;
        public bool skippable = true;
    }

    /// <summary>
    /// Dialogue data for a sequence of lines
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        public string id;
        public string title;
        public string speakerName;
        public List<DialogueLineData> lines = new List<DialogueLineData>();
        public float averageLineDuration = 3f;
        public Sprite speakerPortrait;
        public AudioClip voiceClip;
        public bool skippable = true;
    }

    /// <summary>
    /// Single dialogue line
    /// </summary>
    [Serializable]
    public class DialogueLineData
    {
        public string text;
        public float duration = 3f;
        public string emotion;
        public Sprite portrait;
        public AudioClip audioClip;
        public string animationTrigger;
    }

    /// <summary>
    /// Narrative context for dynamic dialogue
    /// </summary>
    [Serializable]
    public class NarrativeContext
    {
        public int currentChapter;
        public string currentCharacter;
        public string currentScene;
        public bool isStoryActive;
        public ShowTier currentShowTier;
        public float dogSkillLevel;
        public int totalWins;
        public int totalShows;
        public HashSet<string> tutorialCompleted;
        public List<DialogueRecord> dialogueHistory;
    }

    /// <summary>
    /// Trigger for dynamic narration
    /// </summary>
    [Serializable]
    public class NarrativeTrigger
    {
        public TriggerType type;
        public float value;
        public FaultType fault;
        public string obstacle;
        public ShowResult result;
        public string characterId;
    }

    public enum TriggerType
    {
        PerfectRun,
        FirstTimeObstacle,
        Fault,
        NearMiss,
        PersonalBest,
        ShowStart,
        ShowEnd,
        MilestoneReached,
        LevelUp,
        CharacterInteraction
    }

    /// <summary>
    /// Dialogue history record
    /// </summary>
    [Serializable]
    public class DialogueRecord
    {
        public string eventId;
        public float timestamp;
        public NarrativeEventType type;
    }

    #endregion
}
