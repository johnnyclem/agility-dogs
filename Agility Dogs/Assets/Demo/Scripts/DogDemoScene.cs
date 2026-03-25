using UnityEngine;

namespace AgilityDogs.Demo
{
    public class DogDemoScene : MonoBehaviour
    {
        [Header("Dog Prefab")]
        [SerializeField] private GameObject dogPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
        [SerializeField] private float spawnScale = 1f;

        private GameObject spawnedDog;

        private void Start()
        {
            // Create ground plane
            CreateGround();

            // Create directional light if none exists
            if (FindFirstObjectByType<Light>() == null)
            {
                CreateLight();
            }

            // Spawn dog if prefab is assigned
            if (dogPrefab != null)
            {
                SpawnDog();
            }
            else
            {
                Debug.LogWarning("Dog prefab not assigned! Please assign a dog prefab in the inspector.");
                // Create a placeholder cube to show something works
                CreatePlaceholderDog();
            }
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            ground.name = "Ground";

            // Create a simple material for the ground
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat != null)
                {
                    mat.color = new Color(0.3f, 0.5f, 0.3f); // Green grass color
                    renderer.material = mat;
                }
            }
        }

        private void CreateLight()
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void SpawnDog()
        {
            if (dogPrefab == null) return;

            spawnedDog = Instantiate(dogPrefab, spawnPosition, Quaternion.identity);
            spawnedDog.transform.localScale = Vector3.one * spawnScale;
            spawnedDog.name = "Demo Dog";

            Debug.Log($"Dog spawned at {spawnPosition}");
        }

        private void CreatePlaceholderDog()
        {
            // Create a simple placeholder that looks somewhat dog-like
            spawnedDog = new GameObject("Placeholder Dog");

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(spawnedDog.transform);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(1f, 0.8f, 1.5f);
            body.name = "Body";

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(spawnedDog.transform);
            head.transform.localPosition = new Vector3(0f, 0.9f, 0.8f);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            head.name = "Head";

            // Snout
            GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            snout.transform.SetParent(spawnedDog.transform);
            snout.transform.localPosition = new Vector3(0f, 0.8f, 1.1f);
            snout.transform.localScale = new Vector3(0.3f, 0.2f, 0.4f);
            snout.name = "Snout";

            // Legs
            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(spawnedDog.transform);
                float x = (i < 2) ? -0.3f : 0.3f;
                float z = (i % 2 == 0) ? -0.4f : 0.4f;
                leg.transform.localPosition = new Vector3(x, 0.15f, z);
                leg.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
                leg.name = $"Leg_{i}";
            }

            // Tail
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tail.transform.SetParent(spawnedDog.transform);
            tail.transform.localPosition = new Vector3(0f, 0.7f, -0.9f);
            tail.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
            tail.name = "Tail";

            spawnedDog.transform.position = spawnPosition;

            // Make it brown
            Renderer[] renderers = spawnedDog.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat != null)
                {
                    mat.color = new Color(0.6f, 0.4f, 0.2f); // Brown color
                    r.material = mat;
                }
            }

            Debug.Log("Placeholder dog created - assign a real dog prefab in the inspector!");
        }

        public void SetDogPrefab(GameObject prefab)
        {
            dogPrefab = prefab;
        }

        public void RespawnDog()
        {
            if (spawnedDog != null)
            {
                Destroy(spawnedDog);
            }
            SpawnDog();
        }
    }
}
