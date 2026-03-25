using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Presentation.Camera;
using AgilityDogs.Services;
using AgilityDogs.UI;

namespace AgilityDogs.Runtime
{
    /// <summary>
    /// Configures a gameplay scene for competition mode.
    /// This handles spawning the handler, dog, obstacles, and setting up the HUD.
    /// </summary>
    public class CompetitionSceneConfigurator : MonoBehaviour
    {
        [Header("Scene Type")]
        [SerializeField] private SceneType sceneType = SceneType.Competition;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform handlerSpawnPoint;
        [SerializeField] private Transform dogSpawnPoint;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject handlerPrefab;
        [SerializeField] private GameObject[] dogPrefabs;
        
        [Header("Course")]
        [SerializeField] private CourseDefinition competitionCourse;
        [SerializeField] private ObstacleBase[] competitionObstacles;
        
        [Header("Arena")]
        [SerializeField] private Transform arenaTransform;
        
        [Header("HUD")]
        [SerializeField] private GameObject hudPrefab;
        
        private GameObject handlerInstance;
        private GameObject dogInstance;
        private HandlerController handlerController;
        private DogAgentController dogController;
        private CourseRunner courseRunner;
        private AgilityScoringService scoringService;
        private AgilityCameraController cameraController;
        private GameHUD hudInstance;
        
        public enum SceneType
        {
            Competition,
            Training,
            Career
        }
        
        private void Awake()
        {
            ConfigureScene();
        }
        
        private void Start()
        {
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.SetCourse(competitionCourse);
            }
        }
        
        private void ConfigureScene()
        {
            Debug.Log($"[CompetitionSceneConfigurator] Configuring {sceneType} scene...");
            
            // Find or create required managers
            EnsureRequiredManagers();
            
            // Spawn handler
            SpawnHandler();
            
            // Spawn dog
            SpawnDog();
            
            // Wire up references
            WireReferences();
            
            // Setup course
            SetupCourse();
            
            // Setup camera
            SetupCamera();
            
            Debug.Log($"[CompetitionSceneConfigurator] Scene configuration complete.");
        }
        
