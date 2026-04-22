using UnityEngine;

namespace GameJam.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterController3D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float acceleration = 10f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;
        [SerializeField] private LayerMask groundMask;

        private CharacterController _controller;
        private Vector3 _velocity;
        private Vector3 _moveDirection;
        private Vector3 _smoothMoveVelocity;

        private bool _isGrounded;
        private bool _isSprinting;
        private int _jumpCount;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private float _lastGroundedTime;

        private Transform _cameraTransform;

        public bool IsGrounded => _isGrounded;
        public bool IsSprinting => _isSprinting;
        public Vector3 Velocity => _velocity;
        public float CurrentSpeed => new Vector3(_velocity.x, 0, _velocity.z).magnitude;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            _cameraTransform = Camera.main?.transform;
            if (groundCheck == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, -1f, 0);
                groundCheck = go.transform;
            }
        }

        private void Update()
        {
            CheckGround();
            HandleTimers();
        }

        public void Move(Vector2 input, bool sprint = false)
        {
            _isSprinting = sprint && _isGrounded;
            float targetSpeed = _isSprinting ? sprintSpeed : moveSpeed;

            Vector3 direction = Vector3.zero;
            if (input.magnitude > 0.1f)
            {
                if (_cameraTransform != null)
                {
                    Vector3 forward = _cameraTransform.forward;
                    Vector3 right = _cameraTransform.right;
                    forward.y = 0f;
                    right.y = 0f;
                    forward.Normalize();
                    right.Normalize();
                    direction = (forward * input.y + right * input.x).normalized;
                }
                else
                {
                    direction = new Vector3(input.x, 0, input.y).normalized;
                }

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            Vector3 targetMoveDirection = direction * targetSpeed;
            _moveDirection = Vector3.SmoothDamp(_moveDirection, targetMoveDirection, ref _smoothMoveVelocity, 1f / acceleration);
        }

        public void Jump()
        {
            _jumpBufferCounter = jumpBufferTime;
        }

        public void SetSprint(bool sprint)
        {
            _isSprinting = sprint;
        }

        private void CheckGround()
        {
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (_isGrounded)
            {
                _jumpCount = 0;
                _coyoteTimeCounter = coyoteTime;
                _lastGroundedTime = Time.time;

                if (_velocity.y < 0)
                {
                    _velocity.y = -2f;
                }
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void HandleTimers()
        {
            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime;

                bool canJump = (_coyoteTimeCounter > 0 || _jumpCount < maxJumps);
                if (canJump)
                {
                    _velocity.y = jumpForce;
                    _jumpCount++;
                    _coyoteTimeCounter = 0;
                    _jumpBufferCounter = 0;
                }
            }
        }

        private void LateUpdate()
        {
            _velocity.y += gravity * Time.deltaTime;
            _velocity.x = _moveDirection.x;
            _velocity.z = _moveDirection.z;

            _controller.Move(_velocity * Time.deltaTime);
        }

        public void SetPosition(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
        }

        public void ResetVelocity()
        {
            _velocity = Vector3.zero;
            _moveDirection = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
