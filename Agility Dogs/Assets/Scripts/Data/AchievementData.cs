using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewAchievementData", menuName = "Agility Dogs/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        [Header("Achievement Info")]
        public string achievementId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Visual")]
        public Sprite icon;
        public Color glowColor = new Color(1f, 0.84f, 0f, 1f);

        [Header("Category")]
        public AchievementCategory category = AchievementCategory.General;

        [Header("Rewards")]
        public int wingsReward = 50;
        public int xpReward = 100;

        [Header("Rarity")]
        public AchievementRarity rarity = AchievementRarity.Common;

        [Header("Progression")]
        public bool isSecret = false;
        public bool isHiddenUntilUnlocked = false;
        public string[] prerequisites;

        [Header("Localization")]
        public string localizationKey;
    }

    public enum AchievementCategory
    {
        General,
        Competition,
        Progression,
        Collection,
        Mastery,
        Special
    }

    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
