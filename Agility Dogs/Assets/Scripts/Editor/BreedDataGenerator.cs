using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using AgilityDogs.Data;
using AgilityDogs.Core;

namespace AgilityDogs.Editor
{
    public class BreedDataGenerator : EditorWindow
    {
        [MenuItem("Agility Dogs/Generate Breed Data Assets")]
        public static void ShowWindow()
        {
            GetWindow<BreedDataGenerator>("Breed Data Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Breed Data Generator", EditorStyles.boldLabel);
            GUILayout.Label("Generate ScriptableObject assets for each dog breed.");
            
            if (GUILayout.Button("Generate All Breed Data Assets"))
            {
                GenerateAllBreedData();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Or generate for specific breed:");
            if (GUILayout.Button("Border Collie"))
                GenerateBreedData("Border_Collie", "Border Collie", CreateBorderCollieStats());
            if (GUILayout.Button("Sheltie (Shetland Sheepdog)"))
                GenerateBreedData("Sheltie", "Shetland Sheepdog", CreateSheltieStats());
            if (GUILayout.Button("Australian Shepherd"))
                GenerateBreedData("Shepherd", "Australian Shepherd", CreateAustralianShepherdStats());
            if (GUILayout.Button("Jack Russell Terrier"))
                GenerateBreedData("JackRussellTerrier", "Jack Russell Terrier", CreateJackRussellStats());
            if (GUILayout.Button("Papillon"))
                GenerateBreedData("Papillon", "Papillon", CreatePapillonStats());
            if (GUILayout.Button("Poodle (Standard)"))
                GenerateBreedData("Poodle", "Standard Poodle", CreateStandardPoodleStats());
            if (GUILayout.Button("Beagle"))
                GenerateBreedData("Beagle", "Beagle", CreateBeagleStats());
            if (GUILayout.Button("Corgi"))
                GenerateBreedData("Corgi", "Pembroke Welsh Corgi", CreateCorgiStats());
            if (GUILayout.Button("Dalmatian"))
                GenerateBreedData("Dalmatian", "Dalmatian", CreateDalmatianStats());
            if (GUILayout.Button("Golden Retriever"))
                GenerateBreedData("GoldenRetriever", "Golden Retriever", CreateGoldenRetrieverStats());
            if (GUILayout.Button("Labrador"))
                GenerateBreedData("Labrador", "Labrador Retriever", CreateLabradorStats());
            if (GUILayout.Button("Husky"))
                GenerateBreedData("Husky", "Siberian Husky", CreateHuskyStats());
        }

        private static void GenerateAllBreedData()
        {
            string basePath = "Assets/Red_Deer/Dogs";
            string[] breedFolders = Directory.GetDirectories(basePath);
            
            foreach (string breedFolder in breedFolders)
            {
                string breedName = Path.GetFileName(breedFolder);
                if (breedName == "Dog_Object") continue; // Skip placeholder
                
                BreedStats stats = GetBreedStats(breedName);
                if (stats != null)
                {
                    GenerateBreedData(breedName, stats.displayName, stats);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Breed Data assets for all breeds.");
        }

        private static BreedStats GetBreedStats(string breedFolderName)
        {
            switch (breedFolderName)
            {
                case "Border_Collie": return CreateBorderCollieStats();
                case "Sheltie": return CreateSheltieStats();
                case "Shepherd": return CreateAustralianShepherdStats();
                case "JackRussellTerrier": return CreateJackRussellStats();
                case "Papillon": return CreatePapillonStats();
                case "Poodle": return CreateStandardPoodleStats();
                case "Beagle": return CreateBeagleStats();
                case "Corgi": return CreateCorgiStats();
                case "Dalmatian": return CreateDalmatianStats();
                case "GoldenRetriever": return CreateGoldenRetrieverStats();
                case "Labrador": return CreateLabradorStats();
                case "Husky": return CreateHuskyStats();
                case "Boxer": return CreateBoxerStats();
                case "BullTerrier": return CreateBullTerrierStats();
                case "Doberman": return CreateDobermanStats();
                case "FrenchBulldog": return CreateFrenchBulldogStats();
                case "Pitbull": return CreatePitbullStats();
                case "Pug": return CreatePugStats();
                case "Rottweiler": return CreateRottweilerStats();
                case "ShibaInu": return CreateShibaInuStats();
                case "Spitz": return CreateSpitzStats();
                case "ToyTerrier": return CreateToyTerrierStats();
                default: return null;
            }
        }

        private static void GenerateBreedData(string breedFolderName, string displayName, BreedStats stats)
        {
            // Create the BreedData asset
            BreedData breedData = ScriptableObject.CreateInstance<BreedData>();
            
            // Set breed info
            breedData.breedName = breedFolderName;
            breedData.displayName = displayName;
            
            // Set physical properties
            breedData.modelScale = stats.modelScale;
            
            // Set movement properties
            breedData.maxSpeed = stats.maxSpeed;
            breedData.acceleration = stats.acceleration;
            breedData.deceleration = stats.deceleration;
            breedData.turnRate = stats.turnRate;
            
            // Set agility attributes
            breedData.turningRadius = stats.turningRadius;
            breedData.responsiveness = stats.responsiveness;
            breedData.momentumFactor = stats.momentumFactor;
            breedData.handlingTolerance = stats.handlingTolerance;
            
            // Set obstacle performance
            breedData.weaveSpeed = stats.weaveSpeed;
            breedData.contactSpeed = stats.contactSpeed;
            breedData.jumpPower = stats.jumpPower;
            
            // Set visual properties
            breedData.animatorController = stats.animatorController;
            breedData.prefab = stats.prefab;
            
            // Set personality
            breedData.description = stats.description;
            
            // Set size division
            breedData.defaultDivision = stats.defaultDivision;
            
            // Create asset
            string assetPath = $"Assets/Red_Deer/Dogs/{breedFolderName}/{breedFolderName}_BreedData.asset";
            AssetDatabase.CreateAsset(breedData, assetPath);
            
            Debug.Log($"Created Breed Data asset for {displayName} at {assetPath}");
        }

        private static GameObject FindPrefab(string breedFolderName)
        {
            string prefabPath = $"Assets/Red_Deer/Dogs/{breedFolderName}/Dog/Prefabs";
            string[] prefabFiles = Directory.GetFiles(prefabPath, "*.prefab", SearchOption.TopDirectoryOnly);
            
            if (prefabFiles.Length > 0)
            {
                // Prefer a high-quality prefab
                foreach (string prefabFile in prefabFiles)
                {
                    if (prefabFile.Contains("_c1.prefab") || prefabFile.Contains("_anim_IP.prefab"))
                    {
                        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
                    }
                }
                
                // If no preferred prefab found, use first one
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabFiles[0]);
            }
            
            return null;
        }

        private static RuntimeAnimatorController FindAnimatorController(string breedFolderName)
        {
            string animPath = $"Assets/Red_Deer/Dogs/{breedFolderName}/Dog";
            string[] animFiles = Directory.GetFiles(animPath, "*.controller", SearchOption.AllDirectories);
            
            if (animFiles.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animFiles[0]);
            }
            
            return null;
        }

        // Breed statistics structures
        private class BreedStats
        {
            public string displayName;
            public float modelScale = 1.0f;
            public float maxSpeed = 7.5f;
            public float acceleration = 6f;
            public float deceleration = 8f;
            public float turnRate = 180f;
            public float turningRadius = 0.6f;
            public float responsiveness = 0.7f;
            public float momentumFactor = 0.6f;
            public float handlingTolerance = 0.65f;
            public float weaveSpeed = 1.0f;
            public float contactSpeed = 0.85f;
            public float jumpPower = 1.0f;
            public RuntimeAnimatorController animatorController;
            public GameObject prefab;
            public string description;
            public AgilitySizeDivision defaultDivision = AgilitySizeDivision.FourteenInch;
        }

        // Individual breed configurations
        private static BreedStats CreateBorderCollieStats()
        {
            return new BreedStats
            {
                displayName = "Border Collie",
                modelScale = 1.0f,
                maxSpeed = 11.5f,      // Top agility breed
                acceleration = 12f,    // Quick bursts
                deceleration = 10f,
                turnRate = 450f,       // Exceptional turning
                turningRadius = 0.35f, // Very tight turns
                responsiveness = 0.95f, // Highly responsive
                momentumFactor = 0.4f, // Can change direction quickly
                handlingTolerance = 0.9f, // Forgiving of handler errors
                weaveSpeed = 1.4f,     // Excellent weave pole performance
                contactSpeed = 1.1f,   // Good on contact obstacles
                jumpPower = 1.2f,      // Good jumping ability
                description = "The premier agility breed. Border Collies are known for their intelligence, speed, and precision. They excel in all aspects of agility competition.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Border_Collie"),
                prefab = FindPrefab("Border_Collie")
            };
        }

