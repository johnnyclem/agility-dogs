#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AgilityDogs.Data;
using AgilityDogs.Core;
using System.Collections.Generic;

namespace AgilityDogs.Editor
{
    public class DataGenerator : EditorWindow
    {
        [MenuItem("Agility Dogs/Generate All Data")]
        public static void ShowWindow()
        {
            GetWindow<DataGenerator>("Data Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Agility Dogs - Data Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate All BreedData Assets", GUILayout.Height(30)))
            {
                GenerateAllBreedData();
            }

            if (GUILayout.Button("Generate Course Assets", GUILayout.Height(30)))
            {
                GenerateCourseAssets();
            }

            if (GUILayout.Button("Generate Handler Assets", GUILayout.Height(30)))
            {
                GenerateHandlerAssets();
            }

            EditorGUILayout.Space();
            GUILayout.Label("These create ScriptableObject assets in Assets/Data/", EditorStyles.helpBox);
        }

        private static void GenerateAllBreedData()
        {
            var breedsToCreate = new (string name, string prefabPath, float speed, float accel, float responsiveness, float jumpPower)[]
            {
                ("BorderCollie", "Assets/Red_Deer/Dogs/Border_Collie/Dog/Prefabs/BorderCollie_anim_IP.prefab", 9f, 8f, 0.85f, 1.1f),
                ("JackRussellTerrier", "Assets/Red_Deer/Dogs/JackRussellTerrier/Dog/Prefabs/JRTerrier_anim_IP.prefab", 8f, 8.5f, 0.9f, 1.0f),
                ("Corgi", "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_anim_IP.prefab", 6.5f, 7f, 0.8f, 0.9f),
                ("GoldenRetriever", "Assets/Red_Deer/Dogs/GoldenRetriever/Dog/Prefabs/Retriever_anim_IP.prefab", 7f, 6.5f, 0.7f, 1.0f),
                ("ShibaInu", "Assets/Red_Deer/Dogs/Spitz/Dog/Prefabs/Spitz_anim_IP.prefab", 7.5f, 7f, 0.65f, 0.95f),
                ("Beagle", "Assets/Red_Deer/Dogs/Beagle/Dog/Prefabs/Beagle_anim_IP.prefab", 6f, 6f, 0.7f, 0.85f),
                ("Boxer", "Assets/Red_Deer/Dogs/Boxer/Dog/Prefabs/Dog_Boxer_anim_IP.prefab", 7f, 6f, 0.75f, 1.05f),
                ("Husky", "Assets/Red_Deer/Dogs/Husky/Dog/Prefabs/Husky_anim_IP.prefab", 7.5f, 7f, 0.7f, 1.0f),
                ("Labrador", "Assets/Red_Deer/Dogs/Labrador/Dog/Prefabs/Labrador_anim_IP.prefab", 6.5f, 6f, 0.7f, 1.0f),
                ("Rottweiler", "Assets/Red_Deer/Dogs/Rottweiler/Dog/Prefabs/Rottweiler_anim_IP.prefab", 6f, 5.5f, 0.65f, 1.1f),
                ("Pitbull", "Assets/Red_Deer/Dogs/Pitbull/Dog/Prefabs/Pitbull_anim_IP.prefab", 7f, 6.5f, 0.7f, 1.05f),
                ("Dalmatian", "Assets/Red_Deer/Dogs/Dalmatian/Dog/Prefabs/Dalmatian_anim_IP.prefab", 7.5f, 7f, 0.75f, 1.0f),
                ("Doberman", "Assets/Red_Deer/Dogs/Doberman/Dog/Prefabs/Doberman_anim_IP.prefab", 8f, 7.5f, 0.75f, 1.05f),
                ("FrenchBulldog", "Assets/Red_Deer/Dogs/FrenchBulldog/Dog/Prefabs/FrenchBulldog_anim_IP.prefab", 5.5f, 5f, 0.7f, 0.8f),
                ("Pug", "Assets/Red_Deer/Dogs/Pug/Dog/Prefabs/Pug_anim_IP.prefab", 5f, 4.5f, 0.65f, 0.75f),
                ("Shepherd", "Assets/Red_Deer/Dogs/Shepherd/Dog/Prefabs/Shepherd_anim_IP.prefab", 7.5f, 7f, 0.8f, 1.0f),
                ("BullTerrier", "Assets/Red_Deer/Dogs/BullTerrier/Dog/Prefabs/BullTerrier_anim_IP.prefab", 6.5f, 6f, 0.7f, 1.05f),
                ("ToyTerrier", "Assets/Red_Deer/Dogs/ToyTerrier/Dog/Prefabs/ToyTerrier_anim_IP.prefab", 6f, 6.5f, 0.85f, 0.8f),
                ("Spitz", "Assets/Red_Deer/Dogs/Spitz/Dog/Prefabs/Spitz_anim_IP.prefab", 7f, 6.5f, 0.7f, 0.9f),
            };

            foreach (var breed in breedsToCreate)
            {
                CreateBreedData(breed.name, breed.prefabPath, breed.name.Replace("Collie", " Collie").Replace("Russell", " Russell").Replace("Terrier", " Terrier").Replace("Retriever", " Retriever").Replace("Bulldog", " Bulldog").Replace("Bull", " Bull"), breed.speed, breed.accel, breed.responsiveness, breed.jumpPower);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[DataGenerator] Created " + breedsToCreate.Length + " breed assets");
        }

        private static void CreateBreedData(string name, string prefabPath, string displayName, float speed, float accel, float responsiveness, float jumpPower)
        {
            string assetPath = $"Assets/Data/Breeds/{name}.asset";
            
            if (AssetDatabase.LoadAssetAtPath<BreedData>(assetPath) != null)
            {
                return;
            }

            var breedData = ScriptableObject.CreateInstance<BreedData>();
            breedData.breedName = name;
            breedData.displayName = displayName;
            breedData.maxSpeed = speed;
            breedData.acceleration = accel;
            breedData.responsiveness = responsiveness;
            breedData.jumpPower = jumpPower;
            breedData.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            breedData.description = $"A {displayName} with excellent agility potential.";

            AssetDatabase.CreateAsset(breedData, assetPath);
        }

        private static void GenerateCourseAssets()
        {
            CreateCourseDefinition("LocalPark", "Local Park Course", CourseType.Standard, 45f, 60f, 3, "LocalPark");
            CreateCourseDefinition("CountyFair", "County Fair Course", CourseType.Standard, 50f, 65f, 5, "CountyFair");
            CreateCourseDefinition("RegionalChamp", "Regional Championship", CourseType.Standard, 55f, 70f, 7, "RegionalChamp");
            CreateCourseDefinition("StateChamp", "State Championship", CourseType.Championship, 60f, 75f, 8, "StateChamp");
            CreateCourseDefinition("NationalChamp", "National Championship", CourseType.Championship, 65f, 80f, 9, "NationalChamp");
            CreateCourseDefinition("Westminster", "Westminster Agility Kings", CourseType.Championship, 70f, 85f, 10, "Westminster");
            CreateCourseDefinition("JumpersBasic", "Jumpers With Weaves - Basic", CourseType.JumpersWithWeaves, 40f, 55f, 4, "JumpersBasic");
            CreateCourseDefinition("JumpersAdvanced", "Jumpers With Weaves - Advanced", CourseType.JumpersWithWeaves, 50f, 65f, 7, "JumpersAdvanced");

            AssetDatabase.SaveAssets();
            Debug.Log("[DataGenerator] Created 8 course assets");
        }

        private static void CreateCourseDefinition(string name, string courseName, CourseType type, float standardTime, float maxTime, int difficulty, string venue)
        {
            string assetPath = $"Assets/Data/Courses/{name}.asset";
            
            if (AssetDatabase.LoadAssetAtPath<CourseDefinition>(assetPath) != null)
            {
                return;
            }

            var course = ScriptableObject.CreateInstance<CourseDefinition>();
            course.courseName = courseName;
            course.courseType = type;
            course.standardTime = standardTime;
            course.maximumTime = maxTime;
            course.difficultyRating = difficulty;
            course.venueName = venue;

            AssetDatabase.CreateAsset(course, assetPath);
        }

        private static void GenerateHandlerAssets()
        {
            CreateHandlerData("PlayerHandler", "Alex", 5, 5, 5, 5);
            CreateHandlerData("HandlerSarah", "Sarah Mitchell", 6, 7, 6, 6);
            CreateHandlerData("HandlerMike", "Mike Thompson", 7, 5, 7, 5);
            CreateHandlerData("HandlerEmma", "Emma Davis", 5, 8, 5, 7);
            CreateHandlerData("HandlerJames", "James Wilson", 8, 6, 8, 4);

            AssetDatabase.SaveAssets();
            Debug.Log("[DataGenerator] Created 5 handler assets");
        }

        private static void CreateHandlerData(string name, string displayName, int speed, int command, int course, int pressure)
        {
            string assetPath = $"Assets/Data/Handlers/{name}.asset";
            
            if (AssetDatabase.LoadAssetAtPath<HandlerData>(assetPath) != null)
            {
                return;
            }

            var handler = ScriptableObject.CreateInstance<HandlerData>();
            handler.handlerName = name;
            handler.displayName = displayName;
            handler.speedStat = speed;
            handler.commandPrecisionStat = command;
            handler.courseReadingStat = course;
            handler.pressureHandlingStat = pressure;
            handler.backstory = $"A dedicated handler who has been competing in agility shows for years.";

            AssetDatabase.CreateAsset(handler, assetPath);
        }
    }
}
#endif
