using UnityEngine;

namespace GameJam.CameraSystem
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 0.6f, 0);

        [Header("Rotation")]
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;
        [SerializeField] private float smoothTime = 0.03f;

        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobAmplitude = 0.05f;

        private float _yaw;
        private float _pitch;
        private float _yawVelocity;
        private float _pitchVelocity;
        private float _targetYaw;
        private float _targetPitch;

        private float _bobTimer;
        private Vector3 _bobOffset;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _yaw = _targetYaw = angles.y;
            _pitch = _targetPitch = angles.x;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void Rotate(Vector2 input)
        {
            _targetYaw += input.x * sensitivity;
            _targetPitch -= input.y * sensitivity;
            _targetPitch = Mathf.Clamp(_targetPitch, minVerticalAngle, maxVerticalAngle);
        }

        public void UpdateHeadBob(float speed, bool isGrounded)
        {
            if (!enableHeadBob || !isGrounded || speed < 0.1f)
            {
                _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * 10f);
                return;
            }

            _bobTimer += Time.deltaTime * bobFrequency * speed;
            float bobX = Mathf.Sin(_bobTimer) * bobAmplitude * 0.5f;
            float bobY = Mathf.Abs(Mathf.Sin(_bobTimer)) * bobAmplitude;
            _bobOffset = new Vector3(bobX, bobY, 0);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            _yaw = Mathf.SmoothDamp(_yaw, _targetYaw, ref _yawVelocity, smoothTime);
            _pitch = Mathf.SmoothDamp(_pitch, _targetPitch, ref _pitchVelocity, smoothTime);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
            transform.position = target.position + offset + _bobOffset;

            if (target.TryGetComponent<CharacterController>(out _))
            {
                target.rotation = Quaternion.Euler(0, _yaw, 0);
            }
        }

        public float GetYaw() => _yaw;
        public float GetPitch() => _pitch;

        public void SetRotation(float yaw, float pitch)
        {
            _yaw = _targetYaw = yaw;
            _pitch = _targetPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }
}
