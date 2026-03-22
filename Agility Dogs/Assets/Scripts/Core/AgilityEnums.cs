namespace AgilityDogs.Core
{
    public enum DogState
    {
        Idle,
        Heeling,
        Running,
        CommittingToObstacle,
        OnObstacle,
        CompletingObstacle,
        Weaving,
        SeekingObstacle,
        WaitingAtTable,
        Recovering
    }

    public enum HandlerCommand
    {
        None,
        ComeBye,
        Away,
        Jump,
        Tunnel,
        Weave,
        Table,
        Here,
        Out,
        Go,
        Left,
        Right
    }

    public enum ObstacleType
    {
        BarJump,
        TireJump,
        BroadJump,
        WallJump,
        Tunnel,
        WeavePoles,
        AFrame,
        DogWalk,
        Teeter,
        PauseTable
    }

    public enum FaultType
    {
        None,
        MissedContact,
        WrongCourse,
        Refusal,
        RunOut,
        TimeFault,
        KnockedBar,
        OffCourse
    }

    public enum CourseType
    {
        Standard,
        JumpersWithWeaves
    }

    public enum GameState
    {
        MainMenu,
        ModeSelect,
        TeamSelect,
        CourseLoad,
        Countdown,
        Gameplay,
        RunComplete,
        Results,
        Replay,
        Career
    }

    public enum AgilitySizeDivision
    {
        EightInch,
        TwelveInch,
        FourteenInch,
        SixteenInch,
        TwentyInch
    }

    public enum RunResult
    {
        Qualified,
        NonQualified,
        Elimination,
        TimeFaultOnly
    }
}
