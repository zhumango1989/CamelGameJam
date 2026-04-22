using UnityEngine;
using System;
using GameJam.Core;

namespace GameJam.Input
{
    public class InputManager : Singleton<InputManager>
    {
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnJump;
        public event Action OnJumpReleased;
        public event Action OnSprint;
        public event Action OnSprintReleased;
        public event Action OnInteract;
        public event Action OnAttack;
        public event Action OnAttackReleased;
        public event Action OnSecondaryAttack;
        public event Action OnSecondaryAttackReleased;
        public event Action OnPause;
        public event Action OnInventory;
        public event Action<float> OnScroll;


        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 2f;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _isSprintHeld;
        private bool _isAttackHeld;

        public Vector2 MoveInput => _moveInput;
        public Vector2 LookInput => _lookInput;
        public bool IsSprintHeld => _isSprintHeld;
        public bool IsAttackHeld => _isAttackHeld;
        public float MouseSensitivity { get => mouseSensitivity; set => mouseSensitivity = value; }

        private void Update()
        {
            ReadMovementInput();
            ReadMouseInput();
            ReadActionInput();
        }

        private void ReadMovementInput()
        {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");

            _moveInput = new Vector2(horizontal, vertical);
            if (_moveInput.magnitude > 1f)
            {
                _moveInput.Normalize();
            }

            OnMove?.Invoke(_moveInput);
        }

        private void ReadMouseInput()
        {
            float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity;

            _lookInput = new Vector2(mouseX, mouseY);
            OnLook?.Invoke(_lookInput);

            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                OnScroll?.Invoke(scroll * 10f);
            }
        }

        private void ReadActionInput()
        {
            // Jump
            if (UnityEngine.Input.GetButtonDown("Jump"))
            {
                OnJump?.Invoke();
            }
            if (UnityEngine.Input.GetButtonUp("Jump"))
            {
                OnJumpReleased?.Invoke();
            }

            // Sprint
            bool sprintPressed = UnityEngine.Input.GetKey(KeyCode.LeftShift);
            if (sprintPressed && !_isSprintHeld)
            {
                _isSprintHeld = true;
                OnSprint?.Invoke();
            }
            else if (!sprintPressed && _isSprintHeld)
            {
                _isSprintHeld = false;
                OnSprintReleased?.Invoke();
            }

            // Interact
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                OnInteract?.Invoke();
            }

            // Attack (Left Mouse)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _isAttackHeld = true;
                OnAttack?.Invoke();
            }
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _isAttackHeld = false;
                OnAttackReleased?.Invoke();
            }

            // Secondary Attack (Right Mouse)
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                OnSecondaryAttack?.Invoke();
            }
            if (UnityEngine.Input.GetMouseButtonUp(1))
            {
                OnSecondaryAttackReleased?.Invoke();
            }

            // Pause
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                OnPause?.Invoke();
            }

            // Inventory
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) || UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                OnInventory?.Invoke();
            }
        }

        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
