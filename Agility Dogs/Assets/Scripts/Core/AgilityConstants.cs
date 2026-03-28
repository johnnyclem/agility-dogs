using UnityEngine;

namespace AgilityDogs.Core
{
    public static class AgilityConstants
    {
        // Timing
        public const float COMMAND_WINDOW_SECONDS = 1.5f;
        public const float DEFAULT_COUNTDOWN_SECONDS = 3f;
        public const float PAUSE_TABLE_DURATION = 5f;

        // Scoring
        public const int FAULT_PENALTY_SECONDS = 5;
        public const int REFUSAL_FAULT_POINTS = 1;
        public const int RUN_OUT_FAULT_POINTS = 1;

        // Dog AI
        public const float DEFAULT_HEEL_DISTANCE = 1.5f;
        public const float DEFAULT_FOLLOW_DISTANCE = 3f;
        public const float DEFAULT_REACQUISITION_DISTANCE = 8f;
        public const float OBSTACLE_COMMIT_RANGE = 2f;
        public const float DECISION_INTERVAL = 0.1f;

        // Navigation
        public const float NAV_SAMPLE_RADIUS = 5f;
        public const float NAV_AREA_MASK_AGILITY = 1;

        // Tags and Layers
        public const string TAG_HANDLER = "Handler";
        public const string TAG_DOG = "Dog";
        public const string TAG_OBSTACLE = "Obstacle";
        public const string TAG_COMMIT_ZONE = "CommitZone";
        public const string TAG_CONTACT_ZONE = "ContactZone";

        // Animation Parameters
        public const string ANIM_SPEED = "Speed";
        public const string ANIM_IS_RUNNING = "IsRunning";
        public const string ANIM_IS_ON_OBSTACLE = "IsOnObstacle";
        public const string ANIM_WEAVE_INDEX = "WeaveIndex";

        public static string GetOrdinalSuffix(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        public static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int decimals = Mathf.FloorToInt((time % 1f) * 100f);
            return $"{minutes:00}:{seconds:00}.{decimals:00}";
        }
    }
}
