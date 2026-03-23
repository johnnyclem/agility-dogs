using System.Collections.Generic;
using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewSkillTreeData", menuName = "Agility Dogs/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        [Header("Tree Info")]
        public string treeId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Tree Type")]
        public SkillTreeType treeType = SkillTreeType.Handler;

        [Header("Skills")]
        public List<SkillDefinition> tier1Skills = new List<SkillDefinition>();
        public List<SkillDefinition> tier2Skills = new List<SkillDefinition>();
        public List<SkillDefinition> tier3Skills = new List<SkillDefinition>();
        public List<SkillDefinition> tier4Skills = new List<SkillDefinition>();

        [Header("Visual")]
        public Color treeColor = Color.blue;
        public Sprite treeIcon;

        public List<SkillDefinition> GetAllSkills()
        {
            var allSkills = new List<SkillDefinition>();
            allSkills.AddRange(tier1Skills);
            allSkills.AddRange(tier2Skills);
            allSkills.AddRange(tier3Skills);
            allSkills.AddRange(tier4Skills);
            return allSkills;
        }

        public List<SkillDefinition> GetSkillsByTier(SkillTier tier)
        {
            return tier switch
            {
                SkillTier.Tier1 => tier1Skills,
                SkillTier.Tier2 => tier2Skills,
                SkillTier.Tier3 => tier3Skills,
                SkillTier.Tier4 => tier4Skills,
                _ => new List<SkillDefinition>()
            };
        }
    }

    public enum SkillTreeType
    {
        Handler,
        Dog,
        Team
    }
}
