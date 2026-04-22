using UnityEngine;
using GameJam.Core;

namespace GameJam.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float attackDamage = 10f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float detectionRange = 10f;

        protected float currentHealth;
        protected Transform target;
        protected bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            FindTarget();
        }

        protected virtual void Update()
        {
            if (isDead) return;
            UpdateBehavior();
        }

        protected abstract void UpdateBehavior();

        protected virtual void FindTarget()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isDead = true;
            EventSystem.Emit(GameEvents.EnemyKilled, this);
            OnDeath();
        }

        protected virtual void OnDeath()
        {
            Destroy(gameObject, 0.1f);
        }

        protected bool IsTargetInRange(float range)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= range;
        }

        protected Vector3 GetDirectionToTarget()
        {
            if (target == null) return Vector3.zero;
            return (target.position - transform.position).normalized;
        }

        protected float GetDistanceToTarget()
        {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(transform.position, target.position);
        }

        protected virtual void Attack()
        {
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    public class SimpleEnemy : EnemyBase
    {
        public enum EnemyState { Idle, Chase, Attack }

        [Header("AI")]
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float rotationSpeed = 5f;

        private EnemyState currentState = EnemyState.Idle;
        private float lastAttackTime;
        private CharacterController controller;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<CharacterController>();
        }

        protected override void UpdateBehavior()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    IdleUpdate();
                    break;
                case EnemyState.Chase:
                    ChaseUpdate();
                    break;
                case EnemyState.Attack:
                    AttackUpdate();
                    break;
            }
        }

        private void IdleUpdate()
        {
            if (IsTargetInRange(detectionRange))
            {
                currentState = EnemyState.Chase;
            }
        }

        private void ChaseUpdate()
        {
            if (!IsTargetInRange(detectionRange))
            {
                currentState = EnemyState.Idle;
                return;
            }

            if (IsTargetInRange(attackRange))
            {
                currentState = EnemyState.Attack;
                return;
            }

            MoveTowardsTarget();
        }

        private void AttackUpdate()
        {
            if (!IsTargetInRange(attackRange * 1.2f))
            {
                currentState = EnemyState.Chase;
                return;
            }

            LookAtTarget();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        private void MoveTowardsTarget()
        {
            if (target == null) return;

            Vector3 direction = GetDirectionToTarget();
            direction.y = 0;

            LookAtTarget();

            if (controller != null)
            {
                controller.Move(direction * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }

        private void LookAtTarget()
        {
            if (target == null) return;

            Vector3 direction = GetDirectionToTarget();
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        protected override void Attack()
        {
            Debug.Log($"{name} attacks for {attackDamage} damage!");
        }
    }
}
