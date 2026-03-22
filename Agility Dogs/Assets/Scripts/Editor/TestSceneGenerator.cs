using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Gameplay;
using AgilityDogs.Services;
using AgilityDogs.Presentation.Camera;
using AgilityDogs.Gameplay.Replay;

namespace AgilityDogs.Editor
{
    public class TestSceneGenerator : EditorWindow
    {
        [MenuItem("Agility Dogs/Generate Test Scene")]
        public static void ShowWindow()
        {
            GetWindow<TestSceneGenerator>("Test Scene Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Test Scene Generator", EditorStyles.boldLabel);
            GUILayout.Label("Creates a basic test scene with handler, dog, NavMesh, and obstacles.");
            
            if (GUILayout.Button("Generate Test Scene"))
            {
                GenerateTestScene();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Scene will include:");
            GUILayout.Label("- Ground plane with NavMesh");
            GUILayout.Label("- Handler with CharacterController");
            GUILayout.Label("- Dog with NavMeshAgent");
            GUILayout.Label("- 3 sample obstacles (BarJump, Tunnel, WeavePoles)");
            GUILayout.Label("- Main Camera with AgilityCameraController");
            GUILayout.Label("- GameManager and SceneBootstrap");
            GUILayout.Label("- Basic lighting");
            
            GUILayout.Space(10);
            if (GUILayout.Button("Bake NavMesh (Current Scene)"))
            {
                BakeNavMeshForCurrentScene();
            }
            GUILayout.Label("Bakes NavMesh for the current scene's NavMeshSurface components.");
        }

        private static void GenerateTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5); // 50x50 units
            
            // Add NavMeshSurface component for baking
            NavMeshSurface navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navMeshSurface.agentTypeID = 0; // Default agent type
            navMeshSurface.agentRadius = 0.3f; // Match dog NavMeshAgent radius
            navMeshSurface.agentHeight = 0.5f; // Match dog NavMeshAgent height
            navMeshSurface.agentSlope = 45f;
            navMeshSurface.agentClimb = 0.4f;
            navMeshSurface.minRegionArea = 2;
            
            // Create handler
            GameObject handler = CreateHandler();
            
            // Create dog
            GameObject dog = CreateDog();
            
            // Create obstacles
            GameObject obstaclesParent = new GameObject("Obstacles");
            CreateObstacles(obstaclesParent.transform);
            
            // Create camera
            GameObject cameraObj = CreateCamera();
            
            // Create game systems
            GameObject gameManager = CreateGameManager();
            GameObject sceneBootstrap = CreateSceneBootstrap();
            GameObject replayManager = CreateReplayManager();
            GameObject commentarySystem = CreateCommentarySystem();
            
            // Create event system (if needed)
            CreateEventSystem();
            
            // Set up lighting
            SetupLighting();
            
            // Save scene
            string scenePath = "Assets/Scenes/TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"Test scene generated and saved to {scenePath}");
            Debug.Log("Remember to:");
            Debug.Log("1. Bake the NavMesh (Window > AI > Navigation > Bake)");
            Debug.Log("2. Assign BreedData asset to DogAgentController");
            Debug.Log("3. Assign obstacle references in CourseRunner");
            Debug.Log("4. Set up input actions in InputSystem_Actions");
            Debug.Log("5. Configure .env file with ElevenLabs API key and voice IDs");
            Debug.Log("6. Set up Eastworld agents via Agent Studio and assign UUIDs in CommentaryManager");
            
            // Select the scene
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
        }

        private static GameObject CreateHandler()
        {
            GameObject handler = new GameObject("Handler");
            handler.transform.position = new Vector3(0, 0, -5);
            
            // Add CharacterController
            CharacterController cc = handler.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);
            
            // Add HandlerController
            HandlerController hc = handler.AddComponent<HandlerController>();
            
            // Add visual representation (capsule)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(handler.transform);
            visual.transform.localPosition = new Vector3(0, 0.9f, 0);
            visual.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
            visual.name = "Visual";
            
