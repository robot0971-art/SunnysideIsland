# 6. 기술 스펙

## 6.1 Unity 설정

### 프로젝트 설정

| 항목 | 설정값 |
|------|--------|
| Engine Version | Unity 2022.3 LTS |
| Render Pipeline | URP (Universal Render Pipeline) |
| Scripting Backend | IL2CPP |
| API Compatibility | .NET Standard 2.1 |
| Target Platform | PC (Windows/Mac) |
| Resolution | 1920x1080 (16:9) |
| Target FPS | 60 |

### 그래픽 설정

```
Pixel Perfect Camera: 활성화
PPU (Pixels Per Unit): 16
Filter Mode: Point (No Filter)
Compression: None (Pixel Art)
```

### Sorting Layers

| 순서 | 레이어 |
|------|--------|
| 0 | Background |
| 1 | Ground |
| 2 | Objects |
| 3 | Characters |
| 4 | Effects |
| 5 | UI |

---

## 6.2 에셋 구조

### 폴더 구조

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           # 싱글톤, 매니저
│   │   ├── Systems/        # 게임 시스템
│   │   ├── Entities/       # 플레이어, NPC, 몬스터
│   │   ├── UI/            # UI 컨트롤러
│   │   └── Utils/         # 유틸리티
│   ├── Prefabs/
│   │   ├── Characters/
│   │   ├── Buildings/
│   │   ├── Items/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── Items/         # 아이템 데이터
│   │   ├── Recipes/       # 조합법
│   │   ├── Quests/        # 퀘스트 데이터
│   │   └── Buildings/     # 건설물 데이터
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Game.unity
│   │   └── Loading.unity
│   ├── Resources/
│   │   └── Data/          # JSON 데이터
│   └── Audio/
│       ├── BGM/
│       └── SFX/
├── SunnysideAssets/       # 외부 에셋
└── Plugins/               # 외부 플러그인
```

---

## 6.3 핵심 시스템 아키텍처

### 매니저 패턴

```csharp
// GameManager.cs
public class GameManager : Singleton<GameManager>
{
    public TimeManager Time { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public QuestManager Quest { get; private set; }
    public EconomyManager Economy { get; private set; }
    public SaveManager Save { get; private set; }
    public BuildingManager Building { get; private set; }
    public NPCManager NPC { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        InitializeManagers();
    }
}

// Singleton.cs
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

### 이벤트 시스템

```csharp
// GameEvents.cs
public static class GameEvents
{
    // 아이템
    public static Action<ItemData, int> OnItemCollected;
    public static Action<ItemData, int> OnItemUsed;
    
    // 건설
    public static Action<BuildingData> OnBuildingConstructed;
    public static Action<BuildingData> OnBuildingUpgraded;
    
    // 시간
    public static Action<int, int> OnDayChanged; // (oldDay, newDay)
    public static Action<float> OnTimeChanged;   // (currentTime)
    
    // 퀘스트
    public static Action<QuestData> OnQuestStarted;
    public static Action<QuestData> OnQuestCompleted;
    public static Action<QuestData> OnQuestUpdated;
    
    // 전투
    public static Action<EnemyData> OnEnemyDefeated;
    public static Action OnPlayerDeath;
    
    // 경제
    public static Action<int> OnMoneyChanged; // (newAmount)
    public static Action<TouristData> OnTouristArrived;
    
    // NPC
    public static Action<NPCData> OnNPCJoined;
    public static Action<NPCData> OnNPCLeft;
}
```

### 데이터 관리

#### ScriptableObject 기반 데이터

```csharp
// ItemData.cs
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string itemID;
    public Sprite icon;
    public ItemType type;
    
    [Header("Stack & Value")]
    public int maxStack = 99;
    public int baseValue;
    public bool canSell = true;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("Effects")]
    public ItemEffect[] effects;
    
    [Header("Crafting")]
    public bool canCraft = false;
    public CraftingRecipe recipe;
}

// BuildingData.cs
[CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public string buildingID;
    public Sprite icon;
    public BuildingType type;
    
    public Vector2Int size; // x, y tiles
    public int buildTime; // 게임 내 시간(분)
    
    public ResourceCost[] requiredResources;
    
    public GameObject prefab;
    public Sprite previewSprite;
}
```

#### 인벤토리 시스템

```csharp
// Inventory.cs
public class Inventory : MonoBehaviour
{
    [SerializeField] private int capacity = 24;
    private ItemSlot[] slots;
    
    public bool AddItem(ItemData item, int amount)
    {
        // 스택 가능한 아이템 처리
        // 새 슬롯 필요한 경우 처리
        // 용량 초과 확인
    }
    
    public bool RemoveItem(ItemData item, int amount)
    {
        // 아이템 제거 로직
    }
    
    public bool HasItem(ItemData item, int amount)
    {
        // 아이템 보유 확인
    }
}

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int amount;
    
    public bool IsEmpty => item == null || amount <= 0;
    public bool IsFull => item != null && amount >= item.maxStack;
}
```

---

## 6.4 저장 시스템

### 저장 데이터 구조

```json
{
  "version": "1.0",
  "saveDate": "2025-03-12T14:30:00",
  "playTime": 7200,
  
  "player": {
    "name": "Player",
    "position": {"x": 100.5, "y": 200.3},
    "stats": {
      "level": 5,
      "exp": 1200,
      "hp": 80,
      "maxHp": 100,
      "hunger": 75
    },
    "inventory": [
      {"itemID": "wood", "amount": 50},
      {"itemID": "iron_sword", "amount": 1}
    ],
    "equipment": {
      "weapon": "iron_sword",
      "armor": "leather_armor",
      "accessory": null
    },
    "skills": {
      "gathering": 2,
      "farming": 1,
      "combat": 3
    }
  },
  
  "world": {
    "day": 12,
    "time": 870, // 14:30 in minutes
    "weather": "sunny",
    "reputation": 3,
    
    "buildings": [
      {
        "id": "house_01",
        "type": "house",
        "position": {"x": 50, "y": 30},
        "rotation": 0,
        "level": 1
      }
    ],
    
    "crops": [
      {
        "id": "carrot_01",
        "type": "carrot",
        "position": {"x": 60, "y": 40},
        "growthStage": 3,
        "lastWatered": 860
      }
    ],
    
    "npcs": [
      {
        "id": "merchant_01",
        "type": "merchant",
        "homePosition": {"x": 55, "y": 35},
        "relationship": 50
      }
    ],
    
    "resources": {
      "lastRegen": "2025-03-12T10:00:00",
      "regeneratedTrees": ["tree_01", "tree_05"]
    }
  },
  
  "progress": {
    "quests": {
      "active": ["quest_main_02"],
      "completed": ["quest_main_01", "quest_sub_03"]
    },
    "achievements": ["first_harvest", "build_house"],
    "unlocks": ["farming", "fishing", "building"],
    "discoveredAreas": ["beach", "forest", "mountain"]
  },
  
  "settings": {
    "volume": {
      "master": 0.8,
      "bgm": 0.6,
      "sfx": 0.9
    },
    "graphics": {
      "fullscreen": true,
      "resolution": "1920x1080"
    }
  }
}
```

### 저장 매니저

```csharp
// SaveManager.cs
public class SaveManager : MonoBehaviour
{
    private const string SAVE_FOLDER = "/Saves/";
    private const string SAVE_EXTENSION = ".json";
    private const int MAX_SLOTS = 5;
    
    public void SaveGame(int slot)
    {
        SaveData data = new SaveData();
        data.version = Application.version;
        data.saveDate = DateTime.Now.ToString("O");
        data.playTime += Time.time;
        
        // 데이터 수집
        data.player = CollectPlayerData();
        data.world = CollectWorldData();
        data.progress = CollectProgressData();
        
        // JSON 직렬화 및 저장
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(slot);
        File.WriteAllText(path, json);
        
        GameEvents.OnGameSaved?.Invoke(slot);
    }
    
    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return;
        
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        // 데이터 적용
        ApplyPlayerData(data.player);
        ApplyWorldData(data.world);
        ApplyProgressData(data.progress);
        
        GameEvents.OnGameLoaded?.Invoke(slot);
    }
    
    public void AutoSave()
    {
        SaveGame(0); // 슬롯 0은 자동 저장
    }
    
    private string GetSavePath(int slot)
    {
        return Application.persistentDataPath + SAVE_FOLDER + "save" + slot + SAVE_EXTENSION;
    }
}
```

### 자동 저장 트리거

- 매일 아침 6:00 (게임 시간)
- 퀘스트 완료 시
- 건설 완료 시
- 중요한 이벤트 발생 시
- 10분 간격 (실제 시간)

---

## 6.5 성능 최적화

### 오브젝트 풀링

```csharp
// ObjectPool.cs
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private Transform parent;
    
    public ObjectPool(T prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }
    
    public T Get()
    {
        if (pool.Count == 0)
        {
            CreateObject();
        }
        
        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }
    
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
    
    private void CreateObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}

// 사용 예시
public class DamageTextPool : MonoBehaviour
{
    [SerializeField] private DamageText damageTextPrefab;
    private ObjectPool<DamageText> pool;
    
    private void Start()
    {
        pool = new ObjectPool<DamageText>(damageTextPrefab, 20, transform);
    }
    
    public void ShowDamage(Vector3 position, int damage)
    {
        DamageText text = pool.Get();
        text.transform.position = position;
        text.SetText(damage);
        text.OnAnimationEnd += () => pool.Return(text);
    }
}
```

### 렌더링 최적화

#### 타일맵 Chunking

```csharp
// TilemapChunk.cs
public class TilemapChunk : MonoBehaviour
{
    public Vector2Int chunkPosition;
    public BoundsInt bounds;
    
    private bool isVisible = true;
    
    public void SetVisibility(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        gameObject.SetActive(visible);
    }
}

// ChunkManager.cs
public class ChunkManager : MonoBehaviour
{
    [SerializeField] private int chunkSize = 16;
    [SerializeField] private int renderDistance = 3;
    
    private Dictionary<Vector2Int, TilemapChunk> chunks = new Dictionary<Vector2Int, TilemapChunk>();
    private Transform player;
    
    private void Update()
    {
        UpdateChunks();
    }
    
    private void UpdateChunks()
    {
        Vector2Int playerChunk = GetChunkPosition(player.position);
        
        foreach (var chunk in chunks.Values)
        {
            float distance = Vector2Int.Distance(playerChunk, chunk.chunkPosition);
            bool shouldBeVisible = distance <= renderDistance;
            chunk.SetVisibility(shouldBeVisible);
        }
    }
    
    private Vector2Int GetChunkPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkSize);
        return new Vector2Int(x, y);
    }
}
```

#### Sprite Atlasing

```
스프라이트 아틀라스 그룹:
- Characters: 모든 캐릭터 스프라이트
- Environment: 타일, 나무, 돌 등
- UI: 아이콘, 버튼 등
- Items: 모든 아이템 아이콘
```

### 메모리 관리

#### 에셋 번들

```
번들 구성:
- characters.bundle: 캐릭터 스프라이트, 애니메이션
- environment.bundle: 타일맵, 배경
- audio.bundle: BGM, 효과음
- ui.bundle: UI 에셋
```

#### 텍스처 설정

```
Pixel Art:
- Filter Mode: Point
- Compression: None
- Max Size: 원본 크기 유지
- Read/Write: 비활성화 (런타임 수정 불필요 시)
```

#### 오디오 설정

| 타입 | Format | Load Type | Compression |
|------|--------|-----------|-------------|
| BGM | Vorbis | Streaming | 50% |
| SFX | PCM | Decompress On Load | - |
| Ambient | Vorbis | Compressed In Memory | 70% |

---

## 6.6 타임랩스 시스템

### 시간 가속

```csharp
// TimeLapseController.cs
public class TimeLapseController : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float normalSpeed = 1f; // 1초 = 1분
    [SerializeField] private float fastSpeed = 10f;  // 1초 = 10분
    [SerializeField] private float superSpeed = 60f; // 1초 = 1시간
    
    private float currentSpeed = 1f;
    private bool isTimeLapseActive = false;
    
    public void StartTimeLapse(TimeLapseSpeed speed)
    {
        switch (speed)
        {
            case TimeLapseSpeed.Normal:
                currentSpeed = normalSpeed;
                break;
            case TimeLapseSpeed.Fast:
                currentSpeed = fastSpeed;
                break;
            case TimeLapseSpeed.Super:
                currentSpeed = superSpeed;
                break;
        }
        
        isTimeLapseActive = true;
        GameEvents.OnTimeLapseStarted?.Invoke(speed);
    }
    
    public void StopTimeLapse()
    {
        currentSpeed = normalSpeed;
        isTimeLapseActive = false;
        GameEvents.OnTimeLapseStopped?.Invoke();
    }
}

public enum TimeLapseSpeed
{
    Normal,
    Fast,
    Super
}
```

---

## 6.7 디버그 도구

### 개발 콘솔

```csharp
// DebugConsole.cs
public class DebugConsole : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    private bool isVisible = false;
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }
    
    private void OnGUI()
    {
        if (!isVisible) return;
        
        // 콘솔 UI
        GUILayout.BeginArea(new Rect(10, 10, 400, 300), "Debug Console", "box");
        
        if (GUILayout.Button("Add 1000 Coins"))
        {
            GameManager.Instance.Economy.AddMoney(1000);
        }
        
        if (GUILayout.Button("Skip to Next Day"))
        {
            GameManager.Instance.Time.SkipToNextDay();
        }
        
        if (GUILayout.Button("Complete All Quests"))
        {
            GameManager.Instance.Quest.CompleteAll();
        }
        
        if (GUILayout.Button("Spawn Boss"))
        {
            SpawnManager.Instance.SpawnBoss();
        }
        
        GUILayout.EndArea();
    }
    
    private void Toggle()
    {
        isVisible = !isVisible;
    }
}
```

---

## 6.8 외부 에셋 및 플러그인

### 필수 플러그인

| 이름 | 용도 | 링크 |
|------|------|------|
| Sunnyside World | 핵심 에셋 | itch.io |
| TextMeshPro | 텍스트 렌더링 | Unity Package |
| Cinemachine | 카메라 제어 | Unity Package |
| Input System | 입력 처리 | Unity Package |

### 선택적 플러그인

| 이름 | 용도 | 링크 |
|------|------|------|
| DOTween | 애니메이션 | Asset Store |
| Odin Inspector | 에디터 확장 | Asset Store |
| A* Pathfinding | NPC AI | Asset Store |

### 사운드 에셋 필요

- BGM (5~10 트랙)
- 효과음 (50+ 개)
- 환경음 (10+ 개)
- UI 사운드 (20+ 개)
