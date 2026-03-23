using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    public class SkillTreeService : MonoBehaviour
    {
        public static SkillTreeService Instance { get; private set; }

        [Header("Skill Tree Configurations")]
        [SerializeField] private SkillTreeData handlerSkillTree;
        [SerializeField] private SkillTreeData dogSkillTree;
        [SerializeField] private SkillTreeData teamSkillTree;

        [Header("Starting Points")]
        [SerializeField] private int startingSkillPoints = 3;
        [SerializeField] private int skillPointsPerLevel = 2;

        // Player state
        private int availableSkillPoints;
        private Dictionary<string, SkillState> skillStates = new Dictionary<string, SkillState>();
        private Dictionary<SkillTreeType, int> treesLevels = new Dictionary<SkillTreeType, int>();

        // Events
        public event Action<string> OnSkillUnlocked;
        public event Action<string> OnSkillRespecced;
        public event Action<int> OnSkillPointsChanged;
        public event Action<SkillTreeType> OnSkillTreeReset;

        // Properties
        public int AvailableSkillPoints => availableSkillPoints;
        public IReadOnlyDictionary<string, SkillState> SkillStates => skillStates;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeTrees();
            LoadSkillData();
        }

        private void InitializeTrees()
        {
            treesLevels[SkillTreeType.Handler] = 1;
            treesLevels[SkillTreeType.Dog] = 1;
            treesLevels[SkillTreeType.Team] = 1;
            availableSkillPoints = startingSkillPoints;
        }

        #region Skill Unlock/Respec

        public bool CanUnlockSkill(string skillId, SkillDefinition skillDef, SkillTreeType treeType)
        {
            if (skillDef == null) return false;
            if (IsSkillUnlocked(skillId)) return false;
            if (availableSkillPoints < skillDef.skillPointsCost) return false;

            // Check level requirement
            int treeLevel = treesLevels[treeType];
            if (treeLevel < skillDef.requiredLevel) return false;

            // Check prerequisites
            if (skillDef.prerequisiteSkillIds != null)
            {
                foreach (var prereqId in skillDef.prerequisiteSkillIds)
                {
                    if (!IsSkillUnlocked(prereqId))
                        return false;
                }
            }

            return true;
        }

        public bool UnlockSkill(string skillId, SkillDefinition skillDef, SkillTreeType treeType)
        {
            if (!CanUnlockSkill(skillId, skillDef, treeType)) return false;

            availableSkillPoints -= skillDef.skillPointsCost;

            var state = GetOrCreateSkillState(skillId);
            state.isUnlocked = true;
            state.unlockedAt = DateTime.Now;
            skillStates[skillId] = state;

            OnSkillUnlocked?.Invoke(skillId);
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            SaveSkillData();

            Debug.Log($"[SkillTree] Unlocked skill: {skillDef.displayName} ({skillId})");
            return true;
        }

        public bool RespecSkill(string skillId, SkillDefinition skillDef)
        {
            if (!IsSkillUnlocked(skillId)) return false;

            // Check if unlocking prerequisites would still be valid
            var state = GetOrCreateSkillState(skillId);

            // For now, simple respec - refund points and remove skill
            // In production, you'd want to validate the entire tree
            availableSkillPoints += skillDef.skillPointsCost;
            state.isUnlocked = false;
            state.unlockedAt = DateTime.MinValue;
            skillStates[skillId] = state;

            OnSkillRespecced?.Invoke(skillId);
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            SaveSkillData();

            Debug.Log($"[SkillTree] Respecced skill: {skillDef.displayName} ({skillId})");
            return true;
        }

        public void ResetSkillTree(SkillTreeType treeType)
        {
            var skillTree = GetSkillTree(treeType);
            if (skillTree == null) return;

            foreach (var skill in skillTree.GetAllSkills())
            {
                if (skillStates.ContainsKey(skill.skillId))
                {
                    var state = skillStates[skill.skillId];
                    if (state.isUnlocked)
                    {
                        availableSkillPoints += skill.skillPointsCost;
                        state.isUnlocked = false;
                        state.unlockedAt = DateTime.MinValue;
                        skillStates[skill.skillId] = state;
                    }
                }
            }

            OnSkillTreeReset?.Invoke(treeType);
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            SaveSkillData();

            Debug.Log($"[SkillTree] Reset skill tree: {treeType}");
        }

        #endregion

        #region Query Methods

        public bool IsSkillUnlocked(string skillId)
        {
            return skillStates.TryGetValue(skillId, out var state) && state.isUnlocked;
        }

        public SkillState GetSkillState(string skillId)
        {
            return skillStates.TryGetValue(skillId, out var state) ? state : null;
        }

        public SkillTreeData GetSkillTree(SkillTreeType treeType)
        {
            return treeType switch
            {
                SkillTreeType.Handler => handlerSkillTree,
                SkillTreeType.Dog => dogSkillTree,
                SkillTreeType.Team => teamSkillTree,
                _ => null
            };
        }

        public int GetTreeLevel(SkillTreeType treeType)
        {
            return treesLevels.TryGetValue(treeType, out int level) ? level : 1;
        }

        public List<SkillDefinition> GetUnlockedSkills(SkillTreeType treeType)
        {
            var skillTree = GetSkillTree(treeType);
            if (skillTree == null) return new List<SkillDefinition>();

            return skillTree.GetAllSkills()
                .Where(s => IsSkillUnlocked(s.skillId))
                .ToList();
        }

        public float CalculateSkillEffectTotal(SkillEffectType effectType, SkillTreeType treeType)
        {
            float total = 0f;
            var unlockedSkills = GetUnlockedSkills(treeType);

            foreach (var skill in unlockedSkills)
            {
                if (skill.effects != null)
                {
                    foreach (var effect in skill.effects)
                    {
                        if (effect.effectType == effectType)
                        {
                            total += effect.value;
                        }
                    }
                }
            }

            return total;
        }

        #endregion

        #region Level Management

        public void AddSkillPointsFromLevelUp(int newLevel)
        {
            availableSkillPoints += skillPointsPerLevel;
            treesLevels[SkillTreeType.Handler] = newLevel;
            treesLevels[SkillTreeType.Dog] = newLevel;
            treesLevels[SkillTreeType.Team] = newLevel;

            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            SaveSkillData();

            Debug.Log($"[SkillTree] Level up! +{skillPointsPerLevel} skill points. Total: {availableSkillPoints}");
        }

        public void GrantBonusSkillPoints(int amount, string reason = "")
        {
            availableSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            SaveSkillData();

            Debug.Log($"[SkillTree] Bonus +{amount} skill points from {reason}. Total: {availableSkillPoints}");
        }

        #endregion

        #region Persistence

        private void SaveSkillData()
        {
            var data = new SkillTreeSaveData
            {
                availableSkillPoints = availableSkillPoints,
                skillStates = new Dictionary<string, SkillState>(skillStates),
                treeLevels = new Dictionary<SkillTreeType, int>(treesLevels)
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();
            System.IO.File.WriteAllText(path, json);
        }

        private void LoadSkillData()
        {
            string path = GetSavePath();
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = JsonUtility.FromJson<SkillTreeSaveData>(json);

                availableSkillPoints = data.availableSkillPoints;
                skillStates = data.skillStates ?? new Dictionary<string, SkillState>();
                treesLevels = data.treeLevels ?? new Dictionary<SkillTreeType, int>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillTree] Failed to load skill data: {e.Message}");
            }
        }

        private string GetSavePath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "skill_tree.json");
        }

        #endregion

        #region Helpers

        private SkillState GetOrCreateSkillState(string skillId)
        {
            if (!skillStates.ContainsKey(skillId))
            {
                skillStates[skillId] = new SkillState
                {
                    skillId = skillId,
                    isUnlocked = false,
                    unlockedAt = DateTime.MinValue
                };
            }
            return skillStates[skillId];
        }

        #endregion
    }

    [Serializable]
    public class SkillState
    {
        public string skillId;
        public bool isUnlocked;
        public DateTime unlockedAt;
    }

    [Serializable]
    public class SkillTreeSaveData
    {
        public int availableSkillPoints;
        public Dictionary<string, SkillState> skillStates;
        public Dictionary<SkillTreeType, int> treeLevels;
    }
}
