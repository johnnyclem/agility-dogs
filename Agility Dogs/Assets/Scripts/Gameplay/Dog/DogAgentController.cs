using UnityEngine;
using UnityEngine.AI;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Commands;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Services;

namespace AgilityDogs.Gameplay.Dog
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class DogAgentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform handlerTransform;
        [SerializeField] private CommandBuffer commandBuffer;
        [SerializeField] private BreedData breedData;

        [Header("Following")]
        [SerializeField] private float heelDistance = 1.5f;
        [SerializeField] private float heelAngle = 45f;
        [SerializeField] private float followDistance = 3f;
        [SerializeField] private float reacquisitionDistance = 8f;

        [Header("Obstacle Interaction")]
        [SerializeField] private float obstacleApproachRange = 5f;
        [SerializeField] private float obstacleCommitRange = 2f;
        [SerializeField] private float contactZoneSpeed = 1.5f;

        [Header("AI Tuning")]
        [SerializeField] private float decisionInterval = 0.1f;
        [SerializeField] private float commandResponseDelay = 0.15f;
        [SerializeField] private float recoveryTime = 1.5f;

        private NavMeshAgent navAgent;
        private Animator animator;
        private DogState currentState = DogState.Idle;
        private ObstacleBase currentObstacle;
        private ObstacleBase targetObstacle;
        private float lastDecisionTime;
        private float commandResponseTimer;
        private Vector3 lastHandlerPosition;
        private float stateTimer;
        private float currentMomentum;
        private Vector3 lastVelocity;
        private float baseAcceleration;
        private float baseAngularSpeed;
        private float baseDeceleration;
        private HandlerController handlerController;

        public DogState CurrentState => currentState;
        public BreedData Breed => breedData;
        public float Speed => navAgent != null ? navAgent.velocity.magnitude : 0f;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (breedData != null)
            {
                ApplyBreedTuning();
            }

            if (handlerTransform == null)
            {
                var handler = FindObjectOfType<HandlerController>();
                if (handler != null)
                {
                    handlerTransform = handler.transform;
                    handlerController = handler;
                }
            }
            else if (handlerController == null)
            {
                handlerController = handlerTransform.GetComponent<HandlerController>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Gameplay &&
                GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            UpdateMomentum();
            UpdateHandlerInfluence();
            UpdateStateMachine();
            UpdateAnimator();
        }

        private void ApplyBreedTuning()
        {
            if (navAgent == null || breedData == null) return;

            navAgent.speed = breedData.maxSpeed;
            baseAcceleration = breedData.acceleration;
            baseDeceleration = breedData.deceleration;
            baseAngularSpeed = breedData.turnRate / Mathf.Max(0.1f, breedData.turningRadius);
            
            // Apply momentum factor to reduce acceleration and angular speed
            float momentumFactor = Mathf.Clamp01(breedData.momentumFactor);
            navAgent.acceleration = baseAcceleration * (1f - momentumFactor * 0.5f);
            navAgent.angularSpeed = baseAngularSpeed * (1f - momentumFactor * 0.3f);

            if (breedData.animatorController != null && animator != null)
            {
                animator.runtimeAnimatorController = breedData.animatorController;
            }
        }

        private void UpdateMomentum()
        {
            if (navAgent == null || breedData == null) return;
            
            Vector3 velocity = navAgent.velocity;
            float currentSpeed = velocity.magnitude;
            float maxSpeed = breedData.maxSpeed;
            
            // Compute momentum as a factor of current speed relative to max speed
            // Higher momentumFactor means more momentum (harder to change direction)
            float targetMomentum = currentSpeed / Mathf.Max(0.1f, maxSpeed);
            float momentumInfluence = breedData.momentumFactor;
            currentMomentum = Mathf.Lerp(currentMomentum, targetMomentum, Time.deltaTime * (1f - momentumInfluence));
            
            // Adjust acceleration based on momentum (more momentum = slower acceleration)
            float effectiveAcceleration = baseAcceleration * (1f - currentMomentum * momentumInfluence);
            navAgent.acceleration = effectiveAcceleration;
            
            // Adjust angular speed based on momentum (more momentum = slower turning)
            float effectiveAngularSpeed = baseAngularSpeed * (1f - currentMomentum * momentumInfluence * 0.5f);
            navAgent.angularSpeed = effectiveAngularSpeed;
            
            // Store last velocity for next frame
            lastVelocity = velocity;
        }

        private void UpdateHandlerInfluence()
        {
            if (handlerController == null || navAgent == null || breedData == null || handlerTransform == null) return;
            
            float handlerSpeed = handlerController.CurrentSpeed;
            float maxSpeed = breedData.maxSpeed;
            
            // If handler is moving faster than dog's current speed, increase dog's speed up to max
            float currentDogSpeed = navAgent.velocity.magnitude;
            if (handlerSpeed > currentDogSpeed)
            {
                float targetSpeed = Mathf.Lerp(currentDogSpeed, Mathf.Min(handlerSpeed, maxSpeed), Time.deltaTime * 2f);
                navAgent.speed = Mathf.Lerp(navAgent.speed, targetSpeed, Time.deltaTime * 3f);
            }
            else
            {
                // Gradually return to maxSpeed (or based on momentum)
                navAgent.speed = Mathf.Lerp(navAgent.speed, maxSpeed, Time.deltaTime);
            }
            
            // Apply handling tolerance: if dog is too far from handler, reduce speed slightly
            float distanceToHandler = Vector3.Distance(transform.position, handlerTransform.position);
            float tolerance = breedData.handlingTolerance * 5f; // arbitrary scaling
            if (distanceToHandler > tolerance)
            {
                navAgent.speed *= 0.8f;
            }
        }

        private void UpdateStateMachine()
        {
            switch (currentState)
            {
                case DogState.Idle:
                    UpdateIdle();
                    break;
                case DogState.Heeling:
                    UpdateHeeling();
                    break;
                case DogState.Running:
                    UpdateRunning();
                    break;
                case DogState.SeekingObstacle:
                    UpdateSeekingObstacle();
                    break;
                case DogState.CommittingToObstacle:
                    UpdateCommittingToObstacle();
                    break;
                case DogState.OnObstacle:
                    UpdateOnObstacle();
                    break;
                case DogState.CompletingObstacle:
                    UpdateCompletingObstacle();
                    break;
                case DogState.Weaving:
                    UpdateWeaving();
                    break;
                case DogState.WaitingAtTable:
                    UpdateWaitingAtTable();
                    break;
                case DogState.Recovering:
                    UpdateRecovering();
                    break;
            }
        }

        private void TransitionState(DogState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            stateTimer = 0f;
        }

        private void UpdateIdle()
        {
            if (commandBuffer != null && commandBuffer.HasCommandInWindow())
            {
                ProcessCommand();
            }
            else if (handlerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, handlerTransform.position);
                if (dist > heelDistance * 1.5f)
                {
                    TransitionState(DogState.Heeling);
                }
            }
        }

        private void UpdateHeeling()
        {
            if (handlerTransform == null) return;

            if (commandBuffer != null && commandBuffer.HasCommandInWindow())
            {
                ProcessCommand();
                return;
            }

            float dist = Vector3.Distance(transform.position, handlerTransform.position);

            if (dist > reacquisitionDistance)
            {
                NavigateTo(handlerTransform.position);
            }
            else if (dist > heelDistance)
            {
                Vector3 heelPos = CalculateHeelPosition();
                NavigateTo(heelPos);
            }
            else
            {
                if (navAgent.hasPath) navAgent.ResetPath();
            }

            if (commandBuffer != null && commandBuffer.HasCommandInWindow())
            {
                ProcessCommand();
            }
        }

        private void UpdateRunning()
        {
            if (commandBuffer != null && commandBuffer.HasCommandInWindow())
            {
                ProcessCommand();
                return;
            }

            if (targetObstacle != null)
            {
                float dist = Vector3.Distance(transform.position, targetObstacle.GetCommitPoint());

                if (dist <= obstacleCommitRange)
                {
                    TransitionState(DogState.CommittingToObstacle);
                }
                else
                {
                    NavigateTo(targetObstacle.GetCommitPoint());
                }
            }
            else
            {
                TransitionState(DogState.Heeling);
            }
        }

        private void UpdateSeekingObstacle()
        {
            if (targetObstacle != null)
            {
                NavigateTo(targetObstacle.GetCommitPoint());

                float dist = Vector3.Distance(transform.position, targetObstacle.GetCommitPoint());
                if (dist <= obstacleCommitRange)
                {
                    TransitionState(DogState.CommittingToObstacle);
                }
            }
            else
            {
                TransitionState(DogState.Heeling);
            }
        }

        private void UpdateCommittingToObstacle()
        {
            if (targetObstacle == null)
            {
                TransitionState(DogState.Recovering);
                return;
            }

            NavigateTo(targetObstacle.GetEntryPoint());

            float dist = Vector3.Distance(transform.position, targetObstacle.GetEntryPoint());
            if (dist < 0.5f)
            {
                currentObstacle = targetObstacle;
                currentObstacle.OnDogEntered(this);
                TransitionState(DogState.OnObstacle);
            }
        }

        private void UpdateOnObstacle()
        {
            if (currentObstacle == null)
            {
                TransitionState(DogState.Recovering);
                return;
            }

            navAgent.speed = currentObstacle.GetSpeedMultiplier() * (breedData != null ? breedData.contactSpeed : 1f);
            NavigateTo(currentObstacle.GetExitPoint());

            float dist = Vector3.Distance(transform.position, currentObstacle.GetExitPoint());
            if (dist < 0.3f)
            {
                currentObstacle.OnDogExited(this);
                TransitionState(DogState.CompletingObstacle);
            }
        }

        private void UpdateCompletingObstacle()
        {
            if (currentObstacle != null)
            {
                GameEvents.RaiseObstacleCompleted(currentObstacle.ObstacleType, true);
                GameEvents.RaiseObstacleCompletedWithReference(currentObstacle, true);
            }

            currentObstacle = null;
            navAgent.speed = breedData != null ? breedData.maxSpeed : 6f;
            TransitionState(DogState.Running);
        }

        private void UpdateWeaving()
        {
            stateTimer += Time.deltaTime;

            if (navAgent.remainingDistance < 0.5f && targetObstacle != null)
            {
                var weave = targetObstacle as WeavePolesObstacle;
                if (weave != null && weave.IsWeaveComplete(this))
                {
                    currentObstacle = targetObstacle;
                    currentObstacle.OnDogExited(this);
                    TransitionState(DogState.CompletingObstacle);
                }
            }
        }

        private void UpdateWaitingAtTable()
        {
            stateTimer += Time.deltaTime;
            if (navAgent.hasPath) navAgent.ResetPath();

            if (stateTimer >= 5f)
            {
                if (currentObstacle != null)
                {
                    currentObstacle.OnDogExited(this);
                    TransitionState(DogState.CompletingObstacle);
                }
            }
        }

        private void UpdateRecovering()
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= recoveryTime)
            {
                TransitionState(DogState.Heeling);
            }
        }

        private void ProcessCommand()
        {
            var cmd = commandBuffer.GetRecentCommand();
            if (cmd == null) return;

            var entry = cmd.Value;

            switch (entry.command)
            {
                case HandlerCommand.Go:
                    TransitionState(DogState.Running);
                    break;

                case HandlerCommand.Jump:
                    if (FindNearestObstacle(ObstacleType.BarJump, out targetObstacle))
                    {
                        TransitionState(DogState.SeekingObstacle);
                    }
                    break;

                case HandlerCommand.Tunnel:
                    if (FindNearestObstacle(ObstacleType.Tunnel, out targetObstacle))
                    {
                        TransitionState(DogState.SeekingObstacle);
                    }
                    break;

                case HandlerCommand.Weave:
                    if (FindNearestObstacle(ObstacleType.WeavePoles, out targetObstacle))
                    {
                        TransitionState(DogState.Weaving);
                    }
                    break;

                case HandlerCommand.Table:
                    if (FindNearestObstacle(ObstacleType.PauseTable, out targetObstacle))
                    {
                        TransitionState(DogState.SeekingObstacle);
                    }
                    break;

                case HandlerCommand.ComeBye:
                case HandlerCommand.Away:
                case HandlerCommand.Left:
                case HandlerCommand.Right:
                    HandleDirectionCommand(entry.command, entry.handlerForward);
                    break;

                case HandlerCommand.Here:
                    TransitionState(DogState.Heeling);
                    break;

                case HandlerCommand.Out:
                    TransitionState(DogState.Running);
                    break;
            }
        }

        private void HandleDirectionCommand(HandlerCommand dirCmd, Vector3 handlerForward)
        {
            if (handlerTransform == null) return;

            float angle = 0f;
            switch (dirCmd)
            {
                case HandlerCommand.ComeBye:
                case HandlerCommand.Left:
                    angle = -90f;
                    break;
                case HandlerCommand.Away:
                case HandlerCommand.Right:
                    angle = 90f;
                    break;
            }

            Vector3 rotateDir = Quaternion.Euler(0f, angle, 0f) * handlerForward;
            Vector3 targetPos = transform.position + rotateDir * 5f;
            NavigateTo(targetPos);

            ObstacleBase nearest = FindObstacleInDirection(rotateDir);
            if (nearest != null)
            {
                targetObstacle = nearest;
                TransitionState(DogState.SeekingObstacle);
            }
        }

        private bool FindNearestObstacle(ObstacleType type, out ObstacleBase obstacle)
        {
            obstacle = null;
            float nearestDist = float.MaxValue;

            foreach (var obs in FindObjectsOfType<ObstacleBase>())
            {
                if (obs.ObstacleType != type) continue;
                float dist = Vector3.Distance(transform.position, obs.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    obstacle = obs;
                }
            }

            return obstacle != null;
        }

        private ObstacleBase FindObstacleInDirection(Vector3 direction)
        {
            ObstacleBase nearest = null;
            float nearestScore = float.MaxValue;

            foreach (var obs in FindObjectsOfType<ObstacleBase>())
            {
                Vector3 toObs = (obs.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(direction.normalized, toObs);
                if (dot > 0.5f)
                {
                    float dist = Vector3.Distance(transform.position, obs.transform.position);
                    float score = dist / dot;
                    if (score < nearestScore)
                    {
                        nearestScore = score;
                        nearest = obs;
                    }
                }
            }

            return nearest;
        }

        private void NavigateTo(Vector3 target)
        {
            if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(target);
            }
        }

        private Vector3 CalculateHeelPosition()
        {
            if (handlerTransform == null) return transform.position;

            Vector3 behindHandler = handlerTransform.position - handlerTransform.forward * heelDistance;
            return behindHandler;
        }

        public void SetTargetObstacle(ObstacleBase obstacle)
        {
            targetObstacle = obstacle;
            if (obstacle != null)
            {
                TransitionState(DogState.SeekingObstacle);
            }
        }

        public void SetBreedData(BreedData data)
        {
            breedData = data;
            ApplyBreedTuning();
        }

        public void SetHandler(Transform handler)
        {
            handlerTransform = handler;
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;
            animator.SetFloat("Speed", Speed);
            animator.SetBool("IsRunning", currentState == DogState.Running || currentState == DogState.SeekingObstacle);
            animator.SetBool("IsOnObstacle", currentState == DogState.OnObstacle);
        }

        private void OnDrawGizmosSelected()
        {
            if (handlerTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(handlerTransform.position, heelDistance);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(handlerTransform.position, followDistance);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(handlerTransform.position, reacquisitionDistance);
            }

            if (targetObstacle != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, targetObstacle.transform.position);
            }
        }
    }
}
