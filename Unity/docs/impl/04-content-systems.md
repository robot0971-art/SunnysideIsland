# 4. 콘텐츠 시스템 구현 계획

## 4.1 건설 시스템

### 구현 우선순위: P0 (필수)

#### BuildingManager 구현
```csharp
public class BuildingManager : MonoBehaviour
{
    [Header("=== Building Settings ===")]
    [SerializeField] private Grid _buildingGrid;
    [SerializeField] private LayerMask _groundLayer;
    
    [Inject] private IPlayerInventory _inventory;
    [Inject] private ITimeManager _timeManager;
    [Inject] private IEventBus _eventBus;
    
    private BuildingData _selectedBuilding;
    private bool _isInBuildMode;
    
    public void EnterBuildMode();
    public void ExitBuildMode();
    public void SelectBuilding(BuildingData building);
    public bool CanBuildAt(Vector3Int position, BuildingData building);
    public void PlaceBuilding(Vector3Int position);
    public void DemolishBuilding(Building building);
    public void UpgradeBuilding(Building building);
}

public class Building : MonoBehaviour, ISaveable
{
    [SerializeField] private BuildingData _data;
    [SerializeField] private Vector3Int _gridPosition;
    
    public BuildingState State { get; private set; }
    public float BuildProgress { get; private set; }
    
    public void StartConstruction();
    public void OnConstructionProgress(float progress);
    public void CompleteConstruction();
}

public enum BuildingState
{
    Planned,
    Constructing,
    Completed,
    Upgrading
}
```

#### BuildingData ScriptableObject
```csharp
[CreateAssetMenu(fileName = "BuildingData", menuName = "Sunnyside/Building Data")]
public class BuildingData : ScriptableObject
{
    public string BuildingName;
    public BuildingCategory Category;
    public Vector2Int Size;
    public List<BuildingCost> Costs;
    public float BuildTime;
    public int InstantCompleteCost;
    public GameObject Prefab;
    public Sprite Icon;
    
    [TextArea(3, 5)]
    public string Description;
}

public enum BuildingCategory
{
    Housing,      // 주거
    Agriculture,  // 농업
    Commerce,     // 상업
    Tourism,      // 관광
    Production,   // 생산
    Decoration    // 장식
}

[System.Serializable]
public class BuildingCost
{
    public ItemData Item;
    public int Quantity;
}
```

#### 타일 시스템
```csharp
public class TileSystem : MonoBehaviour
{
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _buildingTilemap;
    
    public bool IsBuildable(Vector3Int position);
    public void SetTile(Vector3Int position, TileBase tile);
    public void RemoveTile(Vector3Int position);
    public Vector3Int WorldToCell(Vector3 worldPosition);
    public Vector3 CellToWorld(Vector3Int cellPosition);
}
```

### 건설물 종류

#### 주거 시설
| 건설물 | 크기 | 건설 시간 | 특징 |
|--------|------|-----------|------|
| 텐트 | 2x2 | 즉시 | 초기 제공 |
| 오두막 | 3x3 | 1시간 | 기본 주거 |
| 집 | 4x4 | 2시간 | 주민 수용 |
| 큰 집 | 5x5 | 4시간 | 2인 주거 |
| 저택 | 6x6 | 8시간 | 고급 거처 |

#### 상업 시설
| 건설물 | 크기 | 하루 수익 | 특징 |
|--------|------|-----------|------|
| 노점 | 2x2 | 50~100 | 기본 거래 |
| 식료품점 | 3x3 | 150~300 | 농작물 판매 +20% |
| 대장간 | 4x4 | 200~400 | 장비 수리/강화 |
| 식당 | 4x4 | 400~800 | 요리 판매 |
| 여관 | 5x5 | 500~1000 | 관광객 숙박 |
| 시장 | 8x8 | 1000~2000 | 모든 NPC 거래 |

#### 관광 시설
| 건설물 | 크기 | 관광객 증가 |
|--------|------|-------------|
| 부두 | 6x2 | 기본 |
| 등대 | 2x2 | +10% |
| 광장 | 6x6 | +15% |
| 공원 | 8x8 | +20% |
| 축제장 | 8x8 | +30% |
| 온천 | 6x6 | +35% |
| 리조트 호텔 | 8x6 | +50% |

