# 1. 아키텍처 개요

## 1.1 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/                    # 핵심 시스템
│   │   ├── DI/                  # 의존성 주입
│   │   ├── EventBus/            # 이벤트 버스
│   │   ├── GameManager.cs       # 게임 매니저
│   │   ├── TimeManager.cs       # 시간 관리
│   │   └── SaveSystem.cs        # 저장 시스템
│   │
│   ├── Player/                  # 플레이어
│   │   ├── PlayerController.cs  # 플레이어 컨트롤러
│   │   ├── PlayerStats.cs       # 플레이어 스탯
│   │   ├── PlayerInventory.cs   # 인벤토리
│   │   └── Skills/              # 스킬 시스템
│   │
│   ├── Survival/                # 생존 시스템
│   │   ├── HungerSystem.cs      # 허기 시스템
│   │   ├── HealthSystem.cs      # 체력 시스템
│   │   ├── StaminaSystem.cs     # 스태미나 시스템
│   │   └── WeatherSystem.cs     # 날씨 시스템
│   │
│   ├── Production/              # 생산 시스템
│   │   ├── Farming/             # 농사
│   │   ├── Fishing/             # 낚시
│   │   ├── Hunting/             # 사냥
│   │   └── Gathering/           # 채집
│   │
│   ├── Combat/                  # 전투 시스템
│   │   ├── WeaponSystem.cs      # 무기 시스템
│   │   ├── CombatManager.cs     # 전투 매니저
│   │   ├── Enemy/               # 적 AI
│   │   └── Boss/                # 보스
│   │
│   ├── Building/                # 건설 시스템
│   │   ├── BuildingManager.cs   # 건설 매니저
│   │   ├── BuildingData.cs      # 건설 데이터
│   │   └── TileSystem.cs        # 타일 시스템
│   │
│   ├── Economy/                 # 경제 시스템
│   │   ├── CurrencySystem.cs    # 통화 시스템
│   │   ├── ShopSystem.cs        # 상점
│   │   ├── TourismSystem.cs     # 관광 시스템
│   │   └── ResidentSystem.cs    # 주민 시스템
│   │
│   ├── Crafting/                # 조합 시스템
│   │   ├── CraftingSystem.cs    # 조합 매니저
│   │   ├── RecipeData.cs        # 레시피 데이터
│   │   └── CookingSystem.cs     # 요리
│   │
│   ├── Quest/                   # 퀘스트 시스템
│   │   ├── QuestManager.cs      # 퀘스트 매니저
│   │   ├── QuestData.cs         # 퀘스트 데이터
│   │   └── AchievementSystem.cs # 업적
│   │
│   ├── Items/                   # 아이템 시스템
│   │   ├── ItemDatabase.cs      # 아이템 DB
│   │   ├── ItemData.cs          # 아이템 데이터
│   │   └── EquipmentSystem.cs   # 장비
│   │
│   ├── NPC/                     # NPC 시스템
│   │   ├── NPCManager.cs        # NPC 매니저
│   │   ├── NPCAI.cs             # NPC AI
│   │   └── DialogueSystem.cs    # 대화
│   │
│   ├── Events/                  # 이벤트 시스템
│   │   ├── EventManager.cs      # 이벤트 매니저
│   │   └── RandomEvents.cs      # 랜덤 이벤트
│   │
│   └── UI/                      # UI 시스템
│       ├── UIManager.cs         # UI 매니저
│       ├── HUD/                 # HUD
│       ├── Inventory/           # 인벤토리 UI
│       ├── Building/            # 건설 UI
│       └── Dialog/              # 대화 UI
│
├── Resources/
│   ├── Data/                    # 데이터 에셋
│   │   ├── Items/               # 아이템 데이터
│   │   ├── Buildings/           # 건설물 데이터
│   │   ├── Recipes/             # 레시피 데이터
│   │   ├── Quests/              # 퀘스트 데이터
│   │   └── NPCs/                # NPC 데이터
│   │
│   ├── Prefabs/                 # 프리팹
│   │   ├── Player/              # 플레이어
│   │   ├── NPCs/                # NPC
│   │   ├── Enemies/             # 적
│   │   ├── Buildings/           # 건설물
│   │   └── UI/                  # UI
│   │
│   └── Sounds/                  # 사운드
│       ├── BGM/                 # 배경음
│       └── SFX/                 # 효과음
│
├── Scenes/                      # 씬
│   ├── Bootstrap.unity          # 초기화 씬
│   ├── MainMenu.unity           # 메인 메뉴
│   ├── Game.unity               # 게임 씬
│   └── Test/                    # 테스트 씬
│
└── Plugins/                     # 플러그인
    └── ...
```

## 1.2 의존성 주입 (DI) 구조

```
DIContainer (Global)
├── GameManager
├── TimeManager
├── SaveSystem
├── EventBus
└── ConfigData

Scene Container (per scene)
├── Player
├── NPCManager
├── EnemyManager
├── BuildingManager
└── EconomyManager
```

## 1.3 이벤트 버스 구조

### 코어 이벤트
- `GameStartedEvent`
- `GamePausedEvent`
- `GameSavedEvent`
- `GameLoadedEvent`

### 시간 이벤트
- `TimeChangedEvent`
- `DayStartedEvent`
- `DayEndedEvent`
- `WeatherChangedEvent`

### 플레이어 이벤트
- `PlayerMovedEvent`
- `PlayerDamagedEvent`
- `PlayerDiedEvent`
- `ItemPickedUpEvent`

### 생존 이벤트
- `HungerChangedEvent`
- `HealthChangedEvent`
- `StaminaChangedEvent`

### 경제 이벤트
- `CurrencyChangedEvent`
- `ItemSoldEvent`
- `TouristArrivedEvent`
- `ResidentHiredEvent`

### 전투 이벤트
- `CombatStartedEvent`
- `EnemyKilledEvent`
- `BossDefeatedEvent`

### 건설 이벤트
- `BuildingPlacedEvent`
- `BuildingCompletedEvent`
- `BuildingUpgradedEvent`

## 1.4 데이터 흐름

```
ScriptableObject (Data Source)
        ↓
Service Layer (Business Logic)
        ↓
Manager Layer (Coordination)
        ↓
Controller Layer (MonoBehaviour)
        ↓
View Layer (UI)
```

## 1.5 저장 시스템

### 저장 데이터 구조
```csharp
[Serializable]
public class GameSaveData
{
    public string saveId;
    public DateTime saveTime;
    public int playTimeMinutes;
    
    // 플레이어 데이터
    public PlayerData playerData;
    
    // 세계 데이터
    public WorldData worldData;
    
    // 진행 데이터
    public ProgressData progressData;
}
```

### 저장 위치
- Windows: `%USERPROFILE%\AppData\Local\SunnysideIsland\Saves\`
- Mac: `~/Library/Application Support/SunnysideIsland/Saves/`

## 1.6 주요 매니저 생명주기

```
Bootstrap Scene
├── DIContainer.InitializeGlobal()
├── Service Registration
└── Load Game Scene

Game Scene
├── Scene Installer Bindings
├── Initialize Systems
└── Game Start
```
