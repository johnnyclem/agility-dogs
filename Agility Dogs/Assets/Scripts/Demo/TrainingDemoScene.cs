using UnityEngine;

namespace AgilityDogs.Demo
{
    // Helper class to track jump state
    public class JumpData : MonoBehaviour
    {
        public float barHeight = 0.9f;
        public Renderer barRenderer;
        public Color originalColor = Color.red;
        public bool dogInZone = false;
        public float dogMaxHeight = 0f;
        
        private float flashTimer = 0f;
        private Color flashColor = Color.white;
        private ParticleSystem particles;
        
        public void TriggerSuccess()
        {
            flashColor = Color.green;
            flashTimer = 0.5f;
            SpawnParticles(Color.green, Color.yellow);
        }
        
        public void TriggerFail()
        {
            flashColor = Color.red;
            flashTimer = 0.5f;
            SpawnParticles(Color.red, new Color(1f, 0.3f, 0f));
        }
        
        void SpawnParticles(Color col1, Color col2)
        {
            if (barRenderer == null) return;
            
            GameObject particleObj = new GameObject("JumpParticles");
            particleObj.transform.position = barRenderer.transform.position + Vector3.up * 0.5f;
            
            particles = particleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = col1;
            main.startSize = 0.15f;
            main.startSpeed = 3f;
            main.startLifetime = 0.8f;
            main.maxParticles = 30;
            main.duration = 0.3f;
            main.loop = false;
            
            var emission = particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(col1, 0f), new GradientColorKey(col2, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;
            
            // Add a light for glow effect
            Light pointLight = particleObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = col1;
            pointLight.intensity = 2f;
            pointLight.range = 3f;
            
            Destroy(particleObj, 1.5f);
        }
        
        void Update()
        {
            if (flashTimer > 0 && barRenderer != null)
            {
                flashTimer -= Time.deltaTime;
                barRenderer.material.color = Color.Lerp(flashColor, originalColor, 1f - (flashTimer / 0.5f));
            }
        }
    }
    
    public class TrainingDemoScene : MonoBehaviour
    {
        [Header("Dog")]
        [SerializeField] private GameObject dogPrefab;
        [SerializeField] private Vector3 dogStartPosition = new Vector3(0f, 0f, 0f);

        [Header("Course Settings")]
        [SerializeField] private float obstacleSpacing = 5f;
        [SerializeField] private int numberOfBarriers = 3;

        private GameObject dog;
        private GameObject[] obstacles;

        void Start()
        {
            SetupQualitySettings();
            SetupLighting();
            SetupGround();
            SetupDog();
            SetupObstacles();
            SetupCamera();

            Debug.Log("Training Demo Scene loaded - use arrow keys to move the dog!");
        }

        void SetupQualitySettings()
        {
            // Enable anti-aliasing
            QualitySettings.antiAliasing = 4;
            
            // Set anisotropic filtering
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            
            // Set v-sync
            QualitySettings.vSyncCount = 1;
            
            // Increase shadow quality
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadows = ShadowQuality.All;
            
            // Set texture quality to maximum
            QualitySettings.globalTextureMipmapLimit = 0;
            
            Debug.Log("Quality settings configured for best visuals");
        }

        // Camera control state
        private float cameraDistance = 4f; // Behind dog
        private float cameraAngleX = 25f; // Slightly elevated
        private float cameraAngleY = 0f;
        private Vector3 cameraTarget = Vector3.zero;
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        private bool cameraFollowsDog = true; // Follow dog by default

        void Update()
        {
            HandleCameraControls();
            if (dog != null)
            {
                HandleDogMovement();
            }
        }

        void HandleCameraControls()
        {
            // Right mouse button to orbit camera
            if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }
            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                cameraAngleY += delta.x * 0.5f;
                cameraAngleX -= delta.y * 0.5f;
                cameraAngleX = Mathf.Clamp(cameraAngleX, 5f, 80f);
                lastMousePosition = Input.mousePosition;
            }

            // Scroll wheel to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            cameraDistance -= scroll * 2f;
            cameraDistance = Mathf.Clamp(cameraDistance, 1.5f, 10f);

