using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Agility Dogs/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        [Header("Skill Info")]
        public string skillId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Category")]
        public SkillCategory category = SkillCategory.General;
        public SkillTier tier = SkillTier.Tier1;

        [Header("Requirements")]
        public int requiredLevel = 1;
        public string[] prerequisiteSkillIds;

        [Header("Cost")]
        public int skillPointsCost = 1;

        [Header("Effects")]
        public SkillEffect[] effects;

        [Header("Visual")]
        public Sprite icon;
        public Color iconColor = Color.white;
    }

    public enum SkillCategory
    {
        General,
        Speed,
        Agility,
        Command,
        Obstacle,
        Recovery,
        Mental,
        Handler
    }

    public enum SkillTier
    {
        Tier1, // Basic
        Tier2, // Intermediate  
        Tier3, // Advanced
        Tier4  // Master
    }

    [System.Serializable]
    public class SkillEffect
    {
        public SkillEffectType effectType;
        public float value;
        public string description;
    }

    public enum SkillEffectType
    {
        SpeedBoost,
        AccelerationBoost,
        TurnRateBoost,
        ResponsivenessBoost,
        JumpPowerBoost,
        WeaveSpeedBoost,
        ContactSpeedBoost,
        MomentumReduction,
        CommandWindowExtension,
        RecoveryTimeReduction,
        HandlingToleranceBoost,
        PressureResistance,
        XPGainMultiplier,
        WingBonusMultiplier
    }
}
