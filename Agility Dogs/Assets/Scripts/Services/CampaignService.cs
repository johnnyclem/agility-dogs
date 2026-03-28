using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    /// <summary>
    /// CampaignService - Manages story/campaign mode progression
    /// Handles chapter unlocks, cutscene triggers, and story events
    /// Integrates with NarrativeService for all dialogue and narration
    /// </summary>
    public class CampaignService : MonoBehaviour
    {
        public static CampaignService Instance { get; private set; }

        [Header("Campaign Configuration")]
        [SerializeField] private int startingChapter = 1;
        [SerializeField] private bool skipTutorial = false;

        [Header("Story Data")]
        [SerializeField] private List<StoryChapterData> chapters = new List<StoryChapterData>();
        [SerializeField] private List<CutsceneData> cutscenes = new List<CutsceneData>();
        [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

        // Campaign state
        private int currentChapter = 1;
        private int totalChapters = 8;
        private HashSet<int> unlockedChapters = new HashSet<int>();
        private HashSet<string> completedStoryEvents = new HashSet<string>();
        private Dictionary<string, int> characterRelationship = new Dictionary<string, int>();
        private bool campaignActive = false;
        private bool inCutscene = false;
        private Coroutine cutsceneCoroutine;

        // Events
        public event Action<int> OnChapterUnlocked;
        public event Action<string> OnStoryEventTriggered;
        public event Action<CutsceneData> OnCutsceneStarted;
        public event Action OnCutsceneEnded;
        public event Action<int> OnCampaignCompleted;
        public event Action<CampaignDialogueLine> OnCampaignDialogueLine;

        // Properties
        public int CurrentChapter => currentChapter;
        public int TotalChapters => totalChapters;
        public bool IsCampaignActive => campaignActive;
        public bool IsInCutscene => inCutscene;
        public IReadOnlyCollection<int> UnlockedChapters => unlockedChapters;
        public IReadOnlyCollection<string> CompletedStoryEvents => completedStoryEvents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeChapters();
            LoadCampaignProgress();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            SaveCampaignProgress();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnAchievementUnlocked += HandleAchievementUnlocked;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnAchievementUnlocked -= HandleAchievementUnlocked;
        }

        #region Initialization

        private void InitializeChapters()
        {
            // Unlock starting chapter
            unlockedChapters.Add(startingChapter);
            currentChapter = startingChapter;

            // Create default chapters if none exist
            if (chapters.Count == 0)
            {
                CreateDefaultChapters();
            }

            // Create default characters if none exist
            if (characters.Count == 0)
            {
                CreateDefaultCharacters();
            }
        }

        private void CreateDefaultCharacters()
        {
            characters = new List<CharacterData>
            {
                new CharacterData
                {
                    characterId = "narrator",
                    characterName = "Narrator",
                    title = "Storyteller",
                    defaultDialogue = "The journey continues..."
                },
                new CharacterData
                {
                    characterId = "coach_sarah",
                    characterName = "Coach Sarah Chen",
                    title = "Your Mentor",
                    defaultDialogue = "Remember: patience and persistence beat talent every time."
                },
                new CharacterData
                {
                    characterId = "marcus",
                    characterName = "Marcus Chen",
                    title = "Friendly Rival",
                    defaultDialogue = "Hey! Ready for some competition?"
                },
                new CharacterData
                {
                    characterId = "emily",
                    characterName = "Emily Rodriguez",
                    title = "Elite Handler",
                    defaultDialogue = "You've got potential. Let's see how far you can go."
                },
                new CharacterData
                {
                    characterId = "victoria",
                    characterName = "Victoria Price",
                    title = "Former Champion",
                    defaultDialogue = "Champions aren't born. They're forged through discipline."
                },
                new CharacterData
                {
                    characterId = "announcer",
                    characterName = "Announcer",
                    title = "Voice of the Competition",
                    defaultDialogue = "Welcome to the show!"
                }
            };

            Debug.Log($"[CampaignService] Created {characters.Count} default characters");
        }

        private void CreateDefaultChapters()
        {
            chapters = new List<StoryChapterData>
            {
                new StoryChapterData
                {
                    chapterNumber = 1,
                    title = "A New Beginning",
                    subtitle = "Every champion starts somewhere",
                    description = "Meet your first dog and learn the basics of agility training.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.GameStart },
                    associatedCutscenes = new List<string> { "intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 2,
                    title = "Local Legends",
                    subtitle = "The crowd goes wild",
                    description = "Enter your first local competition and face off against other handlers.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.ShowsCompleted, value = 1 },
                    associatedCutscenes = new List<string> { "chapter2_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 3,
                    title = "Rising Stars",
                    subtitle = "County fair champion",
                    description = "Prove yourself at the county fair and catch the eye of seasoned competitors.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.ShowsCompleted, value = 3 },
                    associatedCutscenes = new List<string> { "chapter3_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 4,
                    title = "The Regionals",
                    subtitle = "Where legends are made",
                    description = "The regional championships bring tougher competition and higher stakes.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.WinsAtTier, tier = ShowTier.County, value = 2 },
                    associatedCutscenes = new List<string> { "chapter4_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 5,
                    title = "State of Mind",
                    subtitle = "The art of focus",
                    description = "State championships demand peak performance and unwavering focus.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.WinsAtTier, tier = ShowTier.Regional, value = 2 },
                    associatedCutscenes = new List<string> { "chapter5_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 6,
                    title = "National Dreams",
                    subtitle = "Among the elite",
                    description = "Only the best handlers make it to the national championships.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.WinsAtTier, tier = ShowTier.State, value = 2 },
                    associatedCutscenes = new List<string> { "chapter6_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 7,
                    title = "Westminster Calling",
                    subtitle = "The dream awaits",
                    description = "You've qualified for the Westminster Agility Kings. The whole world is watching.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.WinsAtTier, tier = ShowTier.National, value = 2 },
                    associatedCutscenes = new List<string> { "chapter7_intro" }
                },
                new StoryChapterData
                {
                    chapterNumber = 8,
                    title = "Agility Kings",
                    subtitle = "Champion of champions",
                    description = "The final championship. Everything you've worked for comes down to this moment.",
                    unlockCondition = new UnlockCondition { type = UnlockConditionType.WestminsterQualified },
                    associatedCutscenes = new List<string> { "finale" }
                }
            };

            Debug.Log($"[CampaignService] Created {chapters.Count} default chapters");
        }

        #endregion

        #region Campaign Flow

        /// <summary>
        /// Start the campaign from the beginning
        /// </summary>
        public void StartCampaign()
        {
            Debug.Log("[CampaignService] Starting campaign");
            campaignActive = true;
            currentChapter = startingChapter;
            
            // Play intro cutscene
            PlayCutscene("intro");
        }

        /// <summary>
        /// Resume campaign from last checkpoint
        /// </summary>
        public void ResumeCampaign()
        {
            if (unlockedChapters.Count > 0)
            {
                currentChapter = unlockedChapters.Max();
            }
            campaignActive = true;
            CheckChapterUnlocks();
        }

        /// <summary>
        /// End the current campaign session
        /// </summary>
        public void EndCampaign()
        {
            campaignActive = false;
            inCutscene = false;
            SaveCampaignProgress();
        }

        /// <summary>
        /// Check if a specific chapter is unlocked
        /// </summary>
        public bool IsChapterUnlocked(int chapterNumber)
        {
            return unlockedChapters.Contains(chapterNumber);
        }

        /// <summary>
        /// Get chapter data by number
        /// </summary>
        public StoryChapterData GetChapter(int chapterNumber)
        {
            return chapters.FirstOrDefault(c => c.chapterNumber == chapterNumber);
        }

        /// <summary>
        /// Get all unlocked chapters
        /// </summary>
        public List<StoryChapterData> GetUnlockedChapters()
        {
            return chapters.Where(c => unlockedChapters.Contains(c.chapterNumber))
                          .OrderBy(c => c.chapterNumber)
                          .ToList();
        }

        /// <summary>
        /// Get the current chapter data
        /// </summary>
        public StoryChapterData GetCurrentChapter()
        {
            return GetChapter(currentChapter);
        }

        #endregion

        #region Story Events

        /// <summary>
        /// Trigger a story event by ID
        /// </summary>
        public void TriggerStoryEvent(string eventId)
        {
            if (completedStoryEvents.Contains(eventId))
            {
                return;
            }

            completedStoryEvents.Add(eventId);
            OnStoryEventTriggered?.Invoke(eventId);
            
            Debug.Log($"[CampaignService] Story event triggered: {eventId}");
            SaveCampaignProgress();
        }

        /// <summary>
        /// Check if a story event has been completed
        /// </summary>
        public bool HasCompletedStoryEvent(string eventId)
        {
            return completedStoryEvents.Contains(eventId);
        }

        #endregion

        #region Cutscene System

        /// <summary>
        /// Play a cutscene by ID
        /// </summary>
        public void PlayCutscene(string cutsceneId)
        {
            CutsceneData cutscene = cutscenes.FirstOrDefault(c => c.cutsceneId == cutsceneId);
            
            if (cutscene == null)
            {
                // Try to create a default cutscene
                cutscene = CreateDefaultCutscene(cutsceneId);
            }

            if (cutscene != null)
            {
                cutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine(cutscene));
            }
            else
            {
                Debug.LogWarning($"[CampaignService] Cutscene not found: {cutsceneId}");
                OnCutsceneEnded?.Invoke();
            }
        }

        private IEnumerator PlayCutsceneCoroutine(CutsceneData cutscene)
        {
            inCutscene = true;
            OnCutsceneStarted?.Invoke(cutscene);

            Debug.Log($"[CampaignService] Playing cutscene: {cutscene.cutsceneId}");

            if (cutscene.dialogueLines == null)
            {
            inCutscene = false;
            cutsceneCoroutine = null;
            OnCutsceneEnded?.Invoke();
                yield break;
            }

            foreach (var line in cutscene.dialogueLines)
            {
                OnCampaignDialogueLine?.Invoke(line);
                
                float elapsed = 0f;
                while (elapsed < line.duration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            inCutscene = false;
            OnCutsceneEnded?.Invoke();

            Debug.Log($"[CampaignService] Cutscene complete: {cutscene.cutsceneId}");
        }

        public void StopCutscene()
        {
            if (!inCutscene) return;
            if (cutsceneCoroutine != null)
            {
                StopCoroutine(cutsceneCoroutine);
                cutsceneCoroutine = null;
            }
            inCutscene = false;
            OnCutsceneEnded?.Invoke();
            Debug.Log("[CampaignService] Cutscene stopped");
        }

        private CutsceneData CreateDefaultCutscene(string cutsceneId)
        {
            var cutscene = new CutsceneData { cutsceneId = cutsceneId };

            switch (cutsceneId)
            {
                case "intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "Every great champion has a beginning. Today, you take your first steps toward becoming an Agility King.",
                            duration = 4f,
                            emotion = "neutral"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "Welcome to the world of dog agility! I'm Coach Sarah Chen, and I'll be guiding you on this journey.",
                            duration = 4f,
                            emotion = "warm"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "I remember when I first started... that was twenty years ago now. Time flies when you're having fun!",
                            duration = 4f,
                            emotion = "nostalgic"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "First things first - let's find you a partner. Every handler needs the right dog. What kind of puppy are you looking for?",
                            duration = 5f,
                            emotion = "encouraging"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter2_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "You've got a natural talent! Your dog responds to you beautifully. I think it's time you tested your skills in a real competition.",
                            duration = 5f,
                            emotion = "proud"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "The local agility scene awaits. But you'll soon learn you're not the only one with dreams of glory...",
                            duration = 4f,
                            emotion = "mysterious"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "Hey there, rookie! I'm Marcus. Welcome to the circuit! Hope you're ready for some real competition!",
                            duration = 4f,
                            emotion = "competitive"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "Marcus is... enthusiastic. He's been competing for three years. Watch and learn from everyone out there.",
                            duration = 4f,
                            emotion = "amused"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter3_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "Word has spread about a promising newcomer. At the County Fair Championships, you'll face tougher competition than ever before.",
                            duration = 5f,
                            emotion = "tense"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "So you're the one everyone's talking about. I'm Emily. I've been training for five years. Let's see what you've got.",
                            duration = 5f,
                            emotion = "respectful"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "Emily's one of the best in the county. But don't let that intimidate you - use it as motivation. She's beatable if you stay focused.",
                            duration = 5f,
                            emotion = "encouraging"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "Good run out there. You've got potential. But the real test is coming - the Regionals aren't for the faint of heart.",
                            duration = 4f,
                            emotion = "friendly"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter4_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "The Regional Championships. Where champions are made and dreams are tested. The competition here is on another level.",
                            duration = 5f,
                            emotion = "dramatic"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "Regionals! I've been training all year for this. My Border Collie, Storm, is faster than ever!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "Marcus qualified last year but didn't place. He's hungry this year. And there's someone else you should know...",
                            duration = 5f,
                            emotion = "serious"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "That's Victoria Price. Three-time regional champion. She won Westminster once, fifteen years ago. Now she coaches.",
                            duration = 5f,
                            emotion = "respectful"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "Coach Sarah speaks highly of you. Let's see if her instincts are still sharp. Give it your all, young one.",
                            duration = 5f,
                            emotion = "authoritative"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter5_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "State Championships. Only the elite reach this level. The courses are longer, the obstacles more challenging.",
                            duration = 5f,
                            emotion = "intense"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "This is where training really pays off. Your dog's stamina, your handling skills - everything matters now.",
                            duration = 5f,
                            emotion = "focused"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "We made it! Both of us. This is what we've been working toward. May the best team win!",
                            duration = 4f,
                            emotion = "friendly"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "I've been watching your progress. You have something special. But so does she. Remember: focus beats talent when talent doesn't focus.",
                            duration = 6f,
                            emotion = "wise"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter6_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "announcer",
                            speakerName = "Announcer",
                            dialogueText = "Welcome to the National Agility Championships! The top handlers from across the country have gathered!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "The atmosphere is electric. Cameras flash, crowds roar. This is the big time.",
                            duration = 4f,
                            emotion = "thrilling"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "I never thought I'd make it this far. Three years ago I was running practice courses in my backyard. Now look at us!",
                            duration = 5f,
                            emotion = "grateful"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "Marcus has grown so much. You've all grown so much. I'm proud of every single one of you.",
                            duration = 5f,
                            emotion = "emotional"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "Coach Sarah... she never tells anyone, but she was a champion too, once. Before injuries forced her to retire from competition.",
                            duration = 5f,
                            emotion = "revelation"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "chapter7_intro":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "A letter arrives. The envelope bears the unmistakable crest of the Westminster Kennel Club.",
                            duration = 4f,
                            emotion = "suspenseful"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "You did it. You've qualified for the Westminster Agility Kings. This is... this is the championship I never got to win.",
                            duration = 5f,
                            emotion = "overwhelmed"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "The world will be watching. Your friends, your family, everyone who believed in you. Don't let the pressure break you - let it forge you.",
                            duration = 6f,
                            emotion = "fierce"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "We've come so far together. Even if we're competing against each other now... we're still family.",
                            duration = 5f,
                            emotion = "brotherly"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "See you at Westminster. May the best handler win. But honestly? I hope you do. You deserve this.",
                            duration = 5f,
                            emotion = "gracious"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "finale":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerName = "Announcer",
                            dialogueText = "Ladies and gentlemen, welcome to the WESTMINSTER AGILITY KINGS CHAMPIONSHIP!",
                            duration = 3f,
                            emotion = "excitement"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Announcer",
                            dialogueText = "In lane one... representing the Western Region... EMILY RODRIGUEZ with her Golden Retriever, Sunshine!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Announcer",
                            dialogueText = "In lane two... the crowd favorite... MARCUS CHEN with Border Collie, Storm!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Announcer",
                            dialogueText = "And in lane three... the newcomer who rose through every tier... with their partner!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Coach Sarah",
                            dialogueText = "This is it. Everything we've worked for. Every early morning, every late night, every time we didn't give up.",
                            duration = 5f,
                            emotion = "emotional"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Coach Sarah",
                            dialogueText = "No matter what happens... you've already made me proud. Now go out there and show the world what you're made of!",
                            duration = 5f,
                            emotion = "tears"
                        },
                        new CampaignDialogueLine
                        {
                            speakerName = "Narrator",
                            dialogueText = "The crowd falls silent as handler and dog take their position at the starting line. This is the moment everything has led to...",
                            duration = 5f,
                            emotion = "suspense"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "first_victory":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "First place! Against all odds, you've claimed victory in your debut competition.",
                            duration = 4f,
                            emotion = "triumphant"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "I knew you had it in you! That run was beautiful - you and your dog moving as one.",
                            duration = 5f,
                            emotion = "proud"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "Hey, not bad rookie! You've got skills. But next time, I'll be ready!",
                            duration = 4f,
                            emotion = "competitive"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "first_defeat":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "The competition proved tough today. But defeat is a teacher, not a verdict.",
                            duration = 4f,
                            emotion = "melancholy"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "You didn't win today, but that doesn't define you. Every champion has lost before becoming a champion.",
                            duration = 5f,
                            emotion = "encouraging"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "That was a solid run. Your dog loves you - I could see it. Keep training, you'll get there.",
                            duration = 5f,
                            emotion = "respectful"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "marcus_rivalry":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "Alright, alright! You beat me fair and square. I've been training Storm for three years, but today... you were better.",
                            duration = 5f,
                            emotion = "gracious"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "That took courage to say, Marcus. Respect between competitors is what makes this sport great.",
                            duration = 4f,
                            emotion = "approving"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "You've got a fan now, rookie. Let's train together sometime. Two handlers pushing each other = better results!",
                            duration = 5f,
                            emotion = "friendly"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "emily_rivalry":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "You know, when I first saw you, I didn't think much. Just another rookie with a dream.",
                            duration = 4f,
                            emotion = "reflective"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "But you've got heart. And heart is what separates good from great. I respect that.",
                            duration = 5f,
                            emotion = "sincere"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "High praise from Emily! She's not easy to impress. You've earned something special here.",
                            duration = 4f,
                            emotion = "proud"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "tier_complete":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "Another tier conquered. The road to Westminster grows shorter, but the path grows steeper.",
                            duration = 4f,
                            emotion = "determined"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "I'm getting emotional just watching you grow. You've become a real competitor.",
                            duration = 4f,
                            emotion = "emotional"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "Don't rest on your laurels. The next tier is waiting, and it doesn't forgive weakness. Stay hungry.",
                            duration = 5f,
                            emotion = "stern"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "westminster_qualify":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "IT'S OFFICIAL. You have qualified for the Westminster Agility Kings Championship.",
                            duration = 3f,
                            emotion = "awe"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "announcer",
                            speakerName = "Announcer",
                            dialogueText = "Ladies and gentlemen, please join us in congratulating our newest Agility Kings qualifier!",
                            duration = 4f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "You did it. After everything... you really did it. This is the moment I've been waiting for.",
                            duration = 5f,
                            emotion = "overwhelmed"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "A lifetime in this sport, and I still remember my first qualification. Cherish this feeling. It's fleeting - use it.",
                            duration = 5f,
                            emotion = "nostalgic"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                case "agility_kings_victory":
                    cutscene.dialogueLines = new List<CampaignDialogueLine>
                    {
                        new CampaignDialogueLine
                        {
                            speakerId = "announcer",
                            speakerName = "Announcer",
                            dialogueText = "AND THE WINNER OF THE WESTMINSTER AGILITY KINGS... [Drumroll]...",
                            duration = 3f,
                            emotion = "excited"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "announcer",
                            speakerName = "Announcer",
                            dialogueText = "YOUR AGILITY KINGS CHAMPION!",
                            duration = 3f,
                            emotion = "triumphant"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "coach_sarah",
                            speakerName = "Coach Sarah",
                            dialogueText = "You did it. You actually did it. I've watched so many competitors try, fail, and try again. But you... you made it.",
                            duration = 5f,
                            emotion = "tears"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "marcus",
                            speakerName = "Marcus Chen",
                            dialogueText = "No way! I can't believe it! I was rooting for you the whole time! Well, okay, maybe not at first, but... I'm so happy for you!",
                            duration = 5f,
                            emotion = "ecstatic"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "emily",
                            speakerName = "Emily Rodriguez",
                            dialogueText = "You earned this. Every step, every fall, every moment of doubt - you overcame it all. Champion.",
                            duration = 5f,
                            emotion = "gracious"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "victoria",
                            speakerName = "Victoria Price",
                            dialogueText = "I once stood where you're standing. It's a lonely peak, but also a sweet one. Welcome to the club, Agility King.",
                            duration = 5f,
                            emotion = "approving"
                        },
                        new CampaignDialogueLine
                        {
                            speakerId = "narrator",
                            speakerName = "Narrator",
                            dialogueText = "And so, a new legend is born. From humble beginnings to the grandest stage in dog agility... the journey is complete.",
                            duration = 5f,
                            emotion = "triumphant"
                        }
                    };
                    cutscenes.Add(cutscene);
                    return cutscene;

                default:
                    return null;
            }
        }

        #endregion

        #region Chapter Unlocks

        /// <summary>
        /// Check and unlock chapters based on current progress
        /// </summary>
        public void CheckChapterUnlocks()
        {
            var careerService = CareerProgressionService.Instance;
            var showManager = ShowManager.Instance;

            foreach (var chapter in chapters)
            {
                if (unlockedChapters.Contains(chapter.chapterNumber))
                    continue;

                if (CheckUnlockCondition(chapter.unlockCondition, careerService, showManager))
                {
                    UnlockChapter(chapter.chapterNumber);
                }
            }
        }

        private bool CheckUnlockCondition(UnlockCondition condition, CareerProgressionService careerService, ShowManager showManager)
        {
            switch (condition.type)
            {
                case UnlockConditionType.GameStart:
                    return true;

                case UnlockConditionType.CareerLevel:
                    return careerService != null && careerService.CurrentLevel >= condition.value;

                case UnlockConditionType.ShowsCompleted:
                    return showManager != null && showManager.TotalCompetitions >= condition.value;

                case UnlockConditionType.TotalWins:
                    return showManager != null && showManager.TotalWins >= condition.value;

                case UnlockConditionType.WinsAtTier:
                    return showManager != null && showManager.GetWinsAtTier(condition.tier) >= condition.value;

                case UnlockConditionType.WestminsterQualified:
                    return showManager != null && showManager.CanEnterWestminster();

                case UnlockConditionType.StoryEventComplete:
                    return completedStoryEvents.Contains(condition.eventId);

                case UnlockConditionType.AchievementUnlocked:
                    return careerService != null && careerService.HasAchievement(condition.achievementId);

                default:
                    return false;
            }
        }

        private void UnlockChapter(int chapterNumber)
        {
            if (unlockedChapters.Contains(chapterNumber))
                return;

            unlockedChapters.Add(chapterNumber);
            currentChapter = chapterNumber;
            
            OnChapterUnlocked?.Invoke(chapterNumber);
            
            var chapter = GetChapter(chapterNumber);
            Debug.Log($"[CampaignService] Chapter {chapterNumber} unlocked: {chapter?.title}");

            if (chapter?.associatedCutscenes != null && chapter.associatedCutscenes.Count > 0)
            {
                PlayCutscene(chapter.associatedCutscenes[0]);
            }

            SaveCampaignProgress();
        }

        #endregion

        #region Character Relationships

        /// <summary>
        /// Improve relationship with a character
        /// </summary>
        public void CompleteChapter(int chapterNumber)
        {
            var chapter = GetChapter(chapterNumber);
            if (chapter != null)
            {
                chapter.isCompleted = true;
                Debug.Log($"[CampaignService] Chapter {chapterNumber} completed: {chapter.title}");
                SaveCampaignProgress();

                if (chapterNumber >= totalChapters)
                {
                    OnCampaignCompleted?.Invoke(chapterNumber);
                    Debug.Log("[CampaignService] Campaign completed!");
                }
            }
        }

        public void ImproveRelationship(string characterId, int amount = 1)
        {
            if (!characterRelationship.ContainsKey(characterId))
            {
                characterRelationship[characterId] = 0;
            }
            characterRelationship[characterId] += amount;
            
            Debug.Log($"[CampaignService] {characterId} relationship: {characterRelationship[characterId]}");
            SaveCampaignProgress();
        }

        /// <summary>
        /// Get relationship level with a character
        /// </summary>
        public int GetRelationshipLevel(string characterId)
        {
            return characterRelationship.TryGetValue(characterId, out int level) ? level : 0;
        }

        /// <summary>
        /// Get character data by ID
        /// </summary>
        public CharacterData GetCharacter(string characterId)
        {
            return characters.FirstOrDefault(c => c.characterId == characterId);
        }

        #endregion

        #region Persistence

        private void SaveCampaignProgress()
        {
            PlayerPrefs.SetInt("Campaign_CurrentChapter", currentChapter);
            PlayerPrefs.SetString("Campaign_UnlockedChapters", string.Join(",", unlockedChapters));
            PlayerPrefs.SetString("Campaign_CompletedEvents", string.Join(",", completedStoryEvents));

            // Save character relationships
            foreach (var kvp in characterRelationship)
            {
                PlayerPrefs.SetInt($"Campaign_Rel_{kvp.Key}", kvp.Value);
            }

            // Save chapter completion
            foreach (var chapter in chapters)
            {
                if (chapter.isCompleted)
                {
                    PlayerPrefs.SetInt($"Campaign_ChapterComplete_{chapter.chapterNumber}", 1);
                }
            }

            PlayerPrefs.Save();
        }

        private void LoadCampaignProgress()
        {
            currentChapter = PlayerPrefs.GetInt("Campaign_CurrentChapter", startingChapter);
            
            string unlockedStr = PlayerPrefs.GetString("Campaign_UnlockedChapters", "");
            if (!string.IsNullOrEmpty(unlockedStr))
            {
                unlockedChapters = new HashSet<int>(
                    unlockedStr.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse)
                );
            }

            string eventsStr = PlayerPrefs.GetString("Campaign_CompletedEvents", "");
            if (!string.IsNullOrEmpty(eventsStr))
            {
                completedStoryEvents = new HashSet<string>(
                    eventsStr.Split(',').Where(s => !string.IsNullOrEmpty(s))
                );
            }

            // Load chapter completion
            foreach (var chapter in chapters)
            {
                chapter.isCompleted = PlayerPrefs.GetInt($"Campaign_ChapterComplete_{chapter.chapterNumber}", 0) == 1;
            }

            // Load character relationships
            foreach (var character in characters)
            {
                int relLevel = PlayerPrefs.GetInt($"Campaign_Rel_{character.characterId}", -1);
                if (relLevel >= 0)
                {
                    characterRelationship[character.characterId] = relLevel;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            if (!campaignActive) return;

            // Check for first victory
            if ((result == RunResult.Qualified || result == RunResult.TimeFaultOnly) && 
                !HasCompletedStoryEvent("first_victory"))
            {
                var showManager = ShowManager.Instance;
                if (showManager != null && showManager.TotalWins == 1)
                {
                    TriggerStoryEvent("first_victory");
                    PlayCutscene("first_victory");
                }
            }

            // Check for first defeat (DNF after completing at least one show)
            if (result == RunResult.NonQualified || result == RunResult.Elimination)
            {
                var showManager = ShowManager.Instance;
                if (showManager != null && showManager.TotalCompetitions >= 1 && 
                    !HasCompletedStoryEvent("first_defeat"))
                {
                    TriggerStoryEvent("first_defeat");
                    PlayCutscene("first_defeat");
                }
            }

            // Check for tier completion
            if (result == RunResult.Qualified && !HasCompletedStoryEvent("tier_complete"))
            {
                CheckForTierCompletion();
            }

            // Check chapter unlocks
            CheckChapterUnlocks();

            // Check for campaign completion (chapter 8 completed = win at Westminster)
            if (result == RunResult.Qualified && currentChapter >= totalChapters)
            {
                var showMgr = ShowManager.Instance;
                if (showMgr != null && showMgr.CurrentTier == ShowTier.Westminster)
                {
                    CompleteChapter(totalChapters);
                }
            }
        }

        private void CheckForTierCompletion()
        {
            var showManager = ShowManager.Instance;
            if (showManager == null) return;

            int wins = showManager.TotalWins;

            // Check if player just reached a tier threshold
            if (wins == 2 && !HasCompletedStoryEvent("tier_2_complete"))
            {
                TriggerStoryEvent("tier_2_complete");
                if (GetCurrentChapter()?.chapterNumber >= 3)
                {
                    PlayCutscene("tier_complete");
                }
            }
            else if (wins == 4 && !HasCompletedStoryEvent("tier_4_complete"))
            {
                TriggerStoryEvent("tier_4_complete");
                if (GetCurrentChapter()?.chapterNumber >= 4)
                {
                    PlayCutscene("tier_complete");
                }
            }
            else if (wins == 6 && !HasCompletedStoryEvent("tier_6_complete"))
            {
                TriggerStoryEvent("tier_6_complete");
                if (GetCurrentChapter()?.chapterNumber >= 5)
                {
                    PlayCutscene("tier_complete");
                }
            }
            else if (wins == 8 && !HasCompletedStoryEvent("tier_8_complete"))
            {
                TriggerStoryEvent("tier_8_complete");
                if (GetCurrentChapter()?.chapterNumber >= 6)
                {
                    PlayCutscene("tier_complete");
                }
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (!campaignActive) return;

            // Trigger tutorial events for specific obstacles
            if (!HasCompletedStoryEvent("first_jump") && type == ObstacleType.BarJump)
            {
                TriggerStoryEvent("first_jump");
            }
            else if (!HasCompletedStoryEvent("first_tunnel") && type == ObstacleType.Tunnel)
            {
                TriggerStoryEvent("first_tunnel");
            }
            else if (!HasCompletedStoryEvent("first_weave") && type == ObstacleType.WeavePoles)
            {
                TriggerStoryEvent("first_weave");
            }
        }

        private void HandleAchievementUnlocked(string achievementId)
        {
            if (!campaignActive) return;
            CheckChapterUnlocks();
        }

        /// <summary>
        /// Play a rivalry interaction with Marcus
        /// </summary>
        public void PlayMarcusRivalry()
        {
            if (!HasCompletedStoryEvent("marcus_rivalry"))
            {
                TriggerStoryEvent("marcus_rivalry");
                PlayCutscene("marcus_rivalry");
            }
            else
            {
                ImproveRelationship("marcus", 1);
            }
        }

        /// <summary>
        /// Play a rivalry interaction with Emily
        /// </summary>
        public void PlayEmilyRivalry()
        {
            if (!HasCompletedStoryEvent("emily_rivalry"))
            {
                TriggerStoryEvent("emily_rivalry");
                PlayCutscene("emily_rivalry");
            }
            else
            {
                ImproveRelationship("emily", 1);
            }
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class StoryChapterData
    {
        public int chapterNumber;
        public string title;
        public string subtitle;
        public string description;
        public UnlockCondition unlockCondition;
        public List<string> associatedCutscenes;
        public bool isCompleted;
    }

    [Serializable]
    public class UnlockCondition
    {
        public UnlockConditionType type;
        public int value;
        public ShowTier tier;
        public string eventId;
        public string achievementId;
    }

    public enum UnlockConditionType
    {
        GameStart,
        CareerLevel,
        ShowsCompleted,
        TotalWins,
        WinsAtTier,
        WestminsterQualified,
        StoryEventComplete,
        AchievementUnlocked
    }

    [Serializable]
    public class CutsceneData
    {
        public string cutsceneId;
        public string title;
        public List<CampaignDialogueLine> dialogueLines;
    }

    [Serializable]
    public class CampaignDialogueLine
    {
        public string speakerId;
        public string speakerName;
        public string dialogueText;
        public float duration = 3f;
        public string emotion;
        public bool showPortrait = true;
    }

    [Serializable]
    public class CharacterData
    {
        public string characterId;
        public string characterName;
        public string title;
        public Sprite portrait;
        public CharacterPortraitData portraitData;
        public string defaultDialogue;
        public List<string> appearsInChapters;

        /// <summary>
        /// Get portrait for a specific emotion, using portrait data if available
        /// </summary>
        public Sprite GetPortraitForEmotion(string emotion)
        {
            if (portraitData != null)
            {
                return portraitData.GetPortraitForEmotion(emotion);
            }
            return portrait;
        }
    }

    #endregion
}