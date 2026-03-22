using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewBreedData", menuName = "Agility Dogs/Breed Data")]
    public class BreedData : ScriptableObject
    {
        [Header("Breed Info")]
        public string breedName;
        public string displayName;

        [Header("Physical")]
        [Range(0.5f, 2.0f)]
        public float modelScale = 1.0f;

        [Header("Movement")]
        [Tooltip("Maximum sprint speed in m/s")]
        [Range(3f, 12f)]
        public float maxSpeed = 7.5f;

        [Tooltip("Acceleration in m/s^2")]
        [Range(2f, 15f)]
        public float acceleration = 6f;

        [Tooltip("Deceleration / braking in m/s^2")]
        [Range(3f, 15f)]
        public float deceleration = 8f;

        [Tooltip("Turn rate in degrees/sec at full speed")]
        [Range(60f, 540f)]
        public float turnRate = 180f;

        [Header("Agility Attributes")]
        [Tooltip("How tight the dog can turn at speed (lower = tighter)")]
        [Range(0.3f, 1.0f)]
        public float turningRadius = 0.6f;

        [Tooltip("Responsiveness to handler commands (0=delayed, 1=instant)")]
        [Range(0.1f, 1.0f)]
        public float responsiveness = 0.7f;

        [Tooltip("How much momentum the dog carries (higher = harder to redirect)")]
        [Range(0.3f, 1.0f)]
        public float momentumFactor = 0.6f;

        [Tooltip("Tolerance for wide handling lines (higher = more forgiving)")]
        [Range(0.3f, 1.0f)]
        public float handlingTolerance = 0.65f;

        [Header("Obstacle Performance")]
        [Tooltip("Weave pole speed multiplier")]
        [Range(0.5f, 1.5f)]
        public float weaveSpeed = 1.0f;

        [Tooltip("Contact obstacle speed multiplier")]
        [Range(0.5f, 1.3f)]
        public float contactSpeed = 0.85f;

        [Tooltip("Jump power / clearance")]
        [Range(0.5f, 1.5f)]
        public float jumpPower = 1.0f;

        [Header("Visual")]
        public RuntimeAnimatorController animatorController;
        public GameObject prefab;

        [Header("Personality")]
        [TextArea(2, 4)]
        public string description;

        [Header("Size Classification")]
        public AgilitySizeDivision defaultDivision = AgilitySizeDivision.FourteenInch;
    }
}
