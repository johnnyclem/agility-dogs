using System.Collections.Generic;
using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "AchievementDefinitions", menuName = "Agility Dogs/Achievement Definitions")]
    public class AchievementDefinitionCollection : ScriptableObject
    {
        public List<AchievementData> achievements = new List<AchievementData>();

        [Header("Categories")]
        public List<AchievementCategoryGroup> categoryGroups = new List<AchievementCategoryGroup>();

        public AchievementData GetAchievement(string achievementId)
        {
            return achievements.Find(a => a.achievementId == achievementId);
        }

        public List<AchievementData> GetAchievementsByCategory(AchievementCategory category)
        {
            return achievements.FindAll(a => a.category == category);
        }

        public List<AchievementData> GetAchievementsByRarity(AchievementRarity rarity)
        {
            return achievements.FindAll(a => a.rarity == rarity);
        }

        public List<AchievementData> GetVisibleAchievements()
        {
            return achievements.FindAll(a => !a.isHiddenUntilUnlocked);
        }

        public int GetTotalWingsReward()
        {
            int total = 0;
            foreach (var achievement in achievements)
            {
                total += achievement.wingsReward;
            }
            return total;
        }

        public int GetTotalXPReward()
        {
            int total = 0;
            foreach (var achievement in achievements)
            {
                total += achievement.xpReward;
            }
            return total;
        }
    }

    [System.Serializable]
    public class AchievementCategoryGroup
    {
        public AchievementCategory category;
        public string displayName;
        public Sprite icon;
        public Color color = Color.white;
        public string description;
    }
}