            // Follow dog by default
            if (cameraFollowsDog && dog != null)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, dog.transform.position, Time.deltaTime * 10f);
            }

            // Update camera position and rotation
            UpdateCamera();

            // Q/E to pan camera target left/right (when not following)
            if (!cameraFollowsDog)
            {
                if (Input.GetKey(KeyCode.Q))
                    cameraTarget.x -= 3f * Time.deltaTime;
                if (Input.GetKey(KeyCode.E))
                    cameraTarget.x += 3f * Time.deltaTime;
            }

            // R to reset camera
            if (Input.GetKeyDown(KeyCode.R))
            {
                cameraAngleX = 25f;
                cameraAngleY = 0f;
                cameraDistance = 4f;
            }

            // Tab to toggle follow dog
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                cameraFollowsDog = !cameraFollowsDog;
                if (cameraFollowsDog && dog != null)
                {
                    cameraTarget = dog.transform.position;
                    cameraDistance = 4f;
                }
            }
        }

        void UpdateCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Quaternion rotation = Quaternion.Euler(cameraAngleX, cameraAngleY, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -cameraDistance);
            cam.transform.position = cameraTarget + offset;
            cam.transform.LookAt(cameraTarget);
        }

        void SetupLighting()
        {
            // Main directional light
            GameObject mainLight = new GameObject("Main Light");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Fill light
            GameObject fillLight = new GameObject("Fill Light");
            Light fill = fillLight.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.4f;
            fill.color = new Color(0.8f, 0.9f, 1f);
            fillLight.transform.rotation = Quaternion.Euler(30f, 210f, 0f);
        }

        void SetupGround()
        {
            // Main grass area
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Training Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 30f);

            // Apply grass material
            Renderer rend = ground.GetComponent<Renderer>();
            Material grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            grassMat.color = new Color(0.35f, 0.55f, 0.25f);
            grassMat.SetFloat("_Smoothness", 0.2f);
            rend.material = grassMat;

            // Add border fences
            CreateBorderFences();
        }

        void CreateBorderFences()
        {
            // Create simple fence posts along edges
            Material fenceMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            fenceMat.color = new Color(0.6f, 0.4f, 0.2f);

            // Side fences
            for (int i = -3; i <= 3; i++)
            {
                CreateFencePost(new Vector3(-15f, 0.5f, i * 8f), fenceMat);
                CreateFencePost(new Vector3(15f, 0.5f, i * 8f), fenceMat);
            }
        }

        void CreateFencePost(Vector3 position, Material mat)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Fence Post";
            post.transform.position = position;
            post.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
            post.GetComponent<Renderer>().material = mat;
        }

        void SetupDog()
        {
            // Try to load dog prefab if not assigned
            if (dogPrefab == null)
            {
                dogPrefab = LoadDefaultDogPrefab();
            }
            
            if (dogPrefab != null)
            {
                dog = Instantiate(dogPrefab, dogStartPosition, Quaternion.identity);
                dog.name = "Training Dog";
                FixMaterialsForURP(dog);
                
                // Scale the dog appropriately
                dog.transform.localScale = Vector3.one * 1.5f;
                
                Debug.Log($"Loaded dog prefab: {dogPrefab.name}");
                currentDogBreed = dogPrefab.name.Split('_')[0]; // Get breed name
            }
            else
            {
                dog = CreatePlaceholderDog();
                Debug.Log("No dog prefab found, using placeholder");
                currentDogBreed = "Placeholder";
            }
            
            // Add physics components to dog
            AddDogPhysics();
            
            // Debug animation state
            DebugAnimationState();
        }
        
        void DebugAnimationState()
        {
            if (dog == null) return;
            
            Animator animator = dog.GetComponent<Animator>();
            Debug.Log("=== Animation Debug Info ===");
            Debug.Log($"Dog name: {dog.name}");
            Debug.Log($"Has Animator: {animator != null}");
            
            if (animator != null)
            {
                Debug.Log($"Has Avatar: {animator.avatar != null}");
                if (animator.avatar != null)
                {
                    Debug.Log($"Avatar name: {animator.avatar.name}");
                    Debug.Log($"Avatar isHuman: {animator.avatar.isHuman}");
                }
                Debug.Log($"Has Controller: {animator.runtimeAnimatorController != null}");
                if (animator.runtimeAnimatorController != null)
                {
                    Debug.Log($"Controller name: {animator.runtimeAnimatorController.name}");
                }
                Debug.Log($"Animator enabled: {animator.enabled}");
                Debug.Log($"Has baked animations: {hasBakedAnimations}");
            }
            
            // Check for SkinnedMeshRenderer
            SkinnedMeshRenderer smr = dog.GetComponentInChildren<SkinnedMeshRenderer>();
            Debug.Log($"Has SkinnedMeshRenderer: {smr != null}");
            if (smr != null)
            {
                Debug.Log($"Bones count: {(smr.bones != null ? smr.bones.Length : 0)}");
                if (smr.bones != null)
                {
                    Debug.Log("Bones in SkinnedMeshRenderer:");
                    foreach (var bone in smr.bones)
                    {
                        Debug.Log($"  - {bone?.name ?? "null"}");
                    }
                }
            }
            
            // Print our dogBones array
            Debug.Log($"Our dogBones array:");
            string[] boneNames = {"Spine", "Head", "Tail", "ThighFL", "LegFL", "FootFL", "ThighFR", "LegFR", "ThighBL", "LegBL", "ThighBR", "LegBR"};
            for (int i = 0; i < dogBones?.Length; i++)
            {
                Debug.Log($"  [{i}] = {dogBones[i]?.name ?? "null"}");
            }
            
            Debug.Log("============================");
        }

        private string currentDogBreed = "Dog";
        private bool isSprinting = false;
        private int jumpsCleared = 0;
        private int jumpsMissed = 0;
        
        // Jump timing mechanic
        private float jumpKeyHoldTime = 0f;
        private bool jumpKeyHeld = false;
        private const float optimalJumpHoldMin = 0.125f;
        private const float optimalJumpHoldMax = 0.5f; // Corgi optimal window
        private const float maxJumpHoldTime = 0.75f;
        
        // Speed ramp
        private float targetSpeed = 0f;
        private float currentMoveSpeed = 0f;

        GameObject LoadDefaultDogPrefab()
        {
            // Try to find pre-animated Corgi prefab first (has animations baked in)
            string[] prefabPaths = {
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_anim_RM.prefab",  // Animated run version
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_anim_IP.prefab",  // Animated idle version
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_c1.prefab",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_c2.prefab",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_c3.prefab",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_NoAlpha_c1.prefab",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_LOD_c1.prefab",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_LowPoly_c1.prefab"
            };
            
#if UNITY_EDITOR
            foreach (string path in prefabPaths)
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) 
                {
                    Debug.Log($"Loading dog prefab: {path}");
                    return prefab;
                }
            }
