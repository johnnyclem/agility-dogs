using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Services;

namespace AgilityDogs.Presentation.Commentary
{
    public class CommentaryDirector : MonoBehaviour
    {
        public static CommentaryDirector Instance { get; private set; }

        [Header("Commentary Settings")]
        [SerializeField] private bool enableCommentary = true;
        [SerializeField] private float cooldownsExtension = 2f;

        [Header("Pressure Escalation")]
        [SerializeField] private float pressureIncreaseRate = 0.1f;
        [SerializeField] private float maxPressure = 1f;
        [SerializeField] private float nearMissThreshold = 1.5f;

        [Header("Breed Callouts")]
        [SerializeField] private bool enableBreedCallouts = true;
        [SerializeField] private float breedCalloutInterval = 30f;

        [Header("Timing Callouts")]
        [SerializeField] private float splitTimeCalloutThreshold = 0.3f;

        private CommentaryManager commentaryManager;
        private float currentPressure;
        private float lastBreedCalloutTime = -999f;
        private float lastSplitCalloutTime = -999f;
        private float lastFaultCalloutTime = -999f;
        private string currentBreedName;
        private bool isRunActive;
        private bool hasNearMissed;
        private bool hasExcitingFinish;

        public float CurrentPressure => currentPressure;

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
            commentaryManager = FindObjectOfType<CommentaryManager>();

            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
            GameEvents.OnNearMiss += HandleNearMiss;
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
            if (isRunActive && enableCommentary)
            {
                UpdatePressureEscalation();
            }
        }

        private void HandleRunStarted()
        {
            isRunActive = true;
            currentPressure = 0f;
            hasNearMissed = false;
            hasExcitingFinish = false;
            lastBreedCalloutTime = Time.time;
            lastFaultCalloutTime = -999f;
            lastSplitCalloutTime = -999f;

            commentaryManager?.TriggerMainAnnouncerCommentary("And they're off! What a start!");
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            isRunActive = false;

            if (commentaryManager == null) return;

            switch (result)
            {
                case RunResult.Qualified:
                    if (faults == 0)
                    {
                        commentaryManager.TriggerMainAnnouncerCommentary("A clean run! Absolutely flawless!");
                        StartCoroutine(DelayedCommentary(() => 
                            commentaryManager.TriggerMainAnnouncerCommentary("What an incredible performance!"), 2f));
                    }
                    else
                    {
                        commentaryManager.TriggerMainAnnouncerCommentary("Qualified! A solid run today.");
                    }
                    break;

                case RunResult.NonQualified:
                    commentaryManager.TriggerMainAnnouncerCommentary("Unfortunately, they didn't qualify today.");
                    break;

                case RunResult.Elimination:
                    commentaryManager.TriggerMainAnnouncerCommentary("Eliminated from the course.");
                    break;

                case RunResult.TimeFaultOnly:
                    commentaryManager.TriggerMainAnnouncerCommentary("Time faults, but they finish the course.");
                    break;
            }
        }

        private IEnumerator DelayedCommentary(System.Action callback, float delay)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (!enableCommentary || commentaryManager == null) return;

            TriggerObstacleCallout(type, clean);

            if (enableBreedCallouts && !string.IsNullOrEmpty(currentBreedName))
            {
                if (Time.time - lastBreedCalloutTime >= breedCalloutInterval)
                {
                    TriggerBreedCallout();
                    lastBreedCalloutTime = Time.time;
                }
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            if (!enableCommentary || commentaryManager == null) return;

            if (Time.time - lastFaultCalloutTime < cooldownsExtension) return;

            currentPressure = Mathf.Min(maxPressure, currentPressure + 0.2f);
            lastFaultCalloutTime = Time.time;

            string message = fault switch
            {
                FaultType.MissedContact => $"Missed contact on the {obstacleName}!",
                FaultType.Refusal => $"Refusal at the {obstacleName}!",
                FaultType.RunOut => $"Run out! The dog left the course at the {obstacleName}!",
                FaultType.KnockedBar => $"Bar down on the {obstacleName}!",
                FaultType.WrongCourse => $"Wrong course taken!",
                FaultType.TimeFault => $"Time fault!",
                _ => $"Fault on the {obstacleName}!"
            };

            commentaryManager.TriggerColorCommentatorCommentary(message);
        }

