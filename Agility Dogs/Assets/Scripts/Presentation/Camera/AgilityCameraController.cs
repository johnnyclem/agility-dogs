using UnityEngine;

namespace AgilityDogs.Presentation.Camera
{
    public class AgilityCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 2f, 0f);

        [Header("Following")]
        [SerializeField] private float followDistance = 6f;
        [SerializeField] private float followHeight = 3f;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float lookSpeed = 10f;

        [Header("Camera Modes")]
        [SerializeField] private float overviewDistance = 15f;
        [SerializeField] private float overviewHeight = 10f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.2f;
        [SerializeField] private float rotationSmoothTime = 0.15f;

        private CameraMode currentMode = CameraMode.Follow;
        private Vector3 smoothVelocity;
        private float currentDistance;
        private float currentHeight;
        private bool isFollowing = true;

        public enum CameraMode
        {
            Follow,
            Overview,
            SideOn,
            DogPOV,
            Cinematic,
            Free
        }

        public CameraMode CurrentMode => currentMode;

        private void LateUpdate()
        {
            if (target == null) return;

            switch (currentMode)
            {
                case CameraMode.Follow:
                    UpdateFollowCamera();
                    break;
                case CameraMode.Overview:
                    UpdateOverviewCamera();
                    break;
                case CameraMode.SideOn:
                    UpdateSideOnCamera();
                    break;
                case CameraMode.DogPOV:
                    UpdateDogPOV();
                    break;
                case CameraMode.Cinematic:
                    break;
                case CameraMode.Free:
                    break;
            }
        }

        private void UpdateFollowCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 desiredPosition = targetPos - target.forward * followDistance + Vector3.up * followHeight;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime
            );

            Vector3 lookTarget = targetPos + target.forward * 2f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * Time.deltaTime
            );
        }

        private void UpdateOverviewCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 desiredPosition = targetPos + Vector3.up * overviewHeight - target.forward * 2f;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime * 2f
            );

            Quaternion desiredRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * 0.5f * Time.deltaTime
            );
        }

        private void UpdateSideOnCamera()
        {
            Vector3 targetPos = target.position + targetOffset;
            Vector3 sideDir = Vector3.Cross(target.forward, Vector3.up).normalized;
            Vector3 desiredPosition = targetPos + sideDir * followDistance * 1.2f + Vector3.up * followHeight * 0.8f;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime
            );

            Quaternion desiredRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                lookSpeed * Time.deltaTime
            );
        }

        private void UpdateDogPOV()
        {
            if (target == null) return;
            transform.position = target.position + Vector3.up * 0.5f;
            transform.rotation = Quaternion.LookRotation(target.forward);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetMode(CameraMode mode)
        {
            currentMode = mode;
        }

        public void SetFollowParameters(float distance, float height, float speed)
        {
            followDistance = distance;
            followHeight = height;
            followSpeed = speed;
        }

        public void ToggleMode()
        {
            int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
            currentMode = (CameraMode)nextMode;
        }

        public void SetCinematicPath(Transform[] waypoints, float duration)
        {
            currentMode = CameraMode.Cinematic;
        }
    }
}
