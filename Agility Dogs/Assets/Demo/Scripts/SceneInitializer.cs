using UnityEngine;

namespace AgilityDogs.Demo
{
    public class SceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            // Create a large ground plane so we have something to see
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.position = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.name = "Ground";
            
            // Create a visible cube in front of the camera
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0f, 1f, 5f);
            cube.transform.localScale = new Vector3(2f, 2f, 2f);
            cube.name = "TestCube";
            
            // Create a sphere to the side
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3(3f, 1f, 2f);
            sphere.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            sphere.name = "TestSphere";
            
            Debug.Log("Agility scene initialized with test objects!");
        }
    }
}