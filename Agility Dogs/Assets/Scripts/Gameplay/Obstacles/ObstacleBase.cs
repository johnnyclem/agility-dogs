using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Events;

namespace AgilityDogs.Gameplay.Obstacles
{
    public abstract class ObstacleBase : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected ObstacleData obstacleData;
        public ObstacleData ObstacleData => obstacleData;
        [SerializeField] protected ObstacleType obstacleType;
        [SerializeField] protected bool isActive = true;

        [Header("Navigation Points")]
        [SerializeField] protected Transform entryPoint;
        [SerializeField] protected Transform commitPoint;
        [SerializeField] protected Transform exitPoint;
        [SerializeField] protected Transform centerPoint;

        [Header("Contact Zones")]
        [SerializeField] protected Transform contactZoneStart;
        [SerializeField] protected Transform contactZoneEnd;

        [Header("Visual")]
        [SerializeField] protected GameObject[] activationVisuals;
        [SerializeField] protected ParticleSystem completionParticles;

        protected bool dogHasEntered;
        protected bool dogHasHitContactStart;
        protected bool dogHasHitContactEnd;
        protected bool dogInCommitZone;
        protected bool hasRunOut;
        protected int obstaclesCompleted;

        public ObstacleType ObstacleType => obstacleType;
        public bool IsActive => isActive;
        public bool DogHasEntered => dogHasEntered;
        public bool DogInCommitZone => dogInCommitZone;

        protected virtual void Awake()
        {
            AutoFindNavigationPoints();
        }

        protected void AutoFindNavigationPoints()
        {
            if (entryPoint == null)
            {
                var child = transform.Find("EntryPoint");
                if (child != null) entryPoint = child;
                else entryPoint = transform;
            }
            if (commitPoint == null)
            {
                var child = transform.Find("CommitPoint");
                if (child != null) commitPoint = child;
                else commitPoint = transform;
            }
            if (exitPoint == null)
            {
                var child = transform.Find("ExitPoint");
                if (child != null) exitPoint = child;
                else exitPoint = transform;
            }
            if (centerPoint == null) centerPoint = transform;
        }

        public virtual Vector3 GetEntryPoint()
        {
            return entryPoint != null ? entryPoint.position : transform.position;
        }

        public virtual Vector3 GetCommitPoint()
        {
            return commitPoint != null ? commitPoint.position : transform.position;
        }

        public virtual Vector3 GetExitPoint()
        {
            return exitPoint != null ? exitPoint.position : transform.position + transform.forward * 2f;
        }

        public virtual Vector3 GetCenterPoint()
        {
            return centerPoint != null ? centerPoint.position : transform.position;
        }

        public virtual float GetSpeedMultiplier()
        {
            return 1f;
        }

        public virtual void OnDogEntered(DogAgentController dog)
        {
            dogHasEntered = true;
            dogHasHitContactStart = false;
            dogHasHitContactEnd = false;
            hasRunOut = false;
            SetVisualsActive(true);
        }

        public virtual void OnDogExited(DogAgentController dog)
        {
            dogHasEntered = false;
            obstaclesCompleted++;
            PlayCompletionEffect();
        }

        public virtual void OnDogContactZoneEnter(DogAgentController dog, bool isStart)
        {
            if (isStart)
                dogHasHitContactStart = true;
            else
                dogHasHitContactEnd = true;
        }

        public virtual void OnDogEnteredCommitZone(DogAgentController dog)
        {
            dogInCommitZone = true;
        }

        public virtual void OnDogExitedCommitZone(DogAgentController dog)
        {
            if (!dogHasEntered)
            {
                // Dog left commit zone without taking the obstacle => Refusal
                GameEvents.RaiseFaultCommitted(FaultType.Refusal, obstacleType.ToString());
            }
            dogInCommitZone = false;
        }

        public virtual void TriggerRunOut()
        {
            if (!hasRunOut)
            {
                hasRunOut = true;
                GameEvents.RaiseFaultCommitted(FaultType.RunOut, obstacleType.ToString());
            }
        }

        public virtual bool CheckForFault(DogAgentController dog)
        {
            return false;
        }

        protected virtual void SetVisualsActive(bool active)
        {
            if (activationVisuals == null) return;
            foreach (var visual in activationVisuals)
            {
                if (visual != null) visual.SetActive(active);
            }
        }

        protected virtual void PlayCompletionEffect()
        {
            if (completionParticles != null)
            {
                completionParticles.Play();
            }
        }

        public virtual void ResetObstacle()
        {
            dogHasEntered = false;
            dogHasHitContactStart = false;
            dogHasHitContactEnd = false;
            dogInCommitZone = false;
            hasRunOut = false;
            SetVisualsActive(false);
        }

        public void SetActive(bool active)
        {
            isActive = active;
            gameObject.SetActive(active);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (entryPoint != null) Gizmos.DrawSphere(entryPoint.position, 0.15f);
            if (commitPoint != null) Gizmos.DrawSphere(commitPoint.position, 0.15f);
            if (exitPoint != null) Gizmos.DrawSphere(exitPoint.position, 0.15f);

            Gizmos.color = Color.blue;
            if (entryPoint != null && exitPoint != null)
            {
                Gizmos.DrawLine(entryPoint.position, exitPoint.position);
            }
        }
    }
}
