using UnityEngine;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Events;

namespace AgilityDogs.Gameplay.Obstacles
{
    /// <summary>
    /// Precise Contact Zone - Implements millimeter-accurate contact detection
    /// Uses raycasting from dog paw positions instead of capsule overlaps
    /// </summary>
    public class PreciseContactZone : MonoBehaviour
    {
        [Header("Contact Zone Configuration")]
        [SerializeField] private ObstacleBase parentObstacle;
        [SerializeField] private Transform contactZoneStart;
        [SerializeField] private Transform contactZoneEnd;
        [SerializeField] private float contactZoneWidth = 0.36f; // Standard contact zone length
        [SerializeField] private LayerMask contactZoneLayerMask = ~0;
        
        [Header("Paw Detection Settings")]
        [SerializeField] private string[] pawBoneNames = { "Paw_Front_L", "Paw_Front_R", "Paw_Rear_L", "Paw_Rear_R" };
        [SerializeField] private float raycastHeight = 0.5f;
        [SerializeField] private float raycastDistance = 1.0f;
        [SerializeField] private float pawHitRadius = 0.1f;
        
        [Header("Precision Settings")]
        [SerializeField] private bool useContinuousDetection = true;
        [SerializeField] private float detectionInterval = 0.05f; // 20Hz detection
        [SerializeField] private bool requireBothFrontPaws = true;
        [SerializeField] private bool requireBothRearPaws = false;
        
        [Header("Visualization")]
        [SerializeField] private bool drawDebugRays = false;
        [SerializeField] private Color contactZoneColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color hitColor = Color.green;
        [SerializeField] private Color missColor = Color.red;
        
        // Detection state
        private DogAgentController currentDog;
        private Animator dogAnimator;
        private bool isInContactZone = false;
        private bool hasHitContactStart = false;
        private bool hasHitContactEnd = false;
        private float lastDetectionTime;
        
        // Paw tracking
        private Transform[] pawTransforms;
        private bool[] pawHitsStart;
        private bool[] pawHitsEnd;
        
        // Contact validation
        private bool contactValid = false;
        private float contactStartTime;
        
        private void Awake()
        {
            if (parentObstacle == null)
                parentObstacle = GetComponentInParent<ObstacleBase>();
            
            // Initialize arrays
            pawHitsStart = new bool[pawBoneNames.Length];
            pawHitsEnd = new bool[pawBoneNames.Length];
        }
        
        private void OnEnable()
        {
            // Subscribe to obstacle events
            if (parentObstacle != null)
            {
                // We'll detect when dog enters/exits the obstacle through other means
            }
        }
        
        private void OnDisable()
        {
            ResetDetection();
        }
        
        private void Update()
        {
            if (!useContinuousDetection || currentDog == null)
                return;
            
            if (Time.time - lastDetectionTime >= detectionInterval)
            {
                lastDetectionTime = Time.time;
                DetectPawContacts();
            }
        }
        
        /// <summary>
        /// Register a dog for precise contact detection
        /// </summary>
        public void RegisterDog(DogAgentController dog)
        {
            if (dog == null) return;
            
            currentDog = dog;
            dogAnimator = dog.GetComponent<Animator>();
            
            // Find paw transforms
            FindPawTransforms();
            
            ResetDetection();
            
            if (drawDebugRays)
            {
                Debug.Log($"[PreciseContactZone] Registered dog for contact detection: {dog.name}");
            }
        }
        
        /// <summary>
        /// Unregister the current dog
        /// </summary>
        public void UnregisterDog()
        {
            currentDog = null;
            dogAnimator = null;
            pawTransforms = null;
            ResetDetection();
        }
        
        /// <summary>
        /// Find paw bone transforms in the dog's skeleton
        /// </summary>
        private void FindPawTransforms()
        {
            if (dogAnimator == null || !dogAnimator.isHuman)
            {
                // Try to find paws by name in children
                pawTransforms = new Transform[pawBoneNames.Length];
                for (int i = 0; i < pawBoneNames.Length; i++)
                {
                    pawTransforms[i] = FindChildRecursive(currentDog.transform, pawBoneNames[i]);
                }
            }
            else
            {
                // Use humanoid avatar bone mapping
                pawTransforms = new Transform[4];
                pawTransforms[0] = dogAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                pawTransforms[1] = dogAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
                pawTransforms[2] = dogAnimator.GetBoneTransform(HumanBodyBones.LeftToes);
                pawTransforms[3] = dogAnimator.GetBoneTransform(HumanBodyBones.RightToes);
            }
        }
        
        /// <summary>
        /// Find a child transform recursively by name
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(childName))
                    return child;
                
                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        /// <summary>
        /// Detect paw contacts using raycasting
        /// </summary>
        private void DetectPawContacts()
        {
            if (currentDog == null || pawTransforms == null)
                return;
            
            // Reset paw hits
            for (int i = 0; i < pawHitsStart.Length; i++)
            {
                pawHitsStart[i] = false;
                pawHitsEnd[i] = false;
            }
            
            // Raycast from each paw
            for (int i = 0; i < pawTransforms.Length; i++)
            {
                if (pawTransforms[i] == null) continue;
                
                Vector3 pawPosition = pawTransforms[i].position;
                Vector3 rayOrigin = pawPosition + Vector3.up * raycastHeight;
                
                // Check contact zone start
                if (contactZoneStart != null)
                {
                    if (RaycastToZone(rayOrigin, contactZoneStart.position, contactZoneWidth))
                    {
                        pawHitsStart[i] = true;
                        
                        if (drawDebugRays)
                            Debug.DrawLine(rayOrigin, contactZoneStart.position, hitColor, 0.1f);
                    }
                }
                
                // Check contact zone end
                if (contactZoneEnd != null)
                {
                    if (RaycastToZone(rayOrigin, contactZoneEnd.position, contactZoneWidth))
                    {
                        pawHitsEnd[i] = true;
                        
                        if (drawDebugRays)
                            Debug.DrawLine(rayOrigin, contactZoneEnd.position, hitColor, 0.1f);
                    }
                }
            }
            
            // Evaluate contact validity
            EvaluateContact();
        }
        
