using UnityEngine;
using DI;
using SunnysideIsland.Events;
using SunnysideIsland.Animal;
using SunnysideIsland.Input;
using SunnysideIsland.Pool;
using SunnysideIsland.Inventory;
using SunnysideIsland.Core;
using SunnysideIsland.UI;

namespace SunnysideIsland.Items
{
    public class EggItem : PoolableObject
    {
        [Header("=== Egg Settings ===")]
        [SerializeField] private string _itemId = "egg";
        [SerializeField] private int _amount = 1;
        [SerializeField] private float _pickupRange = 1.5f;
        [SerializeField] private LayerMask _playerLayer;
        
        [Header("=== Hatching Settings ===")]
        [SerializeField] private int _hatchAfterDays = 1;
        [SerializeField] private GameObject _whiteChickenPrefab;
        [SerializeField] private GameObject _blackChickenPrefab;
        
        private Transform _playerTransform;
        private bool _canPickup = true;
        private EggPoint _parentEggPoint;
        private int _spawnDay = -1;
        private bool _isHatching = false;

        [Inject(Optional = true)]
        private TimeManager _timeManager = default!;

        [Inject(Optional = true)]
        private IInventorySystem _inventorySystem = default!;
        
        public string ItemId => _itemId;
        public int Amount => _amount;
        
        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            _canPickup = true;
            _playerTransform = null;
            _parentEggPoint = null;
            _spawnDay = -1;
            _isHatching = false;
            DIContainer.Inject(this);
            InitializeSpawnDay();
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            // EggPoint?êÍ≤å ?åÎ¶¨Í∏?
            if (_parentEggPoint != null)
            {
                _parentEggPoint.OnEggCollected();
            }
            UnsubscribeFromEvents();
        }
        
        public void SetParentEggPoint(EggPoint eggPoint)
        {
            _parentEggPoint = eggPoint;
        }
        
        private void Start()
        {
            DIContainer.Inject(this);
            if (_playerLayer == 0)
                _playerLayer = LayerMask.GetMask("Player");
            
            // Î∂Ä???úÏûë???§Ï†ï
            if (_spawnDay < 0)
            {
                if (_timeManager == null)
                {
                    DIContainer.TryResolve(out _timeManager);
                }

                if (_timeManager != null)
                {
                    _spawnDay = _timeManager.CurrentDay;
                }
            }
            
            SubscribeToEvents();
        }

        private void InitializeSpawnDay()
        {
            if (_spawnDay >= 0)
            {
                return;
            }

            if (_timeManager == null)
            {
                DIContainer.TryResolve(out _timeManager);
            }

            if (_timeManager != null)
            {
                _spawnDay = _timeManager.CurrentDay;
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }
        
        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }
        
        private void OnDayStarted(DayStartedEvent evt)
        {
            if (_isHatching) return;
            
            // ?∏Î≤§?†Î¶¨???àÏúºÎ©?Î∂Ä?îÌïòÏßÄ ?äÏùå
            if (transform.parent != null && transform.parent.name.Contains("Inventory")) return;
            
            // ?òÎ£®Í∞Ä ÏßÄ?¨ÎäîÏßÄ ?ïÏù∏
            if (_spawnDay >= 0 && evt.Day >= _spawnDay + _hatchAfterDays)
            {
                Hatch();
            }
        }
        
        private void Hatch()
        {
            _isHatching = true;
            _canPickup = false;
            
            Debug.Log($"[EggItem] Hatching at {transform.position}!");
            
            // ?∞Îã≠/ÍπåÎßå???úÎç§ ?†ÌÉù
            bool isWhite = Random.value > 0.5f;
            GameObject chickenPrefab = isWhite ? _whiteChickenPrefab : _blackChickenPrefab;
            
            // ?ÑÎ¶¨?πÏù¥ ?†Îãπ?òÏ? ?äÏïò?ºÎ©¥ Resources?êÏÑú Î°úÎìú
            if (chickenPrefab == null)
            {
                string prefabName = isWhite ? "Prefabs/Animals/Chicken_White" : "Prefabs/Animals/Chicken_Black";
                chickenPrefab = Resources.Load<GameObject>(prefabName);
            }
            
            if (chickenPrefab != null)
            {
                var chicken = Instantiate(chickenPrefab, transform.position, Quaternion.identity);
                
                // Î≥ëÏïÑÎ¶¨Î°ú Ï¥àÍ∏∞??
                var chickenAI = chicken.GetComponent<AnimalBaseAI>();
                if (chickenAI != null)
                {
                    // Î¶¨Ìîå?âÏÖò?ºÎ°ú private ?ÑÎìú ?§Ï†ï
                    var isBabyField = typeof(AnimalBaseAI).GetField("_isBaby", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (isBabyField != null)
                    {
                        isBabyField.SetValue(chickenAI, true);
                        chickenAI.InitializeBaby();
                    }
                }
                
                Debug.Log($"[EggItem] Spawned {(isWhite ? "White" : "Black")} chicken baby at {transform.position}!");
            }
            else
            {
                Debug.LogWarning("[EggItem] Chicken prefab not found!");
            }
            
            // Egg ?úÍ±∞ (PoolÎ°?Î∞òÌôò)
            if (_parentEggPoint != null)
            {
                _parentEggPoint.OnEggCollected();
            }
            ReturnToPool();
        }
        
        private void Update()
        {
            if (UIManager.Instance != null
                && UIManager.Instance.GetPanel<SunnysideIsland.UI.Menu.BoatConfirmPanel>()?.IsOpen == true)
            {
                return;
            }

            if (!_canPickup) return;
            
            // ?åÎ†à?¥Ïñ¥ Ï∞æÍ∏∞
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }
            
            // ?åÎ†à?¥Ïñ¥?Ä??Í±∞Î¶¨ Ï≤¥ÌÅ¨
            if (_playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _playerTransform.position);
                if (distance <= _pickupRange)
                {
                    // ?åÎ†à?¥Ïñ¥Í∞Ä E ?§Î? ?ÑÎ•¥Î©?Ï§çÍ∏∞
                    if (GameInput.GetKeyDown(KeyCode.E))
                    {
                        TryPickup();
                    }
                }
            }
        }
        
        private void TryPickup()
        {
            if (!_canPickup) return;
            
            // ?∏Î≤§?†Î¶¨ ?úÏä§??Ï∞æÍ∏∞
            if (_inventorySystem == null)
            {
                DIContainer.TryResolve(out _inventorySystem);
            }

            if (_inventorySystem != null)
            {
                bool added = _inventorySystem.AddItem(_itemId, _amount);
                if (added)
                {
                    // ?òÌôï ?†ÎãàÎ©îÏù¥???¨ÏÉù
                    if (_playerTransform != null)
                    {
                        var animator = _playerTransform.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger("Harvest");
                        }
                    }

                    // ?¥Î≤§??Î∞úÌñâ
                    EventBus.Publish(new ItemCollectedEvent 
                    { 
                        ItemId = _itemId, 
                        Amount = _amount,
                        Position = transform.position 
                    });
                    
                    // ?Ä??Î∞òÌôò
                    ReturnToPool();
                }
            }
            else
            {
                Debug.LogWarning("[EggItem] InventorySystem not found!");
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _playerLayer) != 0)
            {
                // ?åÎ†à?¥Ïñ¥ Í∑ºÏ≤ò???îÏùÑ ??UI ?úÏãú Í∞Ä??
                // ?? "EÎ•??åÎü¨ Ï§çÍ∏∞"
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
    
    public class ItemCollectedEvent
    {
        public string ItemId;
        public int Amount;
        public Vector3 Position;
    }
}
