using UnityEngine;
using UnityEngine.InputSystem;
using AgilityDogs.Core;

namespace AgilityDogs.Gameplay.Commands
{
    public class CommandInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CommandBuffer commandBuffer;
        [SerializeField] private PlayerInput playerInput;

        private InputAction commandAction;

        private void Awake()
        {
            if (commandBuffer == null)
                commandBuffer = GetComponentInParent<CommandBuffer>();
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
            commandBuffer?.IssueCommand(command);
        }

        private void IssueContextualCommand(HandlerCommand command)
        {
            commandBuffer?.IssueCommand(command);
        }
    }
}
