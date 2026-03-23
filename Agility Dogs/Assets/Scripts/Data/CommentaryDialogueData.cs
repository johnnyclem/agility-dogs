using System;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Data
{
    /// <summary>
    /// Commentary state types matching game triggers
    /// </summary>
    public enum CommentaryState
    {
        MatchIntro,         // Match start/idle banter
        WeavePoles,         // Dog in weave poles
        ContactObstacles,   // A-Frame, Dog Walk
        Tunnel,             // Tunnel traversal
        TeeterTotter,       // Seesaw/Teeter
        Jumps,              // Bar, Tire, Broad jumps
        Mistakes,           // Refusals, wrong course, sniffing
        FinishLine,         // Run completion
        General             // Fallback/general commentary
    }

    /// <summary>
    /// Single dialogue line with metadata
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
        
        [Tooltip("Weight for random selection (higher = more likely)")]
        [Range(0.1f, 5f)]
        public float weight = 1f;
        
        [Tooltip("Priority for playback (higher = played first)")]
        [Range(1, 100)]
        public int priority = 50;
        
        [Tooltip("Minimum time before this line can repeat (seconds)")]
        public float cooldown = 30f;
        
        [Tooltip("Optional tags for filtering")]
        public string[] tags;
    }

    /// <summary>
    /// Announcer character data
    /// </summary>
    [Serializable]
    public class AnnouncerData
    {
        [Header("Identity")]
        public string name;
        [TextArea(1, 2)]
        public string description;
        
        [Header("Voice Settings")]
        public string elevenLabsVoiceId = "default";
        [Range(0.5f, 2f)]
        public float pitch = 1f;
        [Range(0.5f, 2f)]
        public float speed = 1f;
        
        [Header("Dialogue Lines by State")]
        public DialogueCollection matchIntroLines;
        public DialogueCollection weavePolesLines;
        public DialogueCollection contactObstacleLines;
        public DialogueCollection tunnelLines;
        public DialogueCollection teeterTotterLines;
        public DialogueCollection jumpLines;
        public DialogueCollection mistakeLines;
        public DialogueCollection finishLineLines;
        public DialogueCollection generalLines;
        
        /// <summary>
        /// Get dialogue collection for a specific state
        /// </summary>
        public DialogueCollection GetLinesForState(CommentaryState state)
        {
            switch (state)
            {
                case CommentaryState.MatchIntro: return matchIntroLines;
                case CommentaryState.WeavePoles: return weavePolesLines;
                case CommentaryState.ContactObstacles: return contactObstacleLines;
                case CommentaryState.Tunnel: return tunnelLines;
                case CommentaryState.TeeterTotter: return teeterTotterLines;
                case CommentaryState.Jumps: return jumpLines;
                case CommentaryState.Mistakes: return mistakeLines;
                case CommentaryState.FinishLine: return finishLineLines;
                case CommentaryState.General: return generalLines;
                default: return generalLines;
            }
        }
    }

    /// <summary>
    /// Collection of dialogue lines for a specific context
    /// </summary>
    [Serializable]
    public class DialogueCollection
    {
        public string name;
        public DialogueLine[] lines;
        
        [HideInInspector]
        public float totalWeight;
        
        /// <summary>
        /// Calculate total weight for weighted random selection
        /// </summary>
        public void CalculateTotalWeight()
        {
            totalWeight = 0f;
            foreach (var line in lines)
            {
                totalWeight += line.weight;
            }
        }
    }

    /// <summary>
    /// ScriptableObject containing all dialogue data
    /// </summary>
    [CreateAssetMenu(fileName = "CommentaryDialogueData", menuName = "Agility Dogs/Commentary Dialogue Data")]
    public class CommentaryDialogueData : ScriptableObject
    {
        [Header("Announcer Characters")]
        public AnnouncerData arthurData;
        public AnnouncerData buckData;
        
        [Header("Dialogue Settings")]
        [Tooltip("Minimum time between dialogue lines")]
        public float minDialogueInterval = 3f;
        
        [Tooltip("Maximum time to wait before forced dialogue")]
        public float maxDialogueInterval = 15f;
        
        [Tooltip("Number of lines to remember to avoid repetition")]
        public int memorySize = 10;
        
        [Tooltip("Cooldown before a line can be repeated (seconds)")]
        public float globalLineCooldown = 60f;
        
        [Header("State Transitions")]
        [Tooltip("Lines to play when transitioning between states")]
        public DialogueLine[] stateTransitionLines;
        
        [Header("Error Fallbacks")]
        public string[] fallbackPhrases = new string[]
        {
            "An interesting approach there.",
            "The crowd watches intently.",
            "The dog shows great focus.",
            "A moment of tension on the course."
        };
        
        /// <summary>
        /// Initialize all dialogue collections
        /// </summary>
        public void Initialize()
        {
            arthurData?.matchIntroLines?.CalculateTotalWeight();
            arthurData?.weavePolesLines?.CalculateTotalWeight();
            arthurData?.contactObstacleLines?.CalculateTotalWeight();
            arthurData?.tunnelLines?.CalculateTotalWeight();
            arthurData?.teeterTotterLines?.CalculateTotalWeight();
            arthurData?.jumpLines?.CalculateTotalWeight();
            arthurData?.mistakeLines?.CalculateTotalWeight();
            arthurData?.finishLineLines?.CalculateTotalWeight();
            arthurData?.generalLines?.CalculateTotalWeight();
            
            buckData?.matchIntroLines?.CalculateTotalWeight();
            buckData?.weavePolesLines?.CalculateTotalWeight();
            buckData?.contactObstacleLines?.CalculateTotalWeight();
            buckData?.tunnelLines?.CalculateTotalWeight();
            buckData?.teeterTotterLines?.CalculateTotalWeight();
            buckData?.jumpLines?.CalculateTotalWeight();
            buckData?.mistakeLines?.CalculateTotalWeight();
            buckData?.finishLineLines?.CalculateTotalWeight();
            buckData?.generalLines?.CalculateTotalWeight();
        }
        
        /// <summary>
        /// Get announcer data by type
        /// </summary>
        public AnnouncerData GetAnnouncerData(AnnouncerType type)
        {
            switch (type)
            {
                case AnnouncerType.Main: return arthurData;
                case AnnouncerType.Color: return buckData;
                default: return arthurData;
            }
        }
    }
}