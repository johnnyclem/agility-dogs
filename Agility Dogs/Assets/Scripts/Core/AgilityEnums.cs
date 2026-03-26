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
        None,
        BarJump,
        TireJump,
        BroadJump,
        WallJump,
        Tunnel,
        WeavePoles,
        AFrame,
        DogWalk,
        Teeter,
        PauseTable,
        DoubleJump,
        TripleJump,
        PanelJump,
        LongJump,
        SpreadJump
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
        JumpersWithWeaves,
        Championship
    }

    public enum GameState
    {
        MainMenu,
        ModeSelect,
        TeamSelect,
        CourseLoad,
        Countdown,
        Gameplay,
        Pause,
        RunComplete,
        Results,
        Replay,
        Career
    }

    public enum GameMode
    {
        None,
        QuickPlay,
        Training,
        Exhibition,
        Career,
        Campaign,
        Tournament
    }

    public enum CareerPhase
    {
        Breeding,       // Select/create puppy
        Training,       // Train puppy basics
        LocalShows,     // Enter local competitions
        RegionalShows,  // Regional competitions
        NationalShows,  // National championships
        Westminster     // Agility Kings at Westminster
    }

    public enum ShowTier
    {
        Local,          // Local park competitions
        County,         // County fair shows
        Regional,       // Regional championships
        State,          // State championships
        National,       // National championships
        Westminster     // Agility Kings at Westminster Dog Show
    }

    public enum ShowResult
    {
        DidNotPlace,
        HonorableMention,
        ThirdPlace,
        SecondPlace,
        FirstPlace,
        BestInShow
    }

    public enum PuppyTrait
    {
        Energetic,      // Higher stamina, faster fatigue
        Calm,           // Lower stamina, slower fatigue
        Intelligent,    // Faster learning
        Stubborn,       // Slower learning, harder to train
        Agile,          // Better turning, obstacle performance
        Strong,         // Better jump power
        Sensitive,      // More responsive to commands
        Distracted,     // Less focused, more likely to lose concentration
        Confident,      // Less likely to spook
        Nervous         // More likely to make mistakes under pressure
    }

    public enum TrainingSkill
    {
        BasicObedience,
        JumpTechnique,
        WeavePoles,
        ContactObstacles,
        TunnelWork,
        SpeedTraining,
        FocusTraining,
        HandlerBonding,
        CompetitionReady,
        AdvancedHandling
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

    public enum GestureType
    {
        None,
        Point,
        PointLeft,
        PointRight,
        Beckon,
        Wave,
        Stop
    }

    public enum CommandTimingRating
    {
        Poor,
        Good,
        Optimal
    }

    public enum RecoveryReason
    {
        None,
        MissedCommand,
        WrongObstacle,
        Stuck,
        HandlerTooFar,
        GeneralRecovery
    }
}