#endif
            return null;
        }

        // Procedural animation state
        private float walkCycle = 0f;
        private float currentSpeed = 0f;
        // [0]Spine, [1]Head, [2]Tail, [3]ThighFL, [4]LegFL, [5]FootFL, [6]ThighFR, [7]LegFR,
        // [8]ThighBL, [9]LegBL, [10]ThighBR, [11]LegBR
        private Transform[] dogBones;

        void AddDogPhysics()
        {
            // Add Rigidbody for physics
            Rigidbody rb = dog.GetComponent<Rigidbody>();
            if (rb == null)
                rb = dog.AddComponent<Rigidbody>();
            rb.mass = 10f;
            rb.drag = 2f;
            rb.angularDrag = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            // Add Capsule Collider
            CapsuleCollider collider = dog.GetComponent<CapsuleCollider>();
            if (collider == null)
                collider = dog.AddComponent<CapsuleCollider>();
            collider.height = 0.8f;
            collider.radius = 0.3f;
            collider.center = new Vector3(0f, 0.4f, 0f);
            
            // Set up Animator with animation clips
            SetupAnimator();
            
            // Find bones for procedural fallback
            FindDogBones();
        }

        void SetupAnimator()
        {
            Animator animator = dog.GetComponent<Animator>();
            if (animator == null)
                animator = dog.AddComponent<Animator>();
            
            // Check if the prefab already has an animator controller
            if (animator.runtimeAnimatorController != null)
            {
                Debug.Log($"Using existing animator controller: {animator.runtimeAnimatorController.name}");
                hasBakedAnimations = true;
                return;
            }
            
            // Check if the prefab has an Avatar (needed for humanoid animations)
            if (animator.avatar == null)
            {
                Debug.Log("No Avatar found - will use procedural animation");
                hasBakedAnimations = false;
                return;
            }
            
            Debug.Log($"Found Avatar: {animator.avatar.name}");
            
            // Try to load the animator controller
#if UNITY_EDITOR
            RuntimeAnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Scripts/Demo/DogAnimator.controller");
            
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log("Assigned animator controller");
                hasBakedAnimations = true;
            }
            else
            {
                Debug.Log("No animator controller found - using procedural animation");
                hasBakedAnimations = false;
            }
#else
            // At runtime, try to load by name
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("DogAnimator");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                hasBakedAnimations = true;
            }
            else
            {
                Debug.Log("Using procedural animation (runtime)");
                hasBakedAnimations = false;
            }