### 예상 개발 시간: 2일

---

## 4.2 경제 시스템

### 구현 우선순위: P0 (필수)

#### CurrencySystem 구현
```csharp
public class CurrencySystem : MonoBehaviour
{
    [Header("=== Currency Settings ===")]
    [SerializeField] private int _startingGold = 0;
    
    public int CurrentGold { get; private set; }
    
    public event Action<int> OnGoldChanged;
    
    public void AddGold(int amount);
    public bool RemoveGold(int amount);
    public bool CanAfford(int amount);
    
    public void AddIncome(IncomeSource source, int amount);
    public void RecordExpense(ExpenseType type, int amount);
}

public enum IncomeSource
{
    ResourceSale,
    CropSale,
    FishSale,
    ShopRevenue,
    TouristSpending,
    QuestReward
}

public enum ExpenseType
{
    Building,
    ToolPurchase,
    ResidentSalary,
    Upgrade,
    Repair
}
```

#### ShopSystem 구현
```csharp
public class ShopSystem : MonoBehaviour
{
    [Header("=== Shop Settings ===")]
    [SerializeField] private List<ShopSlot> _shopSlots;
    
    [Inject] private IPlayerInventory _inventory;
    [Inject] private ICurrencySystem _currency;
    
    public void BuyItem(ItemData item, int quantity);
    public void SellItem(ItemData item, int quantity);
    public int GetBuyPrice(ItemData item);
    public int GetSellPrice(ItemData item);
    public void Restock();
}

public class ShopSlot
{
    public ItemData Item { get; set; }
    public int Stock { get; set; }
    public float PriceMultiplier { get; set; }
}
```

#### TourismSystem 구현
```csharp
public class TourismSystem : MonoBehaviour
{
    [Header("=== Tourism Settings ===")]
    [SerializeField] private Transform _dockSpawnPoint;
    [SerializeField] private int _baseTouristCount = 2;
    
    [Inject] private ITimeManager _timeManager;
    [Inject] private IBuildingManager _buildingManager;
    
    public int ReputationLevel { get; private set; }
    public int DailyTouristCount { get; private set; }
    public List<Tourist> ActiveTourists { get; private set; }
    
    public void CalculateDailyTourists();
    public void SpawnTourists();
    public void OnTouristSatisfied(Tourist tourist);
    public void OnTouristDisappointed(Tourist tourist);
    public void IncreaseReputation(int amount);
}

public class Tourist : MonoBehaviour
{
    public TouristType Type { get; private set; }
    public List<Building> VisitedBuildings { get; private set; }
    public int SpentGold { get; private set; }
    public int Satisfaction { get; private set; }
    
    public void AIUpdate();
    public void VisitBuilding(Building building);
    public void SpendGold(int amount);
}

public enum TouristType
{
    Normal,
    Luxury,
    Explorer,
    Group
}
```

#### ResidentSystem 구현
```csharp
public class ResidentSystem : MonoBehaviour
{
    [SerializeField] private List<Resident> _hiredResidents;
    
    public void HireResident(ResidentData resident);
    public void FireResident(Resident resident);
    public void PayDailySalaries();
    public void ApplyResidentEffects();
}

public class Resident : MonoBehaviour
{
    public ResidentData Data { get; private set; }
    public bool IsWorking { get; private set; }
    public int DaysEmployed { get; private set; }
    
    public void Work();
    public void OnDayEnd();
}

[CreateAssetMenu(fileName = "ResidentData", menuName = "Sunnyside/Resident Data")]
public class ResidentData : ScriptableObject
{
    public string ResidentName;
    public ResidentType Type;
    public int DailySalary;
    public Sprite Portrait;
    
    [TextArea(3, 5)]
    public string Description;
}

public enum ResidentType
{
    Merchant,
    Blacksmith,
    Chef,
    Farmer,
    Carpenter,
    Guard
}
```

### 예상 개발 시간: 2일

---

## 4.3 조합 시스템

### 구현 우선순위: P1 (권장)

