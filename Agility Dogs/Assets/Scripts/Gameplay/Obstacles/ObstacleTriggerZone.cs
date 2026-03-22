using UnityEngine;
using AgilityDogs.Gameplay.Dog;

namespace AgilityDogs.Gameplay.Obstacles
{
    [RequireComponent(typeof(Collider))]
    public class ObstacleTriggerZone : MonoBehaviour
    {
        [SerializeField] private ObstacleBase parentObstacle;
        [SerializeField] private bool isEntryPoint;
        [SerializeField] private bool isExitPoint;
        [SerializeField] private bool isContactZoneStart;
        [SerializeField] private bool isContactZoneEnd;
        [SerializeField] private bool isCommitZone;

        private void Awake()
        {
            if (parentObstacle == null)
                parentObstacle = GetComponentInParent<ObstacleBase>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var dog = other.GetComponentInParent<DogAgentController>();
            if (dog == null) return;

            if (parentObstacle == null) return;

            if (isCommitZone)
            {
                dog.SetTargetObstacle(parentObstacle);
            }

            if (isContactZoneStart || isContactZoneEnd)
            {
                parentObstacle.OnDogContactZoneEnter(dog, isContactZoneStart);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var dog = other.GetComponentInParent<DogAgentController>();
            if (dog == null) return;
            if (parentObstacle == null) return;

            if (isExitPoint)
            {
                parentObstacle.OnDogExited(dog);
            }
        }
    }
}
