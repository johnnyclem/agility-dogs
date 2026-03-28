using System;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// TutorialNarrator - Manages tutorial dialogue and hints
    /// Provides contextual guidance based on player progress
    /// </summary>
    public class TutorialNarrator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float tutorialDelay = 2f;
        [SerializeField] private bool enableTutorials = true;

        // Tutorial data
        private Dictionary<string, TutorialData> tutorials = new Dictionary<string, TutorialData>();
        private HashSet<string> completedTutorials = new HashSet<string>();
        private Queue<string> tutorialQueue = new Queue<string>();

        // Events
        public event Action<TutorialData> OnTutorialStarted;
        public event Action<TutorialData> OnTutorialCompleted;

        private void Awake()
        {
            InitializeTutorials();
        }

        private void InitializeTutorials()
        {
            // Basic controls
            RegisterTutorial(new TutorialData
            {
                id = "controls_basics",
                title = "Basic Controls",
                trigger = TutorialTrigger.GameStart,
                dialogue = new DialogueData
                {
                    id = "tutorial_controls",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Let me show you the basics.", duration = 2f },
                        new DialogueLineData { text = "Use arrow keys or WASD to move your handler.", duration = 3f },
                        new DialogueLineData { text = "Press SPACE near obstacles to send your dog through.", duration = 3f },
                        new DialogueLineData { text = "Watch the green indicator - it shows when you can command!", duration = 3f }
                    }
                },
                priority = 100
            });

            // First jump
            RegisterTutorial(new TutorialData
            {
                id = "first_jump",
                title = "Jumps",
                trigger = TutorialTrigger.ObstacleFirstTime,
                obstacleType = ObstacleType.BarJump,
                dialogue = new DialogueData
                {
                    id = "tutorial_jump",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Jumps are the foundation of agility.", duration = 2.5f },
                        new DialogueLineData { text = "Time your command as your dog approaches the jump.", duration = 3f },
                        new DialogueLineData { text = "A clean jump means no knocked bars - that's the goal!", duration = 3f }
                    }
                },
                priority = 90
            });

            // First tunnel
            RegisterTutorial(new TutorialData
            {
                id = "first_tunnel",
                title = "Tunnels",
                trigger = TutorialTrigger.ObstacleFirstTime,
                obstacleType = ObstacleType.Tunnel,
                dialogue = new DialogueData
                {
                    id = "tutorial_tunnel",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Tunnels test your dog's confidence.", duration = 2.5f },
                        new DialogueLineData { text = "Give the command early so they know where to go!", duration = 3f },
                        new DialogueLineData { text = "Fast tunnel times can make or break a run.", duration = 3f }
                    }
                },
                priority = 90
            });

            // First weave poles
            RegisterTutorial(new TutorialData
            {
                id = "first_weave",
                title = "Weave Poles",
                trigger = TutorialTrigger.ObstacleFirstTime,
                obstacleType = ObstacleType.WeavePoles,
                dialogue = new DialogueData
                {
                    id = "tutorial_weave",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "The weave poles are the hardest obstacle to master.", duration = 3f },
                        new DialogueLineData { text = "Your dog must enter on the left side of the first pole.", duration = 3f },
                        new DialogueLineData { text = "Speed comes with practice. Focus on proper entry first.", duration = 3f }
                    }
                },
                priority = 95
            });

            // First A-frame
            RegisterTutorial(new TutorialData
            {
                id = "first_aframe",
                title = "Contact Obstacles",
                trigger = TutorialTrigger.ObstacleFirstTime,
                obstacleType = ObstacleType.AFrame,
                dialogue = new DialogueData
                {
                    id = "tutorial_contact",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Contact obstacles require precision.", duration = 2.5f },
                        new DialogueLineData { text = "Your dog must touch the yellow contact zone on the way down.", duration = 3.5f },
                        new DialogueLineData { text = "Missing the contact means faults - don't rush it!", duration = 3f }
                    }
                },
                priority = 90
            });

            // First fault
            RegisterTutorial(new TutorialData
            {
                id = "first_fault",
                title = "Faults",
                trigger = TutorialTrigger.Fault,
                dialogue = new DialogueData
                {
                    id = "tutorial_fault",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Don't worry about that fault.", duration = 2f },
                        new DialogueLineData { text = "Everyone makes mistakes. What matters is how you recover.", duration = 3f },
                        new DialogueLineData { text = "Stay calm and focus on the next obstacle.", duration = 2.5f }
                    }
                },
                priority = 85
            });

            // First competition
            RegisterTutorial(new TutorialData
            {
                id = "first_competition",
                title = "Your First Competition",
                trigger = TutorialTrigger.CompetitionStart,
                dialogue = new DialogueData
                {
                    id = "tutorial_competition",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "This is it - your first real competition!", duration = 3f },
                        new DialogueLineData { text = "Don't think about winning or losing.", duration = 2.5f },
                        new DialogueLineData { text = "Just enjoy the run with your partner. That's what matters.", duration = 3.5f },
                        new DialogueLineData { text = "I'll be watching and cheering for you. Good luck!", duration = 3f }
                    }
                },
                priority = 100
            });

            // First win
            RegisterTutorial(new TutorialData
            {
                id = "first_win",
                title = "First Victory!",
                trigger = TutorialTrigger.FirstWin,
                dialogue = new DialogueData
                {
                    id = "tutorial_win",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Your first victory! I'm so proud of you!", duration = 3f },
                        new DialogueLineData { text = "You and your dog worked as a team out there.", duration = 3f },
                        new DialogueLineData { text = "This is just the beginning. Keep training and improving!", duration = 3f }
                    }
                },
                priority = 100
            });

            // Training basics
            RegisterTutorial(new TutorialData
            {
                id = "training_basics",
                title = "Training Mode",
                trigger = TutorialTrigger.TrainingStart,
                dialogue = new DialogueData
                {
                    id = "tutorial_training",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Training mode is where champions are made.", duration = 3f },
                        new DialogueLineData { text = "Practice obstacles without the pressure of competition.", duration = 3f },
                        new DialogueLineData { text = "Take your time. Speed will come naturally with experience.", duration = 3f }
                    }
                },
                priority = 95
            });

            // Breeding basics
            RegisterTutorial(new TutorialData
            {
                id = "breeding_basics",
                title = "Breeding Your Puppy",
                trigger = TutorialTrigger.BreedingStart,
                dialogue = new DialogueData
                {
                    id = "tutorial_breeding",
                    speakerName = "Coach Sarah",
                    lines = new List<DialogueLineData>
                    {
                        new DialogueLineData { text = "Each breed has unique strengths and tendencies.", duration = 3f },
                        new DialogueLineData { text = "Speed, stamina, and trainability all factor in.", duration = 3f },
                        new DialogueLineData { text = "Choose a puppy that matches your handling style.", duration = 3f }
                    }
                },
                priority = 100
            });
        }

        public void PlayTutorial(DialogueData dialogue)
        {
            if (dialogue == null) return;
            Debug.Log($"[TutorialNarrator] Playing tutorial: {dialogue.title ?? dialogue.id}");
            if (tutorials.TryGetValue(dialogue.id, out TutorialData tutorial))
            {
                OnTutorialStarted?.Invoke(tutorial);
            }
        }

        #region Public Methods

        /// <summary>
        /// Get tutorial dialogue for a specific ID
        /// </summary>
        public DialogueData GetTutorialDialogue(string tutorialId)
        {
            if (!enableTutorials) return null;
            if (completedTutorials.Contains(tutorialId)) return null;

            if (tutorials.TryGetValue(tutorialId, out TutorialData tutorial))
            {
                return tutorial.dialogue;
            }

            return null;
        }

        /// <summary>
        /// Trigger tutorial based on game event
        /// </summary>
        public void TriggerTutorial(TutorialTrigger trigger, object context = null)
        {
            if (!enableTutorials) return;

            foreach (var tutorial in tutorials.Values)
            {
                if (tutorial.trigger != trigger) continue;
                if (completedTutorials.Contains(tutorial.id)) continue;

                // Check obstacle-specific trigger
                if (trigger == TutorialTrigger.ObstacleFirstTime && context is ObstacleType obstacleType)
                {
                    if (tutorial.obstacleType != obstacleType) continue;
                }

                // Queue the tutorial
                tutorialQueue.Enqueue(tutorial.id);
            }
        }

        /// <summary>
        /// Mark a tutorial as completed
        /// </summary>
        public void CompleteTutorial(string tutorialId)
        {
            if (tutorials.TryGetValue(tutorialId, out TutorialData tutorial))
            {
                completedTutorials.Add(tutorialId);
                OnTutorialCompleted?.Invoke(tutorial);
            }
        }

        /// <summary>
        /// Check if tutorial has been completed
        /// </summary>
        public bool IsTutorialCompleted(string tutorialId)
        {
            return completedTutorials.Contains(tutorialId);
        }

        /// <summary>
        /// Reset all tutorials
        /// </summary>
        public void ResetTutorials()
        {
            completedTutorials.Clear();
            tutorialQueue.Clear();
        }

        /// <summary>
        /// Enable or disable tutorials
        /// </summary>
        public void SetTutorialsEnabled(bool enabled)
        {
            enableTutorials = enabled;
        }

        #endregion

        #region Private Methods

        private void RegisterTutorial(TutorialData tutorial)
        {
            tutorials[tutorial.id] = tutorial;
        }

        #endregion
    }

    #region Data Structures

    public enum TutorialTrigger
    {
        GameStart,
        TrainingStart,
        BreedingStart,
        CompetitionStart,
        ObstacleFirstTime,
        Fault,
        FirstWin,
        LevelUp,
        MilestoneReached
    }

    [Serializable]
    public class TutorialData
    {
        public string id;
        public string title;
        public TutorialTrigger trigger;
        public ObstacleType obstacleType;
        public DialogueData dialogue;
        public int priority = 50;
        public float delay = 2f;
        public bool skippable = true;
    }

    #endregion
}
