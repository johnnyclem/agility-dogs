using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Data
{
    /// <summary>
    /// Generic dialogue pool using "bag of clips" pattern for anti-repetition
    /// Implements AAA polish: deplete pool, refill when empty, avoid consecutive repeats
    /// </summary>
    [System.Serializable]
    public class DialoguePool
    {
        [Header("Pool Configuration")]
        [Tooltip("The master list of dialogue lines for this pool")]
        public List<DialogueLineEntry> sourceLines = new List<DialogueLineEntry>();
        
        [Tooltip("Optional weights for each line (same order as sourceLines)")]
        public List<float> weights = new List<float>();
        
        // Internal pool we draw from and deplete
        private List<DialogueLineEntry> availableLines = new List<DialogueLineEntry>();
        private DialogueLineEntry lastPlayedLine;
        
        /// <summary>
        /// Initialize the pool (call on Awake/Start)
        /// </summary>
        public void Initialize()
        {
            RefillPool();
        }
        
        /// <summary>
        /// Get the next dialogue line using bag-of-clips pattern
        /// </summary>
        public DialogueLineEntry GetNextLine()
        {
            if (sourceLines == null || sourceLines.Count == 0)
                return null;
            
            // Refill if empty
            if (availableLines.Count == 0)
            {
                RefillPool();
            }
            
            // Weighted random selection from available lines
            int selectedIndex = GetWeightedRandomIndex();
            DialogueLineEntry selectedLine = availableLines[selectedIndex];
            
            // AAA Polish: Avoid playing same line twice in a row after refill
            if (selectedLine == lastPlayedLine && sourceLines.Count > 1)
            {
                selectedIndex = (selectedIndex + 1) % availableLines.Count;
                selectedLine = availableLines[selectedIndex];
            }
            
            // Remove from available pool and save as last played
            availableLines.RemoveAt(selectedIndex);
            lastPlayedLine = selectedLine;
            
            return selectedLine;
        }
        
        /// <summary>
        /// Get weighted random index from available lines
        /// </summary>
        private int GetWeightedRandomIndex()
        {
            if (availableLines.Count == 0) return 0;
            
            // If no weights provided, use uniform distribution
            if (weights == null || weights.Count != sourceLines.Count)
            {
                return Random.Range(0, availableLines.Count);
            }
            
            // Calculate total weight of available lines
            float totalWeight = 0f;
            foreach (var line in availableLines)
            {
                int sourceIndex = sourceLines.IndexOf(line);
                if (sourceIndex >= 0 && sourceIndex < weights.Count)
                {
                    totalWeight += weights[sourceIndex];
                }
                else
                {
                    totalWeight += 1f;
                }
            }
            
            // Select based on weight
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            for (int i = 0; i < availableLines.Count; i++)
            {
                var line = availableLines[i];
                int sourceIndex = sourceLines.IndexOf(line);
                float lineWeight = (sourceIndex >= 0 && sourceIndex < weights.Count) 
                    ? weights[sourceIndex] 
                    : 1f;
                
                currentWeight += lineWeight;
                if (randomValue <= currentWeight)
                {
                    return i;
                }
            }
            
            return availableLines.Count - 1;
        }
        
        /// <summary>
        /// Refill the available lines pool
        /// </summary>
        private void RefillPool()
        {
            availableLines.Clear();
            availableLines.AddRange(sourceLines);
            
            Debug.Log($"[DialoguePool] Refilled pool with {availableLines.Count} lines");
        }
        
        /// <summary>
        /// Check if pool is empty
        /// </summary>
        public bool IsEmpty()
        {
            return availableLines.Count == 0;
        }
        
        /// <summary>
        /// Get count of remaining lines
        /// </summary>
        public int GetRemainingCount()
        {
            return availableLines.Count;
        }
        
        /// <summary>
        /// Reset the pool (refill and clear last played)
        /// </summary>
        public void Reset()
        {
            RefillPool();
            lastPlayedLine = null;
        }
        
        /// <summary>
        /// Add a line to the source pool dynamically
        /// </summary>
        public void AddLine(DialogueLineEntry line, float weight = 1f)
        {
            if (line == null) return;
            
            sourceLines.Add(line);
            weights.Add(weight);
        }
        
        /// <summary>
        /// Remove a line from the source pool
        /// </summary>
        public bool RemoveLine(DialogueLineEntry line)
        {
            int index = sourceLines.IndexOf(line);
            if (index >= 0)
            {
                sourceLines.RemoveAt(index);
                if (index < weights.Count)
                {
                    weights.RemoveAt(index);
                }
                return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Single dialogue line entry with text and metadata
    /// </summary>
    [System.Serializable]
    public class DialogueLineEntry
    {
        [TextArea(2, 4)]
        public string text;
        
        [Tooltip("Announcer this line belongs to")]
        public AnnouncerType announcerType;
        
        [Tooltip("Optional voice ID override")]
        public string voiceIdOverride = "";
        
        [Tooltip("Tags for filtering/categorization")]
        public string[] tags;
        
        [Tooltip("Minimum priority (higher = more important)")]
        [Range(1, 100)]
        public int priority = 50;
        
        /// <summary>
        /// Constructor for quick creation
        /// </summary>
        public DialogueLineEntry(string text, AnnouncerType announcerType)
        {
            this.text = text;
            this.announcerType = announcerType;
        }
    }
}