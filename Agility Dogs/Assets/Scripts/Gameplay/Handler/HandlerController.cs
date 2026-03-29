using UnityEngine;
using UnityEngine.InputSystem;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Services;

namespace AgilityDogs.Gameplay.Handler
{
    [RequireComponent(typeof(CharacterController))]
    public class HandlerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float turnSmoothTime = 0.15f;
        [SerializeField] private float gravity = -15f;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        [Header("Gesture System")]
        [SerializeField] private float gestureHoldTime = 0.3f;
        [SerializeField] private float gestureReleaseWindow = 0.5f;
        [SerializeField] private float pointGestureThreshold = 45f;
        [SerializeField] private LayerMask obstacleLayerMask = ~0;

        [Header("Directional Lean")]
        [SerializeField] private float leanInfluenceRadius = 10f;
        [SerializeField] private float leanAngleModifier = 0.5f;
        [SerializeField] private float leanBodyRotation = 30f;

        [Header("Contextual Commands")]
        [SerializeField] private float velocityCommandThreshold = 0.5f;
        [SerializeField] private float facingCommandAngle = 60f;
        [SerializeField] private float commandCooldown = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip pointSound;
        [SerializeField] private AudioClip beckonSound;
        [SerializeField] private AudioSource gestureAudioSource;

        private CharacterController characterController;
        private Vector3 velocity;
        private float turnSmoothVelocity;
        private bool isSprinting;
        private bool isEnabled = true;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;

        // Gesture state
        private float gestureHoldTimer;
        private bool isGesturing;
        private GestureType currentGesture = GestureType.None;
        private Vector3 gestureDirection;
        private float gestureReleaseTimer;

        // Lean state
        private float currentLeanAngle;
        private float targetLeanAngle;
        private Vector3 leanDirection;

        // Contextual command state
        private float lastCommandTime;
        private HandlerCommand lastCommand;
        private Vector3 lastCommandForward;