        private static BreedStats CreateSheltieStats()
        {
            return new BreedStats
            {
                displayName = "Shetland Sheepdog",
                modelScale = 0.85f,    // Smaller than Border Collie
                maxSpeed = 10.0f,
                acceleration = 10f,
                deceleration = 9f,
                turnRate = 420f,
                turningRadius = 0.4f,
                responsiveness = 0.9f,
                momentumFactor = 0.45f,
                handlingTolerance = 0.85f,
                weaveSpeed = 1.3f,
                contactSpeed = 1.0f,
                jumpPower = 0.95f,
                description = "Shelties are agile, intelligent, and eager to please. They excel in weave poles and are known for their speed and consistency.",
                defaultDivision = AgilitySizeDivision.FourteenInch,
                animatorController = FindAnimatorController("Shepherd"),
                prefab = FindPrefab("Shepherd")
            };
        }

        private static BreedStats CreateAustralianShepherdStats()
        {
            return new BreedStats
            {
                displayName = "Australian Shepherd",
                modelScale = 1.05f,
                maxSpeed = 10.5f,
                acceleration = 10f,
                deceleration = 9f,
                turnRate = 400f,
                turningRadius = 0.45f,
                responsiveness = 0.85f,
                momentumFactor = 0.5f,
                handlingTolerance = 0.8f,
                weaveSpeed = 1.2f,
                contactSpeed = 1.05f,
                jumpPower = 1.15f,
                description = "Athletic and versatile, Australian Shepherds are natural athletes with strong herding instincts that translate well to agility.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Shepherd"),
                prefab = FindPrefab("Shepherd")
            };
        }

