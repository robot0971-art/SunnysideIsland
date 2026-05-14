using System.Collections;
using UnityEngine;

namespace SunnysideIsland.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        private static readonly int AnimAttack = Animator.StringToHash("Attack");

        private Animator _animator;
        private PlayerMovement _movement;
        private float _attackRange = 1.1f;
        private float _attackRadius = 0.45f;
        private float _attackCooldown = 0.45f;
        private float _attackHitDelay = 0.18f;
        private float _attackRecoverTime = 0.12f;
        private float _attackCooldownTimer;
        private Coroutine _attackCoroutine;

        public bool IsAttacking { get; private set; }
        public float AttackRange => _attackRange;
        public float AttackRadius => _attackRadius;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _movement = GetComponent<PlayerMovement>();
        }

        public void Configure(
            Animator animator,
            PlayerMovement movement,
            float attackRange,
            float attackRadius,
            float attackCooldown,
            float attackHitDelay,
            float attackRecoverTime)
        {
            _animator = animator != null ? animator : GetComponent<Animator>();
            _movement = movement != null ? movement : GetComponent<PlayerMovement>();
            _attackRange = attackRange;
            _attackRadius = attackRadius;
            _attackCooldown = attackCooldown;
            _attackHitDelay = attackHitDelay;
            _attackRecoverTime = attackRecoverTime;
        }

        public void TickTimers(float deltaTime)
        {
            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= deltaTime;
            }
        }

        public bool TryAttack(bool isBuilding, bool isDead)
        {
            if (!CanAttack(isBuilding, isDead))
            {
                return false;
            }

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackRoutine());
            return true;
        }

        public void CancelAttack()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            IsAttacking = false;
        }

        private bool CanAttack(bool isBuilding, bool isDead)
        {
            return _attackCooldownTimer <= 0f
                && _movement != null
                && !_movement.IsRolling
                && !_movement.IsSwimming
                && !IsAttacking
                && !isBuilding
                && !isDead;
        }

        private IEnumerator AttackRoutine()
        {
            IsAttacking = true;
            _attackCooldownTimer = _attackCooldown;
            _movement.Stop();
            _animator.SetTrigger(AnimAttack);

            yield return new WaitForSeconds(_attackHitDelay);
            PerformAttackHit();

            yield return new WaitForSeconds(_attackRecoverTime);
            IsAttacking = false;
            _attackCoroutine = null;
        }

        private void PerformAttackHit()
        {
            Vector2 facingDirection = _movement.FacingDirection;
            Vector2 direction = facingDirection == Vector2.zero ? Vector2.down : facingDirection.normalized;
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.1f);
            Vector2 hitCenter = origin + direction * _attackRange;
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, _attackRadius);

            float nearestDistance = float.MaxValue;
            Animal.PigHuntable nearestPig = null;

            foreach (Collider2D hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                Animal.PigHuntable pigHuntable = hit.GetComponentInParent<Animal.PigHuntable>();
                if (pigHuntable == null || !pigHuntable.IsAlive)
                {
                    continue;
                }

                float distance = Vector2.Distance(origin, hit.ClosestPoint(origin));
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPig = pigHuntable;
                }
            }

            nearestPig?.TryHit();
        }
    }
}
