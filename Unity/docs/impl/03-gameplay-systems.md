# 3. 게임플레이 시스템 구현 계획

## 3.1 플레이어 시스템

### 구현 우선순위: P0 (필수)

#### PlayerController 구현
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("=== Movement Settings ===")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _rollDistance = 3f;
    [SerializeField] private float _rollDuration = 0.3f;
    
    [Header("=== References ===")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Inject] private IInputService _inputService;
    [Inject] private IStaminaSystem _staminaSystem;
    [Inject] private IEventBus _eventBus;
    
    private Vector2 _moveDirection;
    private bool _isRolling;
    private bool _isAttacking;
    
    private void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleInteraction();
    }
    
    private void HandleMovement();
    private void HandleCombat();
    private void HandleInteraction();
    private void PerformRoll();
}
```

#### PlayerStats 구현
```csharp
public class PlayerStats : MonoBehaviour, ISaveable
{
    [Header("=== Base Stats ===")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _maxStamina = 100;
    [SerializeField] private int _attackPower = 10;
    [SerializeField] private int _defense = 0;
    
    public int CurrentHealth { get; private set; }
    public int CurrentStamina { get; private set; }
    public int Level { get; private set; }
    public int Experience { get; private set; }
    
    public void TakeDamage(int damage);
    public void Heal(int amount);
    public void UseStamina(int amount);
    public void RecoverStamina(float amount);
    public void GainExperience(int xp);
}
```

#### 플레이어 애니메이션
- Idle: 4방향
- Walk: 4방향
- Roll: 4방향
- Attack: 4방향
- Hurt
- Die

### 예상 개발 시간: 2일

---

## 3.2 생존 시스템

### 구현 우선순위: P0 (필수)

#### HungerSystem 구현
```csharp
public class HungerSystem : MonoBehaviour
{
    [Header("=== Hunger Settings ===")]
    [SerializeField] private float _maxHunger = 100f;
    [SerializeField] private float _hungerDecreaseRate = 1.5f; // per game minute
    
    public float CurrentHunger { get; private set; }
    public HungerState CurrentState { get; private set; }
    
    public event Action<float> OnHungerChanged;
    public event Action<HungerState> OnHungerStateChanged;
    
    private void Update()
    {
        DecreaseHunger();
        UpdateHungerState();
    }
    
    public void EatFood(float hungerRestore, float healthRestore = 0);
    public void ApplyHungerEffects();
}

public enum HungerState
{
    Full,      // 80-100
    Normal,    // 40-79
    Hungry,    // 20-39
    Starving   // 0-19
}
```

#### HealthSystem 구현
```csharp
public class HealthSystem : MonoBehaviour
{
    [Header("=== Health Settings ===")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _regenRate = 0.5f; // when hunger >= 80
    
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    
    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDied;
    
    public void TakeDamage(float damage);
    public void Heal(float amount);
    public void Revive();
}
```

#### StaminaSystem 구현
```csharp
public class StaminaSystem : MonoBehaviour
{
    [Header("=== Stamina Settings ===")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _recoveryRate = 5f;
    
    public float CurrentStamina { get; private set; }
    
    public event Action<float> OnStaminaChanged;
    
    public bool UseStamina(float amount);
    public void RecoverStamina(float amount);
}
```

#### WeatherSystem 구현
```csharp
public class WeatherSystem : MonoBehaviour
{
    [Header("=== Weather Settings ===")]
    [SerializeField] private float _clearChance = 0.5f;
    [SerializeField] private float _cloudyChance = 0.2f;
    [SerializeField] private float _rainChance = 0.15f;
    [SerializeField] private float _stormChance = 0.1f;
    [SerializeField] private float _rainbowChance = 0.05f;
    
    public WeatherType CurrentWeather { get; private set; }
    public event Action<WeatherType> OnWeatherChanged;
    
    public void ChangeWeather();
    public void ApplyWeatherEffects();
}

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Rainbow
}
```

### 예상 개발 시간: 1.5일

---

## 3.3 인벤토리 시스템

### 구현 우선순위: P0 (필수)

#### PlayerInventory 구현
```csharp
public class PlayerInventory : MonoBehaviour, ISaveable
{
    [Header("=== Inventory Settings ===")]
    [SerializeField] private int _maxSlots = 24;
    [SerializeField] private int _quickSlotCount = 8;
    
    private List<InventorySlot> _slots;
    private List<InventorySlot> _quickSlots;
    private EquipmentSlot[] _equipmentSlots;
    
    public int Gold { get; private set; }
    
    public event Action OnInventoryChanged;
    public event Action<int> OnGoldChanged;
    
    public bool AddItem(ItemData item, int quantity = 1);
    public bool RemoveItem(ItemData item, int quantity = 1);
    public bool HasItem(ItemData item, int quantity = 1);
    public void AddGold(int amount);
    public void RemoveGold(int amount);
}

public class InventorySlot
{
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }
    public bool IsEmpty => Item == null;
}
```

#### EquipmentSystem 구현
```csharp
public class EquipmentSystem : MonoBehaviour
{
    public EquipmentSlot WeaponSlot { get; private set; }
    public EquipmentSlot ArmorSlot { get; private set; }
    public EquipmentSlot[] AccessorySlots { get; private set; }
    
    public void EquipItem(EquipmentData equipment);
    public void UnequipItem(EquipmentSlot slot);
    public int GetTotalAttackPower();
    public int GetTotalDefense();
}
```

### 예상 개발 시간: 1.5일

---

## 3.4 생산 시스템

### 3.4.1 농사 시스템

#### FarmingManager 구현
```csharp
public class FarmingManager : MonoBehaviour
{
    [Header("=== Farming Settings ===")]
    [SerializeField] private List<FarmPlot> _farmPlots;
    