        private static BreedStats CreateJackRussellStats()
        {
            return new BreedStats
            {
                displayName = "Jack Russell Terrier",
                modelScale = 0.65f,
                maxSpeed = 9.0f,
                acceleration = 11f,    // Very quick acceleration
                deceleration = 9f,
                turnRate = 480f,       // Extremely agile
                turningRadius = 0.3f,  // Very tight turns
                responsiveness = 0.8f, // Can be stubborn
                momentumFactor = 0.35f,
                handlingTolerance = 0.6f, // Less forgiving
                weaveSpeed = 1.1f,
                contactSpeed = 0.9f,
                jumpPower = 1.3f,      // Great jumping power for size
                description = "Fearless and energetic, Jack Russells are surprisingly agile for their size. They're known for their speed and jumping ability.",
                defaultDivision = AgilitySizeDivision.TwelveInch,
                animatorController = FindAnimatorController("JackRussellTerrier"),
                prefab = FindPrefab("JackRussellTerrier")
            };
        }

        private static BreedStats CreatePapillonStats()
        {
            return new BreedStats
            {
                displayName = "Papillon",
                modelScale = 0.55f,
                maxSpeed = 8.0f,
                acceleration = 9f,
                deceleration = 8f,
                turnRate = 500f,       // Very agile small dog
                turningRadius = 0.25f, // Extremely tight
                responsiveness = 0.85f,
                momentumFactor = 0.3f,
                handlingTolerance = 0.7f,
                weaveSpeed = 1.0f,
                contactSpeed = 0.8f,
                jumpPower = 0.9f,
                description = "One of the smallest agility breeds, Papillons are surprisingly athletic and excel in technical courses.",
                defaultDivision = AgilitySizeDivision.EightInch,
                animatorController = FindAnimatorController("ToyTerrier"),
                prefab = FindPrefab("ToyTerrier")
            };
        }

