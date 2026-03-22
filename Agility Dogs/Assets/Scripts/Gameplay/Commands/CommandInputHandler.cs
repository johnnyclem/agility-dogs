using UnityEngine;
using UnityEngine.InputSystem;
using AgilityDogs.Core;
using AgilityDogs.Gameplay.Handler;

namespace AgilityDogs.Gameplay.Commands
{
    public class CommandInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CommandBuffer commandBuffer;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private HandlerController handlerController;

        [Header("Contextual Command Settings")]
        [SerializeField] private float velocityContextThreshold = 0.5f;
        [SerializeField] private float facingContextAngle = 60f;

        private InputAction commandAction;

        private void Awake()
        {
            if (commandBuffer == null)
                commandBuffer = GetComponentInParent<CommandBuffer>();
                
            if (handlerController == null)
                handlerController = FindObjectOfType<HandlerController>();
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.actions["Jump"].performed += OnJumpPerformed;
                playerInput.actions["Interact"].performed += OnInteractPerformed;
                playerInput.actions["Crouch"].performed += OnCrouchPerformed;
                playerInput.actions["Previous"].performed += OnPreviousPerformed;
                playerInput.actions["Next"].performed += OnNextPerformed;
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.actions["Jump"].performed -= OnJumpPerformed;
                playerInput.actions["Interact"].performed -= OnInteractPerformed;
                playerInput.actions["Crouch"].performed -= OnCrouchPerformed;
                playerInput.actions["Previous"].performed -= OnPreviousPerformed;
                playerInput.actions["Next"].performed -= OnNextPerformed;
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            IssueContextualCommand(HandlerCommand.Jump);
        }

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            IssueContextualCommand(HandlerCommand.Go);
        }

        private void OnCrouchPerformed(InputAction.CallbackContext ctx)
        {
            IssueContextualCommand(HandlerCommand.Table);
        }

        private void OnPreviousPerformed(InputAction.CallbackContext ctx)
        {
            IssueContextualCommand(HandlerCommand.ComeBye);
        }

        private void OnNextPerformed(InputAction.CallbackContext ctx)
        {
            IssueContextualCommand(HandlerCommand.Away);
        }

        public void IssueCommand(HandlerCommand command)
        {
            if (handlerController != null)
            {
                handlerController.IssueContextualCommand(command);
            }
            else
            {
                commandBuffer?.IssueCommand(command);
            }
        }

        private void IssueContextualCommand(HandlerCommand command)
        {
            if (handlerController != null)
            {
                // Use handler's contextual command system
                handlerController.IssueContextualCommand(command);
            }
            else
            {
                // Fallback to direct command
                commandBuffer?.IssueCommand(command);
            }
            
            // Also raise the standard event for backward compatibility
            Events.GameEvents.RaiseCommandIssued(command);
        }

        /// <summary>
        /// Gets the contextual command based on handler state
        /// </summary>
        public HandlerCommand GetContextualCommand(HandlerCommand baseCommand)
        {
            if (handlerController == null) return baseCommand;
            
            HandlerCommand contextual = baseCommand;
            
            // Modify command based on handler velocity
            if (handlerController.CurrentSpeed > velocityContextThreshold)
            {
                // Handler is moving fast - commands are more urgent
                // Could modify timing windows or add emphasis
            }
            
            // Modify command based on facing direction
            Vector3 facing = handlerController.GetContextualFacing();
            
            // For directional commands, check if handler is facing the intended direction
            if (baseCommand == HandlerCommand.ComeBye || baseCommand == HandlerCommand.Away ||
                baseCommand == HandlerCommand.Left || baseCommand == HandlerCommand.Right)
            {
                // If handler is leaning/looking away from command direction, reverse it
                // This enables intuitive "point with your body" control
            }
            
            return contextual;
        }
    }
}
