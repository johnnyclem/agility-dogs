#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AgilityDogs.Data;

namespace AgilityDogs.Editor
{
    public class SkillDataGenerator : EditorWindow
    {
        private string outputFolder = "Assets/Data/Skills";

        [MenuItem("Agility Dogs/Generate Skill Data")]
        public static void ShowWindow()
        {
            GetWindow<SkillDataGenerator>("Skill Data Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Skill Data Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            GUILayout.Space(10);

            if (GUILayout.Button("Generate Handler Skills"))
            {
                GenerateHandlerSkills();
            }

            if (GUILayout.Button("Generate Dog Skills"))
            {
                GenerateDogSkills();
            }

            if (GUILayout.Button("Generate Team Skills"))
            {
                GenerateTeamSkills();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate All Skill Trees"))
            {
                GenerateAllSkillTrees();
            }
        }

        private void GenerateHandlerSkills()
        {
            // Create output folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                string[] folders = outputFolder.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath += "/" + folders[i];
                }
            }

            // Tier 1 - Basic Handler Skills
            CreateSkillDefinition("handler_sprint_boost", "Sprint Boost", "Increases handler sprint speed by 10%", SkillCategory.Speed, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.SpeedBoost, value = 0.1f, description = "+10% Sprint Speed" } });

            CreateSkillDefinition("handler_command_clarity", "Command Clarity", "Extends command timing window by 15%", SkillCategory.Command, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.CommandWindowExtension, value = 0.15f, description = "+15% Command Window" } });

            CreateSkillDefinition("handler_course_reading", "Course Reading", "Improves obstacle detection range", SkillCategory.Command, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.CommandWindowExtension, value = 0.1f, description = "+10% Detection Range" } });

            // Tier 2 - Intermediate Handler Skills
            CreateSkillDefinition("handler_pressure_resist", "Pressure Resistance", "Reduces timing penalties under pressure", SkillCategory.Mental, SkillTier.Tier2, 2,
                new string[] { "handler_command_clarity" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.PressureResistance, value = 0.2f, description = "+20% Pressure Resistance" } });

            CreateSkillDefinition("handler_agile_movement", "Agile Movement", "Improves turning rate while sprinting", SkillCategory.Agility, SkillTier.Tier2, 2,
                new string[] { "handler_sprint_boost" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.TurnRateBoost, value = 0.15f, description = "+15% Turn Rate" } });

            // Tier 3 - Advanced Handler Skills
            CreateSkillDefinition("handler_master_commander", "Master Commander", "Significantly extends command windows", SkillCategory.Command, SkillTier.Tier3, 3,
                new string[] { "handler_pressure_resist", "handler_course_reading" },
                new SkillEffect[] { 
                    new SkillEffect { effectType = SkillEffectType.CommandWindowExtension, value = 0.25f, description = "+25% Command Window" },
                    new SkillEffect { effectType = SkillEffectType.XPGainMultiplier, value = 0.1f, description = "+10% XP Gain" }
                });

            // Tier 4 - Master Handler Skills
            CreateSkillDefinition("handler_dog_whisperer", "Dog Whisperer", "Master handler ability - all stats significantly improved", SkillCategory.Handler, SkillTier.Tier4, 4,
                new string[] { "handler_master_commander" },
                new SkillEffect[] { 
                    new SkillEffect { effectType = SkillEffectType.CommandWindowExtension, value = 0.3f, description = "+30% Command Window" },
                    new SkillEffect { effectType = SkillEffectType.XPGainMultiplier, value = 0.25f, description = "+25% XP Gain" },
                    new SkillEffect { effectType = SkillEffectType.WingBonusMultiplier, value = 0.2f, description = "+20% Wings" }
                });

            AssetDatabase.SaveAssets();
            Debug.Log("Handler skills generated at: " + outputFolder);
        }

        private void GenerateDogSkills()
        {
            EnsureOutputFolder();

            // Tier 1 - Basic Dog Skills
            CreateSkillDefinition("dog_speed_boost", "Speed Boost", "Increases maximum speed by 5%", SkillCategory.Speed, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.SpeedBoost, value = 0.05f, description = "+5% Max Speed" } });

            CreateSkillDefinition("dog_acceleration", "Quick Start", "Improves acceleration by 10%", SkillCategory.Speed, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.AccelerationBoost, value = 0.1f, description = "+10% Acceleration" } });

            CreateSkillDefinition("dog_jump_power", "Jump Power", "Increases jump clearance by 8%", SkillCategory.Obstacle, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.JumpPowerBoost, value = 0.08f, description = "+8% Jump Power" } });

            // Tier 2 - Intermediate Dog Skills
            CreateSkillDefinition("dog_agile_turning", "Agile Turning", "Improves turning radius at speed", SkillCategory.Agility, SkillTier.Tier2, 2,
                new string[] { "dog_speed_boost" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.TurnRateBoost, value = 0.15f, description = "+15% Turn Rate" } });

            CreateSkillDefinition("dog_weave_master", "Weave Master", "Improves weave pole speed", SkillCategory.Obstacle, SkillTier.Tier2, 2,
                new string[] { "dog_acceleration" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.WeaveSpeedBoost, value = 0.15f, description = "+15% Weave Speed" } });

            CreateSkillDefinition("dog_momentum_control", "Momentum Control", "Reduces momentum penalty for sharp turns", SkillCategory.Agility, SkillTier.Tier2, 2,
                new string[] { "dog_agile_turning" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.MomentumReduction, value = 0.15f, description = "-15% Momentum Penalty" } });

            // Tier 3 - Advanced Dog Skills
            CreateSkillDefinition("dog_contact_precision", "Contact Precision", "Improves contact obstacle performance", SkillCategory.Obstacle, SkillTier.Tier3, 3,
                new string[] { "dog_weave_master", "dog_momentum_control" },
                new SkillEffect[] { 
                    new SkillEffect { effectType = SkillEffectType.ContactSpeedBoost, value = 0.15f, description = "+15% Contact Speed" },
                    new SkillEffect { effectType = SkillEffectType.ResponsivenessBoost, value = 0.1f, description = "+10% Responsiveness" }
                });

            CreateSkillDefinition("dog_recovery_speed", "Quick Recovery", "Faster recovery after mistakes", SkillCategory.Recovery, SkillTier.Tier3, 3,
                new string[] { "dog_momentum_control" },
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.RecoveryTimeReduction, value = 0.25f, description = "-25% Recovery Time" } });

            // Tier 4 - Master Dog Skills
            CreateSkillDefinition("dog_agility_champion", "Agility Champion", "Master dog ability - all performance stats enhanced", SkillCategory.Obstacle, SkillTier.Tier4, 4,
                new string[] { "dog_contact_precision", "dog_recovery_speed" },
                new SkillEffect[] { 
                    new SkillEffect { effectType = SkillEffectType.SpeedBoost, value = 0.1f, description = "+10% Speed" },
                    new SkillEffect { effectType = SkillEffectType.TurnRateBoost, value = 0.2f, description = "+20% Turn Rate" },
                    new SkillEffect { effectType = SkillEffectType.ResponsivenessBoost, value = 0.2f, description = "+20% Responsiveness" }
                });

            AssetDatabase.SaveAssets();
            Debug.Log("Dog skills generated at: " + outputFolder);
        }

        private void GenerateTeamSkills()
        {
            EnsureOutputFolder();

            // Team synergy skills
            CreateSkillDefinition("team_sync_boost", "Team Sync", "Improves handler-dog responsiveness", SkillCategory.Handler, SkillTier.Tier1, 1,
                new SkillEffect[] { new SkillEffect { effectType = SkillEffectType.ResponsivenessBoost, value = 0.1f, description = "+10% Team Sync" } });

            CreateSkillDefinition("team默契", "默契", "Handler and dog work more cohesively", SkillCategory.Handler, SkillTier.Tier2, 2,
                new string[] { "team_sync_boost" },
                new SkillEffect[] { 
                    new SkillEffect { effectType = SkillEffectType.CommandWindowExtension, value = 0.1f, description = "+10% Command Window" },
                    new SkillEffect { effectType = SkillEffectType.HandlingToleranceBoost, value = 0.15f, description = "+15% Handling Tolerance" }
                });

            AssetDatabase.SaveAssets();
            Debug.Log("Team skills generated at: " + outputFolder);
        }

        private void GenerateAllSkillTrees()
        {
            GenerateHandlerSkills();
            GenerateDogSkills();
            GenerateTeamSkills();

            // Create skill tree ScriptableObjects
            CreateSkillTree("handler_skill_tree", "Handler Skill Tree", "Master handler techniques and command precision", SkillTreeType.Handler);
            CreateSkillTree("dog_skill_tree", "Dog Skill Tree", "Unlock your dog's full athletic potential", SkillTreeType.Dog);
            CreateSkillTree("team_skill_tree", "Team Skill Tree", "Build synergy between handler and dog", SkillTreeType.Team);

            Debug.Log("All skill trees generated!");
        }

        private void CreateSkillDefinition(string id, string displayName, string description, SkillCategory category, SkillTier tier, int requiredLevel, SkillEffect[] effects)
        {
            CreateSkillDefinition(id, displayName, description, category, tier, requiredLevel, null, effects);
        }

        private void CreateSkillDefinition(string id, string displayName, string description, SkillCategory category, SkillTier tier, int requiredLevel, string[] prerequisites, SkillEffect[] effects)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.skillId = id;
            skill.displayName = displayName;
            skill.description = description;
            skill.category = category;
            skill.tier = tier;
            skill.requiredLevel = requiredLevel;
            skill.prerequisiteSkillIds = prerequisites;
            skill.effects = effects;
            skill.skillPointsCost = tier switch
            {
                SkillTier.Tier1 => 1,
                SkillTier.Tier2 => 2,
                SkillTier.Tier3 => 3,
                SkillTier.Tier4 => 4,
                _ => 1
            };

            string path = System.IO.Path.Combine(outputFolder, $"Skill_{id}.asset");
            AssetDatabase.CreateAsset(skill, path);
            EditorUtility.SetDirty(skill);
        }

        private void CreateSkillTree(string id, string displayName, string description, SkillTreeType treeType)
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.treeId = id;
            tree.displayName = displayName;
            tree.description = description;
            tree.treeType = treeType;

            // Load skills and assign to appropriate tiers
            var allSkills = new List<SkillDefinition>();
            string[] guids = AssetDatabase.FindAssets("t:SkillDefinition", new[] { outputFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
                if (skill != null)
                {
                    switch (skill.tier)
                    {
                        case SkillTier.Tier1: tree.tier1Skills.Add(skill); break;
                        case SkillTier.Tier2: tree.tier2Skills.Add(skill); break;
                        case SkillTier.Tier3: tree.tier3Skills.Add(skill); break;
                        case SkillTier.Tier4: tree.tier4Skills.Add(skill); break;
                    }
                }
            }

            string treePath = System.IO.Path.Combine(outputFolder, $"SkillTree_{id}.asset");
            AssetDatabase.CreateAsset(tree, treePath);
            EditorUtility.SetDirty(tree);
        }

        private void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                string[] folders = outputFolder.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath += "/" + folders[i];
                }
            }
        }
    }
}
#endif
