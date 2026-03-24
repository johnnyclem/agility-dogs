using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Services;

namespace AgilityDogs.Runtime
{
    /// <summary>
    /// Runtime bootstrapper that configures the game for all modes.
    /// This script should be placed in each gameplay scene and in the StartMenu scene.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetup = true;
        
        [Header("Scene Configuration")]
        [SerializeField] private string menuSceneName = "StartMenu";
        [SerializeField] private bool isGameplayScene = false;
        [SerializeField] private bool isTrainingScene = false;
        
        [Header("Data References")]
        [SerializeField] private BreedData[] availableBreeds;
        [SerializeField] private HandlerData[] availableHandlers;
        [SerializeField] private CourseDefinition[] availableCourses;
        
        [Header("Dog Spawn Configuration")]
        [SerializeField] private Transform dogSpawnPoint;
        [SerializeField] private Transform handlerSpawnPoint;
        
        [Header("Course Configuration")]
        [SerializeField] private CourseDefinition defaultCourse;
        
        private static bool isInitialized = false;
        
        private void Awake()
        {
            if (autoSetup)
            {
                SetupGame();
            }
        }
        
        private void Start()
        {
            if (!isInitialized)
            {
                SetupGame();
            }
        }
        
        private void SetupGame()
        {
            if (isInitialized) return;
            
            Debug.Log("[GameBootstrapper] Setting up game configuration...");
            
            // Ensure GameManager exists
            EnsureGameManager();
            
            // Ensure GameModeManager exists
            EnsureGameModeManager();
            
            // Ensure TransitionManager exists
            EnsureTransitionManager();
            
            // Load data from Resources if not assigned
            LoadDataReferences();
            
            isInitialized = true;
            Debug.Log("[GameBootstrapper] Game setup complete.");
        }
        
        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
                Debug.Log("[GameBootstrapper] Created GameManager");
            }
        }
        
        private void EnsureGameModeManager()
        {
            if (GameModeManager.Instance == null)
            {
                var go = new GameObject("GameModeManager");
                go.AddComponent<GameModeManager>();
                Debug.Log("[GameBootstrapper] Created GameModeManager");
            }
        }
        
        private void EnsureTransitionManager()
        {
            if (TransitionManager.Instance == null)
            {
                var go = new GameObject("TransitionManager");
                var tm = go.AddComponent<TransitionManager>();
                Debug.Log("[GameBootstrapper] Created TransitionManager");
            }
        }
        
        private void LoadDataReferences()
        {
            // Load breeds if not assigned
            if (availableBreeds == null || availableBreeds.Length == 0)
            {
                availableBreeds = Resources.LoadAll<BreedData>("Data/Breeds");
                Debug.Log($"[GameBootstrapper] Loaded {availableBreeds.Length} breeds");
            }
            
            // Load handlers if not assigned
            if (availableHandlers == null || availableHandlers.Length == 0)
            {
                availableHandlers = Resources.LoadAll<HandlerData>("Data/Handlers");
                Debug.Log($"[GameBootstrapper] Loaded {availableHandlers.Length} handlers");
            }
            
            // Load courses if not assigned
            if (availableCourses == null || availableCourses.Length == 0)
            {
                availableCourses = Resources.LoadAll<CourseDefinition>("Data/Courses");
                Debug.Log($"[GameBootstrapper] Loaded {availableCourses.Length} courses");
            }
        }
        
        /// <summary>
        /// Get the current breed data array
        /// </summary>
        public static BreedData[] GetAvailableBreeds()
        {
            return Resources.LoadAll<BreedData>("Data/Breeds");
        }
        
        /// <summary>
        /// Get breed by name
        /// </summary>
        public static BreedData GetBreedByName(string breedName)
        {
            var breeds = Resources.LoadAll<BreedData>("Data/Breeds");
            foreach (var breed in breeds)
            {
                if (breed.breedName == breedName || breed.displayName == breedName)
                {
                    return breed;
                }
            }
            return breeds.Length > 0 ? breeds[0] : null;
        }
        
        /// <summary>
        /// Get the first available course
        /// </summary>
        public static CourseDefinition GetDefaultCourse()
        {
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
            return courses.Length > 0 ? courses[0] : null;
        }
        
        /// <summary>
        /// Reset initialization flag (useful for testing)
        /// </summary>
        public static void ResetInit()
        {
            isInitialized = false;
        }
    }
}
