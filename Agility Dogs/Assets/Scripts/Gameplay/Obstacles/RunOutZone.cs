using UnityEngine;
using AgilityDogs.Gameplay.Dog;

namespace AgilityDogs.Gameplay.Obstacles
{
    [RequireComponent(typeof(Collider))]
    public class RunOutZone : MonoBehaviour
    {
        [SerializeField] private ObstacleBase parentObstacle;
        [SerializeField] private bool isApproachZone = true; // if false, assume departure zone

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

            // Dog entered the approach zone, we'll track if it leaves without taking obstacle
            // We'll rely on parentObstacle's dogHasEntered flag
        }

        private void OnTriggerExit(Collider other)
        {
            var dog = other.GetComponentInParent<DogAgentController>();
            if (dog == null) return;
            if (parentObstacle == null) return;

            // If dog exits the approach zone without having entered the obstacle (dogHasEntered false),
            // and also never entered commit zone (dogInCommitZone false), then it's a run-out.
            if (!parentObstacle.DogHasEntered && !parentObstacle.DogInCommitZone)
            {
                parentObstacle.TriggerRunOut();
            }
        }
    }
}