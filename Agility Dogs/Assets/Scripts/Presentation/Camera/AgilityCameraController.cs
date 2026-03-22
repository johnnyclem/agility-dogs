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

        [Header("Dog POV")]
        [SerializeField] private float dogPOVHeight = 0.4f;
        [SerializeField] private float dogPOVShakeAmount = 0.02f;
        [SerializeField] private float dogPOVSpeedMultiplier = 1.5f;

        [Header("Replay Cameras")]
        [SerializeField] private Transform[] replayCameraPositions;
        [SerializeField] private float replayCameraBlendTime = 0.5f;
        [SerializeField] private float autoReplaySwitchTime = 3f;

        [Header("Freeze Frame")]
        [SerializeField] private float freezeFrameDuration = 0.5f;
        [SerializeField] private float freezeFrameSlowMotionScale = 0.1f;
        [SerializeField] private float freezeFrameZoomFactor = 0.7f;
        [SerializeField] private AnimationCurve freezeFrameEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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

        // Replay camera state
        private int currentReplayCameraIndex;
        private float replayCameraTimer;
        private bool autoSwitchReplayCameras = true;
        private UnityEngine.Camera[] replayCameras;

        // Freeze frame state
        private bool isFreezeFrameActive;
        private float freezeFrameTimer;
        private float originalTimeScale;
        private Vector3 freezeFrameTargetPosition;
        private Quaternion freezeFrameTargetRotation;
        private float freezeFrameFOV;

        public enum CameraMode
        {
            Follow,
            Overview,
            SideOn,
            DogPOV,
            Cinematic,
            Free,
            Cutaway,
            Replay
        }

        public CameraMode CurrentMode => currentMode;

        private void OnEnable()
        {
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTimeRecorded;
        }

        private void OnDisable()
        {
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTimeRecorded;
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (clean)
            {
                // Trigger freeze frame on clean obstacle completion
                TriggerFreezeFrame(0.3f, target.position);
            }
            TriggerCutaway(2f);
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            // Longer freeze frame on faults for dramatic effect
            TriggerFreezeFrame(0.5f, target.position);
            TriggerCutaway(3f);
        }

        private void HandleSplitTimeRecorded(float time)
        {
            // Quick freeze frame on split times
            TriggerFreezeFrame(0.2f, target.position);
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
            
            // Always update freeze frame if active
            if (isFreezeFrameActive)
            {
                UpdateFreezeFrame();
                return;
            }

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
                case CameraMode.Replay:
                    UpdateReplayCamera(Time.deltaTime);
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
            
            // Position camera at dog's eye level with slight offset
            Vector3 dogPos = target.position;
            Vector3 dogForward = target.forward;
            
            // Calculate eye level based on dog size (simplified)
            float eyeHeight = dogPOVHeight;
            
            // Add slight offset to simulate dog's head position
            Vector3 headOffset = Vector3.up * eyeHeight + dogForward * 0.2f;
            
            // Add subtle shake based on speed
            float speed = target.GetComponent<AgilityDogs.Gameplay.Dog.DogAgentController>()?.Speed ?? 0f;
            if (speed > 1f)
            {
                float shake = Mathf.Sin(Time.time * 20f) * dogPOVShakeAmount * speed;
                headOffset += Vector3.right * shake;
                headOffset += Vector3.up * Mathf.Abs(shake) * 0.5f;
            }
            
            // Smooth camera movement
            Vector3 desiredPosition = dogPos + headOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
            
            // Look ahead based on speed and movement direction
            float lookAheadDistance = 2f + speed * dogPOVSpeedMultiplier;
            Vector3 lookTarget = dogPos + dogForward * lookAheadDistance + Vector3.up * 0.3f;
            
            // Add some randomness to make it feel more natural
            lookTarget += Vector3.right * Mathf.Sin(Time.time * 3f) * 0.1f;
            
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * 8f);
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

        /// <summary>
        /// Triggers a freeze-frame highlight moment
        /// </summary>
        public void TriggerFreezeFrame(float duration, Vector3 focusPoint)
        {
            if (isFreezeFrameActive) return;
            
            isFreezeFrameActive = true;
            freezeFrameTimer = 0f;
            freezeFrameDuration = duration;
            freezeFrameTargetPosition = focusPoint;
            
            // Store original time scale
            originalTimeScale = Time.timeScale;
            
            // Calculate camera position for freeze frame
            freezeFrameTargetRotation = Quaternion.LookRotation(focusPoint - transform.position);
            freezeFrameFOV = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.fieldOfView : 60f;
        }

        private void UpdateFreezeFrame()
        {
            if (!isFreezeFrameActive) return;
            
            freezeFrameTimer += Time.unscaledDeltaTime;
            float progress = freezeFrameTimer / freezeFrameDuration;
            
            if (progress >= 1f)
            {
                // End freeze frame
                isFreezeFrameActive = false;
                Time.timeScale = originalTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                
                // Restore FOV
                if (UnityEngine.Camera.main != null)
                {
                    UnityEngine.Camera.main.fieldOfView = freezeFrameFOV;
                }
                return;
            }
            
            // Apply slow motion effect
            float timeScaleCurve = freezeFrameEase.Evaluate(progress);
            Time.timeScale = Mathf.Lerp(freezeFrameSlowMotionScale, originalTimeScale, timeScaleCurve);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            
            // Zoom effect
            if (UnityEngine.Camera.main != null)
            {
                float zoomedFOV = freezeFrameFOV * freezeFrameZoomFactor;
                UnityEngine.Camera.main.fieldOfView = Mathf.Lerp(zoomedFOV, freezeFrameFOV, timeScaleCurve);
            }
            
            // Smooth camera rotation toward focus point
            Quaternion targetRot = Quaternion.LookRotation(freezeFrameTargetPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.unscaledDeltaTime * 5f);
        }

        /// <summary>
        /// Sets up the replay camera network
        /// </summary>
        public void SetupReplayCameras(Transform[] cameraPositions)
        {
            replayCameraPositions = cameraPositions;
            
            if (cameraPositions != null && cameraPositions.Length > 0)
            {
                // Initialize replay cameras array
                replayCameras = new UnityEngine.Camera[cameraPositions.Length];
            }
        }

        /// <summary>
        /// Switches to a specific replay camera
        /// </summary>
        public void SetReplayCamera(int index)
        {
            if (replayCameraPositions == null || index < 0 || index >= replayCameraPositions.Length)
                return;
            
            currentReplayCameraIndex = index;
            replayCameraTimer = 0f;
            
            Transform camPos = replayCameraPositions[index];
            if (camPos != null)
            {
                transform.position = camPos.position;
                transform.rotation = camPos.rotation;
            }
        }

        /// <summary>
        /// Cycles through replay cameras
        /// </summary>
        public void CycleReplayCamera()
        {
            if (replayCameraPositions == null || replayCameraPositions.Length == 0) return;
            
            currentReplayCameraIndex = (currentReplayCameraIndex + 1) % replayCameraPositions.Length;
            SetReplayCamera(currentReplayCameraIndex);
        }

        /// <summary>
        /// Updates replay camera auto-switching
        /// </summary>
        private void UpdateReplayCamera(float deltaTime)
        {
            if (!autoSwitchReplayCameras || replayCameraPositions == null) return;
            
            replayCameraTimer += deltaTime;
            
            if (replayCameraTimer >= autoReplaySwitchTime)
            {
                CycleReplayCamera();
            }
        }

        /// <summary>
        /// Gets the best camera angle for a given moment
        /// </summary>
        public int GetBestCameraAngle(Vector3 focusPoint, Vector3 velocity)
        {
            if (replayCameraPositions == null || replayCameraPositions.Length == 0) return -1;
            
            int bestIndex = 0;
            float bestScore = float.MinValue;
            
            for (int i = 0; i < replayCameraPositions.Length; i++)
            {
                Transform cam = replayCameraPositions[i];
                if (cam == null) continue;
                
                // Score based on:
                // 1. Visibility of focus point
                // 2. Angle relative to velocity
                // 3. Distance (prefer closer for action, farther for overview)
                
                Vector3 camToFocus = focusPoint - cam.position;
                float distance = camToFocus.magnitude;
                
                // Check if focus point is in camera's view
                Vector3 camForward = cam.forward;
                float dotProduct = Vector3.Dot(camForward, camToFocus.normalized);
                
                if (dotProduct < 0.3f) continue; // Not visible
                
                // Score calculation
                float score = dotProduct * 10f;
                
                // Bonus for being perpendicular to velocity (side-on action shot)
                if (velocity.magnitude > 0.5f)
                {
                    float velocityAngle = Vector3.Angle(camForward, velocity.normalized);
                    if (velocityAngle > 70f && velocityAngle < 110f)
                    {
                        score += 5f; // Side-on bonus
                    }
                }
                
                // Penalize very close or very far cameras
                if (distance < 5f) score -= 2f;
                if (distance > 20f) score -= 1f;
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            
            return bestIndex;
        }

        /// <summary>
        /// Enables or disables automatic replay camera switching
        /// </summary>
        public void SetAutoReplayCameraSwitch(bool enabled)
        {
            autoSwitchReplayCameras = enabled;
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
