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

        [Header("Commitment Logic")]
        [SerializeField] private float earlyCommitDistance = 3f;
        [SerializeField] private float lateCommitDistance = 1.5f;
        [SerializeField] private float commitmentBuildupRate = 2f;
        [SerializeField] private float commitmentDecayRate = 1f;
        [SerializeField] private float timingWindowBonus = 0.3f;

        [Header("Obstacle Reading")]
        [SerializeField] private float obstacleReadDistance = 15f;
        [SerializeField] private float obstacleReadAngle = 60f;
        [SerializeField] private float nextObstacleLookahead = 2f;

        [Header("Command Timing")]
        [SerializeField] private float earlyCommandWindow = 0.5f;
        [SerializeField] private float optimalCommandWindow = 0.2f;
        [SerializeField] private float lateCommandWindow = 0.3f;

        [Header("Recovery Logic")]
        [SerializeField] private float missedCommandRecoveryTime = 2f;
        [SerializeField] private float wrongObstacleRecoveryTime = 3f;
        [SerializeField] private float stuckDetectionTime = 1.5f;
        [SerializeField] private float stuckDistanceThreshold = 0.5f;

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

        // Commitment state
        private float currentCommitment;
        private bool isCommitting;
        private float commitmentPointDistance;

        // Obstacle reading state
        private ObstacleBase[] nearbyObstacles;
        private ObstacleBase nextExpectedObstacle;
        private float lastObstacleReadTime;

        // Command timing state
        private float lastCommandTime;
        private CommandTimingRating lastCommandTiming;
        private int consecutiveBadTimingCount;

        // Recovery state
        private bool isInRecovery;
        private float recoveryStartTime;
        private RecoveryReason recoveryReason;
        private Vector3 lastStuckPosition;
        private float stuckCheckTimer;

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

            // Subscribe to handler events
            Events.GameEvents.OnHandlerPathInfluence += OnHandlerPathInfluence;
            Events.GameEvents.OnContextualCommand += OnContextualCommandReceived;
        }

        private void OnDestroy()
        {
            Events.GameEvents.OnHandlerPathInfluence -= OnHandlerPathInfluence;
            Events.GameEvents.OnContextualCommand -= OnContextualCommandReceived;
        }

        private void OnHandlerPathInfluence(float influence)
        {
            // React to handler path influence for better line adjustments
            if (Mathf.Abs(influence) > 0.3f && targetObstacle != null)
            {
                // Adjust commitment based on handler influence
                float adjustment = influence * breedData.handlingTolerance * 0.5f;
                commitmentPointDistance += adjustment;
                commitmentPointDistance = Mathf.Clamp(commitmentPointDistance, lateCommitDistance, earlyCommitDistance);
            }
        }

        private void OnContextualCommandReceived(HandlerCommand command, Vector3 handlerForward, float handlerSpeed)
        {
            // Evaluate command timing
            CommandTimingRating timing = EvaluateCommandTiming(command, handlerForward, handlerSpeed);
            
            // Adjust dog response based on timing
            if (timing == CommandTimingRating.Optimal)
            {
                // Faster response, tighter turns
                navAgent.acceleration = baseAcceleration * 1.2f;
                navAgent.angularSpeed = baseAngularSpeed * 1.1f;
            }
            else if (timing == CommandTimingRating.Poor)
            {
                // Slower response, wider turns
                navAgent.acceleration = baseAcceleration * 0.8f;
                navAgent.angularSpeed = baseAngularSpeed * 0.9f;
                consecutiveBadTimingCount++;
            }
            else
            {
                consecutiveBadTimingCount = 0;
            }
            
            lastCommandTiming = timing;
            lastCommandTime = Time.time;
            
            // Trigger recovery if too many bad timing commands
            if (consecutiveBadTimingCount >= 3)
            {
                StartRecovery(RecoveryReason.MissedCommand);
                consecutiveBadTimingCount = 0;
            }
        }

        private CommandTimingRating EvaluateCommandTiming(HandlerCommand command, Vector3 handlerForward, float handlerSpeed)
        {
            // Evaluate based on multiple factors:
            // 1. Distance to next obstacle
            // 2. Dog's current state
            // 3. Handler speed and facing
            
            float timeToNextObstacle = float.MaxValue;
            if (targetObstacle != null)
            {
                float distance = Vector3.Distance(transform.position, targetObstacle.GetCommitPoint());
                timeToNextObstacle = distance / Mathf.Max(Speed, 0.1f);
            }
            
            // Check if command timing is optimal
            if (timeToNextObstacle < optimalCommandWindow)
            {
                return CommandTimingRating.Optimal;
            }
            else if (timeToNextObstacle < earlyCommandWindow)
            {
                return CommandTimingRating.Good;
            }
            else if (timeToNextObstacle > lateCommandWindow + optimalCommandWindow)
            {
                return CommandTimingRating.Poor; // Too early
            }
            
            return CommandTimingRating.Good;
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
            UpdateCommitment();
            UpdateObstacleReading();
            UpdateStuckDetection();
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
            
            // Apply handler path influence when targeting an obstacle
            if (targetObstacle != null && handlerController != null)
            {
                float pathInfluence = handlerController.GetPathInfluence(transform.position, targetObstacle.GetCommitPoint());
                
                // Adjust destination based on handler's body position
                Vector3 baseTarget = targetObstacle.GetCommitPoint();
                Vector3 adjustedTarget = Vector3.Lerp(
                    baseTarget, 
                    baseTarget + handlerController.LeanDirection * 2f, 
                    Mathf.Abs(pathInfluence) * breedData.handlingTolerance
                );
                
                // Apply the adjusted target if it's reasonable
                if (Vector3.Distance(transform.position, adjustedTarget) < Vector3.Distance(transform.position, baseTarget) * 1.5f)
                {
                    NavigateTo(adjustedTarget);
                }
                
                // Broadcast path influence for UI feedback
                Events.GameEvents.RaiseHandlerPathInfluence(pathInfluence);
            }
        }

        private void UpdateCommitment()
        {
            if (targetObstacle == null)
            {
                currentCommitment = 0f;
                isCommitting = false;
                return;
            }

            float distanceToObstacle = Vector3.Distance(transform.position, targetObstacle.GetCommitPoint());
            
            // Calculate optimal commitment point based on speed and momentum
            float optimalCommitDistance = Mathf.Lerp(lateCommitDistance, earlyCommitDistance, 
                Mathf.Clamp01(Speed / (breedData != null ? breedData.maxSpeed : 6f)));
            
            // Adjust for momentum - more momentum needs earlier commitment
            if (breedData != null)
            {
                optimalCommitDistance *= (1f + currentMomentum * breedData.momentumFactor);
            }
            
            commitmentPointDistance = optimalCommitDistance;
            
            // Build up commitment as we approach the optimal point
            if (distanceToObstacle <= optimalCommitDistance * 1.5f)
            {
                float commitmentRate = commitmentBuildupRate;
                
                // Timing window bonus for good command timing
                if (lastCommandTiming == CommandTimingRating.Optimal)
                {
                    commitmentRate *= (1f + timingWindowBonus);
                }
                
                currentCommitment += Time.deltaTime * commitmentRate;
                
                // Check if we've reached commitment threshold
                if (currentCommitment >= 1f && !isCommitting)
                {
                    isCommitting = true;
                    TransitionState(DogState.CommittingToObstacle);
                }
            }
            else
            {
                // Decay commitment if too far
                currentCommitment -= Time.deltaTime * commitmentDecayRate;
                currentCommitment = Mathf.Max(0f, currentCommitment);
            }
            
            // Reset commitment after passing obstacle
            if (distanceToObstacle < 1f && isCommitting)
            {
                currentCommitment = 0f;
                isCommitting = false;
            }
        }

        private void UpdateObstacleReading()
        {
            // Periodically read nearby obstacles
            if (Time.time - lastObstacleReadTime > 0.5f)
            {
                lastObstacleReadTime = Time.time;
                ReadNearbyObstacles();
            }
            
            // Update next expected obstacle based on course flow
            if (targetObstacle != null && nextExpectedObstacle == null)
            {
                FindNextExpectedObstacle();
            }
        }

        private void ReadNearbyObstacles()
        {
            var obstacles = FindObjectsOfType<ObstacleBase>();
            nearbyObstacles = new ObstacleBase[obstacles.Length];
            
            int index = 0;
            foreach (var obs in obstacles)
            {
                float distance = Vector3.Distance(transform.position, obs.transform.position);
                
                // Filter by distance
                if (distance <= obstacleReadDistance)
                {
                    // Filter by angle (only obstacles we're generally facing)
                    Vector3 toObstacle = (obs.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, toObstacle);
                    
                    if (angle <= obstacleReadAngle)
                    {
                        nearbyObstacles[index] = obs;
                        index++;
                    }
                }
            }
            
            // Resize array to actual count
            System.Array.Resize(ref nearbyObstacles, index);
        }

        private void FindNextExpectedObstacle()
        {
            if (targetObstacle == null || nearbyObstacles == null) return;
            
            // Find the obstacle that logically comes after current target
            // This is simplified - in production would use course sequence data
            float bestScore = float.MaxValue;
            ObstacleBase bestCandidate = null;
            
            foreach (var obs in nearbyObstacles)
            {
                if (obs == targetObstacle || obs == currentObstacle) continue;
                
                // Score based on:
                // 1. Distance from current target
                // 2. Direction of travel
                // 3. Logical course progression
                
                float distFromTarget = Vector3.Distance(targetObstacle.transform.position, obs.transform.position);
                Vector3 travelDir = (targetObstacle.GetExitPoint() - targetObstacle.GetEntryPoint()).normalized;
                Vector3 toCandidate = (obs.transform.position - targetObstacle.transform.position).normalized;
                float directionScore = Vector3.Dot(travelDir, toCandidate);
                
                // Combined score - prefer obstacles in direction of travel
                float score = distFromTarget / Mathf.Max(0.1f, directionScore + 1f);
                
                if (score < bestScore && directionScore > 0.2f)
                {
                    bestScore = score;
                    bestCandidate = obs;
                }
            }
            
            nextExpectedObstacle = bestCandidate;
        }

        private void UpdateStuckDetection()
        {
            if (currentState == DogState.Idle || currentState == DogState.WaitingAtTable)
            {
                stuckCheckTimer = 0f;
                return;
            }
            
            stuckCheckTimer += Time.deltaTime;
            
            // Check position periodically
            if (stuckCheckTimer >= stuckDetectionTime)
            {
                stuckCheckTimer = 0f;
                
                float distanceMoved = Vector3.Distance(transform.position, lastStuckPosition);
                
                if (distanceMoved < stuckDistanceThreshold && navAgent.hasPath && Speed > 0.1f)
                {
                    // We're trying to move but not making progress - stuck!
                    StartRecovery(RecoveryReason.Stuck);
                }
                
                lastStuckPosition = transform.position;
            }
        }

        private void StartRecovery(RecoveryReason reason)
        {
            if (isInRecovery) return;
            
            isInRecovery = true;
            recoveryReason = reason;
            recoveryStartTime = Time.time;
            
            float recoveryDuration = missedCommandRecoveryTime;
            
            switch (reason)
            {
                case RecoveryReason.Stuck:
                    recoveryDuration = 1f;
                    // Try to navigate to a nearby clear spot
                    Vector3 recoveryPos = FindRecoveryPosition();
                    NavigateTo(recoveryPos);
                    break;
                    
                case RecoveryReason.WrongObstacle:
                    recoveryDuration = wrongObstacleRecoveryTime;
                    targetObstacle = null;
                    break;
                    
                case RecoveryReason.MissedCommand:
                    recoveryDuration = missedCommandRecoveryTime;
                    break;
                    
                case RecoveryReason.HandlerTooFar:
                    recoveryDuration = 2f;
                    break;
            }
            
            recoveryTime = recoveryDuration;
            TransitionState(DogState.Recovering);
            
            // Fire recovery event
            Events.GameEvents.RaiseDogRecovery(reason);
        }

        private Vector3 FindRecoveryPosition()
        {
            // Find a nearby position that's clear of obstacles
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, 
                                     (Vector3.forward + Vector3.left).normalized, 
                                     (Vector3.forward + Vector3.right).normalized };
            
            foreach (var dir in directions)
            {
                Vector3 checkPos = transform.position + dir * 3f;
                
                // Simple check - could be more sophisticated
                Collider[] hitColliders = Physics.OverlapSphere(checkPos, 1f);
                bool isClear = true;
                
                foreach (var col in hitColliders)
                {
                    if (col.GetComponent<ObstacleBase>() != null)
                    {
                        isClear = false;
                        break;
                    }
                }
                
                if (isClear)
                {
                    return checkPos;
                }
            }
            
            // Fallback - return to handler
            return handlerTransform != null ? handlerTransform.position : transform.position;
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
            
            // Different recovery behaviors based on reason
            switch (recoveryReason)
            {
                case RecoveryReason.Stuck:
                    // Wait for navigation to complete
                    if (stateTimer >= recoveryTime || !navAgent.hasPath || navAgent.remainingDistance < 1f)
                    {
                        FinishRecovery();
                    }
                    break;
                    
                case RecoveryReason.HandlerTooFar:
                    // Try to get back to handler
                    if (handlerTransform != null)
                    {
                        NavigateTo(handlerTransform.position);
                        float dist = Vector3.Distance(transform.position, handlerTransform.position);
                        if (dist < heelDistance * 2f || stateTimer >= recoveryTime)
                        {
                            FinishRecovery();
                        }
                    }
                    else
                    {
                        FinishRecovery();
                    }
                    break;
                    
                case RecoveryReason.WrongObstacle:
                    // Stop, reorient, and wait for new command
                    if (navAgent.hasPath) navAgent.ResetPath();
                    if (stateTimer >= recoveryTime)
                    {
                        FinishRecovery();
                    }
                    break;
                    
                default:
                    // Generic recovery - just wait
                    if (stateTimer >= recoveryTime)
                    {
                        FinishRecovery();
                    }
                    break;
            }
        }

        private void FinishRecovery()
        {
            isInRecovery = false;
            recoveryReason = RecoveryReason.None;
            currentCommitment = 0f;
            isCommitting = false;
            consecutiveBadTimingCount = 0;
            TransitionState(DogState.Heeling);
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

            // First check nearby obstacles we've already read
            if (nearbyObstacles != null)
            {
                foreach (var obs in nearbyObstacles)
                {
                    if (obs == null || obs.ObstacleType != type) continue;
                    
                    float dist = Vector3.Distance(transform.position, obs.transform.position);
                    
                    // Prefer obstacles in the direction of travel or towards handler
                    Vector3 toObs = (obs.transform.position - transform.position).normalized;
                    float dirScore = 1f;
                    
                    if (Speed > 0.5f)
                    {
                        // Prefer obstacles in direction of movement
                        Vector3 flatVel = new Vector3(navAgent.velocity.x, 0f, navAgent.velocity.z);
                        float moveDot = Vector3.Dot(flatVel.normalized, toObs);
                        dirScore = Mathf.Lerp(0.5f, 1.5f, (moveDot + 1f) / 2f);
                    }
                    
                    // Consider handler's position for better line choice
                    if (handlerTransform != null)
                    {
                        float handlerDot = Vector3.Dot((handlerTransform.position - transform.position).normalized, toObs);
                        dirScore *= Mathf.Lerp(0.7f, 1.3f, (handlerDot + 1f) / 2f);
                    }
                    
                    float weightedDist = dist / dirScore;
                    
                    if (weightedDist < nearestDist)
                    {
                        nearestDist = weightedDist;
                        obstacle = obs;
                    }
                }
            }
            
            // Fallback to full search if nothing found nearby
            if (obstacle == null)
            {
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
