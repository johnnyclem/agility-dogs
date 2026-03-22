using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Presentation.Camera
{
    public class AgilityCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 2f, 0f);

        [Header("Following")]
        [SerializeField] private float followDistance = 6f;
        [SerializeField] private float followHeight = 3f;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float lookSpeed = 10f;

        [Header("Camera Modes")]
        [SerializeField] private float overviewDistance = 15f;
        [SerializeField] private float overviewHeight = 10f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.2f;
        [SerializeField] private float rotationSmoothTime = 0.15f;

        private CameraMode currentMode = CameraMode.Follow;
        private Vector3 smoothVelocity;
        private float currentDistance;
        private float currentHeight;
        private bool isFollowing = true;
        private Transform[] cinematicWaypoints;
        private float cinematicDuration;
        private float cinematicProgress;
        private bool cinematicLoop = false;
        private CameraMode previousMode;
        private float cutawayDuration = 2f;
        private float cutawayTimer;
        private int cutawayAngleIndex;

        public enum CameraMode
        {
            Follow,
            Overview,
            SideOn,
            DogPOV,
            Cinematic,
            Free,
            Cutaway
        }

        public CameraMode CurrentMode => currentMode;

        private void OnEnable()
        {
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
        }

        private void OnDisable()
        {
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            TriggerCutaway(2f);
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            TriggerCutaway(3f);
        }

        public void TriggerCutaway(float duration)
        {
            if (currentMode == CameraMode.Cutaway) return; // already in cutaway
            previousMode = currentMode;
            cutawayDuration = duration;
            cutawayTimer = 0f;
            cutawayAngleIndex = Random.Range(0, 4); // assume 4 predefined angles
            currentMode = CameraMode.Cutaway;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            switch (currentMode)
            {
                case CameraMode.Follow:
                    UpdateFollowCamera();
                    break;
                case CameraMode.Overview:
                    UpdateOverviewCamera();
                    break;
                case CameraMode.SideOn:
                    UpdateSideOnCamera();
                    break;
                case CameraMode.DogPOV:
                    UpdateDogPOV();
                    break;
                case CameraMode.Cinematic:
                    UpdateCinematicCamera();
                    break;
                case CameraMode.Free:
                    break;
                case CameraMode.Cutaway:
                    UpdateCutawayCamera();
                    break;
            }
        }

        private void UpdateFollowCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 desiredPosition = targetPos - target.forward * followDistance + Vector3.up * followHeight;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime
            );

            Vector3 lookTarget = targetPos + target.forward * 2f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * Time.deltaTime
            );
        }

        private void UpdateOverviewCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 desiredPosition = targetPos + Vector3.up * overviewHeight - target.forward * 2f;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime * 2f
            );

            Quaternion desiredRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * 0.5f * Time.deltaTime
            );
        }

        private void UpdateSideOnCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 sideDir = Vector3.Cross(target.forward, Vector3.up).normalized;
            Vector3 desiredPosition = targetPos + sideDir * followDistance * 1.2f + Vector3.up * followHeight * 0.8f;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime
            );

            Quaternion desiredRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * Time.deltaTime
            );
        }

        private void UpdateDogPOV()
        {
            if (target == null) return;
            transform.position = target.position + Vector3.up * 0.5f;
            transform.rotation = Quaternion.LookRotation(target.forward);
        }

        private void UpdateCinematicCamera()
        {
            if (cinematicWaypoints == null || cinematicWaypoints.Length < 2)
            {
                // Not enough waypoints, fallback to follow camera
                UpdateFollowCamera();
                return;
            }

            if (cinematicDuration <= 0f)
            {
                cinematicDuration = 5f; // default duration
            }

            cinematicProgress += Time.deltaTime / cinematicDuration;
            if (cinematicProgress >= 1f)
            {
                if (cinematicLoop)
                {
                    cinematicProgress = 0f;
                }
                else
                {
                    // Cinematic finished, return to follow camera
                    currentMode = CameraMode.Follow;
                    return;
                }
            }

            // Find current segment
            int totalSegments = cinematicWaypoints.Length - 1;
            float segmentProgress = cinematicProgress * totalSegments;
            int segmentIndex = Mathf.FloorToInt(segmentProgress);
            if (segmentIndex >= totalSegments) segmentIndex = totalSegments - 1;
            float segmentT = segmentProgress - segmentIndex;

            Vector3 start = cinematicWaypoints[segmentIndex].position;
            Vector3 end = cinematicWaypoints[segmentIndex + 1].position;
            Vector3 position = Vector3.Lerp(start, end, segmentT);

            // Use smooth damping for smoother movement
            transform.position = Vector3.SmoothDamp(transform.position, position, ref smoothVelocity, positionSmoothTime);

            // Look at target or next waypoint
            Vector3 lookTarget = target != null ? target.position : end;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSpeed * Time.deltaTime);
        }

        private void UpdateCutawayCamera()
        {
            cutawayTimer += Time.unscaledDeltaTime; // use unscaled time if timeScale is 0
            if (cutawayTimer >= cutawayDuration)
            {
                // Cutaway finished, return to previous mode
                currentMode = previousMode;
                return;
            }

            if (target == null) return;

            Vector3 targetPos = target.position + targetOffset;
            Vector3 desiredPosition = Vector3.zero;

            switch (cutawayAngleIndex)
            {
                case 0: // side left
                    desiredPosition = targetPos - target.right * followDistance + Vector3.up * followHeight;
                    break;
                case 1: // side right
                    desiredPosition = targetPos + target.right * followDistance + Vector3.up * followHeight;
                    break;
                case 2: // front low
                    desiredPosition = targetPos - target.forward * followDistance * 0.5f + Vector3.up * followHeight * 0.3f;
                    break;
                case 3: // rear high
                    desiredPosition = targetPos + target.forward * followDistance * 0.8f + Vector3.up * followHeight * 1.5f;
                    break;
                default:
                    desiredPosition = targetPos - target.forward * followDistance + Vector3.up * followHeight;
                    break;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, positionSmoothTime);
            Quaternion desiredRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetMode(CameraMode mode)
        {
            currentMode = mode;
        }

        public void SetFollowParameters(float distance, float height, float speed)
        {
            followDistance = distance;
            followHeight = height;
            followSpeed = speed;
        }

        public void ToggleMode()
        {
            int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
            currentMode = (CameraMode)nextMode;
        }

        public void SetCinematicPath(Transform[] waypoints, float duration, bool loop = false)
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogError("Cinematic path requires at least two waypoints");
                return;
            }
            cinematicWaypoints = waypoints;
            cinematicDuration = duration;
            cinematicProgress = 0f;
            cinematicLoop = loop;
            currentMode = CameraMode.Cinematic;
        }
    }
}