    [Inject] private ITimeManager _timeManager;
    
    public void PlantCrop(FarmPlot plot, CropData crop);
    public void WaterCrop(FarmPlot plot);
    public void RemoveWeed(FarmPlot plot);
    public void HarvestCrop(FarmPlot plot);
    public void ApplyFertilizer(FarmPlot plot);
}

public class FarmPlot : MonoBehaviour
{
    public CropData PlantedCrop { get; private set; }
    public int GrowthDay { get; private set; }
    public bool IsWatered { get; private set; }
    public bool HasWeed { get; private set; }
    public bool HasPest { get; private set; }
    public bool CanHarvest { get; private set; }
    
    public void AdvanceDay();
    public void OnDayChanged(int day);
}
```

#### CropData ScriptableObject
```csharp
[CreateAssetMenu(fileName = "CropData", menuName = "Sunnyside/Crop Data")]
public class CropData : ScriptableObject
{
    public string CropName;
    public int GrowthDays;
    public int YieldAmount;
    public Season[] GrowableSeasons;
    public int BuyPrice;
    public int SellPrice;
    public Sprite[] GrowthSprites;
    public Sprite HarvestSprite;
}
```

### 예상 개발 시간: 1.5일

### 3.4.2 낚시 시스템

#### FishingSystem 구현
```csharp
public class FishingSystem : MonoBehaviour
{
    [Header("=== Fishing Settings ===")]
    [SerializeField] private float _minWaitTime = 3f;
    [SerializeField] private float _maxWaitTime = 10f;
    
    public bool IsFishing { get; private set; }
    public FishingState CurrentState { get; private set; }
    
    public void StartFishing();
    public void OnBite();
    public void OnReel();
    public void EndFishing(bool success);
}

public enum FishingState
{
    Idle,
    Waiting,
    Biting,
    Reeling,
    Success,
    Failed
}
```

#### 낚시 미니게임
```csharp
public class FishingMiniGame : MonoBehaviour
{
    [SerializeField] private Slider _tensionSlider;
    [SerializeField] private Slider _fishHealthSlider;
    
    private float _tension;
    private float _fishHealth;
    
    public void UpdateMiniGame();
    public void OnPlayerInput();
    public bool IsSuccess();
}
```

### 예상 개발 시간: 1.5일

### 3.4.3 채집 시스템

#### GatheringSystem 구현
```csharp
public class GatheringSystem : MonoBehaviour
{
    [Inject] private IPlayerInventory _inventory;
    
    public void ChopTree(TreeResource tree);
    public void MineRock(RockResource rock);
    public void GatherHerb(HerbResource herb);
}

public abstract class GatherableResource : MonoBehaviour
{
    [SerializeField] protected int _durability;
    [SerializeField] protected List<ItemDrop> _drops;
    [SerializeField] protected float _respawnTime;
    
    public abstract void Gather(PlayerController player);
    public abstract void OnDepleted();
    public abstract void Respawn();
}
```

### 예상 개발 시간: 1일

---

## 3.5 전투 시스템

### 구현 우선순위: P0 (필수)

#### CombatManager 구현
```csharp
public class CombatManager : MonoBehaviour
{
    [Header("=== Combat Settings ===")]
    [SerializeField] private LayerMask _enemyLayer;
    
    [Inject] private IPlayerStats _playerStats;
    [Inject] private IEventBus _eventBus;
    
    public void PerformMeleeAttack(WeaponData weapon, Vector2 direction);
    public void PerformRangedAttack(WeaponData weapon, Vector2 target);
    public void PerformRoll(Vector2 direction);
    
    private void DealDamage(IDamageable target, int damage);
}

public interface IDamageable
{
    void TakeDamage(int damage);
    void Die();
    bool IsAlive { get; }
}
```

#### WeaponSystem 구현
```csharp
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData _weaponData;
    
    public abstract void Attack(Vector2 direction);
    public abstract void OnHit(Collider2D target);
    
    public int GetDamage();
    public float GetAttackSpeed();
}

public class MeleeWeapon : Weapon
{
    [SerializeField] private Collider2D _hitBox;
    
    public override void Attack(Vector2 direction);
    public override void OnHit(Collider2D target);
}

public class RangedWeapon : Weapon
{
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private Projectile _projectilePrefab;
    
    public override void Attack(Vector2 direction);
    public override void OnHit(Collider2D target);
}
```

#### 적 AI 시스템
```csharp
public abstract class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("=== Enemy Stats ===")]
    [SerializeField] protected int _maxHealth;
    [SerializeField] protected int _attackPower;
    [SerializeField] protected float _moveSpeed;
    
    [Header("=== AI Settings ===")]
    [SerializeField] protected float _detectionRange;
    [SerializeField] protected float _attackRange;
    
    protected Transform _player;
    protected EnemyState _currentState;
    
    protected abstract void IdleBehavior();
    protected abstract void ChaseBehavior();
    protected abstract void AttackBehavior();
    protected abstract void OnTakeDamage(int damage);
    
    public void TakeDamage(int damage);
    public void Die();
}

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Stunned,
    Dead
}
```

### 예상 개발 시간: 2일

---

## 3.6 게임플레이 시스템 개발 일정

| 시스템 | 우선순위 | 예상 시간 | 담당 |
|--------|----------|-----------|------|
| PlayerController | P0 | 2일 | - |
| HungerSystem | P0 | 1.5일 | - |
| HealthSystem | P0 | 1.5일 | - |
| InventorySystem | P0 | 1.5일 | - |
| FarmingSystem | P0 | 1.5일 | - |
| FishingSystem | P0 | 1.5일 | - |
| GatheringSystem | P1 | 1일 | - |
| CombatSystem | P0 | 2일 | - |
| **합계** | | **12.5일** | |
