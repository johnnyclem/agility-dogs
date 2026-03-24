using UnityEngine;

namespace AgilityDogs.Demo
{
    public class SimpleSceneStarter : MonoBehaviour
    {
        private void Start()
        {
            // Create a visible ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.position = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.name = "Ground";
            
            // Create a visible cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0f, 1f, 0f);
            cube.transform.localScale = new Vector3(2f, 2f, 2f);
            cube.name = "TestCube";
            
            // Create a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3(3f, 1f, 0f);
            sphere.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            sphere.name = "TestSphere";
            
            Debug.Log("Agility test scene initialized with visible objects!");
        }
    }
}