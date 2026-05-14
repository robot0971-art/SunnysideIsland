using UnityEngine;
using UnityEngine.Tilemaps;
using DI;
using SunnysideIsland.Events;
using SunnysideIsland.Weather;
using SunnysideIsland.GameData;

namespace SunnysideIsland.Animal
{
    public enum AnimalState
    {
        Idle,
        Wander,
        Flee
    }

    public abstract class AnimalBaseAI : MonoBehaviour
    {
        [Header("=== Movement Settings ===")]
        [SerializeField] protected float _moveSpeed = 2f;
        [SerializeField] protected float _wanderRadius = 15f;
        [SerializeField] protected float _wanderInterval = 3f;
        [SerializeField] protected float _idleTime = 2f;
        
        [Header("=== Flee Settings ===")]
        [SerializeField] protected float _fleeRange = 3f;
        [SerializeField] protected float _fleeSpeed = 4f;
        [SerializeField] protected LayerMask _playerLayer;
        
        [Header("=== Ground Check ===")]
        [SerializeField] protected LayerMask _groundLayer;
        
        protected Tilemap _groundTilemap;
        
        [Header("=== Breeding Settings ===")]
        [SerializeField] protected bool _isBaby = false;
        [SerializeField] protected float _babyScale = 0.5f;
        [SerializeField] protected float _growthDuration = 120f;
        [SerializeField] protected float _breedInterval = 60f;
        [SerializeField] [Range(0f, 1f)] protected float _breedChance = 0.3f;
        [SerializeField] protected int _maxBabiesPerArea = 3;
        [SerializeField] protected float _babySpeedMultiplier = 0.8f;
        
        protected float _growthTimer;
        protected float _breedTimer;
        protected float _originalMoveSpeed;
        protected Vector3 _originalScale;
        
        protected bool CanBreed => !_isBaby && _breedTimer <= 0f;
        public bool IsBaby => _isBaby;
        public bool CanProvideHarvestProducts => !_isBaby;
        
        [Header("=== Weather Settings ===")]
        [SerializeField] protected float _rainSpeedMultiplier = 0.8f;
        protected bool _isRaining = false;

        [Inject(Optional = true)]
        protected WeatherSystem _weatherSystem = default!;
        
        [Header("=== Components ===")]
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected Animator _animator;
        private readonly System.Collections.Generic.HashSet<int> _animatorParameterHashes = new System.Collections.Generic.HashSet<int>();
        private static readonly int AnimState = Animator.StringToHash("State");
        
        protected Vector3 _spawnPosition;
        protected Vector3 _wanderTarget;
        protected float _wanderTimer;
        protected float _idleTimer;
        protected Transform _playerTransform;
        protected AnimalState _currentState = AnimalState.Idle;
        
        protected virtual void Awake()
        {
            _spawnPosition = transform.position;
            _originalScale = transform.localScale;
            // _originalMoveSpeed??Start?êÏÑú ?§Ï†ï (?òÏúÑ ?¥Îûò??AwakeÎ≥¥Îã§ ?òÏ§ë??
            
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            CacheAnimatorParameters();
            if (_playerLayer == 0)
                _playerLayer = LayerMask.GetMask("Player");
            
            // Ground Tilemap Ï∞æÍ∏∞
            var groundObj = GameObject.Find("Grass");
            if (groundObj != null)
                _groundTilemap = groundObj.GetComponent<Tilemap>();
            
            // ?àÎÅº Ï¥àÍ∏∞??
            InitializeBaby();
        }
        
        public virtual void InitializeBaby()
        {
            if (_isBaby)
            {
                _growthTimer = _growthDuration;
                transform.localScale = _originalScale * _babyScale;
                _moveSpeed = _originalMoveSpeed * _babySpeedMultiplier;
            }
        }
        
        protected virtual void Start()
        {
            DIContainer.Inject(this);
            _spawnPosition = transform.position;
            _idleTimer = _idleTime;
            
            // ?êÎûò ?çÎèÑ ?Ä??(?òÏúÑ ?¥Îûò??Awake ?¥ÌõÑ)
            _originalMoveSpeed = _moveSpeed;
            
            // ?åÎ†à?¥Ïñ¥ Ï∞æÍ∏∞
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
            
            // ?†Ïî® ?¥Î≤§??Íµ¨ÎèÖ
            EventBus.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            
            // ?ÑÏû¨ ?†Ïî® ?ïÏù∏
            CheckCurrentWeather();
                
        }
        
        protected virtual void OnDestroy()
        {
            EventBus.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
        }
        
        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            UpdateWeatherEffect(evt.CurrentWeather);
        }
        
        private void CheckCurrentWeather()
        {
            // ?ÑÏû¨ ?¨Ïóê??WeatherSystem Ï∞æÍ∏∞
            if (_weatherSystem == null)
            {
                DIContainer.TryResolve(out _weatherSystem);
            }

            if (_weatherSystem != null)
            {
                UpdateWeatherEffect(_weatherSystem.CurrentWeather);
            }
        }
        
