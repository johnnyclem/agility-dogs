using UnityEngine;
using AgilityDogs.Gameplay.Dog;

namespace AgilityDogs.Gameplay.Obstacles
{
    public static class ObstacleExtensions
    {
        public static void SetTargetObstacle(this ObstacleBase obstacle, ObstacleBase target)
        {
            var dog = FindObjectOfType<DogAgentController>();
            if (dog != null)
            {
                dog.SetTargetObstacle(target);
            }
        }
    }
}
