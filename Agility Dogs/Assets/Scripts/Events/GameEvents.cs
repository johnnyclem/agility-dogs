using System;
using AgilityDogs.Core;

namespace AgilityDogs.Events
{
    public static class GameEvents
    {
        public static event Action<GameState, GameState> OnGameStateChanged;
        public static event Action<HandlerCommand> OnCommandIssued;
        public static event Action<FaultType, string> OnFaultCommitted;
        public static event Action<ObstacleType, bool> OnObstacleCompleted;
        public static event Action<float> OnSplitTimeRecorded;
        public static event Action OnRunStarted;
        public static event Action<RunResult, float, int> OnRunCompleted;
        public static event Action OnCountdownTick;
        public static event Action OnCourseLoaded;

        public static void RaiseGameStateChanged(GameState from, GameState to)
            => OnGameStateChanged?.Invoke(from, to);

        public static void RaiseCommandIssued(HandlerCommand command)
            => OnCommandIssued?.Invoke(command);

        public static void RaiseFaultCommitted(FaultType fault, string obstacleName)
            => OnFaultCommitted?.Invoke(fault, obstacleName);

        public static void RaiseObstacleCompleted(ObstacleType type, bool clean)
            => OnObstacleCompleted?.Invoke(type, clean);

        public static void RaiseSplitTime(float time)
            => OnSplitTimeRecorded?.Invoke(time);

        public static void RaiseRunStarted()
            => OnRunStarted?.Invoke();

        public static void RaiseRunCompleted(RunResult result, float time, int faults)
            => OnRunCompleted?.Invoke(result, time, faults);

        public static void RaiseCountdownTick()
            => OnCountdownTick?.Invoke();

        public static void RaiseCourseLoaded()
            => OnCourseLoaded?.Invoke();
    }
}
