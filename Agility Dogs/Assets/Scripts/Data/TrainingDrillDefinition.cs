using System;
using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewTrainingDrill", menuName = "Agility Dogs/Training Drill")]
    public class TrainingDrillDefinition : ScriptableObject
    {
        [Header("Drill Info")]
        public string drillId;
        public string displayName;
        [TextArea(3, 6)]
        public string description;
        [TextArea(2, 4)]
        public string instructions;

        [Header("Drill Type")]
        public TrainingDrillType drillType = TrainingDrillType.ObstacleFocus;

        [Header("Difficulty")]
        [Range(1, 5)]
        public int difficulty = 1;
        public bool isUnlockedByDefault = true;

        [Header("Course Setup")]
        public CourseDefinition courseOverride;
        public ObstacleType[] obstacleSequence;
        public int repetitions = 3;

        [Header("Success Criteria")]
        public float targetTime = 30f;
        public int maxFaults = 0;
        public float minimumCleanRate = 0.8f; // 80% of repetitions must be clean

        [Header("Rewards")]
        public int xpReward = 50;
        public int wingsReward = 25;
        public string[] unlockableSkillIds;

        [Header("Visual")]
        public Sprite thumbnail;
        public Color drillColor = Color.green;

        [Header("Drill Settings")]
        public bool slowMotionEnabled = false;
        public float slowMotionSpeed = 0.5f;
        public bool showPathPreview = true;
        public bool autoResetOnFault = false;
        public float resetDelay = 1.5f;
    }

    public enum TrainingDrillType
    {
        ObstacleFocus,      // Practice specific obstacles
        WeavePoleMastery,   // Weave pole specific drills
        ContactPractice,    // Contact zone drills
        JumpGrid,           // Jump sequences
        TimingDrill,        // Command timing practice
        CourseFlow,         // Full course sections
        SpeedChallenge,     // Beat the clock
        PrecisionRun,       // Zero fault runs
        HandlerMovement,    // Handler positioning focus
        RecoveryPractice    // Practice recovery from mistakes
    }

    [Serializable]
    public class TrainingSessionData
    {
        public string drillId;
        public int completedRuns;
        public int cleanRuns;
        public float bestTime;
        public float bestCleanTime;
        public DateTime lastPlayed;
        public bool isCompleted;
        public int totalStars; // 0-3 stars

        public float CleanRate => completedRuns > 0 ? (float)cleanRuns / completedRuns : 0f;
        public float AverageTime => completedRuns > 0 ? bestTime : 0f;
    }
}
