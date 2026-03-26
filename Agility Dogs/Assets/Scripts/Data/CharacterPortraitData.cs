using System.Collections.Generic;
using UnityEngine;

namespace AgilityDogs.Data
{
    /// <summary>
    /// CharacterPortraitData - Stores portrait sprites and emotion variants for characters
    /// Create these as ScriptableObject assets for each character
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterPortraits", menuName = "Agility Dogs/Character Portraits")]
    public class CharacterPortraitData : ScriptableObject
    {
        [Header("Character Info")]
        public string characterId;
        public string characterName;

        [Header("Portraits")]
        public Sprite defaultPortrait;
        public Sprite happyPortrait;
        public Sprite sadPortrait;
        public Sprite excitedPortrait;
        public Sprite competitivePortrait;
        public Sprite thoughtfulPortrait;
        public Sprite emotionalPortrait;
        public Sprite authoritativePortrait;
        public Sprite friendlyPortrait;
        public Sprite triumphantPortrait;

        [Header("Emotion Mapping")]
        [Tooltip("Maps emotion strings to portrait sprites for dynamic portrait selection")]
        public List<EmotionPortraitPair> emotionPortraits = new List<EmotionPortraitPair>();

        /// <summary>
        /// Get portrait for a specific emotion
        /// </summary>
        public Sprite GetPortraitForEmotion(string emotion)
        {
            if (string.IsNullOrEmpty(emotion))
                return defaultPortrait;

            // Find matching emotion portrait
            foreach (var pair in emotionPortraits)
            {
                if (pair.emotion.Equals(emotion, System.StringComparison.OrdinalIgnoreCase))
                {
                    return pair.portrait ?? defaultPortrait;
                }
            }

            // Fallback to emotion-specific portraits
            return emotion.ToLower() switch
            {
                "happy" or "proud" or "gracious" or "approving" => happyPortrait ?? defaultPortrait,
                "sad" or "melancholy" or "nostalgic" => sadPortrait ?? defaultPortrait,
                "excited" or "ecstatic" or "thrilling" => excitedPortrait ?? defaultPortrait,
                "competitive" or "fierce" or "determined" => competitivePortrait ?? defaultPortrait,
                "thoughtful" or "reflective" or "wise" => thoughtfulPortrait ?? defaultPortrait,
                "emotional" or "tears" or "overwhelmed" => emotionalPortrait ?? defaultPortrait,
                "authoritative" or "stern" or "serious" => authoritativePortrait ?? defaultPortrait,
                "friendly" or "warm" or "amused" or "brotherly" => friendlyPortrait ?? defaultPortrait,
                "triumphant" or "awe" => triumphantPortrait ?? defaultPortrait,
                _ => defaultPortrait
            };
        }
    }

    [System.Serializable]
    public class EmotionPortraitPair
    {
        public string emotion;
        public Sprite portrait;
    }
}
