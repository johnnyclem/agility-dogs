using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Presentation.Crowd
{
    public class CrowdReactionSystem : MonoBehaviour
    {
        public static CrowdReactionSystem Instance { get; private set; }

        [Header("Reaction Timing")]
        [SerializeField] private float reactionDelay = 0.3f;
        [SerializeField] private float sustainedApplauseDuration = 3f;
        [SerializeField] private float tensionBuildRate = 0.1f;

        [Header("Intensity Curves")]
        [SerializeField] private AnimationCurve faultReactionCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private AnimationCurve successReactionCurve = AnimationCurve.EaseInOut(0f, 0.5f, 0.5f, 0f);
        [SerializeField] private AnimationCurve nearMissReactionCurve = AnimationCurve.EaseInOut(0f, 0.8f, 0.3f, 0f);

        [Header("Exciting Moments")]
        [SerializeField] private float nearMissExcitementThreshold = 0.7f;
        [SerializeField] private float closeFinishThreshold = 2f;
        [SerializeField] private float championshipExcitementMultiplier = 1.5f;

        [Header("Audio Integration")]
        [SerializeField] private AudioSource crowdAudioSource;
        [SerializeField] private AudioClip[] crowdCheerClips;
        [SerializeField] private AudioClip[] crowdBooClips;
        [SerializeField] private AudioClip[] crowdGaspClips;
        [SerializeField] private AudioClip[] crowdAnticipationClips;
        [SerializeField] private float minTimeBetweenCrowdSounds = 0.5f;

        private CrowdManager crowdManager;
        private float currentExcitement;
        private float targetExcitement;
        private float lastSoundTime;
        private bool isRunActive;
        private bool isChampionshipMode;
        private Dictionary<string, float> reactionCooldowns = new Dictionary<string, float>();
        private Coroutine tensionCoroutine;

        public float CurrentExcitement => currentExcitement;

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
            crowdManager = FindObjectOfType<CrowdManager>();

            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
            GameEvents.OnNearMiss += HandleNearMiss;
            GameEvents.OnRunCompleted += HandleRunCompleted;
        }

        private void OnDestroy()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTime;
            GameEvents.OnNearMiss -= HandleNearMiss;
        }

        private void Update()
        {
            if (!isRunActive) return;

            UpdateExcitementLevel();
        }

        private void HandleRunStarted()
        {
            isRunActive = true;
            currentExcitement = 0f;
            targetExcitement = 0f;
            isChampionshipMode = false;
            reactionCooldowns.Clear();

            crowdManager?.SetCrowdState(CrowdState.Anticipation, 0.4f);
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            isRunActive = false;

            if (result == RunResult.Qualified)
            {
                if (faults == 0)
                {
                    TriggerOvation();
                }
                else
                {
                    TriggerApplause();
                }
            }
            else if (result == RunResult.NonQualified || result == RunResult.Elimination)
            {
                TriggerDisappointment();
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (!isRunActive) return;

            if (clean)
            {
                float intensity = 0.4f + (0.1f * (float)type);
                TriggerCheer(intensity);
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            if (!isRunActive) return;

            float intensity = fault switch
            {
                FaultType.MissedContact => 0.6f,
                FaultType.Refusal => 0.7f,
                FaultType.RunOut => 0.8f,
                FaultType.KnockedBar => 0.5f,
                FaultType.WrongCourse => 0.7f,
                _ => 0.5f
            };

            if (isChampionshipMode)
            {
                intensity *= championshipExcitementMultiplier;
            }

            TriggerDisappointment(intensity);
            BuildTension(-0.2f);
        }

        private void HandleSplitTime(float splitTime)
        {
            if (!isRunActive) return;

            float personalBest = PlayerPrefs.GetFloat("PersonalBestSplit", float.MaxValue);
            float diff = splitTime - personalBest;

            if (Mathf.Abs(diff) <= closeFinishThreshold)
            {
                TriggerExcitedMurmur(0.5f);
            }
            else if (diff < 0)
            {
                TriggerCheer(0.6f);
            }
        }

        private void HandleNearMiss()
        {
            if (!isRunActive) return;

            if (currentExcitement < nearMissExcitementThreshold)
            {
                currentExcitement = nearMissExcitementThreshold;
            }

            TriggerGasp();
            BuildTension(0.3f);
        }

        private void UpdateExcitementLevel()
        {
            if (Mathf.Abs(currentExcitement - targetExcitement) > 0.01f)
            {
                currentExcitement = Mathf.Lerp(currentExcitement, targetExcitement, Time.deltaTime * 2f);
                crowdManager?.SetIntensity(currentExcitement);
            }
            else
            {
                targetExcitement = Mathf.Max(0f, targetExcitement - Time.deltaTime * 0.1f);
            }
        }

        private void TriggerCheer(float intensity)
        {
            if (!CanTriggerReaction("cheer")) return;

            intensity = Mathf.Clamp01(intensity);
            targetExcitement = Mathf.Max(targetExcitement, intensity);

            crowdManager?.TriggerCheer(intensity);
            PlayCrowdSound(CrowdSoundType.Cheer, intensity);

            reactionCooldowns["cheer"] = Time.time + (sustainedApplauseDuration * 0.5f);
        }

        private void TriggerApplause()
        {
            if (!CanTriggerReaction("applause")) return;

            targetExcitement = 0.7f;
            crowdManager?.TriggerApplause();
            PlayCrowdSound(CrowdSoundType.Cheer, 0.6f);

            reactionCooldowns["applause"] = Time.time + sustainedApplauseDuration;
        }

        private void TriggerOvation()
        {
            if (!CanTriggerReaction("ovation")) return;

            targetExcitement = 1f;
            crowdManager?.TriggerOvation();
            PlayCrowdSound(CrowdSoundType.Cheer, 1f);

            reactionCooldowns["ovation"] = Time.time + sustainedApplauseDuration;
        }

        private void TriggerDisappointment(float intensity = 0.5f)
        {
            if (!CanTriggerReaction("disappointment")) return;

            targetExcitement = Mathf.Max(0.2f, intensity - 0.3f);
            crowdManager?.TriggerDisappointed(intensity);
            PlayCrowdSound(CrowdSoundType.Boo, intensity);

            reactionCooldowns["disappointment"] = Time.time + (sustainedApplauseDuration * 0.3f);
        }

        private void TriggerGasp()
        {
            if (!CanTriggerReaction("gasp")) return;

            targetExcitement = Mathf.Max(targetExcitement, 0.6f);
            PlayCrowdSound(CrowdSoundType.Gasp, 0.7f);

            reactionCooldowns["gasp"] = Time.time + reactionDelay;
        }

        private void TriggerExcitedMurmur(float intensity)
        {
            if (!CanTriggerReaction("murmur")) return;

            targetExcitement = Mathf.Max(targetExcitement, intensity * 0.5f);
            PlayCrowdSound(CrowdSoundType.Anticipation, intensity * 0.3f);

            reactionCooldowns["murmur"] = Time.time + reactionDelay;
        }

        private void BuildTension(float delta)
        {
            targetExcitement = Mathf.Clamp01(targetExcitement + delta);

            if (tensionCoroutine == null)
            {
                tensionCoroutine = StartCoroutine(ReleaseTensionCoroutine());
            }
        }

        private IEnumerator ReleaseTensionCoroutine()
        {
            yield return new WaitForSeconds(2f);

            while (targetExcitement > currentExcitement)
            {
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            float releaseRate = 0.05f;
            while (currentExcitement > 0.3f)
            {
                currentExcitement -= releaseRate * Time.deltaTime;
                yield return null;
            }

            tensionCoroutine = null;
        }

        private bool CanTriggerReaction(string reactionType)
        {
            if (reactionCooldowns.TryGetValue(reactionType, out float lastTime))
            {
                if (Time.time < lastTime) return false;
            }

            if (Time.time - lastSoundTime < minTimeBetweenCrowdSounds) return false;

            return true;
        }

        private void PlayCrowdSound(CrowdSoundType soundType, float volume)
        {
            if (crowdAudioSource == null) return;

            AudioClip[] clips = soundType switch
            {
                CrowdSoundType.Cheer => crowdCheerClips,
                CrowdSoundType.Boo => crowdBooClips,
                CrowdSoundType.Gasp => crowdGaspClips,
                CrowdSoundType.Anticipation => crowdAnticipationClips,
                _ => null
            };

            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            crowdAudioSource.PlayOneShot(clip, volume);
            lastSoundTime = Time.time;
        }

        public void SetChampionshipMode(bool isChampionship)
        {
            isChampionshipMode = isChampionship;
        }

        public void TriggerCustomReaction(float intensity, CrowdState state)
        {
            targetExcitement = Mathf.Max(targetExcitement, intensity);
            crowdManager?.SetCrowdState(state, intensity);
        }

        private enum CrowdSoundType
        {
            Cheer,
            Boo,
            Gasp,
            Anticipation
        }
    }
}