        private void EnsureRequiredManagers()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager").AddComponent<GameManager>();
            }
            
            // GameModeManager  
            if (GameModeManager.Instance == null)
            {
                var gmm = new GameObject("GameModeManager").AddComponent<GameModeManager>();
            }
            
            // TransitionManager
            if (TransitionManager.Instance == null)
            {
                var tm = new GameObject("TransitionManager").AddComponent<TransitionManager>();
            }
            
            // Scoring service
            if (FindObjectOfType<AgilityScoringService>() == null)
            {
                var scoringObj = new GameObject("ScoringService");
                scoringService = scoringObj.AddComponent<AgilityScoringService>();
            }
            else
            {
                scoringService = FindObjectOfType<AgilityScoringService>();
            }
            
            // Course runner
            if (FindObjectOfType<CourseRunner>() == null)
            {
                var courseObj = new GameObject("CourseRunner");
                courseRunner = courseObj.AddComponent<CourseRunner>();
            }
            else
            {
                courseRunner = FindObjectOfType<CourseRunner>();
            }
        }
        
        private void SpawnHandler()
        {
            if (handlerPrefab == null)
            {
                Debug.LogWarning("[CompetitionSceneConfigurator] No handler prefab assigned!");
                return;
            }
            
            Vector3 spawnPos = handlerSpawnPoint != null ? handlerSpawnPoint.position : Vector3.zero;
            Quaternion spawnRot = handlerSpawnPoint != null ? handlerSpawnPoint.rotation : Quaternion.identity;
            
            handlerInstance = Instantiate(handlerPrefab, spawnPos, spawnRot);
            handlerController = handlerInstance.GetComponent<HandlerController>();
            
            if (handlerController == null)
            {
                handlerController = handlerInstance.AddComponent<HandlerController>();
            }
            
            Debug.Log("[CompetitionSceneConfigurator] Handler spawned.");
        }
        
        private void SpawnDog()
        {
            // Get the selected breed from GameModeManager
            BreedData selectedBreed = null;
            if (GameModeManager.Instance != null)
            {
                selectedBreed = GameModeManager.Instance.SelectedDog;
            }
            
            // Fallback to first available breed
            if (selectedBreed == null)
            {
                var breeds = Resources.LoadAll<BreedData>("Data/Breeds");
                if (breeds.Length > 0)
                {
                    selectedBreed = breeds[0];
                }
            }
            
            // Find the prefab index
            int prefabIndex = 0;
            if (selectedBreed != null && selectedBreed.prefab != null)
            {
                for (int i = 0; i < dogPrefabs.Length; i++)
                {
                    if (dogPrefabs[i] != null && dogPrefabs[i].name.Contains(selectedBreed.breedName.Replace(" ", "")))
                    {
                        prefabIndex = i;
                        break;
                    }
                }
            }
            
            if (dogPrefabs == null || dogPrefabs.Length == 0 || dogPrefabs[prefabIndex] == null)
            {
                Debug.LogWarning("[CompetitionSceneConfigurator] No dog prefabs assigned!");
                return;
            }
            
            Vector3 spawnPos = dogSpawnPoint != null ? dogSpawnPoint.position : Vector3.zero;
            Quaternion spawnRot = dogSpawnPoint != null ? dogSpawnPoint.rotation : Quaternion.identity;
            
            dogInstance = Instantiate(dogPrefabs[prefabIndex], spawnPos, spawnRot);
            
            // Apply breed scale
            if (selectedBreed != null)
            {
                dogInstance.transform.localScale *= selectedBreed.modelScale;
            }
            
            dogController = dogInstance.GetComponent<DogAgentController>();
            if (dogController == null)
            {
                dogController = dogInstance.AddComponent<DogAgentController>();
            }
            
            // Set handler reference for the dog
            if (handlerController != null)
            {
                dogController.SetHandler(handlerController.transform);
            }
            
            Debug.Log("[CompetitionSceneConfigurator] Dog spawned: " + (selectedBreed?.breedName ?? "Unknown"));
        }
        
        private void WireReferences()
        {
            // Wire handler to dog
            if (handlerController != null && dogController != null)
            {
                // Handler needs reference to dog for following/commands
                var handlerDogLink = handlerController.GetComponent<HandlerDogLink>();
                if (handlerDogLink == null)
                {
                    handlerDogLink = handlerController.gameObject.AddComponent<HandlerDogLink>();
                }
                handlerDogLink.SetDog(dogController);
            }
            
            // Wire course runner
            if (courseRunner != null && handlerController != null && dogController != null)
            {
                courseRunner.SetHandlerAndDog(handlerController, dogController);
            }
            
            // Wire scoring
            if (scoringService != null && dogController != null)
            {
                scoringService.SetDog(dogController);
            }
        }
        
        private void SetupCourse()
        {
            if (courseRunner != null && competitionCourse != null)
            {
                courseRunner.LoadCourse(competitionCourse);
            }
        }
        
        private void SetupCamera()
        {
            // Find or create camera controller
            cameraController = FindObjectOfType<AgilityCameraController>();
            if (cameraController == null)
            {
                var camObj = new GameObject("AgilityCamera");
                var cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                cameraController = camObj.AddComponent<AgilityCameraController>();
            }
            
            if (handlerController != null)
            {
                cameraController.SetTarget(handlerController.transform);
            }
        }
        
        /// <summary>
        /// Start the competition countdown
        /// </summary>
        public void StartCompetition()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCountdown();
            }
        }
    }
    
    /// <summary>
    /// Simple component to link handler with dog for command communication
    /// </summary>
    public class HandlerDogLink : MonoBehaviour
    {
        [SerializeField] private DogAgentController dog;
        
        public void SetDog(DogAgentController dogController)
        {
            dog = dogController;
        }
        
        public DogAgentController GetDog()
        {
            return dog;
        }
        
        public void SendCommand(HandlerCommand command)
        {
            if (dog != null)
            {
                dog.ReceiveCommand(command);
            }
        }
    }
}
