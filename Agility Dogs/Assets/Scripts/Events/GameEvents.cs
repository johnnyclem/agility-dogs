using System;
using AgilityDogs.Core;
using AgilityDogs.Gameplay.Obstacles;

namespace AgilityDogs.Events
{
    public static class GameEvents
    {
        public static event Action<GameState, GameState> OnGameStateChanged;
        public static event Action<HandlerCommand> OnCommandIssued;
        public static event Action<GestureType, Vector3> OnHandlerGesture;
        public static event Action<HandlerCommand, Vector3, float> OnContextualCommand;
        public static event Action<FaultType, string> OnFaultCommitted;
        public static event Action<ObstacleType, bool> OnObstacleCompleted;
        public static event Action<ObstacleBase, bool> OnObstacleCompletedWithReference;
        public static event Action<float> OnSplitTimeRecorded;
        public static event Action OnRunStarted;
        public static event Action<RunResult, float, int> OnRunCompleted;
        public static event Action OnCountdownTick;
        public static event Action OnCourseLoaded;
        public static event Action<Vector3> OnHandlerLeanChanged;
        public static event Action<float> OnHandlerPathInfluence;
        public static event Action<Core.RecoveryReason> OnDogRecovery;

        public static void RaiseGameStateChanged(GameState from, GameState to)
            => OnGameStateChanged?.Invoke(from, to);

        public static void RaiseCommandIssued(HandlerCommand command)
            => OnCommandIssued?.Invoke(command);

        public static void RaiseFaultCommitted(FaultType fault, string obstacleName)
            => OnFaultCommitted?.Invoke(fault, obstacleName);

        public static void RaiseObstacleCompleted(ObstacleType type, bool clean)
            => OnObstacleCompleted?.Invoke(type, clean);

        public static void RaiseObstacleCompletedWithReference(ObstacleBase obstacle, bool clean)
            => OnObstacleCompletedWithReference?.Invoke(obstacle, clean);

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

        public static void RaiseHandlerGesture(GestureType gesture, Vector3 direction)
            => OnHandlerGesture?.Invoke(gesture, direction);

        public static void RaiseContextualCommand(HandlerCommand command, Vector3 forward, float speed)
            => OnContextualCommand?.Invoke(command, forward, speed);

        public static void RaiseHandlerLeanChanged(Vector3 leanDirection)
            => OnHandlerLeanChanged?.Invoke(leanDirection);

        public static void RaiseHandlerPathInfluence(float influence)
            => OnHandlerPathInfluence?.Invoke(influence);

        public static void RaiseDogRecovery(Core.RecoveryReason reason)
            => OnDogRecovery?.Invoke(reason);
    }
}
