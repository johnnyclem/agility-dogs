using UnityEngine;

public class SimpleTest : MonoBehaviour
{
    [Header("Dog Prefab (Optional)")]
    [SerializeField] private GameObject dogPrefab;

    void Start()
    {
        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = new Vector3(0f, 0f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        ground.name = "Ground";
        
        // Make ground green
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat != null)
            {
                mat.color = new Color(0.3f, 0.5f, 0.3f);
                groundRenderer.material = mat;
            }
        }

        // Create light if none exists
        if (FindFirstObjectByType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        
        // Try to spawn dog if prefab is assigned
        if (dogPrefab != null)
        {
            GameObject dog = Instantiate(dogPrefab, new Vector3(0f, 0f, 3f), Quaternion.identity);
            dog.name = "Demo Dog";
            Debug.Log("Dog spawned successfully!");
        }
        else
        {
            // Create a simple placeholder dog
            CreatePlaceholderDog();
        }
        
        Debug.Log("Simple test scene loaded successfully!");
    }

    void CreatePlaceholderDog()
    {
        GameObject placeholder = new GameObject("Placeholder Dog");
        
        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(placeholder.transform);
        body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        body.transform.localScale = new Vector3(1f, 0.8f, 1.5f);
        
        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.transform.SetParent(placeholder.transform);
        head.transform.localPosition = new Vector3(0f, 0.9f, 0.8f);
        head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        // Snout
        GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Cube);
        snout.transform.SetParent(placeholder.transform);
        snout.transform.localPosition = new Vector3(0f, 0.8f, 1.1f);
        snout.transform.localScale = new Vector3(0.3f, 0.2f, 0.4f);
        
        // Legs
        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(placeholder.transform);
            float x = (i < 2) ? -0.3f : 0.3f;
            float z = (i % 2 == 0) ? -0.4f : 0.4f;
            leg.transform.localPosition = new Vector3(x, 0.15f, z);
            leg.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
        }
        
        placeholder.transform.position = new Vector3(0f, 0f, 3f);
        
        // Make it brown
        Renderer[] renderers = placeholder.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat != null)
            {
                mat.color = new Color(0.6f, 0.4f, 0.2f);
                r.material = mat;
            }
        }
        
        Debug.Log("Placeholder dog created - assign a real dog prefab to see the actual dog model!");
    }
}