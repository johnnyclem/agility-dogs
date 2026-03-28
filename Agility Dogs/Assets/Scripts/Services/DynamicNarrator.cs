using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Services;

namespace AgilityDogs.Services
{
    /// <summary>
    /// DynamicNarrator - Generates dynamic narration based on game state
    /// Creates contextual commentary based on player performance and progress
    /// </summary>
    public class DynamicNarrator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minTimeBetweenNarration = 15f;
        [SerializeField] private int maxHistorySize = 20;

        // State
        private float lastNarrationTime;
        private Queue<string> narrationHistory = new Queue<string>();

        // Events
        public event Action<string> OnNarrationPlayed;

        #region Public Methods

        /// <summary>
        /// Get narration based on a trigger
        /// </summary>
        public DialogueData GetNarration(NarrativeTrigger trigger)
        {
            if (Time.time - lastNarrationTime < minTimeBetweenNarration)
                return null;

            string narration = trigger.type switch
            {
                TriggerType.PerfectRun => GetPerfectRunNarration(trigger.value),
                TriggerType.Fault => GetFaultNarration(trigger.fault, trigger.obstacle),
                TriggerType.NearMiss => GetNearMissNarration(),
                TriggerType.PersonalBest => GetPersonalBestNarration(trigger.value),
                TriggerType.ShowStart => GetShowStartNarration(),
                TriggerType.ShowEnd => GetShowEndNarration(trigger.result),
                TriggerType.MilestoneReached => GetMilestoneNarration(trigger.characterId),
                TriggerType.LevelUp => GetLevelUpNarration(),
                _ => GetGeneralNarration()
            };

            if (string.IsNullOrEmpty(narration)) return null;

            // Check for repetition
            if (narrationHistory.Contains(narration)) return null;

            // Track history
            narrationHistory.Enqueue(narration);
            while (narrationHistory.Count > maxHistorySize)
                narrationHistory.Dequeue();

            lastNarrationTime = Time.time;

            return new DialogueData
            {
                id = $"dynamic_{trigger.type}_{UnityEngine.Random.Range(1000, 9999)}",
                speakerName = GetSpeakerForTrigger(trigger.type),
                lines = new List<DialogueLineData>
                {
                    new DialogueLineData
                    {
                        text = narration,
                        duration = Mathf.Max(2.5f, narration.Length * 0.08f)
                    }
                },
                skippable = true
            };
        }

        public void PlayNarration(DialogueData dialogue)
        {
            if (dialogue == null) return;
            Debug.Log($"[DynamicNarrator] Playing narration: {dialogue.id}");
            OnNarrationPlayed?.Invoke(dialogue.id);
        }

        /// <summary>
        /// Complete current line (for skipping)
        /// </summary>
        public void CompleteCurrentLine()
        {
            lastNarrationTime = 0f;
        }

        #endregion

        #region Narration Generation

        private string GetPerfectRunNarration(float time)
        {
            string[] lines = {
                $"A perfect run in {time:F2} seconds! That's championship material!",
                "Flawless! Not a single fault! The dog and handler moved as one!",
                $"That's {time:F2} seconds of pure perfection. Incredible!",
                "Absolutely stunning! Zero faults and a blazing fast time!",
                "That's the kind of run that makes history! Perfect execution!"
            };
            return GetRandomUnique(lines);
        }

        private string GetFaultNarration(FaultType fault, string obstacle)
        {
            string[] lines = {
                $"A {fault} at the {obstacle}. That's going to add to the score.",
                $"The {fault} at {obstacle} wasn't ideal, but there's still time to recover.",
                $"Oh! {fault} on {obstacle}! Every second counts now.",
                $"That {fault} at the {obstacle} - the handler needs to stay focused.",
                $"Miscommunication at {obstacle}. {fault}! But they can make up time."
            };
            return GetRandomUnique(lines);
        }

        private string GetNearMissNarration()
        {
            string[] lines = {
                "So close! That could have been a fault!",
                "Barely made it! That's the kind of edge-of-your-seat agility we love!",
                "A near miss! The dog's reflexes saved them there!",
                "Whew! That was dangerously close to a fault!",
                "Heart-stopping moment! But they pulled through!"
            };
            return GetRandomUnique(lines);
        }

        private string GetPersonalBestNarration(float time)
        {
            string[] lines = {
                $"Personal best! {time:F2} seconds! You're getting faster every run!",
                $"A new record for you! {time:F2} seconds - that's amazing progress!",
                $"Incredible! Your best time yet - {time:F2}!",
                $"That's a personal best! All that training is paying off!",
                $"New personal record! {time:F2}! You should be proud!"
            };
            return GetRandomUnique(lines);
        }

        private string GetShowStartNarration()
        {
            string[] lines = {
                "Here we go! Another exciting competition awaits!",
                "The crowd is ready. The dogs are eager. Let's see what happens!",
                "Another opportunity to shine. Deep breath - you've got this!",
                "The competition is fierce today, but so is your determination!",
                "Time to show what you and your partner can do!"
            };
            return GetRandomUnique(lines);
        }

        private string GetShowEndNarration(ShowResult result)
        {
            return result switch
            {
                ShowResult.FirstPlace => "First place! A well-deserved victory!",
                ShowResult.BestInShow => "Best in Show! The judges have spoken!",
                ShowResult.SecondPlace => "Second place! So close to the top!",
                ShowResult.ThirdPlace => "Third place! On the podium - that's an achievement!",
                ShowResult.HonorableMention => "Honorable mention! You're making progress!",
                _ => "Every competition is a learning experience. Keep pushing!"
            };
        }

        private string GetMilestoneNarration(string milestone)
        {
            string[] lines = {
                "A new milestone reached! The journey continues!",
                "You're making incredible progress! Keep it up!",
                "Another achievement unlocked! Your dedication is paying off!",
                "Milestone achieved! Every step forward counts!",
                "Look how far you've come! Amazing progress!"
            };
            return GetRandomUnique(lines);
        }

        private string GetLevelUpNarration()
        {
            string[] lines = {
                "Level up! You're getting better every day!",
                "New level reached! Your skills are improving!",
                "You've leveled up! Time to take on bigger challenges!",
                "Level up! The hard work is paying off!",
                "Another level! You're becoming a true competitor!"
            };
            return GetRandomUnique(lines);
        }

        private string GetGeneralNarration()
        {
            string[] lines = {
                "What a beautiful day for agility!",
                "The bond between handler and dog is special to watch.",
                "Every run teaches you something new.",
                "Keep your focus. Trust your partner. You've got this.",
                "The crowd is captivated by your performance!"
            };
            return GetRandomUnique(lines);
        }

        private string GetSpeakerForTrigger(TriggerType trigger)
        {
            return trigger switch
            {
                TriggerType.PerfectRun => "Announcer",
                TriggerType.Fault => "Color Commentator",
                TriggerType.PersonalBest => "Coach Sarah",
                TriggerType.ShowStart => "Announcer",
                TriggerType.ShowEnd => "Announcer",
                TriggerType.MilestoneReached => "Coach Sarah",
                TriggerType.LevelUp => "Coach Sarah",
                _ => "Narrator"
            };
        }

        private string GetRandomUnique(string[] lines)
        {
            var available = lines.Where(l => !narrationHistory.Contains(l)).ToList();
            if (available.Count == 0)
            {
                narrationHistory.Clear();
                available = lines.ToList();
            }
            return available[UnityEngine.Random.Range(0, available.Count)];
        }

        #endregion
    }
}