        private void HandleSplitTime(float splitTime)
        {
            if (!enableCommentary || commentaryManager == null) return;

            if (Time.time - lastSplitCalloutTime < cooldownsExtension) return;

            float personalBest = PlayerPrefs.GetFloat("PersonalBestSplit", float.MaxValue);
            float diff = splitTime - personalBest;

            if (diff <= -splitTimeCalloutThreshold)
            {
                commentaryManager.TriggerMainAnnouncerCommentary("New personal best split time!");
                lastSplitCalloutTime = Time.time;
            }
            else if (diff <= splitTimeCalloutThreshold)
            {
                commentaryManager.TriggerColorCommentatorCommentary("Good split time there.");
                lastSplitCalloutTime = Time.time;
            }
        }

        private void HandleNearMiss()
        {
            if (!enableCommentary || hasNearMissed || commentaryManager == null) return;

            hasNearMissed = true;
            commentaryManager.TriggerColorCommentatorCommentary("That was close! Near miss there!");
            currentPressure = Mathf.Min(maxPressure, currentPressure + 0.15f);
        }

        private void TriggerObstacleCallout(ObstacleType type, bool clean)
        {
            string message;

            if (clean)
            {
                message = type switch
                {
                    ObstacleType.BarJump => "Clean jump!",
                    ObstacleType.Tunnel => "Through the tunnel smoothly!",
                    ObstacleType.WeavePoles => "Beautiful weave poles!",
                    ObstacleType.PauseTable => "Perfect pause on the table!",
                    ObstacleType.AFrame => "Great A-frame contact!",
                    ObstacleType.DogWalk => "Solid dog walk!",
                    ObstacleType.Teeter => "Teeter completed cleanly!",
                    _ => "Clean obstacle!"
                };
            }
            else
            {
                message = type switch
                {
                    ObstacleType.BarJump => "Rough jump, but made it through.",
                    ObstacleType.Tunnel => "Got through the tunnel.",
                    ObstacleType.WeavePoles => "Weave poles complete.",
                    ObstacleType.PauseTable => "Finished at the table.",
                    ObstacleType.AFrame => "A-frame done.",
                    ObstacleType.DogWalk => "Dog walk complete.",
                    ObstacleType.Teeter => "Teeter done.",
                    _ => "Obstacle complete."
                };
            }

            commentaryManager.TriggerColorCommentatorCommentary(message);
        }

        private void TriggerBreedCallout()
        {
            if (!enableBreedCallouts || commentaryManager == null || string.IsNullOrEmpty(currentBreedName)) return;

            commentaryManager.TriggerMainAnnouncerCommentary($"That's a beautiful {currentBreedName} out there!");
        }

        private void UpdatePressureEscalation()
        {
            if (!isRunActive) return;

            float pressureDelta = pressureIncreaseRate * Time.deltaTime;
            currentPressure = Mathf.Min(maxPressure, currentPressure + pressureDelta);

            if (currentPressure >= 0.8f)
            {
                commentaryManager?.AddPressure(0.5f);
            }
            else if (currentPressure >= 0.5f)
            {
                commentaryManager?.AddPressure(0.2f);
            }
        }

        public void SetBreed(string breedName)
        {
            currentBreedName = breedName;
        }

        public void SetCommentaryEnabled(bool enabled)
        {
            enableCommentary = enabled;
        }

        public void TriggerExcitement(string context)
        {
            if (!enableCommentary || commentaryManager == null) return;

            if (context == "finish" && !hasExcitingFinish)
            {
                hasExcitingFinish = true;
                commentaryManager.TriggerMainAnnouncerCommentary("This is going to be a very exciting finish!");
            }
        }

        public void TriggerChampionshipPressure()
        {
            if (!enableCommentary || commentaryManager == null) return;

            currentPressure = maxPressure;
            commentaryManager.SetChampionshipMode(true);
            commentaryManager.TriggerMainAnnouncerCommentary("This is it! Championship pressure at its finest!");
        }
    }
}