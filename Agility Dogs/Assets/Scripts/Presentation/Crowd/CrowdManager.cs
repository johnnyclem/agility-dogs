using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Presentation.Crowd
{
    public class CrowdManager : MonoBehaviour
    {
        public static CrowdManager Instance { get; private set; }

        [Header("Population Settings")]
        [SerializeField] private int targetCrowdSize = 250;
        [SerializeField] private int minCrowdSize = 100;
        [SerializeField] private int maxCrowdSize = 400;
        [SerializeField] private GameObject[] crowdPrefabVariants;
        [SerializeField] private Transform[] spawnAreas;
        [SerializeField] private float spawnRadius = 50f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Crowd Behavior")]
        [SerializeField] private float idleAnimationSpeed = 0.5f;
        [SerializeField] private float cheerAnimationSpeed = 1f;
        [SerializeField] private float animationBlendSpeed = 2f;
        [SerializeField] private float reactionDelay = 0.3f;
        [SerializeField] private float reactionDuration = 2f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem[] confettiSystems;
        [SerializeField] private Light[] spotlights;
        [SerializeField] private float spotlightIntensityMultiplier = 2f;

        [Header("Broadcast Cutaways")]
        [SerializeField] private Transform[] cutawayCameraPositions;
        [SerializeField] private float cutawayDuration = 2f;
        [SerializeField] private float minCutawayInterval = 10f;
        [SerializeField] private float maxCutawayInterval = 30f;
        [SerializeField] private int maxCutawaysPerRun = 3;

        [Header("Reactivity")]
        [SerializeField] private float faultReactionIntensity = 0.8f;
        [SerializeField] private float successReactionIntensity = 0.6f;
        [SerializeField] private float splitTimeReactionIntensity = 0.7f;
        [SerializeField] private float championshipReactionMultiplier = 1.5f;

        // Crowd state
        private List<CrowdMember> crowdMembers = new List<CrowdMember>();
        private CrowdState currentState = CrowdState.Idle;
        private float currentIntensity;
        private float targetIntensity;
        private float reactionTimer;
        private bool isChampionshipMode;

        // Cutaway state
        private float cutawayTimer;
        private int cutawaysThisRun;
        private int lastCutawayIndex = -1;
        private bool isCutawayActive;

        // Performance tracking
        private float crowdSatisfaction;
        private float excitementLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SpawnCrowd();
            SubscribeToEvents();

            cutawayTimer = Random.Range(minCutawayInterval, maxCutawayInterval);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTime;
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            UpdateCrowdBehavior();
            UpdateCutawayTimer();
            UpdateVisualEffects();
        }

        #region Crowd Spawning

        private void SpawnCrowd()
        {
            ClearCrowd();

            int actualSize = Mathf.Clamp(targetCrowdSize, minCrowdSize, maxCrowdSize);
            actualSize = Mathf.Min(actualSize, crowdPrefabVariants.Length * 50); // Limit based on variants

            for (int i = 0; i < actualSize; i++)
            {
                SpawnCrowdMember(i);
            }

            Debug.Log($"Spawned {crowdMembers.Count} crowd members");
        }

        private void SpawnCrowdMember(int index)
        {
            if (crowdPrefabVariants == null || crowdPrefabVariants.Length == 0) return;

            // Select random variant
            GameObject prefab = crowdPrefabVariants[Random.Range(0, crowdPrefabVariants.Length)];

            // Calculate spawn position
            Vector3 spawnPosition = CalculateSpawnPosition(index);
            Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Instantiate
            GameObject crowdObj = Instantiate(prefab, spawnPosition, spawnRotation, transform);

            // Get or add CrowdMember component
            CrowdMember member = crowdObj.GetComponent<CrowdMember>();
            if (member == null)
            {
                member = crowdObj.AddComponent<CrowdMember>();
            }

            // Initialize
            member.Initialize(Random.Range(0.8f, 1.2f));
            crowdMembers.Add(member);
        }

        private Vector3 CalculateSpawnPosition(int index)
        {
            // Use spawn areas if available, otherwise circular distribution
            if (spawnAreas != null && spawnAreas.Length > 0)
            {
                Transform area = spawnAreas[index % spawnAreas.Length];
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                return area.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
            }

            // Circular distribution around center
            float angle = (float)index / targetCrowdSize * 360f;
            float radius = spawnRadius * (0.5f + Random.Range(0f, 0.5f));
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            Vector3 position = transform.position + new Vector3(x, 0f, z);

            // Raycast to find ground
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                position.y = hit.point.y;
            }

            return position;
        }

        private void ClearCrowd()
        {
            foreach (var member in crowdMembers)
            {
                if (member != null)
                {
                    Destroy(member.gameObject);
                }
            }
            crowdMembers.Clear();
        }

        public void SetCrowdSize(int size)
        {
            targetCrowdSize = Mathf.Clamp(size, minCrowdSize, maxCrowdSize);
            SpawnCrowd();
        }

        #endregion

        #region Crowd Behavior

        private void UpdateCrowdBehavior()
        {
            // Update intensity
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * animationBlendSpeed);

            // Decay intensity over time
            targetIntensity = Mathf.Lerp(targetIntensity, 0.2f, Time.deltaTime * 0.3f);

            // Update reaction timer
            if (reactionTimer > 0)
            {
                reactionTimer -= Time.deltaTime;
                if (reactionTimer <= 0)
                {
                    ReturnToIdle();
                }
            }

            // Update each crowd member
            foreach (var member in crowdMembers)
            {
                if (member != null)
                {
                    member.UpdateAnimation(currentIntensity, animationBlendSpeed);
                }
            }
        }

        private void SetCrowdState(CrowdState state, float intensity)
        {
            currentState = state;
            targetIntensity = Mathf.Clamp01(intensity);

            if (isChampionshipMode)
            {
                targetIntensity *= championshipReactionMultiplier;
            }

            reactionTimer = reactionDuration;
        }

        private void ReturnToIdle()
        {
            currentState = CrowdState.Idle;
            targetIntensity = 0.2f;
        }

        #endregion

        #region Broadcast Cutaways

        private void UpdateCutawayTimer()
        {
            if (isCutawayActive || cutawayCameraPositions == null || cutawayCameraPositions.Length == 0)
                return;

            cutawayTimer -= Time.deltaTime;

            if (cutawayTimer <= 0 && cutawaysThisRun < maxCutawaysPerRun)
            {
                TriggerCutaway();
                cutawayTimer = Random.Range(minCutawayInterval, maxCutawayInterval);
            }
        }

        private void TriggerCutaway()
        {
            if (cutawayCameraPositions.Length == 0) return;

            // Select a random camera position (avoid repeating)
            int index;
            do
            {
                index = Random.Range(0, cutawayCameraPositions.Length);
            } while (index == lastCutawayIndex && cutawayCameraPositions.Length > 1);

            lastCutawayIndex = index;
            isCutawayActive = true;
            cutawaysThisRun++;

            // Trigger camera cutaway
            // This would integrate with the camera system
            Debug.Log($"Triggering crowd cutaway at position {index}");

            // End cutaway after duration
            Invoke(nameof(EndCutaway), cutawayDuration);
        }

        private void EndCutaway()
        {
            isCutawayActive = false;
        }

        public Transform[] GetCutawayPositions()
        {
            return cutawayCameraPositions;
        }

        #endregion

        #region Visual Effects

        private void UpdateVisualEffects()
        {
            // Update spotlight intensity based on crowd excitement
            if (spotlights != null)
            {
                foreach (var spotlight in spotlights)
                {
                    if (spotlight != null)
                    {
                        spotlight.intensity = 1f + currentIntensity * spotlightIntensityMultiplier;
                    }
                }
            }
        }

        public void TriggerConfetti()
        {
            if (confettiSystems == null) return;

            foreach (var confetti in confettiSystems)
            {
                if (confetti != null)
                {
                    confetti.Play();
                }
            }
        }

        public void StopConfetti()
        {
            if (confettiSystems == null) return;

            foreach (var confetti in confettiSystems)
            {
                if (confetti != null)
                {
                    confetti.Stop();
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (clean)
            {
                SetCrowdState(CrowdState.Applause, successReactionIntensity);
                crowdSatisfaction += 0.1f;
            }
            else
            {
                // Mild reaction to near-miss
                SetCrowdState(CrowdState.Watching, 0.3f);
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            SetCrowdState(CrowdState.Gasp, faultReactionIntensity);
            excitementLevel += 0.2f;
            crowdSatisfaction -= 0.1f;
        }

        private void HandleSplitTime(float time)
        {
            SetCrowdState(CrowdState.Cheering, splitTimeReactionIntensity);
            excitementLevel += 0.15f;
            crowdSatisfaction += 0.15f;
        }

        private void HandleRunStarted()
        {
            cutawaysThisRun = 0;
            excitementLevel = 0.3f;
            crowdSatisfaction = 0.5f;
            SetCrowdState(CrowdState.Anticipation, 0.4f);
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            switch (result)
            {
                case RunResult.Qualified:
                    SetCrowdState(CrowdState.Ovation, 1f);
                    TriggerConfetti();
                    break;

                case RunResult.NonQualified:
                    SetCrowdState(CrowdState.Applause, 0.6f);
                    break;

                case RunResult.Elimination:
                    SetCrowdState(CrowdState.Disappointed, 0.4f);
                    break;

                case RunResult.TimeFaultOnly:
                    SetCrowdState(CrowdState.Applause, 0.5f);
                    break;
            }
        }

        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    SetCrowdState(CrowdState.Idle, 0.1f);
                    break;

                case GameState.Countdown:
                    SetCrowdState(CrowdState.Anticipation, 0.5f);
                    break;

                case GameState.Replay:
                    // Quieter during replay
                    SetCrowdState(CrowdState.Watching, 0.2f);
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void SetChampionshipMode(bool enabled)
        {
            isChampionshipMode = enabled;
        }

        public float GetExcitementLevel()
        {
            return excitementLevel;
        }

        public float GetSatisfaction()
        {
            return crowdSatisfaction;
        }

        public CrowdState GetCurrentState()
        {
            return currentState;
        }

        public int GetCrowdSize()
        {
            return crowdMembers.Count;
        }

        #endregion
    }

    public class CrowdMember : MonoBehaviour
    {
        private Animator animator;
        private float speedMultiplier = 1f;
        private float currentIntensity;

        public void Initialize(float speed)
        {
            speedMultiplier = speed;
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetFloat("SpeedMultiplier", speedMultiplier);
            }
        }

        public void UpdateAnimation(float intensity, float blendSpeed)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, intensity, Time.deltaTime * blendSpeed);

            if (animator != null)
            {
                animator.SetFloat("Intensity", currentIntensity);
                animator.SetBool("IsCheering", intensity > 0.5f);
                animator.SetBool("IsApplauding", intensity > 0.7f);
            }
        }
    }

    public enum CrowdState
    {
        Idle,
        Watching,
        Anticipation,
        Applause,
        Cheering,
        Ovation,
        Gasp,
        Disappointed
    }
}
