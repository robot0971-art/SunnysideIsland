using System.Collections.Generic;
using UnityEngine;

namespace SunnysideIsland.Animal
{
    public class DuckAI : MonoBehaviour
    {
        [Header("=== Duck Settings ===")]
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _wanderRadius = 15f;
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
            
            // ?뚮젅?댁뼱 李얘린
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }
        
        private void Update()
        {
            // 臾?諛뽰뿉 ?덉쑝硫?利됱떆 臾쇰줈 ?뚯븘媛湲?
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
            
            // ?뚮젅?댁뼱 媛먯?
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
            // ?뚮젅?댁뼱 媛먯?
            if (DetectPlayer() && _playerTransform != null)
            {
                ChangeState(DuckState.Flee);
                return;
            }
            
            // 紐⑺몴媛 臾?諛뽰씠硫???紐⑺몴 ?ㅼ젙
            if (!IsOnSea(_wanderTarget))
            {
                SetRandomWanderTargetOnSea();
                if (_wanderTarget == Vector3.zero)
                {
                    ChangeState(DuckState.Idle);
                    return;
                }
            }
            
            // 紐⑺몴???꾨떖?덉쑝硫?Idle濡?
            if (Vector3.Distance(transform.position, _wanderTarget) < 0.3f)
            {
                ChangeState(DuckState.Idle);
                return;
            }
            
            // ?대룞
            Vector3 direction = (_wanderTarget - transform.position).normalized;
            Vector3 nextPosition = transform.position + direction * _moveSpeed * Time.deltaTime;
            
            // ?ㅼ쓬 ?꾩튂媛 臾??꾩씤吏 ?뺤씤
            if (!IsOnSea(nextPosition))
            {
                // 臾?諛뽰쑝濡?媛?ㅽ븯硫???紐⑺몴 ?ㅼ젙
                SetRandomWanderTargetOnSea();
                return;
            }
            
            transform.position = nextPosition;
            
            // ?ㅽ봽?쇱씠??諛⑺뼢
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
            
            // ?꾨쭩 嫄곕━ 踰쀬뼱?섎㈃ Idle濡?
            if (distance > _fleeRange * 1.5f)
            {
                ChangeState(DuckState.Idle);
                return;
            }
            
            // ?뚮젅?댁뼱 諛섎? 諛⑺뼢?쇰줈 ?꾨쭩
            Vector3 fleeDirection = (transform.position - _playerTransform.position).normalized;
            Vector3 nextPosition = transform.position + fleeDirection * _moveSpeed * 1.5f * Time.deltaTime;
            
            // ?꾨쭩???꾩튂媛 臾??꾩씤吏 ?뺤씤
            if (!IsOnSea(nextPosition))
            {
                // 臾?諛뽰쑝濡??꾨쭩媛???섎㈃ Idle濡?(?꾨쭩 ?ш린)
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
            // 臾??꾩뿉?쒕쭔 ?쒕뜡 ?꾩튂 李얘린
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
            
            // ?ㅽ뙣?섎㈃ ?꾩옱 ?꾩튂 ?좎?
            _wanderTarget = Vector3.zero;
        }
        
        private void ReturnToWater()
        {
            // 媛??媛源뚯슫 臾쇰줈 ?대룞
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
