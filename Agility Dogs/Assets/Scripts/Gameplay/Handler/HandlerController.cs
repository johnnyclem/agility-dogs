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

        private CharacterController characterController;
        private Vector3 velocity;
        private float turnSmoothVelocity;
        private bool isSprinting;
        private bool isEnabled = true;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;

        public Vector3 Velocity => velocity;
        public Vector3 FlatVelocity => new Vector3(velocity.x, 0f, velocity.z);
        public float CurrentSpeed => FlatVelocity.magnitude;
        public bool IsSprinting => isSprinting;
        public Vector3 Forward => transform.forward;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

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