        private void UpdateWeatherEffect(WeatherType weather)
        {
            bool wasRaining = _isRaining;
            _isRaining = weather == WeatherType.Rainy || weather == WeatherType.Stormy;
            
            if (_isRaining && !wasRaining)
            {
                // Îπ??úÏûë - ?çÎèÑ Í∞êÏÜå
                _moveSpeed = _originalMoveSpeed * _rainSpeedMultiplier;
            }
            else if (!_isRaining && wasRaining)
            {
                // Îπ?Ï¢ÖÎ£å - ?çÎèÑ Î≥µÏõê
                _moveSpeed = _originalMoveSpeed;
            }
        }
        
        protected virtual void Update()
        {
            // Grass Î∞ñÏóê ?àÏúºÎ©?Ï¶âÏãú GrassÎ°??åÏïÑÍ∞ÄÍ∏?
            if (!IsOnGround(transform.position))
            {
                ReturnToGround();
                return;
            }
            
            // ?±Ïû• Ï≤òÎ¶¨
            UpdateGrowth();
            
            // Î≤àÏãù ?Ä?¥Î®∏ ?ÖÎç∞?¥Ìä∏
            if (_breedTimer > 0f)
            {
                _breedTimer -= Time.deltaTime;
            }
            
            // ?òÏúÑ ?¥Îûò?§Ïóê??Ï∂îÍ? Î°úÏßÅ ?§Ìñâ
            UpdateState();
            
            switch (_currentState)
            {
                case AnimalState.Idle:
                    UpdateIdle();
                    break;
                case AnimalState.Wander:
                    UpdateWander();
                    break;
                case AnimalState.Flee:
                    UpdateFlee();
                    break;
            }
        }
        
        protected virtual void UpdateGrowth()
        {
            if (_isBaby && _growthTimer > 0f)
            {
                _growthTimer -= Time.deltaTime;
                if (_growthTimer <= 0f)
                {
                    GrowUp();
                }
            }
        }
        
        protected virtual void GrowUp()
        {
            _isBaby = false;
            transform.localScale = _originalScale;
            _moveSpeed = _originalMoveSpeed;
        }
        
        protected virtual void UpdateState()
        {
            // ?òÏúÑ ?¥Îûò?§Ïóê???§Î≤Ñ?ºÏù¥?úÌïò??Ï∂îÍ? Î°úÏßÅ Íµ¨ÌòÑ
        }
        
        protected virtual void UpdateIdle()
        {
            _idleTimer -= Time.deltaTime;
            
            if (DetectPlayer())
            {
                ChangeState(AnimalState.Flee);
                return;
            }
            
            // Î≤àÏãù ?úÎèÑ
            TryBreed();
            
            if (_idleTimer <= 0f)
            {
                ChangeState(AnimalState.Wander);
            }
        }
        
        protected virtual void UpdateWander()
        {
            if (DetectPlayer())
            {
                ChangeState(AnimalState.Flee);
                return;
            }
            
            // Î™©ÌëúÍ∞Ä Grass Î∞ñÏù¥Î©???Î™©Ìëú ?§Ï†ï
            if (!IsOnGround(_wanderTarget))
            {
                SetRandomWanderTargetOnGround();
                if (_wanderTarget == Vector3.zero)
                {
                    ChangeState(AnimalState.Idle);
                    return;
                }
            }
            
            // Î™©Ìëú???ÑÎã¨?àÏúºÎ©?IdleÎ°?
            if (Vector3.Distance(transform.position, _wanderTarget) < 0.3f)
            {
                ChangeState(AnimalState.Idle);
                return;
            }
            
            // ?¥Îèô
            Vector3 direction = (_wanderTarget - transform.position).normalized;
            Vector3 nextPosition = transform.position + direction * _moveSpeed * Time.deltaTime;
            
            // ?§Ïùå ?ÑÏπòÍ∞Ä Grass ?ÑÏù∏ÏßÄ ?ïÏù∏
            if (!IsOnGround(nextPosition))
            {
                SetRandomWanderTargetOnGround();
                return;
            }
            
            transform.position = nextPosition;
            
            // ?§ÌîÑ?ºÏù¥??Î∞©Ìñ•
            if (_spriteRenderer != null && direction.x != 0)
            {
                _spriteRenderer.flipX = direction.x > 0;
            }
        }
        
        protected virtual void UpdateFlee()
        {
            if (_playerTransform == null)
            {
                ChangeState(AnimalState.Idle);
                return;
            }
            
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            
            if (distance > _fleeRange * 1.5f)
            {
                ChangeState(AnimalState.Idle);
                return;
            }
            
            Vector3 fleeDirection = (transform.position - _playerTransform.position).normalized;
            Vector3 nextPosition = transform.position + fleeDirection * _fleeSpeed * Time.deltaTime;
            
            if (!IsOnGround(nextPosition))
            {
                ChangeState(AnimalState.Idle);
                return;
            }
            
            transform.position = nextPosition;
            
            if (_spriteRenderer != null && fleeDirection.x != 0)
            {
                _spriteRenderer.flipX = fleeDirection.x > 0;
            }
        }
        
