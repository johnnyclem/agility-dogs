using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// StoryNarrator - Handles story-driven dialogue and narration
    /// Manages character dialogue, story beats, and narrative progression
    /// </summary>
    public class StoryNarrator : MonoBehaviour
    {
        [Header("Story Data")]
        [SerializeField] private StoryData storyData;

        [Header("Display")]
        [SerializeField] private float typeWriterSpeed = 30f;

        // State
        private bool isPlaying = false;
        private DialogueData currentDialogue;
        private Coroutine playbackCoroutine;

        // Events
        public event Action<DialogueData> OnStoryDialogueStarted;
        public event Action<DialogueData> OnStoryDialogueCompleted;
        public event Action<string, string> OnLineDisplayed; // speaker, text

        // Story state tracking
        private Dictionary<string, StoryBeat> storyBeats = new Dictionary<string, StoryBeat>();
        private HashSet<string> completedEvents = new HashSet<string>();

        private void Awake()
        {
            InitializeStoryData();
        }

        private void InitializeStoryData()
        {
            if (storyData == null)
            {
                storyData = CreateDefaultStoryData();
            }

            // Build lookup dictionary
            foreach (var chapter in storyData.chapters)
            {
                foreach (var beat in chapter.beats)
                {
                    storyBeats[beat.id] = beat;
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Play narration for a story event
        /// </summary>
        public void PlayNarration(DialogueData dialogue, int priority = 50)
        {
            if (isPlaying)
            {
                Debug.Log("[StoryNarrator] Already playing, queuing dialogue");
                return;
            }

            currentDialogue = dialogue;
            playbackCoroutine = StartCoroutine(PlayDialogueSequence(dialogue));
        }

        /// <summary>
        /// Get dialogue for a specific event ID
        /// </summary>
        public DialogueData GetDialogueForEvent(string eventId)
        {
            if (storyBeats.TryGetValue(eventId, out StoryBeat beat))
            {
                return beat.dialogue;
            }
            return CreateFallbackDialogue(eventId);
        }

        /// <summary>
        /// Complete current line (for skipping)
        /// </summary>
        public void CompleteCurrentLine()
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                CompleteDialogue(currentDialogue);
            }
        }

        /// <summary>
        /// Check if a story beat has been completed
        /// </summary>
        public bool HasCompleted(string eventId)
        {
            return completedEvents.Contains(eventId);
        }

        /// <summary>
        /// Get current chapter data
        /// </summary>
        public StoryChapter GetCurrentChapter(int chapterNumber)
        {
            return storyData.chapters.Find(c => c.chapterNumber == chapterNumber);
        }

        #endregion

        #region Playback

        private IEnumerator PlayDialogueSequence(DialogueData dialogue)
        {
            isPlaying = true;
            OnStoryDialogueStarted?.Invoke(dialogue);

            foreach (var line in dialogue.lines)
            {
                // Display the line
                OnLineDisplayed?.Invoke(dialogue.speakerName, line.text);

                // Wait for line duration
                yield return new WaitForSeconds(line.duration);

                // Handle skip input
                if (Input.anyKeyDown)
                {
                    break;
                }
            }

            CompleteDialogue(dialogue);
        }

        private void CompleteDialogue(DialogueData dialogue)
        {
            isPlaying = false;
            currentDialogue = null;
            playbackCoroutine = null;

            if (dialogue != null)
            {
                completedEvents.Add(dialogue.id);
                OnStoryDialogueCompleted?.Invoke(dialogue);
            }

            // Notify NarrativeService
            NarrativeService.Instance?.NotifyEventComplete(new NarrativeEvent
            {
                type = NarrativeEventType.Story,
                dialogue = dialogue
            });
        }

        #endregion

        #region Default Story Data

        private StoryData CreateDefaultStoryData()
        {
            var data = ScriptableObject.CreateInstance<StoryData>();
            data.chapters = new List<StoryChapter>();

            // Chapter 1: A New Beginning
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 1,
                title = "A New Beginning",
                subtitle = "Every champion starts somewhere",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_1_intro", "Coach Sarah", new[]
                    {
                        ("Welcome to the world of dog agility! I'm Coach Sarah Chen.", 4f),
                        ("I've been training champions for twenty years. Now it's your turn.", 4f),
                        ("First, let's find you a partner - a dog who matches your spirit.", 4f)
                    }),
                    CreateBeat("chapter_1_puppy_selected", "Coach Sarah", new[]
                    {
                        ("Excellent choice! You can already see the connection forming.", 4f),
                        ("Every great handler-dog team starts with trust. You're off to a good start.", 4f)
                    }),
                    CreateBeat("chapter_1_first_run", "Coach Sarah", new[]
                    {
                        ("Remember, don't worry about speed yet. Focus on clear communication.", 4f),
                        ("Your dog is reading your body language. Be confident and consistent.", 4f)
                    })
                }
            });

            // Chapter 2: Local Legends
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 2,
                title = "Local Legends",
                subtitle = "The crowd goes wild",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_2_intro", "Coach Sarah", new[]
                    {
                        ("Your first competition! The local shows are where legends begin.", 4f),
                        ("You'll meet other handlers here. Some will become rivals, others friends.", 4f),
                        ("Marcus Chen is one to watch - he's been competing for three years.", 4f)
                    }),
                    CreateBeat("marcus_rivalry", "Marcus Chen", new[]
                    {
                        ("Hey rookie! I've heard about you. Coach Sarah doesn't praise just anyone.", 4f),
                        ("May the best team win! But don't expect me to go easy on you.", 4f)
                    }),
                    CreateBeat("chapter_2_victory", "Coach Sarah", new[]
                    {
                        ("Your first victory! I knew you had it in you!", 4f),
                        ("This is just the beginning. The county fair will be much tougher.", 4f)
                    }),
                    CreateBeat("chapter_2_defeat", "Coach Sarah", new[]
                    {
                        ("That was a tough one. But every champion has lost before becoming a champion.", 4f),
                        ("Learn from this. Analyze what went wrong and come back stronger.", 4f)
                    })
                }
            });

            // Chapter 3: Rising Stars
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 3,
                title = "Rising Stars",
                subtitle = "County fair champion",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_3_intro", "Coach Sarah", new[]
                    {
                        ("The County Fair Championships draw serious competitors from all over.", 4f),
                        ("Emily Rodriguez will be there - she's been training for five years.", 4f),
                        ("Don't be intimidated. Use it as motivation to push yourself.", 4f)
                    }),
                    CreateBeat("emily_rivalry", "Emily Rodriguez", new[]
                    {
                        ("So you're the one Coach Sarah has been training.", 4f),
                        ("I've seen your videos. You have potential. Let's see how you perform under pressure.", 4f)
                    }),
                    CreateBeat("emily_respect", "Emily Rodriguez", new[]
                    {
                        ("That was a solid run. You've improved significantly.", 4f),
                        ("Keep training with Coach Sarah. You might actually make it to Regionals.", 4f)
                    }),
                    CreateBeat("chapter_3_coach_backstory", "Coach Sarah", new[]
                    {
                        ("...I once stood where you're standing now. The county fair was my first big win.", 5f),
                        ("Before the injury, I competed at Westminster. That feels like a lifetime ago.", 5f),
                        ("Seeing you progress... it reminds me why I became a coach.", 4f)
                    })
                }
            });

            // Chapter 4: The Regionals
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 4,
                title = "The Regionals",
                subtitle = "Where legends are made",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_4_intro", "Coach Sarah", new[]
                    {
                        ("The Regional Championships. This is where talent meets opportunity.", 4f),
                        ("The courses are longer, the competition fiercer, the stakes higher.", 4f),
                        ("There's someone you should know about - Victoria Price.", 4f)
                    }),
                    CreateBeat("victoria_introduction", "Victoria Price", new[]
                    {
                        ("Coach Sarah speaks highly of you. Let's see if her instincts are sharp.", 5f),
                        ("I won Westminster once, fifteen years ago. The sport has changed since then.", 4f),
                        ("But fundamentals never go out of style. Show me yours.", 4f)
                    }),
                    CreateBeat("chapter_4_marcus_alliance", "Marcus Chen", new[]
                    {
                        ("Hey, we've been rivals but... Victoria's dogs are on another level.", 4f),
                        ("Want to train together before the big day? Two heads are better than one.", 4f)
                    })
                }
            });

            // Chapter 5: State of Mind
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 5,
                title = "State of Mind",
                subtitle = "The art of focus",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_5_intro", "Victoria Price", new[]
                    {
                        ("State championships demand everything you have.", 4f),
                        ("Technical skill alone won't win here. Mental fortitude is key.", 4f),
                        ("Remember: focus beats talent when talent doesn't focus.", 5f)
                    }),
                    CreateBeat("chapter_5_bonding", "Coach Sarah", new[]
                    {
                        ("Look at how your dog responds to you now. The bond has grown.", 4f),
                        ("That's not just training - that's partnership. That's trust.", 4f),
                        ("This connection will carry you further than any technique.", 4f)
                    }),
                    CreateBeat("chapter_5_emily_friendly", "Emily Rodriguez", new[]
                    {
                        ("We both made it here. Not bad for two locals, huh?", 3f),
                        ("Whoever wins today - let's meet at Nationals and do this again.", 4f)
                    })
                }
            });

            // Chapter 6: National Dreams
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 6,
                title = "National Dreams",
                subtitle = "Among the elite",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_6_intro", "Announcer", new[]
                    {
                        ("Ladies and gentlemen, welcome to the National Agility Championships!", 4f),
                        ("Only the best handlers from across the country have earned their place here today.", 5f)
                    }),
                    CreateBeat("chapter_6_marcus_growth", "Marcus Chen", new[]
                    {
                        ("Three years ago I was running practice courses in my backyard.", 4f),
                        ("Now look at us - nationals! Can you believe it?", 3f),
                        ("We've come so far together. Win or lose, this has been amazing.", 4f)
                    }),
                    CreateBeat("chapter_6_coach_secret", "Emily Rodriguez", new[]
                    {
                        ("Coach Sarah... she never tells anyone, but she was a champion once.", 5f),
                        ("Before injuries forced her to retire. She won Regionals three times.", 4f),
                        ("You're living her dream. That's why she's so invested in you.", 5f)
                    }),
                    CreateBeat("chapter_6_realization", "Coach Sarah", new[]
                    {
                        ("I never thought I'd see this day from the coaching side.", 4f),
                        ("Every early morning, every correction, every encouragement - it all led here.", 5f),
                        ("No matter what happens, you've already made me proud.", 4f)
                    })
                }
            });

            // Chapter 7: Westminster Calling
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 7,
                title = "Westminster Calling",
                subtitle = "The dream awaits",
                beats = new List<StoryBeat>
                {
                    CreateBeat("chapter_7_qualification", "Narrator", new[]
                    {
                        ("A letter arrives. The envelope bears the unmistakable crest of Westminster.", 5f),
                        ("Words cannot describe the moment when dreams become reality.", 4f)
                    }),
                    CreateBeat("chapter_7_westminster_reveal", "Coach Sarah", new[]
                    {
                        ("You did it. You've qualified for the Westminster Agility Kings.", 5f),
                        ("This is... this is the championship I never got to win.", 5f),
                        ("Now you'll walk where I once walked. Give it everything.", 5f)
                    }),
                    CreateBeat("chapter_7_rivals_allies", "Victoria Price", new[]
                    {
                        ("At Westminster, the world will be watching.", 4f),
                        ("Everyone who believed in you, everyone who doubted you - they'll all be there.", 5f),
                        ("Don't let the pressure break you. Let it forge you into a champion.", 5f)
                    }),
                    CreateBeat("chapter_7_farewell", "Marcus Chen", new[]
                    {
                        ("This is it. The big one.", 3f),
                        ("We've been through so much together - rivals, then friends.", 4f),
                        ("Go out there and show them what we're made of!", 4f)
                    }),
                    CreateBeat("chapter_7_emily_wish", "Emily Rodriguez", new[]
                    {
                        ("I'll be cheering for you from the sidelines.", 3f),
                        ("Make us all proud. You've got this.", 3f)
                    })
                }
            });

            // Chapter 8: Agility Kings
            data.chapters.Add(new StoryChapter
            {
                chapterNumber = 8,
                title = "Agility Kings",
                subtitle = "Champion of champions",
                beats = new List<StoryBeat>
                {
                    CreateBeat("finale_intro", "Announcer", new[]
                    {
                        ("Ladies and gentlemen, welcome to the WESTMINSTER AGILITY KINGS CHAMPIONSHIP!", 4f),
                        ("In lane one... EMILY RODRIGUEZ with her Golden Retriever, Sunshine!", 4f),
                        ("In lane two... MARCUS CHEN with Border Collie, Storm!", 4f),
                        ("And in lane three... representing everything this sport is about...", 4f)
                    }),
                    CreateBeat("finale_coach_moment", "Coach Sarah", new[]
                    {
                        ("This is it. Everything we've worked for.", 4f),
                        ("Every early morning, every late night, every time we didn't give up.", 5f),
                        ("No matter what happens... you've already made me proud.", 5f),
                        ("Now go out there and show the world what you're made of!", 5f)
                    }),
                    CreateBeat("finale_victory", "Narrator", new[]
                    {
                        ("The crowd erupts. Tears flow freely. A new champion is crowned.", 5f),
                        ("From a puppy's first steps to the grandest stage in dog agility.", 4f),
                        ("A journey of trust, dedication, and unbreakable bonds.", 5f),
                        ("Agility Kings... your journey is complete.", 4f)
                    }),
                    CreateBeat("finale_celebration", "Coach Sarah", new[]
                    {
                        ("You did it... you actually did it!", 4f),
                        ("I'm so proud of you. Words can't express...", 4f),
                        ("Come here. Let's celebrate together.", 3f)
                    }),
                    CreateBeat("finale_credits", "Narrator", new[]
                    {
                        ("And so ends one journey... but another begins.", 5f),
                        ("The bond between handler and dog transcends competition.", 4f),
                        ("Thank you for playing Westminster Agility Masters.", 4f),
                        ("May your journey with your own companions be just as rewarding.", 5f)
                    })
                }
            });

            return data;
        }

        private StoryBeat CreateBeat(string id, string speaker, (string text, float duration)[] lines)
        {
            var dialogue = new DialogueData
            {
                id = id,
                title = id,
                speakerName = speaker,
                lines = new List<DialogueLineData>(),
                averageLineDuration = 3.5f,
                skippable = true
            };

            foreach (var (text, duration) in lines)
            {
                dialogue.lines.Add(new DialogueLineData
                {
                    text = text,
                    duration = duration
                });
            }

            return new StoryBeat
            {
                id = id,
                dialogue = dialogue
            };
        }

        private DialogueData CreateFallbackDialogue(string eventId)
        {
            return new DialogueData
            {
                id = eventId,
                title = eventId,
                speakerName = "Narrator",
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData
                    {
                        text = "The journey continues...",
                        duration = 3f
                    }
                },
                skippable = true
            };
        }

        #endregion
    }

    #region Story Data Structures

    [Serializable]
    public class StoryData
    {
        public List<StoryChapter> chapters = new List<StoryChapter>();
    }

    [Serializable]
    public class StoryChapter
    {
        public int chapterNumber;
        public string title;
        public string subtitle;
        public string description;
        public List<StoryBeat> beats = new List<StoryBeat>();
    }

    [Serializable]
    public class StoryBeat
    {
        public string id;
        public DialogueData dialogue;
        public string triggerEvent;
        public bool skippable = true;
    }

    #endregion
}