            // Remove collider from visual (CharacterController handles collision)
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            
            return handler;
        }

        private static GameObject CreateDog()
        {
            GameObject dog = new GameObject("Dog");
            dog.transform.position = new Vector3(2, 0, -5);
            
            // Add NavMeshAgent
            NavMeshAgent agent = dog.AddComponent<NavMeshAgent>();
            agent.height = 0.5f;
            agent.radius = 0.3f;
            agent.speed = 5f;
            agent.angularSpeed = 180f;
            agent.acceleration = 8f;
            
            // Add DogAgentController
            DogAgentController dac = dog.AddComponent<DogAgentController>();
            
            // Add visual representation (cube as placeholder)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(dog.transform);
            visual.transform.localPosition = new Vector3(0, 0.25f, 0);
            visual.transform.localScale = new Vector3(0.6f, 0.5f, 1.0f);
            visual.name = "Visual";
            
            // Remove collider from visual
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            
            // Tag the dog
            dog.tag = "Dog";
            dog.layer = LayerMask.NameToLayer("Dog");
            
            return dog;
        }

        private static void CreateObstacles(Transform parent)
        {
            // Bar Jump
            GameObject barJump = CreateBarJump();
            barJump.transform.SetParent(parent);
            barJump.transform.position = new Vector3(0, 0, 0);
            barJump.name = "BarJump";
            
            // Tunnel
            GameObject tunnel = CreateTunnel();
            tunnel.transform.SetParent(parent);
            tunnel.transform.position = new Vector3(10, 0, 0);
            tunnel.name = "Tunnel";
            
            // Weave Poles
            GameObject weavePoles = CreateWeavePoles();
            weavePoles.transform.SetParent(parent);
            weavePoles.transform.position = new Vector3(20, 0, 0);
            weavePoles.name = "WeavePoles";
        }

        private static GameObject CreateBarJump()
        {
            GameObject barJump = new GameObject("BarJump_Obstacle");
            
            // Add ObstacleBase (concrete type will be added automatically)
            BarJumpObstacle obstacle = barJump.AddComponent<BarJumpObstacle>();
            
            // Create visual
            GameObject base1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            base1.transform.SetParent(barJump.transform);
            base1.transform.localPosition = new Vector3(-0.5f, 0.25f, 0);
            base1.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            
            GameObject base2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            base2.transform.SetParent(barJump.transform);
            base2.transform.localPosition = new Vector3(0.5f, 0.25f, 0);
            base2.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.transform.SetParent(barJump.transform);
            bar.transform.localPosition = new Vector3(0, 0.5f, 0);
            bar.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            bar.transform.localRotation = Quaternion.Euler(0, 0, 90);
            
            // Tag and layer
            barJump.tag = "Obstacle";
            barJump.layer = LayerMask.NameToLayer("Obstacle");
            
            return barJump;
        }

        private static GameObject CreateTunnel()
        {
            GameObject tunnel = new GameObject("Tunnel_Obstacle");
            
            // Add TunnelObstacle
            TunnelObstacle obstacle = tunnel.AddComponent<TunnelObstacle>();
            
            // Create visual (cylinder)
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.SetParent(tunnel.transform);
            cylinder.transform.localPosition = new Vector3(0, 0.5f, 0);
            cylinder.transform.localScale = new Vector3(2, 3, 2);
            cylinder.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Tag and layer
            tunnel.tag = "Obstacle";
            tunnel.layer = LayerMask.NameToLayer("Obstacle");
            
            return tunnel;
        }

        private static GameObject CreateWeavePoles()
        {
            GameObject weavePoles = new GameObject("WeavePoles_Obstacle");
            
            // Add WeavePolesObstacle
            WeavePolesObstacle obstacle = weavePoles.AddComponent<WeavePolesObstacle>();
            
            // Create 12 poles
            for (int i = 0; i < 12; i++)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(weavePoles.transform);
                pole.transform.localPosition = new Vector3(i * 0.6f, 0.5f, 0);
                pole.transform.localScale = new Vector3(0.05f, 1.0f, 0.05f);
            }
            
            // Tag and layer
            weavePoles.tag = "Obstacle";
            weavePoles.layer = LayerMask.NameToLayer("Obstacle");
            
            return weavePoles;
        }

        private static GameObject CreateCamera()
        {
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            camera.fieldOfView = 60;
            
            // Add AudioListener
            cameraObj.AddComponent<AudioListener>();
            
            // Add AgilityCameraController
            AgilityCameraController acc = cameraObj.AddComponent<AgilityCameraController>();
            
            // Position camera
            cameraObj.transform.position = new Vector3(0, 10, -15);
            cameraObj.transform.rotation = Quaternion.Euler(30, 0, 0);
            
            // Tag as MainCamera
            cameraObj.tag = "MainCamera";
            
            return cameraObj;
        }

        private static GameObject CreateGameManager()
        {
            GameObject gameManager = new GameObject("GameManager");
            GameManager gm = gameManager.AddComponent<GameManager>();
            
            return gameManager;
        }

        private static GameObject CreateSceneBootstrap()
        {
            GameObject sceneBootstrap = new GameObject("SceneBootstrap");
            SceneBootstrap sb = sceneBootstrap.AddComponent<SceneBootstrap>();
            
            return sceneBootstrap;
        }

        private static GameObject CreateReplayManager()
        {
            GameObject replayManagerObj = new GameObject("ReplayManager");
            ReplayManager rm = replayManagerObj.AddComponent<ReplayManager>();
            
            // Find dog and handler in scene to assign references
            DogAgentController dog = Object.FindObjectOfType<DogAgentController>();
            HandlerController handler = Object.FindObjectOfType<HandlerController>();
            
            // Use reflection to set private fields (since they are SerializeField)
            // For simplicity, we'll just rely on auto-find in ReplayManager's Start
            // The ReplayManager will find them via FindObjectOfType
            
            return replayManagerObj;
        }

        private static GameObject CreateCommentarySystem()
        {
            GameObject commentaryObj = new GameObject("CommentarySystem");
            
            // Add required services
            ElevenLabsService elevenLabs = commentaryObj.AddComponent<ElevenLabsService>();
            EastworldClient eastworld = commentaryObj.AddComponent<EastworldClient>();
            CommentaryManager commentaryManager = commentaryObj.AddComponent<CommentaryManager>();
            
            // Add AudioSources for two announcers
            AudioSource mainAnnouncerSource = commentaryObj.AddComponent<AudioSource>();
            AudioSource colorCommentatorSource = commentaryObj.AddComponent<AudioSource>();
            
            // Configure AudioSources
            mainAnnouncerSource.playOnAwake = false;
            colorCommentatorSource.playOnAwake = false;
            mainAnnouncerSource.spatialBlend = 0f; // 2D
            colorCommentatorSource.spatialBlend = 0f;
            
            // Assign references via reflection (since they are private SerializeField)
            // We'll rely on FindObjectOfType in Awake for now
            // The CommentaryManager will find the ElevenLabsService and EastworldClient on the same GameObject
            
            return commentaryObj;
        }

        private static void CreateEventSystem()
        {
            // Check if EventSystem exists
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static void SetupLighting()
        {
            // Create directional light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, 0.95f, 0.84f);
            light.intensity = 1.0f;
            light.shadows = LightShadows.Soft;
            
            // Position and rotate light
            lightObj.transform.position = new Vector3(0, 10, 0);
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            // Set ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
        }

        private static void BakeNavMeshForCurrentScene()
        {
            // Find all NavMeshSurface components in the scene
            NavMeshSurface[] surfaces = Object.FindObjectsOfType<NavMeshSurface>();
            
            if (surfaces.Length == 0)
            {
                Debug.LogWarning("No NavMeshSurface components found in the current scene.");
                return;
            }
            
            foreach (NavMeshSurface surface in surfaces)
            {
                surface.BuildNavMesh();
                Debug.Log($"NavMesh baked for surface: {surface.gameObject.name}");
            }
            
            Debug.Log($"Total {surfaces.Length} NavMeshSurface(s) baked.");
        }
    }
}