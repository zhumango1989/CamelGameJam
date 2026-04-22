using UnityEngine;

namespace GameJam.CameraSystem
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        [Header("Distance")]
        [SerializeField] private float defaultDistance = 5f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float zoomSmoothTime = 0.1f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("Collision")]
        [SerializeField] private bool enableCollision = true;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float collisionSmoothTime = 0.05f;

        private float _currentDistance;
        private float _targetDistance;
        private float _distanceVelocity;

        private float _yaw;
        private float _pitch;
        private float _yawVelocity;
        private float _pitchVelocity;
        private float _targetYaw;
        private float _targetPitch;

        private Vector3 _currentPosition;
        private Vector3 _positionVelocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            _currentDistance = defaultDistance;
            _targetDistance = defaultDistance;

            if (target != null)
            {
                Vector3 angles = transform.eulerAngles;
                _yaw = _targetYaw = angles.y;
                _pitch = _targetPitch = angles.x;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void Rotate(Vector2 input)
        {
            _targetYaw += input.x * rotationSpeed;
            _targetPitch -= input.y * rotationSpeed;
            _targetPitch = Mathf.Clamp(_targetPitch, minVerticalAngle, maxVerticalAngle);
        }

        public void Zoom(float delta)
        {
            _targetDistance -= delta * zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            _yaw = Mathf.SmoothDamp(_yaw, _targetYaw, ref _yawVelocity, rotationSmoothTime);
            _pitch = Mathf.SmoothDamp(_pitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);

            _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _distanceVelocity, zoomSmoothTime);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 targetPosition = target.position + targetOffset;

            float actualDistance = _currentDistance;
            if (enableCollision)
            {
                actualDistance = CheckCollision(targetPosition, rotation, _currentDistance);
            }

            Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * actualDistance;
            _currentPosition = Vector3.SmoothDamp(_currentPosition, desiredPosition, ref _positionVelocity, collisionSmoothTime);

            transform.position = _currentPosition;
            transform.LookAt(targetPosition);
        }

        private float CheckCollision(Vector3 targetPosition, Quaternion rotation, float distance)
        {
            Vector3 direction = rotation * Vector3.back;

            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out RaycastHit hit, distance, collisionMask))
            {
                return hit.distance - collisionRadius;
            }

            return distance;
        }

        public void SetRotation(float yaw, float pitch)
        {
            _yaw = _targetYaw = yaw;
            _pitch = _targetPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }

        public void SetDistance(float distance)
        {
            _currentDistance = _targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        public void SnapToTarget()
        {
            if (target == null) return;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 targetPosition = target.position + targetOffset;
            _currentPosition = targetPosition - rotation * Vector3.forward * _currentDistance;
            transform.position = _currentPosition;
            transform.LookAt(targetPosition);
        }
    }
}
