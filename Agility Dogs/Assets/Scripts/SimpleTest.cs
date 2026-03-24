using UnityEngine;

public class SimpleTest : MonoBehaviour
{
    void Start()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = new Vector3(0f, 0f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0f, 1f, 0f);
        cube.transform.localScale = new Vector3(2f, 2f, 2f);
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(3f, 1f, 0f);
        
        Debug.Log("Simple test scene loaded successfully!");
    }
}