        protected virtual void ChangeState(AnimalState newState)
        {
            _currentState = newState;
            
            switch (newState)
            {
                case AnimalState.Idle:
                    _idleTimer = Random.Range(_idleTime * 0.5f, _idleTime * 1.5f);
                    UpdateAnimatorState(0);
                    break;
                case AnimalState.Wander:
                    SetRandomWanderTargetOnGround();
                    UpdateAnimatorState(1);
                    break;
                case AnimalState.Flee:
                    UpdateAnimatorState(1);
                    break;
            }
            
            OnEnterState(newState);
        }
        
        protected virtual void OnEnterState(AnimalState state)
        {
            // ?òÏúÑ ?¥Îûò?§Ïóê???§Î≤Ñ?ºÏù¥?úÌïò???ÅÌÉú ÏßÑÏûÖ ??Ï∂îÍ? Î°úÏßÅ Íµ¨ÌòÑ
        }
        
        protected virtual void SetRandomWanderTargetOnGround()
        {
            for (int i = 0; i < 30; i++)
            {
                float randomX = (Random.value * 2f - 1f) * _wanderRadius;
                float randomY = (Random.value * 2f - 1f) * _wanderRadius;
                Vector3 candidatePos = _spawnPosition + new Vector3(randomX, randomY, 0);
                
                if (IsOnGround(candidatePos))
                {
                    _wanderTarget = candidatePos;
                    return;
                }
            }
            
            _wanderTarget = Vector3.zero;
        }
        
        protected virtual void ReturnToGround()
        {
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
                    
                    if (IsOnGround(checkPos))
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
        
        protected virtual bool IsOnGround(Vector3 position)
        {
            // Tilemap?ºÎ°ú ÏßÅÏ†ë Ï≤¥ÌÅ¨ (Collider ?ÜÏù¥???ëÎèô)
            if (_groundTilemap != null)
            {
                var cellPos = _groundTilemap.WorldToCell(position);
                return _groundTilemap.GetTile(cellPos) != null;
            }
            
            // ?¥Î∞±: LayerMask Í∏∞Î∞ò Ï≤¥ÌÅ¨
            if (_groundLayer == 0) 
            {
                return true;
            }
            
            return Physics2D.OverlapCircle(position, 0.3f, _groundLayer) != null;
        }
        
        protected virtual bool DetectPlayer()
        {
            if (_playerTransform == null) return false;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            return distance < _fleeRange;
        }
        
        protected virtual void TryBreed()
        {
            if (!CanBreed) return;
            
            // Î≤àÏãù ?ïÎ•† Ï≤¥ÌÅ¨
            if (Random.value > _breedChance) return;
            
            // ÏßÄ??ãπ ?àÎÅº ???úÌïú Ï≤¥ÌÅ¨
            int babyCount = CountBabiesInArea();
            if (babyCount >= _maxBabiesPerArea) return;
            
            // ?àÎÅº ?ùÏÑ±
            SpawnBaby();
            
            // Î≤àÏãù Ïø®Ì????§Ï†ï
            _breedTimer = _breedInterval;
        }
        
        protected virtual int CountBabiesInArea()
        {
            var animals = FindObjectsByType<AnimalBaseAI>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var animal in animals)
            {
                if (animal._isBaby && Vector3.Distance(transform.position, animal.transform.position) < _wanderRadius * 2f)
                {
                    count++;
                }
            }
            return count;
        }
        
        protected virtual void SpawnBaby()
        {
            // Î∂ÄÎ™?Í∑ºÏ≤ò?êÏÑú Grass ?ÑÎ? Ï∞æÏïÑ ?àÎÅº ?ùÏÑ±
            Vector3 spawnPos = FindValidBabySpawnPosition();
            if (spawnPos == Vector3.zero) return;
            
            var baby = Instantiate(gameObject, spawnPos, Quaternion.identity);
            baby.name = $"{gameObject.name}_Baby";
            
            var babyAI = baby.GetComponent<AnimalBaseAI>();
            if (babyAI != null)
            {
                babyAI._isBaby = true;
                babyAI.InitializeBaby();
            }
            
        }
        
        protected virtual Vector3 FindValidBabySpawnPosition()
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = Random.value * 360f * Mathf.Deg2Rad;
                float distance = Random.Range(1f, 3f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
                Vector3 candidatePos = transform.position + offset;
                
                if (IsOnGround(candidatePos))
                {
                    return candidatePos;
                }
            }
            return Vector3.zero;
        }
        
        protected virtual void UpdateAnimatorState(int state)
        {
            if (_animator != null && _animatorParameterHashes.Contains(AnimState))
            {
                _animator.SetInteger(AnimState, state);
            }
        }

        private void CacheAnimatorParameters()
        {
            _animatorParameterHashes.Clear();

            if (_animator == null)
            {
                return;
            }

            foreach (var parameter in _animator.parameters)
            {
                _animatorParameterHashes.Add(parameter.nameHash);
            }
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _wanderRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _fleeRange);
        }
    }
}
