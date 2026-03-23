using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgilityDogs.Data
{
    /// <summary>
    /// State-specific dialogue pool for a single announcer
    /// </summary>
    [Serializable]
    public class CommentaryStatePool
    {
        [Header("State Configuration")]
        public CommentaryState state;
        
        [Header("Arthur's Lines (Play-by-Play)")]
        public DialoguePool arthurPool = new DialoguePool();
        
        [Header("Buck's Lines (Color Commentary)")]
        public DialoguePool buckPool = new DialoguePool();
        
        [Header("Timing Settings")]
        [Tooltip("Delay before Arthur speaks after entering this state")]
        public float arthurDelay = 0f;
        
        [Tooltip("Delay before Buck speaks (after Arthur, if both speak)")]
        public float buckDelay = 0.5f;
        
        [Tooltip("Chance Buck will comment in this state (0-1)")]
        [Range(0f, 1f)]
        public float buckChance = 0.4f;
        
        /// <summary>
        /// Initialize both pools
        /// </summary>
        public void Initialize()
        {
            arthurPool?.Initialize();
            buckPool?.Initialize();
        }
        
        /// <summary>
        /// Reset both pools
        /// </summary>
        public void Reset()
        {
            arthurPool?.Reset();
            buckPool?.Reset();
        }
        
        /// <summary>
        /// Check if any lines remain
        /// </summary>
        public bool HasLinesRemaining()
        {
            return (arthurPool?.GetRemainingCount() ?? 0) > 0 || 
                   (buckPool?.GetRemainingCount() ?? 0) > 0;
        }
    }
    
    /// <summary>
    /// ScriptableObject containing all Arthur and Buck dialogue lines
    /// Organized by commentary state using pool pattern
    /// </summary>
    [CreateAssetMenu(fileName = "BestInShowDialogue", menuName = "Agility Dogs/Best In Show Dialogue")]
    public class BestInShowDialogue : ScriptableObject
    {
        [Header("Arthur Pendelton (Play-by-Play)")]
        [TextArea(2, 4)]
        public string arthurDescription = "Deadpan, intensely knowledgeable about dog agility. Uses real terms like 'front crosses,' 'contact zones,' and 'refusals.' Constantly trying to steer the broadcast back to reality.";
        
        [Header("Buck Hastings (Color Commentator)")]
        [TextArea(2, 4)]
        public string buckDescription = "Aggressively ignorant, wildly off-topic, and overly confident. Thinks he knows everything but knows nothing.";
        
        [Header("Match Intro / Idle Banter")]
        public CommentaryStatePool matchIntroPool;
        
        [Header("The Weave Poles (Slalom)")]
        public CommentaryStatePool weavePolesPool;
        
        [Header("Contact Obstacles (A-Frame & Dog Walk)")]
        public CommentaryStatePool contactObstaclesPool;
        
        [Header("The Tunnel")]
        public CommentaryStatePool tunnelPool;
        
        [Header("The Teeter-Totter (Seesaw)")]
        public CommentaryStatePool teeterTotterPool;
        
        [Header("Jumps (Bar, Tire, Broad)")]
        public CommentaryStatePool jumpsPool;
        
        [Header("Mistakes, Wandering, Refusals")]
        public CommentaryStatePool mistakesPool;
        
        [Header("Finish Line / Match Outro")]
        public CommentaryStatePool finishLinePool;
        
        /// <summary>
        /// Get pool for a specific state
        /// </summary>
        public CommentaryStatePool GetPoolForState(CommentaryState state)
        {
            switch (state)
            {
                case CommentaryState.MatchIntro: return matchIntroPool;
                case CommentaryState.WeavePoles: return weavePolesPool;
                case CommentaryState.ContactObstacles: return contactObstaclesPool;
                case CommentaryState.Tunnel: return tunnelPool;
                case CommentaryState.TeeterTotter: return teeterTotterPool;
                case CommentaryState.Jumps: return jumpsPool;
                case CommentaryState.Mistakes: return mistakesPool;
                case CommentaryState.FinishLine: return finishLinePool;
                case CommentaryState.General: return matchIntroPool; // Fallback
                default: return matchIntroPool;
            }
        }
        
        /// <summary>
        /// Initialize all pools
        /// </summary>
        public void InitializeAllPools()
        {
            matchIntroPool?.Initialize();
            weavePolesPool?.Initialize();
            contactObstaclesPool?.Initialize();
            tunnelPool?.Initialize();
            teeterTotterPool?.Initialize();
            jumpsPool?.Initialize();
            mistakesPool?.Initialize();
            finishLinePool?.Initialize();
        }
        
        /// <summary>
        /// Reset all pools (refill all bags)
        /// </summary>
        public void ResetAllPools()
        {
            matchIntroPool?.Reset();
            weavePolesPool?.Reset();
            contactObstaclesPool?.Reset();
            tunnelPool?.Reset();
            teeterTotterPool?.Reset();
            jumpsPool?.Reset();
            mistakesPool?.Reset();
            finishLinePool?.Reset();
        }
        
        /// <summary>
        /// Get total line count across all pools
        /// </summary>
        public int GetTotalLineCount()
        {
            int count = 0;
            count += matchIntroPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += matchIntroPool?.buckPool?.sourceLines?.Count ?? 0;
            count += weavePolesPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += weavePolesPool?.buckPool?.sourceLines?.Count ?? 0;
            count += contactObstaclesPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += contactObstaclesPool?.buckPool?.sourceLines?.Count ?? 0;
            count += tunnelPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += tunnelPool?.buckPool?.sourceLines?.Count ?? 0;
            count += teeterTotterPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += teeterTotterPool?.buckPool?.sourceLines?.Count ?? 0;
            count += jumpsPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += jumpsPool?.buckPool?.sourceLines?.Count ?? 0;
            count += mistakesPool?.arthurPool?.sourceLines?.Count ?? 0;
            count += mistakesPool?.buckPool?.sourceLines?.Count ?? 0;
            count += finishLinePool?.arthurPool?.sourceLines?.Count ?? 0;
            count += finishLinePool?.buckPool?.sourceLines?.Count ?? 0;
            return count;
        }
    }
}