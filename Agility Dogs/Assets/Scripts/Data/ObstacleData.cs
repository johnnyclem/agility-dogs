using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewObstacleData", menuName = "Agility Dogs/Obstacle Data")]
    public class ObstacleData : ScriptableObject
    {
        [Header("Obstacle Info")]
        public string obstacleName;
        public ObstacleType obstacleType;
        public CourseType[] validCourseTypes = { CourseType.Standard, CourseType.JumpersWithWeaves };
        public int sequenceOrder;

        [Header("Dimensions")]
        public float length = 3f;
        public float width = 1.2f;
        public float height = 1.5f;

        [Header("Contact Zones")]
        public bool hasContactZones = false;
        [Range(0f, 1f)]
        public float contactZoneLength = 0.3f;

        [Header("Scoring")]
        public bool requiresCompletion = true;
        public bool countsForTime = true;

        [Header("Visual")]
        public GameObject prefab;

        [Header("Entry/Exit")]
        public Vector3 entryOffset = Vector3.zero;
        public Vector3 exitOffset = Vector3.zero;

        [Tooltip("Acceptable entry angle range in degrees")]
        [Range(30f, 90f)]
        public float entryAngleTolerance = 60f;
    }
}