        /// <summary>
        /// Raycast to a zone position
        /// </summary>
        private bool RaycastToZone(Vector3 origin, Vector3 zonePosition, float zoneRadius)
        {
            Vector3 direction = zonePosition - origin;
            float distance = direction.magnitude;
            
            if (distance > raycastDistance)
                return false;
            
            RaycastHit hit;
            if (Physics.SphereCast(origin, pawHitRadius, direction.normalized, out hit, distance, contactZoneLayerMask))
            {
                // Check if we hit the contact zone
                ObstacleTriggerZone zone = hit.collider.GetComponent<ObstacleTriggerZone>();
                if (zone != null)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Evaluate contact validity based on paw hits
        /// </summary>
        private void EvaluateContact()
        {
            if (currentDog == null) return;
            
            bool startHit = false;
            bool endHit = false;
            
            // Check front paws
            if (requireBothFrontPaws && pawTransforms.Length >= 2)
            {
                startHit = pawHitsStart[0] && pawHitsStart[1];
                endHit = pawHitsEnd[0] && pawHitsEnd[1];
            }
            else
            {
                // Any paw hit counts
                for (int i = 0; i < pawHitsStart.Length; i++)
                {
                    if (pawHitsStart[i]) startHit = true;
                    if (pawHitsEnd[i]) endHit = true;
                }
            }
            
            // Update state
            if (startHit && !hasHitContactStart)
            {
                hasHitContactStart = true;
                contactStartTime = Time.time;
                
                if (drawDebugRays)
                    Debug.Log($"[PreciseContactZone] Dog hit contact zone start at {contactStartTime}");
            }
            
            if (endHit && !hasHitContactEnd)
            {
                hasHitContactEnd = true;
                
                if (drawDebugRays)
                    Debug.Log($"[PreciseContactZone] Dog hit contact zone end at {Time.time}");
            }
            
            // Validate contact (must hit both start and end)
            contactValid = hasHitContactStart && hasHitContactEnd;
            
            // Notify parent obstacle
            if (parentObstacle != null)
            {
                parentObstacle.OnDogContactZoneEnter(currentDog, hasHitContactStart);
            }
        }
        
        /// <summary>
        /// Check if contact was valid (dog touched both start and end zones)
        /// </summary>
        public bool IsContactValid()
        {
            return contactValid;
        }
        
        /// <summary>
        /// Check if dog hit the contact zone start
        /// </summary>
        public bool HasHitContactStart()
        {
            return hasHitContactStart;
        }
        
        /// <summary>
        /// Check if dog hit the contact zone end
        /// </summary>
        public bool HasHitContactEnd()
        {
            return hasHitContactEnd;
        }
        
        /// <summary>
        /// Get contact accuracy percentage
        /// </summary>
        public float GetContactAccuracy()
        {
            if (!hasHitContactStart && !hasHitContactEnd)
                return 0f;
            
            int hits = 0;
            int total = 0;
            
            for (int i = 0; i < pawHitsStart.Length; i++)
            {
                if (pawHitsStart[i]) hits++;
                if (pawHitsEnd[i]) hits++;
                total += 2;
            }
            
            return (float)hits / total;
        }
        
        /// <summary>
        /// Reset detection state
        /// </summary>
        public void ResetDetection()
        {
            hasHitContactStart = false;
            hasHitContactEnd = false;
            contactValid = false;
            isInContactZone = false;
            
            for (int i = 0; i < pawHitsStart.Length; i++)
            {
                pawHitsStart[i] = false;
                pawHitsEnd[i] = false;
            }
        }
        
        /// <summary>
        /// Manually trigger contact detection (for testing)
        /// </summary>
        [ContextMenu("Force Contact Detection")]
        public void ForceContactDetection()
        {
            DetectPawContacts();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!drawDebugRays) return;
            
            // Draw contact zone
            if (contactZoneStart != null && contactZoneEnd != null)
            {
                Gizmos.color = contactZoneColor;
                Gizmos.DrawWireSphere(contactZoneStart.position, contactZoneWidth);
                Gizmos.DrawWireSphere(contactZoneEnd.position, contactZoneWidth);
                Gizmos.DrawLine(contactZoneStart.position, contactZoneEnd.position);
            }
            
            // Draw paw positions if dog is registered
            if (currentDog != null && pawTransforms != null)
            {
                for (int i = 0; i < pawTransforms.Length; i++)
                {
                    if (pawTransforms[i] == null) continue;
                    
                    Gizmos.color = pawHitsStart[i] || pawHitsEnd[i] ? hitColor : missColor;
                    Gizmos.DrawWireSphere(pawTransforms[i].position, pawHitRadius);
                }
            }
        }
    }
}