        private static BreedStats CreateStandardPoodleStats()
        {
            return new BreedStats
            {
                displayName = "Standard Poodle",
                modelScale = 1.1f,
                maxSpeed = 9.5f,
                acceleration = 8f,
                deceleration = 9f,
                turnRate = 350f,
                turningRadius = 0.55f,
                responsiveness = 0.9f,
                momentumFactor = 0.55f,
                handlingTolerance = 0.85f,
                weaveSpeed = 1.1f,
                contactSpeed = 0.95f,
                jumpPower = 1.1f,
                description = "Highly intelligent and trainable, Standard Poodles are versatile athletes that perform well in agility.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Poodle"),
                prefab = FindPrefab("Poodle")
            };
        }

        private static BreedStats CreateBeagleStats()
        {
            return new BreedStats
            {
                displayName = "Beagle",
                modelScale = 0.9f,
                maxSpeed = 8.5f,
                acceleration = 7f,
                deceleration = 7f,
                turnRate = 300f,
                turningRadius = 0.6f,
                responsiveness = 0.6f, // Can be distracted by scents
                momentumFactor = 0.7f, // High momentum
                handlingTolerance = 0.5f, // Less forgiving
                weaveSpeed = 0.8f,
                contactSpeed = 0.85f,
                jumpPower = 0.9f,
                description = "Beagles are determined and can be stubborn, but their enthusiasm makes them fun agility partners.",
                defaultDivision = AgilitySizeDivision.TwelveInch,
                animatorController = FindAnimatorController("Beagle"),
                prefab = FindPrefab("Beagle")
            };
        }

        private static BreedStats CreateCorgiStats()
        {
            return new BreedStats
            {
                displayName = "Pembroke Welsh Corgi",
                modelScale = 0.8f,
                maxSpeed = 8.0f,
                acceleration = 7f,
                deceleration = 8f,
                turnRate = 320f,
                turningRadius = 0.55f,
                responsiveness = 0.8f,
                momentumFactor = 0.65f,
                handlingTolerance = 0.75f,
                weaveSpeed = 0.9f,
                contactSpeed = 0.9f,
                jumpPower = 0.75f, // Lower jump power due to short legs
                description = "Corgis are surprisingly agile for their body type. They're known for their determination and strong work ethic.",
                defaultDivision = AgilitySizeDivision.EightInch,
                animatorController = FindAnimatorController("Corgi"),
                prefab = FindPrefab("Corgi")
            };
        }

        private static BreedStats CreateDalmatianStats()
        {
            return new BreedStats
            {
                displayName = "Dalmatian",
                modelScale = 1.15f,
                maxSpeed = 10.0f,
                acceleration = 9f,
                deceleration = 8f,
                turnRate = 350f,
                turningRadius = 0.6f,
                responsiveness = 0.75f,
                momentumFactor = 0.6f,
                handlingTolerance = 0.7f,
                weaveSpeed = 1.0f,
                contactSpeed = 0.95f,
                jumpPower = 1.05f,
                description = "Athletic and energetic, Dalmatians have the stamina for multiple runs and excel in longer courses.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Dalmatian"),
                prefab = FindPrefab("Dalmatian")
            };
        }

        private static BreedStats CreateGoldenRetrieverStats()
        {
            return new BreedStats
            {
                displayName = "Golden Retriever",
                modelScale = 1.2f,
                maxSpeed = 9.0f,
                acceleration = 8f,
                deceleration = 7f,
                turnRate = 300f,
                turningRadius = 0.7f,
                responsiveness = 0.85f,
                momentumFactor = 0.75f, // High momentum
                handlingTolerance = 0.8f,
                weaveSpeed = 0.95f,
                contactSpeed = 0.9f,
                jumpPower = 1.0f,
                description = "Golden Retrievers are eager to please and have a good temperament for agility, though their size can make tight courses challenging.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("GoldenRetriever"),
                prefab = FindPrefab("GoldenRetriever")
            };
        }

        private static BreedStats CreateLabradorStats()
        {
            return new BreedStats
            {
                displayName = "Labrador Retriever",
                modelScale = 1.15f,
                maxSpeed = 9.0f,
                acceleration = 8f,
                deceleration = 7f,
                turnRate = 300f,
                turningRadius = 0.7f,
                responsiveness = 0.8f,
                momentumFactor = 0.75f,
                handlingTolerance = 0.75f,
                weaveSpeed = 0.9f,
                contactSpeed = 0.9f,
                jumpPower = 1.05f,
                description = "Labradors are enthusiastic and food-motivated, making them good agility candidates despite their larger size.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Labrador"),
                prefab = FindPrefab("Labrador")
            };
        }

        private static BreedStats CreateHuskyStats()
        {
            return new BreedStats
            {
                displayName = "Siberian Husky",
                modelScale = 1.1f,
                maxSpeed = 10.0f,
                acceleration = 9f,
                deceleration = 8f,
                turnRate = 320f,
                turningRadius = 0.65f,
                responsiveness = 0.65f, // Independent-minded
                momentumFactor = 0.7f,
                handlingTolerance = 0.6f, // Less forgiving
                weaveSpeed = 0.85f,
                contactSpeed = 0.9f,
                jumpPower = 1.0f,
                description = "Huskies are athletic but independent. They can be challenging to handle but excel in straight-line speed.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Husky"),
                prefab = FindPrefab("Husky")
            };
        }

        private static BreedStats CreateBoxerStats()
        {
            return new BreedStats
            {
                displayName = "Boxer",
                modelScale = 1.15f,
                maxSpeed = 8.5f,
                acceleration = 8f,
                deceleration = 8f,
                turnRate = 300f,
                turningRadius = 0.65f,
                responsiveness = 0.75f,
                momentumFactor = 0.7f,
                handlingTolerance = 0.7f,
                weaveSpeed = 0.85f,
                contactSpeed = 0.85f,
                jumpPower = 1.1f,
                description = "Boxers are playful and energetic, with a unique gait that can be advantageous on certain obstacles.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Boxer"),
                prefab = FindPrefab("Boxer")
            };
        }

        private static BreedStats CreateBullTerrierStats()
        {
            return new BreedStats
            {
                displayName = "Bull Terrier",
                modelScale = 1.1f,
                maxSpeed = 8.0f,
                acceleration = 7f,
                deceleration = 7f,
                turnRate = 280f,
                turningRadius = 0.7f,
                responsiveness = 0.7f,
                momentumFactor = 0.75f,
                handlingTolerance = 0.65f,
                weaveSpeed = 0.8f,
                contactSpeed = 0.8f,
                jumpPower = 1.0f,
                description = "Bull Terriers are strong and determined, requiring consistent training for agility success.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("BullTerrier"),
                prefab = FindPrefab("BullTerrier")
            };
        }

        private static BreedStats CreateDobermanStats()
        {
            return new BreedStats
            {
                displayName = "Doberman Pinscher",
                modelScale = 1.2f,
                maxSpeed = 10.5f,
                acceleration = 10f,
                deceleration = 9f,
                turnRate = 380f,
                turningRadius = 0.55f,
                responsiveness = 0.85f,
                momentumFactor = 0.6f,
                handlingTolerance = 0.75f,
                weaveSpeed = 1.0f,
                contactSpeed = 0.95f,
                jumpPower = 1.15f,
                description = "Dobermans are athletic and intelligent, with good speed and turning ability for agility.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Doberman"),
                prefab = FindPrefab("Doberman")
            };
        }

        private static BreedStats CreateFrenchBulldogStats()
        {
            return new BreedStats
            {
                displayName = "French Bulldog",
                modelScale = 0.75f,
                maxSpeed = 6.5f,      // Brachycephalic breathing limits
                acceleration = 6f,
                deceleration = 7f,
                turnRate = 350f,
                turningRadius = 0.5f,
                responsiveness = 0.8f,
                momentumFactor = 0.6f,
                handlingTolerance = 0.7f,
                weaveSpeed = 0.7f,   // Limited by breathing
                contactSpeed = 0.75f,
                jumpPower = 0.8f,    // Limited by body shape
                description = "French Bulldogs are charming but their brachycephalic nature limits their endurance and speed in agility.",
                defaultDivision = AgilitySizeDivision.TwelveInch,
                animatorController = FindAnimatorController("FrenchBulldog"),
                prefab = FindPrefab("FrenchBulldog")
            };
        }

        private static BreedStats CreatePitbullStats()
        {
            return new BreedStats
            {
                displayName = "American Pit Bull Terrier",
                modelScale = 1.1f,
                maxSpeed = 9.5f,
                acceleration = 11f,    // Very quick bursts
                deceleration = 9f,
                turnRate = 380f,
                turningRadius = 0.55f,
                responsiveness = 0.8f,
                momentumFactor = 0.6f,
                handlingTolerance = 0.7f,
                weaveSpeed = 1.0f,
                contactSpeed = 0.9f,
                jumpPower = 1.2f,     // Strong jumping ability
                description = "Pit Bulls are powerful and athletic, with excellent acceleration and jumping ability.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Pitbull"),
                prefab = FindPrefab("Pitbull")
            };
        }

        private static BreedStats CreatePugStats()
        {
            return new BreedStats
            {
                displayName = "Pug",
                modelScale = 0.6f,
                maxSpeed = 5.5f,      // Very limited by brachycephalic nature
                acceleration = 5f,
                deceleration = 6f,
                turnRate = 300f,
                turningRadius = 0.6f,
                responsiveness = 0.75f,
                momentumFactor = 0.7f,
                handlingTolerance = 0.65f,
                weaveSpeed = 0.6f,
                contactSpeed = 0.65f,
                jumpPower = 0.6f,     // Limited jumping ability
                description = "Pugs are playful but their brachycephalic nature severely limits their agility performance.",
                defaultDivision = AgilitySizeDivision.EightInch,
                animatorController = FindAnimatorController("Pug"),
                prefab = FindPrefab("Pug")
            };
        }

        private static BreedStats CreateRottweilerStats()
        {
            return new BreedStats
            {
                displayName = "Rottweiler",
                modelScale = 1.25f,
                maxSpeed = 9.0f,
                acceleration = 8f,
                deceleration = 8f,
                turnRate = 280f,
                turningRadius = 0.75f, // Larger turning radius
                responsiveness = 0.8f,
                momentumFactor = 0.8f, // High momentum
                handlingTolerance = 0.7f,
                weaveSpeed = 0.85f,
                contactSpeed = 0.9f,
                jumpPower = 1.15f,
                description = "Rottweilers are powerful but their size and weight make technical agility courses challenging.",
                defaultDivision = AgilitySizeDivision.TwentyInch,
                animatorController = FindAnimatorController("Rottweiler"),
                prefab = FindPrefab("Rottweiler")
            };
        }

        private static BreedStats CreateShibaInuStats()
        {
            return new BreedStats
            {
                displayName = "Shiba Inu",
                modelScale = 0.85f,
                maxSpeed = 8.5f,
                acceleration = 8f,
                deceleration = 8f,
                turnRate = 350f,
                turningRadius = 0.55f,
                responsiveness = 0.7f, // Independent
                momentumFactor = 0.65f,
                handlingTolerance = 0.6f, // Less forgiving
                weaveSpeed = 0.9f,
                contactSpeed = 0.85f,
                jumpPower = 0.9f,
                description = "Shiba Inus are agile but independent-minded, requiring patient training for agility success.",
                defaultDivision = AgilitySizeDivision.TwelveInch,
                animatorController = FindAnimatorController("ShibaInu"),
                prefab = FindPrefab("ShibaInu")
            };
        }

        private static BreedStats CreateSpitzStats()
        {
            return new BreedStats
            {
                displayName = "German Spitz",
                modelScale = 0.8f,
                maxSpeed = 8.0f,
                acceleration = 7f,
                deceleration = 7f,
                turnRate = 360f,
                turningRadius = 0.5f,
                responsiveness = 0.75f,
                momentumFactor = 0.6f,
                handlingTolerance = 0.7f,
                weaveSpeed = 0.9f,
                contactSpeed = 0.85f,
                jumpPower = 0.85f,
                description = "Spitz breeds are alert and agile, with good turning ability for their size.",
                defaultDivision = AgilitySizeDivision.TwelveInch,
                animatorController = FindAnimatorController("Spitz"),
                prefab = FindPrefab("Spitz")
            };
        }

        private static BreedStats CreateToyTerrierStats()
        {
            return new BreedStats
            {
                displayName = "Toy Manchester Terrier",
                modelScale = 0.6f,
                maxSpeed = 7.5f,
                acceleration = 8f,
                deceleration = 8f,
                turnRate = 450f,
                turningRadius = 0.35f,
                responsiveness = 0.85f,
                momentumFactor = 0.4f,
                handlingTolerance = 0.7f,
                weaveSpeed = 0.95f,
                contactSpeed = 0.8f,
                jumpPower = 0.85f,
                description = "Toy Terriers are quick and agile, excelling in technical courses despite their small size.",
                defaultDivision = AgilitySizeDivision.EightInch,
                animatorController = FindAnimatorController("ToyTerrier"),
                prefab = FindPrefab("ToyTerrier")
            };
        }
    }
}