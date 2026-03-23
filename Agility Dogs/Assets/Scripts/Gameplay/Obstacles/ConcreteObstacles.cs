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
        
        [Header("Precision Contact Detection")]
        [SerializeField] protected bool usePreciseContactDetection = true;
        [SerializeField] protected PreciseContactZone preciseContactZone;

        protected bool hitContactStart;
        protected bool hitContactEnd;
        protected DogAgentController currentDog;

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            hitContactStart = false;
            hitContactEnd = false;
            currentDog = dog;
            
            // Initialize precise contact detection if enabled
            if (usePreciseContactDetection && preciseContactZone != null)
            {
                preciseContactZone.RegisterDog(dog);
                preciseContactZone.ResetDetection();
            }
        }

        public override void OnDogContactZoneEnter(DogAgentController dog, bool isStart)
        {
            if (isStart)
                hitContactStart = true;
            else
                hitContactEnd = true;
                
            // Forward to precise contact zone if using it
            if (usePreciseContactDetection && preciseContactZone != null && dog == currentDog)
            {
                // The precise contact zone handles its own detection
                // This is kept for backward compatibility
            }
        }

        public override void OnDogExited(DogAgentController dog)
        {
            // Check for contact fault before completing
            bool contactFault = CheckForFault(dog);
            
            if (!contactFault)
            {
                base.OnDogExited(dog);
            }
            
            // Clean up precise contact detection
            if (usePreciseContactDetection && preciseContactZone != null)
            {
                preciseContactZone.UnregisterDog();
            }
            
            currentDog = null;
        }

        public override bool CheckForFault(DogAgentController dog)
        {
            // Use precise contact zone if available
            if (usePreciseContactDetection && preciseContactZone != null && dog == currentDog)
            {
                // Check if dog hit the contact zones
                bool startHit = preciseContactZone.HasHitContactStart();
                bool endHit = preciseContactZone.HasHitContactEnd();
                bool contactValid = preciseContactZone.IsContactValid();
                
                // Fault if dog exited without proper contact
                if (!contactValid)
                {
                    GameEvents.RaiseFaultCommitted(FaultType.MissedContact, obstacleType.ToString());
                    return true;
                }
            }
            else
            {
                // Fallback to legacy detection
                if (hitContactEnd && !hitContactStart)
                {
                    GameEvents.RaiseFaultCommitted(FaultType.MissedContact, obstacleType.ToString());
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get contact accuracy for UI feedback
        /// </summary>
        public virtual float GetContactAccuracy()
        {
            if (usePreciseContactDetection && preciseContactZone != null)
            {
                return preciseContactZone.GetContactAccuracy();
            }
            
            // Legacy accuracy calculation
            if (!hitContactStart && !hitContactEnd) return 0f;
            if (hitContactStart && hitContactEnd) return 1f;
            return 0.5f;
        }
        
        /// <summary>
        /// Check if using precise contact detection
        /// </summary>
        public bool IsUsingPreciseDetection()
        {
            return usePreciseContactDetection && preciseContactZone != null;
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

    public class TireJumpObstacle : ObstacleBase
    {
        [Header("Tire Jump")]
        [SerializeField] private Transform tireVisual;
        [SerializeField] private float tireRestHeight = 1.2f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool tireKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.TireJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            tireKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!tireKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Tire Jump");
            }
        }

        public void RegisterTireKnock()
        {
            tireKnocked = true;
            if (tireVisual != null)
            {
                tireVisual.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            tireKnocked = false;
            if (tireVisual != null)
            {
                tireVisual.localPosition = new Vector3(0, tireRestHeight, 0);
            }
        }
    }

    public class BroadJumpObstacle : ObstacleBase
    {
        [Header("Broad Jump")]
        [SerializeField] private Transform[] hurdles; // Array of hurdle transforms
        [SerializeField] private float hurdleHeight = 0.2f;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.BroadJump;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            // Check if dog cleared all hurdles (simplified)
            // For now, just count as completed
            base.OnDogExited(dog);
        }
    }

    public class WallJumpObstacle : ObstacleBase
    {
        [Header("Wall Jump")]
        [SerializeField] private Transform wallVisual;
        [SerializeField] private float wallHeight = 1.5f;
        [SerializeField] private float topBarHeight = 1.8f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool topBarKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.WallJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            topBarKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!topBarKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Wall Jump");
            }
        }

        public void RegisterTopBarKnock()
        {
            topBarKnocked = true;
            // Optionally lower the top bar visual
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            topBarKnocked = false;
            // Reset top bar visual
        }
    }

    public class DoubleJumpObstacle : ObstacleBase
    {
        [Header("Double Jump")]
        [SerializeField] private Transform firstBar;
        [SerializeField] private Transform secondBar;
        [SerializeField] private float barSpacing = 0.5f;
        [SerializeField] private float barRestHeight = 1.2f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool firstBarKnocked;
        private bool secondBarKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.DoubleJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            firstBarKnocked = false;
            secondBarKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!firstBarKnocked && !secondBarKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Double Jump");
            }
        }

        public void RegisterFirstBarKnock()
        {
            firstBarKnocked = true;
            if (firstBar != null)
            {
                firstBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public void RegisterSecondBarKnock()
        {
            secondBarKnocked = true;
            if (secondBar != null)
            {
                secondBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            firstBarKnocked = false;
            secondBarKnocked = false;
            if (firstBar != null)
            {
                firstBar.localPosition = new Vector3(0, barRestHeight, 0);
            }
            if (secondBar != null)
            {
                secondBar.localPosition = new Vector3(0, barRestHeight, barSpacing);
            }
        }
    }

    public class TripleJumpObstacle : ObstacleBase
    {
        [Header("Triple Jump")]
        [SerializeField] private Transform firstBar;
        [SerializeField] private Transform secondBar;
        [SerializeField] private Transform thirdBar;
        [SerializeField] private float barSpacing = 0.5f;
        [SerializeField] private float barRestHeight = 1.2f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool firstBarKnocked;
        private bool secondBarKnocked;
        private bool thirdBarKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.TripleJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            firstBarKnocked = false;
            secondBarKnocked = false;
            thirdBarKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!firstBarKnocked && !secondBarKnocked && !thirdBarKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Triple Jump");
            }
        }

        public void RegisterFirstBarKnock()
        {
            firstBarKnocked = true;
            if (firstBar != null)
            {
                firstBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public void RegisterSecondBarKnock()
        {
            secondBarKnocked = true;
            if (secondBar != null)
            {
                secondBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public void RegisterThirdBarKnock()
        {
            thirdBarKnocked = true;
            if (thirdBar != null)
            {
                thirdBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            firstBarKnocked = false;
            secondBarKnocked = false;
            thirdBarKnocked = false;
            if (firstBar != null)
            {
                firstBar.localPosition = new Vector3(0, barRestHeight, 0);
            }
            if (secondBar != null)
            {
                secondBar.localPosition = new Vector3(0, barRestHeight, barSpacing);
            }
            if (thirdBar != null)
            {
                thirdBar.localPosition = new Vector3(0, barRestHeight, barSpacing * 2);
            }
        }
    }

    public class PanelJumpObstacle : ObstacleBase
    {
        [Header("Panel Jump")]
        [SerializeField] private Transform panelVisual;
        [SerializeField] private float panelHeight = 1.5f;
        [SerializeField] private float panelWidth = 1.0f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool panelKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.PanelJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            panelKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!panelKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Panel Jump");
            }
        }

        public void RegisterPanelKnock()
        {
            panelKnocked = true;
            if (panelVisual != null)
            {
                panelVisual.localScale = new Vector3(panelVisual.localScale.x, 0.1f, panelVisual.localScale.z);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            panelKnocked = false;
            if (panelVisual != null)
            {
                panelVisual.localScale = new Vector3(panelVisual.localScale.x, panelHeight, panelVisual.localScale.z);
            }
        }
    }

    public class LongJumpObstacle : ObstacleBase
    {
        [Header("Long Jump")]
        [SerializeField] private Transform[] jumpBars;
        [SerializeField] private float jumpLength = 3.0f;
        [SerializeField] private float barRestHeight = 0.3f;
        [SerializeField] private float knockThreshold = 0.2f;

        private bool[] barsKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.LongJump;
            
            if (jumpBars != null)
            {
                barsKnocked = new bool[jumpBars.Length];
            }
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            if (barsKnocked != null)
            {
                for (int i = 0; i < barsKnocked.Length; i++)
                {
                    barsKnocked[i] = false;
                }
            }
        }

        public override void OnDogExited(DogAgentController dog)
        {
            bool allCleared = true;
            if (barsKnocked != null)
            {
                foreach (bool knocked in barsKnocked)
                {
                    if (knocked)
                    {
                        allCleared = false;
                        break;
                    }
                }
            }

            if (allCleared)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Long Jump");
            }
        }

        public void RegisterBarKnock(int barIndex)
        {
            if (barsKnocked != null && barIndex >= 0 && barIndex < barsKnocked.Length)
            {
                barsKnocked[barIndex] = true;
                if (jumpBars != null && barIndex < jumpBars.Length && jumpBars[barIndex] != null)
                {
                    jumpBars[barIndex].localPosition = new Vector3(0, -0.1f, 0);
                }
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            if (barsKnocked != null)
            {
                for (int i = 0; i < barsKnocked.Length; i++)
                {
                    barsKnocked[i] = false;
                }
            }
            
            if (jumpBars != null)
            {
                for (int i = 0; i < jumpBars.Length; i++)
                {
                    if (jumpBars[i] != null)
                    {
                        float zOffset = (jumpLength / (jumpBars.Length - 1)) * i;
                        jumpBars[i].localPosition = new Vector3(0, barRestHeight, zOffset);
                    }
                }
            }
        }
    }

    public class SpreadJumpObstacle : ObstacleBase
    {
        [Header("Spread Jump")]
        [SerializeField] private Transform frontBar;
        [SerializeField] private Transform backBar;
        [SerializeField] private float spreadWidth = 1.0f;
        [SerializeField] private float barRestHeight = 1.2f;
        [SerializeField] private float knockThreshold = 0.3f;

        private bool frontBarKnocked;
        private bool backBarKnocked;

        protected override void Awake()
        {
            base.Awake();
            obstacleType = ObstacleType.SpreadJump;
        }

        public override void OnDogEntered(DogAgentController dog)
        {
            base.OnDogEntered(dog);
            frontBarKnocked = false;
            backBarKnocked = false;
        }

        public override void OnDogExited(DogAgentController dog)
        {
            if (!frontBarKnocked && !backBarKnocked)
            {
                base.OnDogExited(dog);
            }
            else
            {
                GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "Spread Jump");
            }
        }

        public void RegisterFrontBarKnock()
        {
            frontBarKnocked = true;
            if (frontBar != null)
            {
                frontBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public void RegisterBackBarKnock()
        {
            backBarKnocked = true;
            if (backBar != null)
            {
                backBar.localPosition = new Vector3(0, -0.5f, 0);
            }
        }

        public override void ResetObstacle()
        {
            base.ResetObstacle();
            frontBarKnocked = false;
            backBarKnocked = false;
            if (frontBar != null)
            {
                frontBar.localPosition = new Vector3(0, barRestHeight, 0);
            }
            if (backBar != null)
            {
                backBar.localPosition = new Vector3(0, barRestHeight, spreadWidth);
            }
        }
    }
}
