using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewCourseDefinition", menuName = "Agility Dogs/Course Definition")]
    public class CourseDefinition : ScriptableObject
    {
        [Header("Course Info")]
        public string courseName;
        public CourseType courseType;
        public string venueName;

        [Header("Layout")]
        public GameObject coursePrefab;
        public Vector3 startPosition = Vector3.zero;
        public Vector3 startDirection = Vector3.forward;

        [Header("Timing")]
        [Tooltip("Standard course time in seconds")]
        public float standardTime = 45f;

        [Tooltip("Maximum allowed time in seconds")]
        public float maximumTime = 60f;

        [Header("Obstacle Sequence")]
        public ObstacleData[] obstacleSequence;

        [Header("Size Divisions")]
        public bool supportsAllDivisions = true;
        public AgilitySizeDivision[] supportedDivisions;

        [Header("Difficulty")]
        [Range(1, 10)]
        public int difficultyRating = 5;

        [Header("Course Record")]
        public float bestTime = float.MaxValue;
        public string bestHandlerName = "";
    }
}
