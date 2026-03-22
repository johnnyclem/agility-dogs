using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Gameplay;
using AgilityDogs.Services;
using AgilityDogs.Presentation.Camera;
using AgilityDogs.Gameplay.Replay;
using AgilityDogs.Gameplay.Commands;

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
            GUILayout.Label("Creates a runnable test scene with all gameplay systems.");

            if (GUILayout.Button("Generate Test Scene"))
            {
                GenerateTestScene();
            }

            GUILayout.Space(10);
            GUILayout.Label("Scene will include:");
            GUILayout.Label("- Ground plane with NavMesh (auto-baked)");
            GUILayout.Label("- Handler with input, commands, CharacterController");
            GUILayout.Label("- Dog with NavMeshAgent and Animator");
            GUILayout.Label("- 3 obstacles (BarJump, Tunnel, WeavePoles) with trigger zones");
            GUILayout.Label("- CourseRunner + AgilityScoringService");
            GUILayout.Label("- Main Camera with AgilityCameraController");
            GUILayout.Label("- GameManager, SceneBootstrap, ReplayManager");
            GUILayout.Label("- DevTestRunner (auto-starts gameplay, HUD)");
            GUILayout.Label("- Basic lighting");

            GUILayout.Space(10);
            if (GUILayout.Button("Bake NavMesh (Current Scene)"))
            {
                BakeNavMeshForCurrentScene();
            }
        }

        private static void GenerateTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ground with NavMesh
            GameObject ground = CreateGround();

            // Handler (player-controlled)
            GameObject handler = CreateHandler();

            // Dog (AI-controlled)
            GameObject dog = CreateDog();

            // Obstacles
            GameObject obstaclesParent = new GameObject("Obstacles");
            CreateObstacles(obstaclesParent.transform);

            // Camera
            GameObject cameraObj = CreateCamera();

            // Game systems
            GameObject gameManager = CreateGameManager();
            GameObject courseRunner = CreateCourseRunner();
            GameObject sceneBootstrap = CreateSceneBootstrap();
            GameObject replayManager = CreateReplayManager();

            // Dev test runner (auto-starts gameplay)
            GameObject devRunner = new GameObject("DevTestRunner");
            devRunner.AddComponent<DevTestRunner>();

            // Event system
            CreateEventSystem();

            // Lighting
            SetupLighting();

            // Bake NavMesh
            NavMeshSurface[] surfaces = Object.FindObjectsOfType<NavMeshSurface>();
            foreach (var surface in surfaces)
            {
                surface.BuildNavMesh();
            }

            // Save scene
            string scenePath = "Assets/Scenes/TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Test scene generated at {scenePath}");
            Debug.Log("Hit Play to test! WASD to move handler, Space=Jump, E=Go, Q=ComeBye, Tab=Away");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
        }

        private static GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5); // 50x50 units

            // Green-ish material
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.3f, 0.55f, 0.2f);
                renderer.material = mat;
            }

            NavMeshSurface navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navMeshSurface.agentTypeID = 0;
            navMeshSurface.agentRadius = 0.3f;
            navMeshSurface.agentHeight = 0.5f;
            navMeshSurface.agentSlope = 45f;
            navMeshSurface.agentClimb = 0.4f;
            navMeshSurface.minRegionArea = 2;

            return ground;
        }

        private static GameObject CreateHandler()
        {
            GameObject handler = new GameObject("Handler");
            handler.transform.position = new Vector3(0, 0, -5);

            // CharacterController
            CharacterController cc = handler.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // HandlerController
            handler.AddComponent<HandlerController>();

            // Command system
            CommandBuffer cmdBuffer = handler.AddComponent<CommandBuffer>();
            handler.AddComponent<CommandInputHandler>();

            // PlayerInput - load InputSystem_Actions asset
            PlayerInput playerInput = handler.AddComponent<PlayerInput>();
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/InputSystem_Actions.inputactions");
            if (inputAsset != null)
            {
                playerInput.actions = inputAsset;
                playerInput.defaultActionMap = "Player";
                playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            }
            else
            {
                Debug.LogWarning("InputSystem_Actions.inputactions not found. Assign manually.");
            }

            // Visual (capsule placeholder)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(handler.transform);
            visual.transform.localPosition = new Vector3(0, 0.9f, 0);
            visual.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            // Blue material for handler
            var rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.4f, 0.8f);
                rend.material = mat;
            }

            return handler;
        }

        private static GameObject CreateDog()
        {
            GameObject dog = new GameObject("Dog");
            dog.transform.position = new Vector3(1.5f, 0, -5);

            // NavMeshAgent
            NavMeshAgent agent = dog.AddComponent<NavMeshAgent>();
            agent.height = 0.5f;
            agent.radius = 0.3f;
            agent.speed = 7f;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.3f;

            // DogAgentController (auto-adds Animator via RequireComponent)
            dog.AddComponent<DogAgentController>();

            // Collider for trigger zone detection
            CapsuleCollider col = dog.AddComponent<CapsuleCollider>();
            col.height = 0.6f;
            col.radius = 0.25f;
            col.center = new Vector3(0, 0.3f, 0);
            col.isTrigger = false;

            // Rigidbody (kinematic, for trigger interactions)
            Rigidbody rb = dog.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Visual (cube placeholder shaped like a dog body)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(dog.transform);
            visual.transform.localPosition = new Vector3(0, 0.25f, 0);
            visual.transform.localScale = new Vector3(0.4f, 0.4f, 0.8f);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            // Orange material for dog
            var rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.85f, 0.5f, 0.15f);
                rend.material = mat;
            }

            // Head indicator (so you can see which way the dog faces)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(dog.transform);
            head.transform.localPosition = new Vector3(0, 0.35f, 0.45f);
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());

            var headRend = head.GetComponent<Renderer>();
            if (headRend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.75f, 0.4f, 0.1f);
                headRend.material = mat;
            }

            return dog;
        }

        private static void CreateObstacles(Transform parent)
        {
            // Lay out obstacles in a course pattern
            CreateBarJump(parent, new Vector3(0, 0, 5), "BarJump_1");
            CreateBarJump(parent, new Vector3(5, 0, 10), "BarJump_2");
            CreateTunnel(parent, new Vector3(10, 0, 5), "Tunnel_1");
            CreateWeavePoles(parent, new Vector3(18, 0, 10), "WeavePoles_1");
        }

        private static void CreateBarJump(Transform parent, Vector3 position, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = position;

            BarJumpObstacle obstacle = obj.AddComponent<BarJumpObstacle>();

            // Uprights
            GameObject post1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post1.transform.SetParent(obj.transform);
            post1.transform.localPosition = new Vector3(-0.6f, 0.4f, 0);
            post1.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            post1.name = "Post_L";
            Object.DestroyImmediate(post1.GetComponent<Collider>());

            GameObject post2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post2.transform.SetParent(obj.transform);
            post2.transform.localPosition = new Vector3(0.6f, 0.4f, 0);
            post2.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            post2.name = "Post_R";
            Object.DestroyImmediate(post2.GetComponent<Collider>());

            // Bar
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.transform.SetParent(obj.transform);
            bar.transform.localPosition = new Vector3(0, 0.5f, 0);
            bar.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
            bar.transform.localRotation = Quaternion.Euler(0, 0, 90);
            bar.name = "Bar";
            Object.DestroyImmediate(bar.GetComponent<Collider>());

            SetObstacleMaterial(post1, Color.white);
            SetObstacleMaterial(post2, Color.white);
            SetObstacleMaterial(bar, Color.red);

            // Navigation points
            CreateNavPoints(obj, entryOffset: new Vector3(0, 0, -2f),
                commitOffset: new Vector3(0, 0, -1f),
                exitOffset: new Vector3(0, 0, 2f));

            // Trigger zone
            CreateTriggerZone(obj, new Vector3(0, 0.5f, 0), new Vector3(2f, 1.5f, 1f));
        }

        private static void CreateTunnel(Transform parent, Vector3 position, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = position;

            obj.AddComponent<TunnelObstacle>();

            // Tunnel visual (cylinder on its side)
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.SetParent(obj.transform);
            cylinder.transform.localPosition = new Vector3(0, 0.5f, 0);
            cylinder.transform.localScale = new Vector3(1.2f, 2.5f, 1.2f);
            cylinder.transform.localRotation = Quaternion.Euler(90, 0, 0);
            cylinder.name = "TunnelBody";
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());

            SetObstacleMaterial(cylinder, new Color(0.3f, 0.3f, 0.8f, 0.7f));

            CreateNavPoints(obj, entryOffset: new Vector3(0, 0, -3f),
                commitOffset: new Vector3(0, 0, -2f),
                exitOffset: new Vector3(0, 0, 3f));

            CreateTriggerZone(obj, new Vector3(0, 0.5f, 0), new Vector3(1.5f, 1.5f, 5f));
        }

        private static void CreateWeavePoles(Transform parent, Vector3 position, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = position;

            obj.AddComponent<WeavePolesObstacle>();

            // 12 poles spaced along X
            for (int i = 0; i < 12; i++)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(obj.transform);
                pole.transform.localPosition = new Vector3(i * 0.6f, 0.5f, 0);
                pole.transform.localScale = new Vector3(0.06f, 1.0f, 0.06f);
                pole.name = $"Pole_{i}";
                Object.DestroyImmediate(pole.GetComponent<Collider>());

                SetObstacleMaterial(pole, i % 2 == 0 ? Color.red : Color.white);
            }

            float totalLength = 11 * 0.6f;
            float midX = totalLength / 2f;

            CreateNavPoints(obj,
                entryOffset: new Vector3(-1f, 0, 0),
                commitOffset: new Vector3(0, 0, 0),
                exitOffset: new Vector3(totalLength + 1f, 0, 0));

            CreateTriggerZone(obj, new Vector3(midX, 0.5f, 0),
                new Vector3(totalLength + 2f, 1.5f, 1.5f));
        }

        private static void CreateNavPoints(GameObject obstacle,
            Vector3 entryOffset, Vector3 commitOffset, Vector3 exitOffset)
        {
            GameObject entry = new GameObject("EntryPoint");
            entry.transform.SetParent(obstacle.transform);
            entry.transform.localPosition = entryOffset;

            GameObject commit = new GameObject("CommitPoint");
            commit.transform.SetParent(obstacle.transform);
            commit.transform.localPosition = commitOffset;

            GameObject exit = new GameObject("ExitPoint");
            exit.transform.SetParent(obstacle.transform);
            exit.transform.localPosition = exitOffset;

            GameObject center = new GameObject("CenterPoint");
            center.transform.SetParent(obstacle.transform);
            center.transform.localPosition = Vector3.zero;

            // Wire up via SerializedObject
            ObstacleBase obs = obstacle.GetComponent<ObstacleBase>();
            if (obs != null)
            {
                SerializedObject so = new SerializedObject(obs);
                so.FindProperty("entryPoint").objectReferenceValue = entry.transform;
                so.FindProperty("commitPoint").objectReferenceValue = commit.transform;
                so.FindProperty("exitPoint").objectReferenceValue = exit.transform;
                so.FindProperty("centerPoint").objectReferenceValue = center.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateTriggerZone(GameObject obstacle, Vector3 center, Vector3 size)
        {
            GameObject zone = new GameObject("TriggerZone");
            zone.transform.SetParent(obstacle.transform);
            zone.transform.localPosition = center;

            BoxCollider col = zone.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            col.center = Vector3.zero;

            ObstacleTriggerZone trigger = zone.AddComponent<ObstacleTriggerZone>();

            // Wire up parent obstacle reference
            SerializedObject so = new SerializedObject(trigger);
            so.FindProperty("parentObstacle").objectReferenceValue =
                obstacle.GetComponent<ObstacleBase>();
            so.FindProperty("isCommitZone").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObstacleMaterial(GameObject obj, Color color)
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                rend.material = mat;
            }
        }

        private static GameObject CreateCamera()
        {
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.4f, 0.65f, 0.95f);
            camera.fieldOfView = 60;

            cameraObj.AddComponent<AudioListener>();
            cameraObj.AddComponent<AgilityCameraController>();

            cameraObj.transform.position = new Vector3(0, 10, -15);
            cameraObj.transform.rotation = Quaternion.Euler(30, 0, 0);
            cameraObj.tag = "MainCamera";

            return cameraObj;
        }

        private static GameObject CreateGameManager()
        {
            GameObject obj = new GameObject("GameManager");
            obj.AddComponent<GameManager>();
            return obj;
        }

        private static GameObject CreateCourseRunner()
        {
            GameObject obj = new GameObject("CourseRunner");
            obj.AddComponent<CourseRunner>();
            obj.AddComponent<AgilityScoringService>();
            return obj;
        }

        private static GameObject CreateSceneBootstrap()
        {
            GameObject obj = new GameObject("SceneBootstrap");
            obj.AddComponent<SceneBootstrap>();
            return obj;
        }

        private static GameObject CreateReplayManager()
        {
            GameObject obj = new GameObject("ReplayManager");
            obj.AddComponent<ReplayManager>();
            return obj;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static void SetupLighting()
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, 0.96f, 0.84f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;

            lightObj.transform.position = new Vector3(0, 10, 0);
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
        }

        private static void BakeNavMeshForCurrentScene()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsOfType<NavMeshSurface>();

            if (surfaces.Length == 0)
            {
                Debug.LogWarning("No NavMeshSurface components found in the current scene.");
                return;
            }

            foreach (NavMeshSurface surface in surfaces)
            {
                surface.BuildNavMesh();
                Debug.Log($"NavMesh baked for: {surface.gameObject.name}");
            }
        }
    }
}
