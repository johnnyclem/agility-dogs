using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// CompetitionCommentator - Handles competition commentary
    /// Features Arthur (Main) and Buck (Color) as dual commentators
    /// </summary>
    public class CompetitionCommentator : MonoBehaviour
    {
        [Header("Commentator Configuration")]
        [SerializeField] private CommentatorData arthurData;
        [SerializeField] private CommentatorData buckData;
        [SerializeField] private CommentatorData paAnnouncerData;

        [Header("Settings")]
        [SerializeField] private float commentaryChance = 0.6f;
        [SerializeField] private float banterChance = 0.3f;
        [SerializeField] private float cooldownTime = 8f;
        [SerializeField] private bool enableBanter = true;

        // State
        private Dictionary<CommentatorType, float> lastCommentaryTime = new Dictionary<CommentatorType, float>();
        private Dictionary<CommentatorType, Queue<string>> recentLines = new Dictionary<CommentatorType, Queue<string>>();
        private Queue<CommentaryItem> commentaryQueue = new Queue<CommentaryItem>();
        private bool isCommentating = false;

        // Events
        public event Action<string, CommentatorType> OnCommentaryStarted;
        public event Action<string, CommentatorType> OnCommentaryCompleted;
        public event Action<string, CommentatorType> OnBanterStarted;

        // Properties
        public bool IsCommentating => isCommentating;

        private void Awake()
        {
            InitializeCommentators();
        }

        private void InitializeCommentators()
        {
            // Initialize Arthur (Main Commentator)
            if (arthurData == null)
            {
                arthurData = CreateArthurData();
            }
            arthurData.Initialize();

            // Initialize Buck (Color Commentator)
            if (buckData == null)
            {
                buckData = CreateBuckData();
            }
            buckData.Initialize();

            // Initialize PA Announcer
            if (paAnnouncerData == null)
            {
                paAnnouncerData = CreatePAAnnouncerData();
            }
            paAnnouncerData.Initialize();

            // Initialize tracking
            foreach (CommentatorType type in Enum.GetValues(typeof(CommentatorType)))
            {
                lastCommentaryTime[type] = 0f;
                recentLines[type] = new Queue<string>();
            }
        }

        #region Public Methods

        /// <summary>
        /// Play commentary for a game state
        /// </summary>
        public void PlayCommentary(DialogueData dialogue)
        {
            if (commentaryQueue.Count > 10) return; // Prevent overflow

            commentaryQueue.Enqueue(new CommentaryItem
            {
                dialogue = dialogue,
                timestamp = Time.time
            });

            if (!isCommentating)
            {
                StartCoroutine(ProcessCommentaryQueue());
            }
        }

        /// <summary>
        /// Get commentary for a specific state
        /// </summary>
        public DialogueData GetCommentaryForState(CommentaryState state)
        {
            // Choose commentator based on state
            CommentatorData commentator = state switch
            {
                CommentaryState.MatchIntro => arthurData,
                CommentaryState.FinishLine => arthurData,
                CommentaryState.Mistakes => buckData,
                _ => UnityEngine.Random.value > 0.5f ? arthurData : buckData
            };

            var lines = commentator.GetLinesForState(state);
            if (lines == null || lines.Count == 0) return null;

            var line = SelectLineWithoutRepetition(commentator.Type, lines);
            if (line == null) return null;

            return new DialogueData
            {
                id = $"commentary_{state}_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = commentator.DisplayName,
                lines = new List<DialogueLineData> { new DialogueLineData { text = line, duration = 3f } },
                speakerPortrait = commentator.portrait
            };
        }

        /// <summary>
        /// Trigger banter between commentators
        /// </summary>
        public void TriggerBanter(CommentaryState context)
        {
            if (!enableBanter || UnityEngine.Random.value > banterChance) return;

            var banterLines = GetBanterLines(context);
            if (banterLines.Count >= 2)
            {
                // Play Arthur's line then Buck's response
                StartCoroutine(PlayBanterSequence(banterLines[0], banterLines[1]));
            }
        }

        /// <summary>
        /// Introduce a competitor
        /// </summary>
        public void IntroduceCompetitor(string competitorName, string dogName, bool isPlayer = false)
        {
            string intro = isPlayer
                ? $"And now, making their debut at the local level... {competitorName} with {dogName}!"
                : $"Next up, {competitorName} with their partner {dogName}.";

            var dialogue = new DialogueData
            {
                id = $"intro_{competitorName}",
                speakerName = arthurData.DisplayName,
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData { text = intro, duration = 4f }
                }
            };

            PlayCommentary(dialogue);
        }

        /// <summary>
        /// Announce a fault
        /// </summary>
        public void AnnounceFault(string faultType, string obstacleName)
        {
            string[] faultLines = {
                $"Oh no! A {faultType} at the {obstacleName}!",
                $"That's going to cost them. {faultType} on {obstacleName}!",
                $"Unfortunate! The {faultType} means extra faults.",
                $"They'll want that one back. {faultType} at {obstacleName}!"
            };

            var dialogue = new DialogueData
            {
                id = $"fault_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = buckData.DisplayName,
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData { text = faultLines[UnityEngine.Random.Range(0, faultLines.Length)], duration = 3f }
                }
            };

            PlayCommentary(dialogue);
        }

        /// <summary>
        /// Announce a good obstacle completion
        /// </summary>
        public void AnnounceGoodObstacle(string obstacleName, float time)
        {
            string[] goodLines = {
                $"Beautiful {obstacleName}! Smooth and fast!",
                $"Look at that {obstacleName}! Textbook technique!",
                $"Excellent form on the {obstacleName}!",
                $"That {obstacleName} was perfectly executed!"
            };

            var dialogue = new DialogueData
            {
                id = $"good_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = arthurData.DisplayName,
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData { text = goodLines[UnityEngine.Random.Range(0, goodLines.Length)], duration = 2.5f }
                }
            };

            PlayCommentary(dialogue);
        }

        /// <summary>
        /// Announce run completion
        /// </summary>
        public void AnnounceRunComplete(bool qualified, float time, int faults)
        {
            string resultLine;

            if (qualified && faults == 0)
            {
                resultLine = $"FLAWLESS! A perfect run in {time:F2} seconds!";
            }
            else if (qualified)
            {
                resultLine = $"Qualified! {faults} faults but they made it through in {time:F2}!";
            }
            else
            {
                resultLine = $"That's a shame. {faults} faults too many for a qualified run.";
            }

            var dialogue = new DialogueData
            {
                id = $"complete_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = arthurData.DisplayName,
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData { text = resultLine, duration = 4f }
                }
            };

            PlayCommentary(dialogue);

            // Buck follows up with analysis
            if (qualified)
            {
                StartCoroutine(DelayedCommentary(2f, buckData, $"That's the kind of performance that wins championships!"));
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator ProcessCommentaryQueue()
        {
            isCommentating = true;

            while (commentaryQueue.Count > 0)
            {
                var item = commentaryQueue.Dequeue();

                if (item.dialogue?.lines != null && item.dialogue.lines.Count > 0)
                {
                    OnCommentaryStarted?.Invoke(item.dialogue.lines[0].text, CommentatorType.Main);

                    yield return new WaitForSeconds(item.dialogue.lines[0].duration);

                    OnCommentaryCompleted?.Invoke(item.dialogue.lines[0].text, CommentatorType.Main);
                }

                yield return new WaitForSeconds(0.5f); // Gap between commentaries
            }

            isCommentating = false;
        }

        private IEnumerator PlayBanterSequence(string line1, string line2)
        {
            OnBanterStarted?.Invoke(line1, CommentatorType.Main);
            yield return new WaitForSeconds(3f);

            OnBanterStarted?.Invoke(line2, CommentatorType.Color);
            yield return new WaitForSeconds(3f);
        }

        private IEnumerator DelayedCommentary(float delay, CommentatorData commentator, string line)
        {
            yield return new WaitForSeconds(delay);

            var dialogue = new DialogueData
            {
                id = $"delayed_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = commentator.DisplayName,
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData { text = line, duration = 3f }
                }
            };

            PlayCommentary(dialogue);
        }

        private string SelectLineWithoutRepetition(CommentatorType type, List<string> lines)
        {
            var recent = recentLines[type];
            var available = lines.Where(l => !recent.Contains(l)).ToList();

            if (available.Count == 0)
            {
                // Reset if all lines have been used
                recent.Clear();
                available = lines;
            }

            if (available.Count == 0) return null;

            string selected = available[UnityEngine.Random.Range(0, available.Count)];

            // Track for anti-repetition
            recent.Enqueue(selected);
            while (recent.Count > 6) recent.Dequeue();

            return selected;
        }

        private List<string> GetBanterLines(CommentaryState context)
        {
            return new List<string>
            {
                $"You know Buck, the {context} is where champions are made.",
                $"Couldn't agree more, Arthur. This is what it's all about!"
            };
        }

        #endregion

        #region Default Commentator Data

        private CommentatorData CreateArthurData()
        {
            var data = ScriptableObject.CreateInstance<CommentatorData>();
            data.commentatorName = "Arthur";
            data.displayName = "Arthur Mitchell";
            data.description = "Main commentator, veteran sportscaster with 25 years experience";
            data.Type = CommentatorType.Main;

            data.matchIntroLines = new List<string>
            {
                "Welcome to another exciting day of dog agility!",
                "The energy here is electric! Let's see what these teams can do!",
                "The crowd is ready, the dogs are eager - let the competition begin!"
            };

            data.finishLines = new List<string>
            {
                "What an incredible performance!",
                "That's championship-level agility right there!",
                "The crowd goes wild! What a finish!"
            };

            data.generalLines = new List<string>
            {
                "Beautiful day for agility!",
                "The bond between handler and dog is clear to see.",
                "This is what we live for!"
            };

            return data;
        }

        private CommentatorData CreateBuckData()
        {
            var data = ScriptableObject.CreateInstance<CommentatorData>();
            data.commentatorName = "Buck";
            data.displayName = "Buck Dawson";
            data.description = "Color commentator, former champion handler";
            data.Type = CommentatorType.Color;

            data.mistakeLines = new List<string>
            {
                "Ooh, that's going to cost them!",
                "The dog hesitated there - handler needs to be clearer.",
                "That's the pressure getting to them. Happens to the best!"
            };

            data.contactLines = new List<string>
            {
                "Perfect contact zone! See how the dog hit the yellow?",
                "That's textbook form right there.",
                "The training really shows on these contact obstacles."
            };

            data.weaveLines = new List<string>
            {
                "Watch the footwork here - beautiful weave pole technique!",
                "Those weave poles are like second nature to this dog.",
                "Speed AND precision in the weaves - that's elite!"
            };

            return data;
        }

        private CommentatorData CreatePAAnnouncerData()
        {
            var data = ScriptableObject.CreateInstance<CommentatorData>();
            data.commentatorName = "PA";
            data.displayName = "Event Announcer";
            data.description = "Public address announcer";
            data.Type = CommentatorType.PA;

            data.generalLines = new List<string>
            {
                "Please welcome our next competitor!",
                "Ladies and gentlemen, your attention please!",
                "The judge is ready. Handler, are you ready?"
            };

            return data;
        }

        #endregion
    }

    #region Data Structures

    public enum CommentatorType
    {
        Main,       // Arthur
        Color,      // Buck
        PA          // PA Announcer
    }

    [Serializable]
    public class CommentatorData
    {
        [Header("Identity")]
        public string commentatorName;
        public string displayName;
        [TextArea(1, 2)]
        public string description;
        public Sprite portrait;

        [Header("Voice Settings")]
        public string voiceId;
        [Range(0.5f, 2f)]
        public float pitch = 1f;
        [Range(0.5f, 2f)]
        public float speed = 1f;

        [Header("Dialogue Lines")]
        public List<string> matchIntroLines = new List<string>();
        public List<string> finishLines = new List<string>();
        public List<string> mistakeLines = new List<string>();
        public List<string> contactLines = new List<string>();
        public List<string> weaveLines = new List<string>();
        public List<string> tunnelLines = new List<string>();
        public List<string> jumpLines = new List<string>();
        public List<string> generalLines = new List<string>();

        [NonSerialized]
        public CommentatorType Type;

        public string DisplayName => displayName ?? commentatorName;

        public void Initialize()
        {
            // Ensure we have at least some lines
            if (generalLines.Count == 0)
            {
                generalLines.Add("Great run so far!");
            }
        }

        public List<string> GetLinesForState(CommentaryState state)
        {
            return state switch
            {
                CommentaryState.MatchIntro => matchIntroLines,
                CommentaryState.FinishLine => finishLines,
                CommentaryState.Mistakes => mistakeLines,
                CommentaryState.ContactObstacles => contactLines,
                CommentaryState.WeavePoles => weaveLines,
                CommentaryState.Tunnel => tunnelLines,
                CommentaryState.Jumps => jumpLines,
                _ => generalLines
            };
        }
    }

    [Serializable]
    public class CommentaryItem
    {
        public DialogueData dialogue;
        public float timestamp;
    }

    #endregion
}
