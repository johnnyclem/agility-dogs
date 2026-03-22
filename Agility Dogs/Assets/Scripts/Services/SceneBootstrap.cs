using UnityEngine;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Commands;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Presentation.Camera;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Bootstraps the gameplay scene. Attach to a root GameObject in the gameplay scene.
    /// Wires up handler, dog, scoring, and camera references at runtime.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private HandlerController handler;
        [SerializeField] private DogAgentController dog;
        [SerializeField] private AgilityCameraController cameraController;
        [SerializeField] private CourseRunner courseRunner;
        [SerializeField] private AgilityScoringService scoringService;

        [Header("Auto-Find")]
        [SerializeField] private bool autoFindReferences = true;

        private void Awake()
        {
            if (autoFindReferences)
            {
                FindReferences();
            }

            WireReferences();
        }

        private void FindReferences()
        {
            if (handler == null)
                handler = FindObjectOfType<HandlerController>();

            if (dog == null)
                dog = FindObjectOfType<DogAgentController>();

            if (cameraController == null)
                cameraController = FindObjectOfType<AgilityCameraController>();

            if (courseRunner == null)
                courseRunner = FindObjectOfType<CourseRunner>();

            if (scoringService == null)
                scoringService = FindObjectOfType<AgilityScoringService>();
        }

        private void WireReferences()
        {
            if (courseRunner != null && handler != null && dog != null)
            {
                courseRunner.SetHandlerAndDog(handler, dog);
            }

            if (cameraController != null && handler != null)
            {
                cameraController.SetTarget(handler.transform);
            }

            if (scoringService != null && dog != null)
            {
                scoringService.SetDog(dog);
            }

            var commandBuffers = FindObjectsOfType<CommandBuffer>();
            foreach (var cb in commandBuffers)
            {
                cb.GetComponentInParent<HandlerController>();
            }
        }

        public HandlerController GetHandler() => handler;
        public DogAgentController GetDog() => dog;
        public AgilityCameraController GetCamera() => cameraController;
        public CourseRunner GetCourseRunner() => courseRunner;
        public AgilityScoringService GetScoringService() => scoringService;
    }
}
