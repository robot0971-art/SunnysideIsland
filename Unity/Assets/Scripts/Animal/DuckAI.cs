using System.Collections.Generic;
using UnityEngine;

namespace SunnysideIsland.Animal
{
    public class DuckAI : MonoBehaviour
    {
        [Header("=== Duck Settings ===")]
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _wanderRadius = 15f;
        [SerializeField] private float _wanderInterval = 3f;
        [SerializeField] private float _idleTime = 2f;
        [SerializeField] private LayerMask _seaLayer;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        
        [Header("=== Flee Settings ===")]
        [SerializeField] private float _fleeRange = 3f;
        [SerializeField] private LayerMask _playerLayer;
        
        private Vector3 _spawnPosition;
        private Vector3 _wanderTarget;
        private float _wanderTimer;
        private float _idleTimer;
        private Transform _playerTransform;
        private bool _isFleeing;
        
        private enum DuckState { Idle, Wander, Flee }
        private DuckState _currentState = DuckState.Idle;
        
        private void Awake()
        {
            _spawnPosition = transform.position;
            
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            if (_seaLayer == 0)
                _seaLayer = LayerMask.GetMask("Sea");
            if (_playerLayer == 0)
                _playerLayer = LayerMask.GetMask("Player");
        }
        
        private void Start()
        {
            _spawnPosition = transform.position;
            _idleTimer = _idleTime;
            
            // 플레이어 찾기
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }
        
        private void Update()
        {
            // 물 밖에 있으면 즉시 물로 돌아가기
            if (!IsOnSea(transform.position))
            {
                ReturnToWater();
                return;
            }
            
            switch (_currentState)
            {
                case DuckState.Idle:
                    UpdateIdle();
                    break;
                case DuckState.Wander:
                    UpdateWander();
                    break;
                case DuckState.Flee:
                    UpdateFlee();
                    break;
            }
        }
        
        private void UpdateIdle()
        {
            _idleTimer -= Time.deltaTime;
            
            // 플레이어 감지
            if (DetectPlayer() && _playerTransform != null)
            {
                ChangeState(DuckState.Flee);
                return;
            }
            
            if (_idleTimer <= 0f)
            {
                ChangeState(DuckState.Wander);
            }
        }
        
        private void UpdateWander()
        {
            // 플레이어 감지
            if (DetectPlayer() && _playerTransform != null)
            {
                ChangeState(DuckState.Flee);
                return;
            }
            
            // 목표가 물 밖이면 새 목표 설정
            if (!IsOnSea(_wanderTarget))
            {
                SetRandomWanderTargetOnSea();
                if (_wanderTarget == Vector3.zero)
                {
                    ChangeState(DuckState.Idle);
                    return;
                }
            }
            
            // 목표에 도달했으면 Idle로
            if (Vector3.Distance(transform.position, _wanderTarget) < 0.3f)
            {
                ChangeState(DuckState.Idle);
                return;
            }
            
            // 이동
            Vector3 direction = (_wanderTarget - transform.position).normalized;
            Vector3 nextPosition = transform.position + direction * _moveSpeed * Time.deltaTime;
            
            // 다음 위치가 물 위인지 확인
            if (!IsOnSea(nextPosition))
            {
                // 물 밖으로 가려하면 새 목표 설정
                SetRandomWanderTargetOnSea();
                return;
            }
            
            transform.position = nextPosition;
            
            // 스프라이트 방향
            if (_spriteRenderer != null && direction.x != 0)
            {
                _spriteRenderer.flipX = direction.x > 0;
            }
        }
        
        private void UpdateFlee()
        {
            if (_playerTransform == null)
            {
                ChangeState(DuckState.Idle);
                return;
            }
            
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            
            // 도망 거리 벗어나면 Idle로
            if (distance > _fleeRange * 1.5f)
            {
                ChangeState(DuckState.Idle);
                return;
            }
            
            // 플레이어 반대 방향으로 도망
            Vector3 fleeDirection = (transform.position - _playerTransform.position).normalized;
            Vector3 nextPosition = transform.position + fleeDirection * _moveSpeed * 1.5f * Time.deltaTime;
            
            // 도망할 위치가 물 위인지 확인
            if (!IsOnSea(nextPosition))
            {
                // 물 밖으로 도망가려 하면 Idle로 (도망 포기)
                ChangeState(DuckState.Idle);
                return;
            }
            
            transform.position = nextPosition;
            
            if (_spriteRenderer != null && fleeDirection.x != 0)
            {
                _spriteRenderer.flipX = fleeDirection.x > 0;
            }
        }
        
        private void ChangeState(DuckState newState)
        {
            _currentState = newState;
            
            switch (newState)
            {
                case DuckState.Idle:
                    _idleTimer = Random.Range(_idleTime * 0.5f, _idleTime * 1.5f);
                    UpdateAnimatorState(0); // Idle animation
                    break;
                case DuckState.Wander:
                    SetRandomWanderTargetOnSea();
                    UpdateAnimatorState(1); // Swim/Walk animation
                    break;
                case DuckState.Flee:
                    UpdateAnimatorState(1); // Swim/Walk animation
                    break;
            }
        }
        
        private void SetRandomWanderTargetOnSea()
        {
            // 물 위에서만 랜덤 위치 찾기
            for (int i = 0; i < 30; i++)
            {
                float randomX = (Random.value * 2f - 1f) * _wanderRadius;
                float randomY = (Random.value * 2f - 1f) * _wanderRadius;
                Vector3 candidatePos = _spawnPosition + new Vector3(randomX, randomY, 0);
                
                if (IsOnSea(candidatePos))
                {
                    _wanderTarget = candidatePos;
                    return;
                }
            }
            
            // 실패하면 현재 위치 유지
            _wanderTarget = Vector3.zero;
        }
        
        private void ReturnToWater()
        {
            // 가장 가까운 물로 이동
            for (float radius = 1f; radius <= 10f; radius += 1f)
            {
                for (int angle = 0; angle < 360; angle += 30)
                {
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 checkPos = transform.position + new Vector3(
                        Mathf.Cos(rad) * radius, 
                        Mathf.Sin(rad) * radius, 
                        0
                    );
                    
                    if (IsOnSea(checkPos))
                    {
                        transform.position = Vector3.MoveTowards(
                            transform.position, 
                            checkPos, 
                            _moveSpeed * 2f * Time.deltaTime
                        );
                        return;
                    }
                }
            }
        }
        
        private bool IsOnSea(Vector3 position)
        {
            if (_seaLayer == 0) return true;
            return Physics2D.OverlapPoint(position, _seaLayer) != null;
        }
        
        private bool DetectPlayer()
        {
            if (_playerTransform == null) return false;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            return distance < _fleeRange;
        }
        
        private void UpdateAnimatorState(int state)
        {
            if (_animator != null)
            {
                _animator.SetInteger("State", state);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _wanderRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _fleeRange);
        }
    }
}
