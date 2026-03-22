using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Events;

namespace AgilityDogs.Gameplay.Obstacles
{
    public class BarJumpObstacle : ObstacleBase
    {
        [Header("Bar Jump")]
        [SerializeField] private Transform jumpBar;
        [SerializeField] private float barRestHeight = 1.2f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool barKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.BarJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            barKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!barKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Bar Jump");
            }
        }

        public void RegisterBarKnock()
        {
            barKnocked = true;
            if (jumpBar != null)
            {
                jumpBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            barKnocked = false;
            if (jumpBar != null)
            {
                jumpBar.localPosition = new Vector3(0, barRestHeight, 0);
            }
        }
    }

    public class TunnelObstacle : ObstacleBase
    {
        [Header("Tunnel")]
        [SerializeField] private float tunnelSpeedMultiplier = 1.1f;
        [SerializeField] private Transform[] tunnelPath;

        private int currentPathIndex;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.Tunnel;
        }

        public override float GetSpeedMultiplier() => tunnelSpeedMultiplier;

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            currentPathIndex = 0;
        }

        public Vector3 GetNextPathPoint()
        {
            if (tunnelPath == null || tunnelPath.Length == 0) return GetExitPoint();

            if (currentPathIndex < tunnelPath.Length)
            {
                Vector3 point = tunnelPath[currentPathIndex].position;
                currentPathIndex++;
                return point;
            }

            return GetExitPoint();
        }
    }

    public class WeavePolesObstacle : ObstacleBase
    {
        [Header("Weave Poles")]
        [SerializeField] private Transform[] polePositions;
        [SerializeField] private float weaveEntryTolerance = 1f;
        [SerializeField] private int requiredWeaves = 12;

        private int currentWeaveIndex;
        private bool enteredCorrectly;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.WeavePoles;
        }

        public override Vector3 GetEntryPoint()
        {
            if (polePositions != null && polePositions.Length > 1)
            {
                Vector3 firstPole = polePositions[0].position;
                Vector3 secondPole = polePositions[1].position;
                Vector3 entryDir = (secondPole - firstPole).normalized;
                return firstPole - entryDir * weaveEntryTolerance;
            }
            return base.GetEntryPoint();
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            currentWeaveIndex = 0;
            enteredCorrectly = false;
        }

        public Vector3 GetCurrentPoleTarget()
        {
            if (polePositions == null || polePositions.Length == 0) return GetExitPoint();

            if (currentWeaveIndex < polePositions.Length)
            {
                return polePositions[currentWeaveIndex].position;
            }

            return GetExitPoint();
        }

        public void AdvanceWeave()
        {
            currentWeaveIndex++;
        }

        public bool IsWeaveComplete(DogAgentController dog)
        {
            return currentWeaveIndex >= requiredWeaves ||
                   (polePositions != null && currentWeaveIndex >= polePositions.Length);
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (IsWeaveComplete(dog))
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.Refusal, "Weave Poles");
            }
        }
    }

    public class PauseTableObstacle : ObstacleBase
    {
        [Header("Pause Table")]
        [SerializeField] private Transform tableSurface;
        [SerializeField] private float requiredPauseDuration = 5f;

        private float pauseTimer;
        private bool dogIsOnTable;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.PauseTable;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            pauseTimer = 0f;
            dogIsOnTable = true;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (pauseTimer >= requiredPauseDuration)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.Refusal, "Pause Table");
            }
            dogIsOnTable = false;
        }

        private void Update()
        {
            if (dogIsOnTable)
            {
                pauseTimer += Time.deltaTime;
            }
        }
    }

    public class ContactObstacleBase : ObstacleBase
    {
        [Header("Contact Obstacle")]
        [SerializeField] protected float contactZoneLength = 0.36f;
        [SerializeField] protected Transform contactZoneStartMarker;
        [SerializeField] protected Transform contactZoneEndMarker;

        protected bool hitContactStart;
        protected bool hitContactEnd;

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            hitContactStart = false;
            hitContactEnd = false;
        }

        public override void OnDogContactZoneEnter(DogAgentController dog, bool isStart)
        {
            if (isStart)
                hitContactStart = true;
            else
                hitContactEnd = true;
        }

        public override bool CheckForFault(DogAgentController dog)
        {
            if (hitContactEnd && !hitContactStart)
            {
                GameEvents.RaiseFaultCommitted(FaultType.MissedContact, obstacleType.ToString());
                return true;
            }
            return false;
        }
    }

    public class AFrameObstacle : ContactObstacleBase
    {
        [Header("A-Frame")]
        [SerializeField] private float climbAngle = 45f;
        [SerializeField] private float aFrameSpeedMultiplier = 0.75f;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.AFrame;
        }

        public override float GetSpeedMultiplier() => aFrameSpeedMultiplier;
    }

    public class DogWalkObstacle : ContactObstacleBase
    {
        [Header("Dog Walk")]
        [SerializeField] private float plankLength = 10f;
        [SerializeField] private float plankWidth = 0.3f;
        [SerializeField] private float dogWalkSpeedMultiplier = 0.65f;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.DogWalk;
        }

        public override float GetSpeedMultiplier() => dogWalkSpeedMultiplier;
    }

    public class TeeterObstacle : ContactObstacleBase
    {
        [Header("Teeter/Tipple")]
        [SerializeField] private Transform teeterBoard;
        [SerializeField] private float teeterSpeedMultiplier = 0.7f;
        [SerializeField] private float pivotHeight = 0.6f;
        [SerializeField] private float teeterDropAngle = 35f;

        private bool hasTeetered;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.Teeter;
        }

        public override float GetSpeedMultiplier() => teeterSpeedMultiplier;

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            hasTeetered = false;
        }

        public void TriggerTeeter()
        {
            hasTeetered = true;
            if (teeterBoard != null)
            {
                teeterBoard.localRotation = Quaternion.Euler(teeterDropAngle, 0, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            hasTeetered = false;
            if (teeterBoard != null)
            {
                teeterBoard.localRotation = Quaternion.identity;
            }
        }
    }
}