#### CraftingSystem 구현
```csharp
public class CraftingSystem : MonoBehaviour
{
    [Header("=== Crafting Settings ===")]
    [SerializeField] private List<RecipeData> _availableRecipes;
    
    [Inject] private IPlayerInventory _inventory;
    [Inject] private ITimeManager _timeManager;
    
    public List<RecipeData> GetAvailableRecipes();
    public List<RecipeData> GetCraftableRecipes();
    public bool CanCraft(RecipeData recipe);
    public void CraftItem(RecipeData recipe);
    public void CraftMultiple(RecipeData recipe, int count);
}

[CreateAssetMenu(fileName = "RecipeData", menuName = "Sunnyside/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string RecipeName;
    public CraftingCategory Category;
    public ItemData OutputItem;
    public int OutputAmount;
    public List<Ingredient> Ingredients;
    public float CraftingTime;
    public int RequiredLevel;
    public Sprite Icon;
}

[System.Serializable]
public class Ingredient
{
    public ItemData Item;
    public int Amount;
}

public enum CraftingCategory
{
    Tool,
    Equipment,
    Furniture,
    BuildingMaterial,
    Consumable,
    Processed
}
```

#### CookingSystem 구현
```csharp
public class CookingSystem : MonoBehaviour
{
    [SerializeField] private List<RecipeData> _cookingRecipes;
    
    [Inject] private IPlayerInventory _inventory;
    
    public void CookRecipe(RecipeData recipe);
    public void CookMultiple(RecipeData recipe, int count);
    public List<RecipeData> GetAvailableCookingRecipes();
}
```

### 주요 레시피

#### 도구
- 나무 도끼: 나무 x10
- 돌 곡괭이: 나무 x5 + 돌 x10
- 철 낚싯대: 나무 x10 + 철 x5
- 철 검: 철 x10 + 나무 x5

#### 가구
- 침대: 나무 x20 + 천 x10
- 책상: 나무 x15
- 의자: 나무 x10
- 벽난로: 돌 x30

#### 요리
- 구운 생선: 생선 x1 (허기 +20)
- 스튜: 고기 x2 + 감자 x2 + 당근 x1 (허기 +60)
- 샐러드: 채소 x3 (허기 +30)
- 스테이크: 고급 고기 x2 (허기 +80)

### 예상 개발 시간: 1일

---

## 4.4 퀘스트 시스템

### 구현 우선순위: P1 (권장)

#### QuestManager 구현
```csharp
public class QuestManager : MonoBehaviour
{
    [Header("=== Quest Settings ===")]
    [SerializeField] private List<QuestData> _availableQuests;
    
    [Inject] private IEventBus _eventBus;
    [Inject] private IPlayerInventory _inventory;
    
    public List<QuestData> ActiveQuests { get; private set; }
    public List<QuestData> CompletedQuests { get; private set; }
    
    public void AcceptQuest(QuestData quest);
    public void AbandonQuest(QuestData quest);
    public void UpdateQuestProgress(QuestData quest, int progress);
    public void CompleteQuest(QuestData quest);
    public void GiveQuestRewards(QuestData quest);
    
    public void CheckQuestConditions();
    public void OnEventTriggered<T>(T eventData);
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Sunnyside/Quest Data")]
public class QuestData : ScriptableObject
{
    public string QuestId;
    public string QuestName;
    public QuestType Type;
    public QuestState State;
    
    [TextArea(3, 5)]
    public string Description;
    
    public List<QuestObjective> Objectives;
    public List<QuestReward> Rewards;
    
    public string NextQuestId;
    public int RequiredLevel;
}

[System.Serializable]
public class QuestObjective
{
    public ObjectiveType Type;
    public string TargetId;
    public int RequiredAmount;
    public int CurrentAmount;
    public bool IsCompleted => CurrentAmount >= RequiredAmount;
}

[System.Serializable]
public class QuestReward
{
    public RewardType Type;
    public string ItemId;
    public int Amount;
}

public enum QuestType
{
    Main,    // 메인 퀘스트
    Sub,     // 서브 퀘스트
    Daily,   // 일일 퀘스트
    Event    // 이벤트 퀘스트
}

public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    TurnedIn
}
```

#### 메인 퀘스트 라인

**Chapter 1: 생존 (Day 1-3)**
1. [불시착] 텐트 건설 - 보상: 기본 도구 세트
2. [첫 식사] 생선 3마리 낚시 - 보상: 낚싯대 내구도 증가
3. [안전한 곳] 오두막 완성 - 보상: 침대 제작법