#endif
        }
        
        private bool hasBakedAnimations = false;

        void FindDogBones()
        {
            // Try to find bones for procedural animation
            // Array: [spine, head, tail, thighFL, legFL, footFL, thighFR, legFR, thighBL, legBL, thighBR, legBR]
            dogBones = new Transform[12];
            
            Debug.Log("Searching for dog bones...");
            
            // Get bones from SkinnedMeshRenderer
            SkinnedMeshRenderer smr = dog.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null && smr.bones != null && smr.bones.Length > 0)
            {
                Debug.Log($"Using SkinnedMeshRenderer bones (count: {smr.bones.Length})");
                
                foreach (Transform bone in smr.bones)
                {
                    if (bone == null) continue;
                    string name = bone.name;
                    
                    // Front legs
                    if (name == "Spine_base" && dogBones[0] == null)
                        dogBones[0] = bone;
                    else if (name == "head" && dogBones[1] == null)
                        dogBones[1] = bone;
                    else if (name == "Tail_01" && dogBones[2] == null)
                        dogBones[2] = bone;
                    else if (name == "thigh_f.L" && dogBones[3] == null)
                        dogBones[3] = bone;
                    else if (name == "leg_f.L" && dogBones[4] == null)
                        dogBones[4] = bone;
                    else if (name == "foot_f.L" && dogBones[5] == null)
                        dogBones[5] = bone;
                    else if (name == "thigh_f.R" && dogBones[6] == null)
                        dogBones[6] = bone;
                    else if (name == "leg_f.R" && dogBones[7] == null)
                        dogBones[7] = bone;
                    // Back legs
                    else if (name == "thigh_b.L" && dogBones[8] == null)
                        dogBones[8] = bone;
                    else if (name == "leg_b.L" && dogBones[9] == null)
                        dogBones[9] = bone;
                    else if (name == "thigh_b.R" && dogBones[10] == null)
                        dogBones[10] = bone;
                    else if (name == "leg_b.R" && dogBones[11] == null)
                        dogBones[11] = bone;
                }
                
                int found = 0;
                foreach (var b in dogBones) if (b != null) found++;
                Debug.Log($"Found {found} bones via SkinnedMeshRenderer");
            }
            
            // Log results
            Debug.Log($"Final bone assignments:");
            string[] boneNames = {"Spine", "Head", "Tail", "ThighFL", "LegFL", "FootFL", "ThighFR", "LegFR", "ThighBL", "LegBL", "ThighBR", "LegBR"};
            for (int i = 0; i < dogBones.Length; i++)
            {
                Debug.Log($"  [{i}] {boneNames[i]}: {dogBones[i]?.name ?? "NULL"}");
            }
        }

        GameObject CreatePlaceholderDog()
        {
            GameObject placeholder = new GameObject("Training Dog");

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(placeholder.transform);
            body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            body.transform.localScale = new Vector3(0.8f, 0.6f, 1.2f);

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(placeholder.transform);
            head.transform.localPosition = new Vector3(0f, 0.7f, 0.6f);
            head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Legs
            Vector3[] legPositions = {
                new Vector3(-0.25f, 0.15f, -0.35f),
                new Vector3(0.25f, 0.15f, -0.35f),
                new Vector3(-0.25f, 0.15f, 0.35f),
                new Vector3(0.25f, 0.15f, 0.35f)
            };
            foreach (Vector3 pos in legPositions)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(placeholder.transform);
                leg.transform.localPosition = pos;
                leg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
            }

            placeholder.transform.position = dogStartPosition;

            // Apply brown material
            Material dogMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            dogMat.color = new Color(0.6f, 0.4f, 0.2f);
            foreach (Renderer r in placeholder.GetComponentsInChildren<Renderer>())
            {
                r.material = dogMat;
            }

            return placeholder;
        }

        void SetupObstacles()
        {
            obstacles = new GameObject[numberOfBarriers + 3];
            int index = 0;

            // Barriers/Jumps
            Material jumpMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            jumpMat.color = new Color(0.9f, 0.3f, 0.2f);

            for (int i = 0; i < numberOfBarriers; i++)
            {
                Vector3 pos = new Vector3(0f, 0f, 8f + (i * obstacleSpacing));
                obstacles[index++] = CreateJumpObstacle(pos, jumpMat, $"Jump {i + 1}");
            }

            // Tunnel (using stretched cube as placeholder)
            Vector3 tunnelPos = new Vector3(0f, 0f, 8f + (numberOfBarriers * obstacleSpacing));
            obstacles[index++] = CreateTunnelObstacle(tunnelPos);

            // Weave poles
            Vector3 weavePos = new Vector3(0f, 0f, tunnelPos.z + 8f);
            obstacles[index++] = CreateWeavePoles(weavePos);

            // Pause table
            CreatePauseTable(new Vector3(0f, 0f, weavePos.z + 8f));
        }

        GameObject CreateJumpObstacle(Vector3 position, Material mat, string name)
        {
            GameObject jump = new GameObject(name);

            // Two upright poles
            Material poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            poleMat.color = new Color(0.95f, 0.95f, 0.9f);

            GameObject pole1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole1.transform.SetParent(jump.transform);
            pole1.transform.localPosition = new Vector3(-0.6f, 0.5f, 0f);
            pole1.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            pole1.GetComponent<Renderer>().material = poleMat;

            GameObject pole2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole2.transform.SetParent(jump.transform);
            pole2.transform.localPosition = new Vector3(0.6f, 0.5f, 0f);
            pole2.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            pole2.GetComponent<Renderer>().material = poleMat;

            // Cross bar - solid so dog must jump over it
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "CrossBar";
            bar.transform.SetParent(jump.transform);
            bar.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            bar.transform.localScale = new Vector3(1.4f, 0.08f, 0.08f);
            bar.GetComponent<Renderer>().material = mat;
            // Shrink the collider to match the visual bar
            BoxCollider barCollider = bar.GetComponent<BoxCollider>();
            barCollider.size = new Vector3(1f, 1f, 1f);

            // Side post colliders
            // The Cylinder primitives already have CapsuleColliders from CreatePrimitive

            // Add trigger zone for detecting if dog clears the hurdle
            BoxCollider triggerZone = jump.AddComponent<BoxCollider>();
            triggerZone.isTrigger = true;
            triggerZone.size = new Vector3(2f, 3f, 1.5f); // Spans full jump area
            triggerZone.center = new Vector3(0f, 1f, 0f);

            // Store jump data for tracking
            JumpData jumpData = jump.AddComponent<JumpData>();
            jumpData.barHeight = 0.9f;
            jumpData.barRenderer = bar.GetComponent<Renderer>();
            jumpData.originalColor = mat.color;

            jump.transform.position = position;
            return jump;
        }

        GameObject CreateTunnelObstacle(Vector3 position)
        {
            GameObject tunnel = new GameObject("Tunnel");

            // Main tunnel body
            Material tunnelMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tunnelMat.color = new Color(0.2f, 0.6f, 0.3f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(tunnel.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.GetComponent<Renderer>().material = tunnelMat;
            // Make tunnel collider a trigger
            CapsuleCollider tunnelCollider = body.GetComponent<CapsuleCollider>();
            tunnelCollider.isTrigger = true;

            // Add entrance trigger for scoring
            BoxCollider entrance = tunnel.AddComponent<BoxCollider>();
            entrance.size = new Vector3(1.5f, 1.5f, 0.5f);
            entrance.center = new Vector3(0f, 0.75f, 1.8f);
            entrance.isTrigger = true;

            tunnel.transform.position = position;
            return tunnel;
        }

        GameObject CreateWeavePoles(Vector3 position)
        {
            GameObject weave = new GameObject("Weave Poles");

            Material poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            poleMat.color = new Color(0.9f, 0.7f, 0.1f);

            for (int i = 0; i < 6; i++)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(weave.transform);
                pole.transform.localPosition = new Vector3((i - 2.5f) * 0.6f, 0.5f, 0f);
                pole.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
                pole.GetComponent<Renderer>().material = poleMat;
            }

            // Add a box collider for the weave pole area
            BoxCollider weaveCollider = weave.AddComponent<BoxCollider>();
            weaveCollider.size = new Vector3(3.6f, 1f, 0.5f);
            weaveCollider.center = new Vector3(0f, 0.5f, 0f);
            weaveCollider.isTrigger = true;

            weave.transform.position = position;
            return weave;
        }

        void CreatePauseTable(Vector3 position)
        {
            GameObject table = new GameObject("Pause Table");

            Material tableMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tableMat.color = new Color(0.5f, 0.3f, 0.15f);

            // Table top (no collider - handled by parent trigger)
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(table.transform);
            top.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            top.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            top.GetComponent<Renderer>().material = tableMat;
            Object.Destroy(top.GetComponent<BoxCollider>());

            // Legs (no collider - handled by parent trigger)
            Vector3[] legPos = {
                new Vector3(-0.5f, 0.2f, -0.5f),
                new Vector3(0.5f, 0.2f, -0.5f),
                new Vector3(-0.5f, 0.2f, 0.5f),
                new Vector3(0.5f, 0.2f, 0.5f)
            };
            foreach (Vector3 pos in legPos)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(table.transform);
                leg.transform.localPosition = pos;
                leg.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
                leg.GetComponent<Renderer>().material = tableMat;
                Object.Destroy(leg.GetComponent<BoxCollider>());
            }

            // Add a box collider for the table
            BoxCollider tableCollider = table.AddComponent<BoxCollider>();
            tableCollider.size = new Vector3(1.2f, 0.8f, 1.2f);
            tableCollider.center = new Vector3(0f, 0.4f, 0f);
            tableCollider.isTrigger = true;

            table.transform.position = position;
        }

        void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.5f, 0.7f, 0.9f);
                cam.nearClipPlane = 0.1f; // Allow closer zoom without clipping
                UpdateCamera();
            }
        }

        void OnGUI()
        {
            // Control panel
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));

            GUILayout.BeginArea(new Rect(10, 10, 280, 230), boxStyle);
            GUILayout.Label("<b><size=16>TRAINING MODE</size></b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Space(5);
            GUILayout.Label("DOG CONTROLS:", GUI.skin.label);
            GUILayout.Label("  WASD / Arrow Keys - Move");
            GUILayout.Label("  Hold Shift - Sprint");
            GUILayout.Label("  Hold & Release Space - Jump");
            GUILayout.Label("    (0.1-0.5s = max height)");
            GUILayout.Space(5);
            GUILayout.Label("CAMERA:", GUI.skin.label);
            GUILayout.Label("  Right Mouse Drag - Orbit");
            GUILayout.Label("  Scroll - Zoom");
            GUILayout.Label("  R - Reset camera");
            GUILayout.Label("  Tab - Toggle follow dog");
            GUILayout.Space(5);
            
            // Debug button to re-setup animations
            if (GUILayout.Button("Re-setup Animations"))
            {
                if (dog != null)
                {
                    SetupAnimator();
                    DebugAnimationState();
                }
            }
            GUILayout.EndArea();

            // Info panel showing dog position
            if (dog != null)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 150), boxStyle);
                GUILayout.Label($"<b>{currentDogBreed.ToUpper()}</b>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label($"X: {dog.transform.position.x:F1}  Z: {dog.transform.position.z:F1}");
                GUILayout.Label($"Speed: {currentSpeed:F2}");
                GUILayout.Label($"State: {(!isGrounded ? "Jumping" : isSprinting && currentSpeed > 0.1f ? "Running" : currentSpeed > 0.1f ? "Walking" : "Idle")}");
                GUILayout.Label($"Animations: {(hasBakedAnimations ? "Baked" : "Procedural")}");
                GUILayout.Space(5);
                GUILayout.Label($"<b>SCORE</b>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label($"Cleared: <color=green>{jumpsCleared}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label($"Missed: <color=red>{jumpsMissed}</color>", new GUIStyle(GUI.skin.label) { richText = true });
                
                // Show animator info if available
                Animator animator = dog.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    GUILayout.Label($"Controller: {animator.runtimeAnimatorController.name}");
                }
                GUILayout.EndArea();
            }
            
            // Jump timing indicator (shown when holding space)
            if (jumpKeyHeld)
            {
                float barWidth = 200f;
                float barHeight = 30f;
                float barX = (Screen.width - barWidth) / 2f;
                float barY = Screen.height - 100f;
                
                // Background
                GUI.Box(new Rect(barX, barY, barWidth, barHeight), "");
                
                // Optimal zone (green)
                float optimalStart = optimalJumpHoldMin / maxJumpHoldTime;
                float optimalEnd = optimalJumpHoldMax / maxJumpHoldTime;
                GUI.Box(new Rect(barX + barWidth * optimalStart, barY, barWidth * (optimalEnd - optimalStart), barHeight), "");
                
                // Current position
                float fillPercent = Mathf.Clamp01(jumpKeyHoldTime / maxJumpHoldTime);
                Color fillColor = (jumpKeyHoldTime >= optimalJumpHoldMin && jumpKeyHoldTime <= optimalJumpHoldMax) ? Color.green : 
                                  (jumpKeyHoldTime < optimalJumpHoldMin) ? Color.yellow : Color.red;
                
                GUIStyle fillStyle = new GUIStyle(GUI.skin.box);
                fillStyle.normal.background = MakeTex(1, 1, fillColor);
                GUI.Box(new Rect(barX + 2, barY + 2, (barWidth - 4) * fillPercent, barHeight - 4), "", fillStyle);
                
                // Label
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(barX, barY - 20, barWidth, 20), "Hold Space - Release in GREEN zone!", labelStyle);
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        void FixMaterialsForURP(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");

            foreach (Renderer r in renderers)
            {
                if (r.material != null && urpShader != null)
                {
                    Color originalColor = r.material.HasProperty("_Color") ? r.material.color : Color.white;
                    Texture mainTex = r.material.GetTexture("_MainTex");

                    r.material = new Material(urpShader);
                    r.material.color = originalColor;
                    if (mainTex != null) r.material.SetTexture("_BaseMap", mainTex);
                }
            }
        }

        void HandleDogMovement()
        {
            Rigidbody rb = dog.GetComponent<Rigidbody>();
            Animator animator = dog.GetComponent<Animator>();
            
            // Get input
            float moveX = 0f;
            float moveZ = 0f;
            
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                moveZ = 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                moveZ = -1f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                moveX = -1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                moveX = 1f;

            // Jump timing mechanic - track hold duration
            if (Input.GetKeyDown(KeyCode.Space) && rb != null && isGrounded)
            {
                jumpKeyHeld = true;
                jumpKeyHoldTime = 0f;
            }
            
            if (jumpKeyHeld)
            {
                jumpKeyHoldTime += Time.deltaTime;
            }
            
            // Release jump on key up
            if (Input.GetKeyUp(KeyCode.Space) && jumpKeyHeld && isGrounded)
            {
                jumpKeyHeld = false;
                
                // Calculate jump power based on hold time
                float jumpPower = CalculateJumpPower(jumpKeyHoldTime);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                isGrounded = false;
                
                // Trigger jump animation
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.SetBool("Jump", true);
                }
                
                Debug.Log($"Jump! Hold time: {jumpKeyHoldTime:F3}s, Power: {jumpPower:F1}");
            }
            
            // Reset jump animation when landing
            if (isGrounded && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool("Jump", false);
            }

            // Calculate movement direction
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            float speed = moveDirection.magnitude;
            
            // Check for sprint (hold Shift)
            isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            // Smooth speed transitions
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 8f);
            
            // Apply physics movement with acceleration ramp
            if (rb != null && speed > 0f)
            {
                float maxSpeed = isSprinting ? 12f : 3f; // Run vs walk speed
                float accelRate = isSprinting ? 6f : 4f; // Faster acceleration when sprinting
                
                // Ramp up speed
                currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, maxSpeed, accelRate * Time.deltaTime);
                rb.MovePosition(rb.position + moveDirection * currentMoveSpeed * Time.deltaTime);
                
                // Rotate dog smoothly to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                dog.transform.rotation = Quaternion.Slerp(dog.transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            else
            {
                // Decelerate when not moving
                currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, 0f, 8f * Time.deltaTime);
            }
            
            // Update animator - use Speed parameter for blending
            if (animator != null && hasBakedAnimations)
            {
                animator.SetFloat("Speed", currentSpeed * 2f); // Scale for smoother transitions
                animator.SetBool("IsMoving", currentSpeed > 0.1f);
            }
        }
        
        float CalculateJumpPower(float holdTime)
        {
            // Optimal range: 0.125s - 0.5s gives max power
            // Too quick (<0.125s) or too long (>0.75s) = weaker jump
            
            if (holdTime >= optimalJumpHoldMin && holdTime <= optimalJumpHoldMax)
            {
                // Optimal timing - max power (70 for ~3x current height)
                return 70f;
            }
            else if (holdTime < optimalJumpHoldMin)
            {
                // Too quick - scale from 30 to 70 based on hold time
                return Mathf.Lerp(30f, 70f, holdTime / optimalJumpHoldMin);
            }
            else
            {
                // Too long - decay power after optimal window
                float decay = Mathf.InverseLerp(optimalJumpHoldMax, maxJumpHoldTime, holdTime);
                return Mathf.Lerp(70f, 35f, decay);
            }
        }

        void UpdateProceduralAnimation()
        {
            if (currentSpeed > 0.01f)
            {
                walkCycle += Time.deltaTime * currentSpeed * 12f;
            }
            else
            {
                // Idle breathing animation
                walkCycle += Time.deltaTime * 2f;
            }

            // Animate spine (slight bob)
            if (dogBones[0] != null)
            {
                float spineBob = currentSpeed > 0.1f ? Mathf.Sin(walkCycle * 2f) * 0.02f : Mathf.Sin(walkCycle) * 0.005f;
                dogBones[0].localPosition = new Vector3(
                    dogBones[0].localPosition.x,
                    dogBones[0].localPosition.y + spineBob * 0.1f,
                    dogBones[0].localPosition.z
                );
            }
        }

        private bool isGrounded = true;
        private float groundY = 0.1f;
        private float groundCheckRadius = 0.15f;

        void FixedUpdate()
        {
            if (dog != null)
            {
                Rigidbody rb = dog.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Raycast ground check for reliability
                    isGrounded = Physics.Raycast(dog.transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f, ~0);
                    
                    // Track max height in jump zones
                    foreach (GameObject obs in obstacles)
                    {
                        if (obs == null) continue;
                        JumpData jd = obs.GetComponent<JumpData>();
                        if (jd != null && jd.dogInZone)
                        {
                            if (dog.transform.position.y > jd.dogMaxHeight)
                                jd.dogMaxHeight = dog.transform.position.y;
                        }
                    }
                    
                    // Clamp ground position
                    if (dog.transform.position.y <= 0f)
                    {
                        Vector3 pos = rb.position;
                        pos.y = 0f;
                        rb.MovePosition(pos);
                        
                        // Stop vertical velocity when hitting ground
                        if (rb.linearVelocity.y < 0)
                        {
                            Vector3 vel = rb.linearVelocity;
                            vel.y = 0f;
                            rb.linearVelocity = vel;
                        }
                        
                        isGrounded = true;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (dog == null) return;
            
            // Check if dog entered a jump trigger zone
            JumpData jumpData = other.GetComponent<JumpData>();
            if (jumpData != null)
            {
                jumpData.dogInZone = true;
                jumpData.dogMaxHeight = dog.transform.position.y;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (dog == null) return;
            
            // Check if dog exited a jump trigger zone
            JumpData jumpData = other.GetComponent<JumpData>();
            if (jumpData != null && jumpData.dogInZone)
            {
                jumpData.dogInZone = false;
                
                // Check if dog cleared the bar (max height in zone > bar height)
                if (jumpData.dogMaxHeight >= jumpData.barHeight)
                {
                    jumpData.TriggerSuccess();
                    jumpsCleared++;
                    Debug.Log($"Jump cleared! Max height: {jumpData.dogMaxHeight:F2}");
                }
                else
                {
                    jumpData.TriggerFail();
                    jumpsMissed++;
                    Debug.Log($"Jump missed! Max height: {jumpData.dogMaxHeight:F2}, Bar height: {jumpData.barHeight}");
                }
                
                jumpData.dogMaxHeight = 0f;
            }
        }

        void LateUpdate()
        {
            // Always apply procedural animation - it supplements or replaces baked animations
            if (dog != null && dogBones != null)
            {
                ApplyProceduralAnimation();
            }
        }

        void ApplyProceduralAnimation()
        {
            if (dogBones == null) return;

            // Store original rotations on first frame if not stored
            if (originalBoneRotations == null)
            {
                originalBoneRotations = new Quaternion[dogBones.Length];
                for (int i = 0; i < dogBones.Length; i++)
                {
                    if (dogBones[i] != null)
                        originalBoneRotations[i] = dogBones[i].localRotation;
                    else
                        originalBoneRotations[i] = Quaternion.identity;
                }
                Debug.Log("Stored original bone rotations");
            }

            float walkSpeed = isSprinting ? currentSpeed * 20f : currentSpeed * 12f;
            if (currentSpeed > 0.01f)
                walkCycle += Time.deltaTime * walkSpeed;
            else
                walkCycle += Time.deltaTime * 2f;

            // When grounded and not moving, reset bones to idle pose smoothly
            if (isGrounded && currentSpeed < 0.1f)
            {
                for (int i = 0; i < dogBones.Length; i++)
                {
                    if (dogBones[i] != null && originalBoneRotations[i] != Quaternion.identity)
                    {
                        dogBones[i].localRotation = Quaternion.Slerp(dogBones[i].localRotation, originalBoneRotations[i], Time.deltaTime * 5f);
                    }
                }
                
                // Still animate spine slightly for idle breathing and tail wag
                if (dogBones[0] != null && originalBoneRotations[0] != Quaternion.identity)
                {
                    float spineBob = Mathf.Sin(walkCycle * 0.5f) * 1f;
                    dogBones[0].localRotation = originalBoneRotations[0] * Quaternion.Euler(spineBob, 0f, 0f);
                }
                if (dogBones[2] != null && originalBoneRotations[2] != Quaternion.identity)
                {
                    float tailWag = Mathf.Sin(walkCycle * 2f) * 15f;
                    dogBones[2].localRotation = originalBoneRotations[2] * Quaternion.Euler(0f, tailWag, 0f);
                }
                return;
            }

            // Animate spine (body bob when running)
            if (dogBones[0] != null && originalBoneRotations[0] != Quaternion.identity)
            {
                float spineBob = !isGrounded ? -15f : currentSpeed > 0.1f ? Mathf.Sin(walkCycle * 2f) * 6f : Mathf.Sin(walkCycle * 0.5f) * 1f;
                dogBones[0].localRotation = originalBoneRotations[0] * Quaternion.Euler(spineBob, 0f, 0f);
            }

            // Animate head (nodding when walking, looking up when jumping)
            if (dogBones[1] != null && originalBoneRotations[1] != Quaternion.identity)
            {
                float headNod = currentSpeed > 0.1f ? Mathf.Sin(walkCycle) * 10f : Mathf.Sin(walkCycle * 0.5f) * 3f;
                if (!isGrounded) headNod = 25f; // Look up when jumping
                dogBones[1].localRotation = originalBoneRotations[1] * Quaternion.Euler(headNod, 0f, 0f);
            }

            // Animate tail (wagging)
            if (dogBones[2] != null && originalBoneRotations[2] != Quaternion.identity)
            {
                float tailWag = !isGrounded ? 0f : currentSpeed > 0.1f ? Mathf.Sin(walkCycle * 4f) * 40f : Mathf.Sin(walkCycle * 2f) * 20f;
                dogBones[2].localRotation = originalBoneRotations[2] * Quaternion.Euler(0f, tailWag, 0f);
            }

            // Animate front left leg - thigh (hip swing)
            if (dogBones[3] != null && originalBoneRotations[3] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = 35f; // Forward when jumping
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Sin(walkCycle) * 35f; // Running swing
                dogBones[3].localRotation = originalBoneRotations[3] * Quaternion.Euler(angle, 0f, 0f);
            }
            
            // Animate front left leg - shin (knee bend)
            if (dogBones[4] != null && originalBoneRotations[4] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = -50f; // Tucked when jumping
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Abs(Mathf.Sin(walkCycle)) * -30f; // Bend during swing phase
                dogBones[4].localRotation = originalBoneRotations[4] * Quaternion.Euler(angle, 0f, 0f);
            }
            
            // Animate front left foot
            if (dogBones[5] != null && originalBoneRotations[5] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = 20f; // Pointed when jumping
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Sin(walkCycle) * 15f; // Toe movement
                dogBones[5].localRotation = originalBoneRotations[5] * Quaternion.Euler(angle, 0f, 0f);
            }
            
            // Animate front right leg - thigh (opposite phase)
            if (dogBones[6] != null && originalBoneRotations[6] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = 35f;
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Sin(walkCycle + Mathf.PI) * 35f; // Opposite phase
                dogBones[6].localRotation = originalBoneRotations[6] * Quaternion.Euler(angle, 0f, 0f);
            }
            
            // Animate front right leg - shin (knee bend)
            if (dogBones[7] != null && originalBoneRotations[7] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = -50f;
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Abs(Mathf.Sin(walkCycle + Mathf.PI)) * -30f; // Opposite phase
                dogBones[7].localRotation = originalBoneRotations[7] * Quaternion.Euler(angle, 0f, 0f);
            }

            // Animate back left leg - thigh (opposite to front)
            if (dogBones[8] != null && originalBoneRotations[8] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = -40f; // Backward when jumping
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Sin(walkCycle + Mathf.PI) * 30f; // Opposite to front left
                dogBones[8].localRotation = originalBoneRotations[8] * Quaternion.Euler(angle, 0f, 0f);
            }

            // Animate back left leg - shin (knee bend)
            if (dogBones[9] != null && originalBoneRotations[9] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = 50f; // Extended when jumping
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Abs(Mathf.Sin(walkCycle + Mathf.PI)) * -25f;
                dogBones[9].localRotation = originalBoneRotations[9] * Quaternion.Euler(angle, 0f, 0f);
            }

            // Animate back right leg - thigh (opposite to front, opposite phase to back left)
            if (dogBones[10] != null && originalBoneRotations[10] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = -40f;
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Sin(walkCycle) * 30f; // Same as front left
                dogBones[10].localRotation = originalBoneRotations[10] * Quaternion.Euler(angle, 0f, 0f);
            }

            // Animate back right leg - shin (knee bend)
            if (dogBones[11] != null && originalBoneRotations[11] != Quaternion.identity)
            {
                float angle = 0f;
                if (!isGrounded)
                    angle = 50f;
                else if (currentSpeed > 0.1f)
                    angle = Mathf.Abs(Mathf.Sin(walkCycle)) * -25f;
                dogBones[11].localRotation = originalBoneRotations[11] * Quaternion.Euler(angle, 0f, 0f);
            }
        }
        
        private Quaternion[] originalBoneRotations;
    }
}
