using UnityEngine;
using UnityEngine.AI;
using AgilityDogs.Data;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Commands;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Presentation.Camera;

namespace AgilityDogs.Services
{
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

        [Header("Auto-Start")]
        [SerializeField] private bool autoStartRun = true;
        [SerializeField] private bool autoBakeNavMesh = true;
        [SerializeField] private CourseDefinition fallbackCourse;

        private void Awake()
        {
            if (autoFindReferences)
            {
                FindReferences();
            }

            WireReferences();
        }

        private void Start()
        {
            if (autoBakeNavMesh)
            {
                var surfaces = FindObjectsOfType<NavMeshSurface>();
                foreach (var surface in surfaces)
                {
                    if (surface.navMeshData == null)
                    {
                        Debug.Log($"[SceneBootstrap] Baking NavMesh on {surface.gameObject.name}...");
                        surface.BuildNavMesh();
                        Debug.Log($"[SceneBootstrap] NavMesh baked.");
                    }
                }
            }

            if (autoStartRun && courseRunner != null)
            {
                CourseDefinition course = courseRunner.CurrentCourse ?? fallbackCourse;

                if (course == null)
                {
                    var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");
                    if (courses != null && courses.Length > 0)
                    {
                        course = courses[0];
                    }
                }

                if (course != null)
                {
                    courseRunner.LoadCourse(course);
                    courseRunner.StartCountdown();
                }
                else
                {
                    Debug.LogWarning("[SceneBootstrap] No course found. Run not started.");
                }
            }
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

            if (courseRunner != null && scoringService != null)
            {
                courseRunner.SetScoringService(scoringService);
            }
        }

        public HandlerController GetHandler() => handler;
        public DogAgentController GetDog() => dog;
        public AgilityCameraController GetCamera() => cameraController;
        public CourseRunner GetCourseRunner() => courseRunner;
        public AgilityScoringService GetScoringService() => scoringService;
    }
}