**Chapter 2: 정착 (Day 4-7)**
4. [농부의 시작] 첫 작물 수확 - 보상: 씨앗 20개
5. [새로운 이웃] 첫 주민 유치 - 보상: 가구 세트
6. [고블린의 위협] 고블린 5마리 처치 - 보상: 전투 도구 제작법

**Chapter 3: 교류 (Day 8-14)**
7. [고블린 마을 발견] 고블린 마을 탐색 - 보상: 고블린어 번역기
8. [평화의 조건] 족장 처치 또는 교역 협정 - 보상: 동맹 or 보물
9. [첫 관광객] 관광객 10명 유치 - 보상: 부두 업그레이드

**Chapter 4: 번영 (Day 15-28)**
10. [상인의 길] 상점 5개 건설 - 보상: 시장 건설 가능
11. [축제의 날] 첫 축제 개최 - 보상: 평판 대폭 상승
12. [리조트 완성] 리조트 호텔 건설 - 보상: 게임 클리어

### 예상 개발 시간: 1.5일

---

## 4.5 이벤트 시스템

### 구현 우선순위: P2 (선택)

#### EventManager 구현
```csharp
public class EventManager : MonoBehaviour
{
    [Header("=== Event Settings ===")]
    [SerializeField] private List<RandomEventData> _randomEvents;
    
    [Inject] private ITimeManager _timeManager;
    
    public void CheckDailyEvents();
    public void TriggerEvent(RandomEventData eventData);
    public void StartFestival(FestivalType festival);
}

[CreateAssetMenu(fileName = "RandomEventData", menuName = "Sunnyside/Random Event")]
public class RandomEventData : ScriptableObject
{
    public string EventName;
    public float DailyChance;
    public EventCondition Condition;
    public List<EventEffect> Effects;
}

public enum RandomEvent
{
    MerchantVisit,    // 상인 방문 (10%)
    GoblinRaid,       // 고블린 습격 (5%)
    TreasureFind,     // 보물 발견 (2%)
    Rainbow,          // 무지개 (1%)
    GhostAppearance,  // 유령 출몰 (3%)
    BumperCrop,       // 풍작 (5%)
    Drought           // 가뭄 (3%)
}
```

#### 주간 이벤트
| 요일 | 이벤트 | 효과 |
|------|--------|------|
| 월요일 | 채집의 날 | 채집 XP 2배 |
| 수요일 | 낚시 대회 | 특별 물고기 출현 |
| 금요일 | 시장의 날 | 모든 가격 할인 20% |
| 일요일 | 쉬는 날 | 모든 NPC 휴식 |

### 예상 개발 시간: 1일

---

## 4.6 업적 시스템

### 구현 우선순위: P2 (선택)

#### AchievementSystem 구현
```csharp
public class AchievementSystem : MonoBehaviour
{
    [SerializeField] private List<AchievementData> _achievements;
    
    public void CheckAchievements();
    public void UnlockAchievement(string achievementId);
    public bool IsAchievementUnlocked(string achievementId);
}

[CreateAssetMenu(fileName = "AchievementData", menuName = "Sunnyside/Achievement")]
public class AchievementData : ScriptableObject
{
    public string AchievementId;
    public string Title;
    public string Description;
    public Sprite Icon;
    public AchievementType Type;
    public int RequiredAmount;
}

public enum AchievementType
{
    Woodcutting,   // 나무 1000개 벌목
    Mining,        // 철광석 500개 채굴
    Farming,       // 1000개 작물 수확
    Fishing,       // 물고기 1000마리
    Combat,        // 몬스터 100마리 처치
    Building,      // 건물 50개 완성
    Wealth,        // 코인 100000 보유
    Completion     // 모든 업적 달성
}
```

### 예상 개발 시간: 0.5일

---

## 4.7 콘텐츠 시스템 개발 일정

| 시스템 | 우선순위 | 예상 시간 | 담당 |
|--------|----------|-----------|------|
| BuildingSystem | P0 | 2일 | - |
| EconomySystem | P0 | 2일 | - |
| CraftingSystem | P1 | 1일 | - |
| QuestSystem | P1 | 1.5일 | - |
| EventSystem | P2 | 1일 | - |
| AchievementSystem | P2 | 0.5일 | - |
| **합계** | | **8일** | |
