using UnityEngine;
using GameJam.Character;
using GameJam.CameraSystem;
using GameJam.Input;
using GameJam.Core;

namespace GameJam.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController3D characterController;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private FirstPersonCamera firstPersonCamera;

        [Header("Settings")]
        [SerializeField] private bool useThirdPerson = true;
        [SerializeField] private float maxHealth = 100f;

        private float _currentHealth;
        private bool _isDead;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDead;

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController3D>();
            }
        }

        private void Start()
        {
            _currentHealth = maxHealth;
            SetupCamera();
            SubscribeToInput();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInput();
        }

        private void SetupCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return;

            if (useThirdPerson)
            {
                if (thirdPersonCamera == null)
                {
                    thirdPersonCamera = mainCamera.GetComponent<ThirdPersonCamera>();
                    if (thirdPersonCamera == null)
                    {
                        thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
                    }
                }
                thirdPersonCamera.SetTarget(transform);
                if (firstPersonCamera != null) firstPersonCamera.enabled = false;
            }
            else
            {
                if (firstPersonCamera == null)
                {
                    firstPersonCamera = mainCamera.GetComponent<FirstPersonCamera>();
                    if (firstPersonCamera == null)
                    {
                        firstPersonCamera = mainCamera.gameObject.AddComponent<FirstPersonCamera>();
                    }
                }
                firstPersonCamera.SetTarget(transform);
                if (thirdPersonCamera != null) thirdPersonCamera.enabled = false;
            }
        }

        private void SubscribeToInput()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            input.OnJump += HandleJump;
            input.OnSprint += HandleSprintStart;
            input.OnSprintReleased += HandleSprintEnd;
            input.OnScroll += HandleZoom;
        }

        private void UnsubscribeFromInput()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            input.OnJump -= HandleJump;
            input.OnSprint -= HandleSprintStart;
            input.OnSprintReleased -= HandleSprintEnd;
            input.OnScroll -= HandleZoom;
        }

        private void Update()
        {
            if (_isDead) return;

            HandleMovement();
            HandleCameraRotation();
        }

        private void HandleMovement()
        {
            var input = InputManager.Instance;
            if (input == null || characterController == null) return;

            characterController.Move(input.MoveInput, input.IsSprintHeld);
        }

        private void HandleCameraRotation()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            if (useThirdPerson && thirdPersonCamera != null)
            {
                thirdPersonCamera.Rotate(input.LookInput);
            }
            else if (!useThirdPerson && firstPersonCamera != null)
            {
                firstPersonCamera.Rotate(input.LookInput);
                firstPersonCamera.UpdateHeadBob(characterController.CurrentSpeed, characterController.IsGrounded);
            }
        }

        private void HandleJump()
        {
            characterController?.Jump();
        }

        private void HandleSprintStart()
        {
            characterController?.SetSprint(true);
        }

        private void HandleSprintEnd()
        {
            characterController?.SetSprint(false);
        }

        private void HandleZoom(float delta)
        {
            if (useThirdPerson && thirdPersonCamera != null)
            {
                thirdPersonCamera.Zoom(delta * 0.1f);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHealth -= damage;
            EventSystem.Emit(GameEvents.HealthChanged, _currentHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            EventSystem.Emit(GameEvents.HealthChanged, _currentHealth);
        }

        public void SetHealth(float health)
        {
            _currentHealth = Mathf.Clamp(health, 0, maxHealth);
            EventSystem.Emit(GameEvents.HealthChanged, _currentHealth);
        }

        private void Die()
        {
            _isDead = true;
            EventSystem.Emit(GameEvents.PlayerDeath);
        }

        public void Respawn(Vector3 position)
        {
            _isDead = false;
            _currentHealth = maxHealth;
            characterController.SetPosition(position);
            characterController.ResetVelocity();
            EventSystem.Emit(GameEvents.PlayerRespawn);
            EventSystem.Emit(GameEvents.HealthChanged, _currentHealth);
        }

        public void SwitchCameraMode()
        {
            useThirdPerson = !useThirdPerson;
            SetupCamera();
        }
    }
}