        // Properties
        public Vector3 Velocity => velocity;
        public Vector3 FlatVelocity => new Vector3(velocity.x, 0f, velocity.z);
        public float CurrentSpeed => FlatVelocity.magnitude;
        public bool IsSprinting => isSprinting;
        public Vector3 Forward => transform.forward;
        public float LeanAngle => currentLeanAngle;
        public Vector3 LeanDirection => leanDirection;
        public bool IsGesturing => isGesturing;
        public GestureType CurrentGesture => currentGesture;
        public Vector3 GestureDirection => gestureDirection;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();

            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                moveAction = playerInput.actions["Move"];
                lookAction = playerInput.actions["Look"];
                sprintAction = playerInput.actions["Sprint"];
            }
        }

        private void Update()
        {
            if (!isEnabled) return;
            if (GameManager.Instance.CurrentState != GameState.Gameplay) return;

            HandleMovement();
            UpdateGestureSystem();
            UpdateDirectionalLean();
            ApplyGravity();
        }

        private void HandleMovement()
        {
            if (moveAction == null) return;

            Vector2 input = moveAction.ReadValue<Vector2>();
            isSprinting = sprintAction != null && sprintAction.IsPressed();

            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                if (cameraTransform != null)
                {
                    targetAngle += cameraTransform.eulerAngles.y;
                }

                float smoothedAngle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );

                transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

                float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                characterController.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

                velocity.x = moveDir.x * currentSpeed;
                velocity.z = moveDir.z * currentSpeed;
            }
            else
            {
                velocity.x = 0f;
                velocity.z = 0f;
            }
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
        }

        private void UpdateGestureSystem()
        {
            if (lookAction == null) return;

            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            // Check for gesture initiation based on look direction while moving
            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            
            if (moveInput.magnitude > 0.1f && lookInput.magnitude > 0.3f)
            {
                float angleToLook = Vector2.SignedAngle(moveInput, lookInput);
                
                if (Mathf.Abs(angleToLook) > pointGestureThreshold && !isGesturing)
                {
                    StartGesture(angleToLook);
                }
            }
            
            // Update gesture timer
            if (isGesturing)
            {
                gestureHoldTimer += Time.deltaTime;
                
                // Release gesture when look returns to forward or hold time exceeds
                if (lookInput.magnitude < 0.2f || gestureHoldTimer > gestureHoldTime * 3f)
                {
                    EndGesture();
                }
            }
            
            // Track gesture release window
            if (currentGesture != GestureType.None && !isGesturing)
            {
                gestureReleaseTimer += Time.deltaTime;
                if (gestureReleaseTimer > gestureReleaseWindow)
                {
                    currentGesture = GestureType.None;
                }
            }
        }

        private void StartGesture(float angle)
        {
            isGesturing = true;
            gestureHoldTimer = 0f;
            gestureReleaseTimer = 0f;
            
            // Determine gesture type based on angle
            if (angle > 0)
            {
                currentGesture = GestureType.PointRight;
                gestureDirection = transform.right;
            }
            else
            {
                currentGesture = GestureType.PointLeft;
                gestureDirection = -transform.right;
            }
            
            // Play gesture sound
            if (gestureAudioSource != null && pointSound != null)
            {
                gestureAudioSource.PlayOneShot(pointSound);
            }
            
            // Fire gesture event
            GameEvents.RaiseHandlerGesture(currentGesture, gestureDirection);
        }

        private void EndGesture()
        {
            isGesturing = false;
            
            // Determine if gesture was a point or beckon based on duration
            if (gestureHoldTimer < gestureHoldTime * 0.5f)
            {
                // Quick gesture = beckon
                currentGesture = GestureType.Beckon;
                gestureDirection = transform.forward;
                
                if (gestureAudioSource != null && beckonSound != null)
                {
                    gestureAudioSource.PlayOneShot(beckonSound);
                }
            }
            else
            {
                // Long hold = sustained point
                currentGesture = GestureType.Point;
            }
            
            // Fire gesture end event
            GameEvents.RaiseHandlerGesture(currentGesture, gestureDirection);
        }

        private void UpdateDirectionalLean()
        {
            // Calculate lean based on movement direction and speed
            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            
            if (moveInput.magnitude > 0.1f)
            {
                // Convert input to world space lean direction
                float inputAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                Vector3 worldLeanDir = Quaternion.Euler(0f, inputAngle + (cameraTransform != null ? cameraTransform.eulerAngles.y : 0f), 0f) * Vector3.forward;
                
                leanDirection = worldLeanDir;
                
                // Calculate lean angle based on speed and turn rate
                float speedFactor = CurrentSpeed / (isSprinting ? sprintSpeed : walkSpeed);
                float turnRate = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 
                    Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg)) / Time.deltaTime;
                
                targetLeanAngle = (speedFactor * 15f) + (turnRate * leanAngleModifier);
                targetLeanAngle = Mathf.Clamp(targetLeanAngle, 0f, leanBodyRotation);
            }
            else
            {
                targetLeanAngle = 0f;
                leanDirection = Vector3.zero;
            }
            
            // Smooth lean transition
            currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, Time.deltaTime * 8f);
        }

        /// <summary>
        /// Gets the handler's body orientation for contextual commands
        /// </summary>
        public Vector3 GetContextualFacing()
        {
            // Return facing direction adjusted by lean
            Vector3 facing = transform.forward;
            if (leanDirection.magnitude > 0.1f)
            {
                facing = Quaternion.Euler(0f, currentLeanAngle * 0.5f, 0f) * facing;
            }
            return facing;
        }

        /// <summary>
        /// Checks if handler is facing a specific direction relative to velocity
        /// </summary>
        public bool IsFacingDirection(Vector3 direction, float threshold = 0.5f)
        {
            Vector3 facing = GetContextualFacing();
            float dot = Vector3.Dot(facing.normalized, direction.normalized);
            return dot > threshold;
        }

        /// <summary>
        /// Issues a contextual command based on handler state
        /// </summary>
        public void IssueContextualCommand(HandlerCommand baseCommand)
        {
            if (Time.time - lastCommandTime < commandCooldown) return;
            
            lastCommandTime = Time.time;
            lastCommand = baseCommand;
            lastCommandForward = GetContextualFacing();
            
            // Contextual modifications based on handler state
            HandlerCommand contextualCommand = baseCommand;
            
            // Add direction context for directional commands
            if (baseCommand == HandlerCommand.ComeBye || baseCommand == HandlerCommand.Away ||
                baseCommand == HandlerCommand.Left || baseCommand == HandlerCommand.Right)
            {
                // Use lean direction to influence the command
                Vector3 commandDirection = GetCommandDirection(baseCommand);
                
                // If leaning in a different direction, adjust the command
                if (leanDirection.magnitude > 0.3f)
                {
                    float leanDot = Vector3.Dot(commandDirection, leanDirection);
                    if (leanDot < 0)
                    {
                        // Reverse direction command if leaning opposite way
                        contextualCommand = ReverseDirectionCommand(baseCommand);
                    }
                }
            }
            
            // Fire contextual command event
            GameEvents.RaiseContextualCommand(contextualCommand, lastCommandForward, CurrentSpeed);
        }

        private Vector3 GetCommandDirection(HandlerCommand command)
        {
            switch (command)
            {
                case HandlerCommand.ComeBye:
                case HandlerCommand.Left:
                    return -transform.right;
                case HandlerCommand.Away:
                case HandlerCommand.Right:
                    return transform.right;
                default:
                    return transform.forward;
            }
        }

        private HandlerCommand ReverseDirectionCommand(HandlerCommand command)
        {
            switch (command)
            {
                case HandlerCommand.ComeBye:
                    return HandlerCommand.Away;
                case HandlerCommand.Away:
                    return HandlerCommand.ComeBye;
                case HandlerCommand.Left:
                    return HandlerCommand.Right;
                case HandlerCommand.Right:
                    return HandlerCommand.Left;
                default:
                    return command;
            }
        }

        /// <summary>
        /// Gets the handler's influence on dog's path based on position
        /// </summary>
        public float GetPathInfluence(Vector3 dogPosition, Vector3 obstaclePosition)
        {
            // Calculate how handler position influences dog's line to obstacle
            Vector3 dogToObstacle = (obstaclePosition - dogPosition).normalized;
            Vector3 handlerToObstacle = (obstaclePosition - transform.position).normalized;
            Vector3 dogToHandler = (transform.position - dogPosition).normalized;
            
            // Handler has more influence when closer to dog
            float distanceToDog = Vector3.Distance(transform.position, dogPosition);
            float distanceFactor = Mathf.Clamp01(1f - (distanceToDog / leanInfluenceRadius));
            
            // Calculate angle between handler's position and dog's current path
            float pathAngle = Vector3.SignedAngle(dogToObstacle, dogToHandler, Vector3.up);
            
            // Normalize to -1 to 1 range
            float influence = (pathAngle / 180f) * distanceFactor;
            
            return influence;
        }

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        public void SetCameraTransform(Transform cam)
        {
            cameraTransform = cam;
        }

        public Vector3 GetDirectionTo(Vector3 target)
        {
            return (target - transform.position).normalized;
        }

        public float GetAngleTo(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            return Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        }
    }
}
