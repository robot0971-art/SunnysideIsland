# Addressable 리소스 관리 개선 계획

## 현재 상태 분석

### 문제점
1. **String 기반 참조**: 데이터 클래스들이 Sprite나 Prefab을 직접 참조하지 않고 string path 사용
2. **Addressable 미적용**: Addressable 시스템이 전혀 사용되지 않음
3. **런타임 로딩 없음**: Resources.Load나 Addressables.Load 등의 런타임 로딩 패턴 부재
4. **에셋 관리**: 모든 데이터가 단일 GameData.asset에 병합되어 관리

### 현재 데이터 클래스 구조
```csharp
// 현재 - string path 사용
public class ItemData
{
    public string itemId;
    public string itemName;
    public string iconPath;  // string으로 경로만 저장
    // Sprite icon;  // 없음!
}

// 현재 - Prefab 참조 없음
public class BuildingData
{
    public string buildingId;
    public string buildingName;
    // GameObject prefab;  // 없음!
}
```

---

## 개선 목표

1. **Addressable 통합**: 모든 게임 에셋을 Addressable로 관리
2. **AssetReference 사용**: 데이터 클래스에 Addressable 참조 추가
3. **비동기 로딩**: 런타임에 비동기로 에셋 로드
4. **메모리 관리**: 필요한 시점에 로드하고 해제

---

## 구현 계획

### 1. Addressable 설정 (1일)

#### Addressable Groups 구성
```
Addressable Groups
├── Default Local Group
│   └── (자동 생성됨)
├── Items
│   └── 아이템 아이콘, 프리팹
├── Buildings
│   └── 건설물 프리팹
├── Characters
│   └── 플레이어, NPC, 몬스터 프리팹
├── UI
│   └── UI 프리팹, 아이콘
├── Effects
│   └── 파티클, 이펙트
└── Audio
    └── BGM, SFX
```

#### Addressable Settings
```
- Build Path: StreamingAssets/com.unity.addressables
- Load Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}
- Bundle Mode: Pack Together by Label
- Compression: LZ4
```

---

### 2. 데이터 클래스 개선 (2일)

#### AssetReference 필드 추가

**ItemData 개선**
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public ItemRarity rarity;
    
    // Addressable 참조
    public AssetReferenceT<Sprite> iconReference;
    public AssetReferenceGameObject worldPrefabReference;
    
    // 런타임 캐싱 (선택적)
    [System.NonSerialized] private Sprite _cachedIcon;
    [System.NonSerialized] private GameObject _cachedPrefab;
    
    public int maxStack;
    public int buyPrice;
    public int sellPrice;
    
    [TextArea(3, 5)]
    public string description;
    
    // 비동기 로드 메서드
    public async Task<Sprite> LoadIconAsync()
    {
        if (_cachedIcon != null) return _cachedIcon;
        
        var handle = iconReference.LoadAssetAsync<Sprite>();
        _cachedIcon = await handle.Task;
        return _cachedIcon;
    }
    
    public async Task<GameObject> LoadPrefabAsync()
    {
        if (_cachedPrefab != null) return _cachedPrefab;
        
        var handle = worldPrefabReference.LoadAssetAsync<GameObject>();
        _cachedPrefab = await handle.Task;
        return _cachedPrefab;
    }
    
    // 메모리 해제
    public void ReleaseAssets()
    {
        if (_cachedIcon != null)
        {
            iconReference.ReleaseAsset();
            _cachedIcon = null;
        }
        if (_cachedPrefab != null)
        {
            worldPrefabReference.ReleaseAsset();
            _cachedPrefab = null;
        }
    }
}
```

**BuildingData 개선**
```csharp
[System.Serializable]
public class BuildingData
{
    public string buildingId;
    public string buildingName;
    public BuildingCategory category;
    public Vector2Int size;
    
    // Addressable 참조
    public AssetReferenceGameObject prefabReference;
    public AssetReferenceT<Sprite> iconReference;
    
    public List<BuildingCost> costs;
    public float buildTime;
    public int instantCompleteCost;
    
    [TextArea(3, 5)]
    public string description;
    
    // 비동기 로드
    public async Task<GameObject> LoadPrefabAsync()
    {
        var handle = prefabReference.LoadAssetAsync<GameObject>();
        return await handle.Task;
    }
}
```

**WeaponData 개선**
```csharp
[System.Serializable]
public class WeaponData
{
    public string weaponId;
    public string weaponName;
    public int attackPower;
    public float attackSpeed;
    public AttackRangeType rangeType;
    
    // Addressable 참조
    public AssetReferenceGameObject weaponPrefabReference;
    public AssetReferenceGameObject projectilePrefabReference; // 원거리 무기용
    public AssetReferenceT<AnimationClip> attackAnimationReference;
    
    public string specialEffect;
    public string recipeId;
    public string itemId;
}
```

**MonsterData 개선**
```csharp
[System.Serializable]
public class MonsterData
{
    public string monsterId;
    public string monsterName;
    public int hp;
    public int attackPower;
    public int defense;
    public float speed;
    
    // Addressable 참조
    public AssetReferenceGameObject prefabReference;
    public AssetReferenceT<Sprite> iconReference;
    public List<AssetReferenceGameObject> dropItemReferences;
    
    public int expReward;
}
```

---

### 3. ResourceManager 구현 (1.5일)

#### AddressableResourceManager
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddressableResourceManager : MonoBehaviour
{
    public static AddressableResourceManager Instance { get; private set; }
    
    // 로드된 에셋 캐시
    private Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();
    
    [Header("=== Preload Settings ===")]
    [SerializeField] private List<AssetReference> _preloadAssets;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 비동기 로드
    public async Task<T> LoadAssetAsync<T>(AssetReference assetReference) where T : Object
    {
        if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
        {
            Debug.LogWarning("Invalid asset reference");
            return null;
        }
        
        string key = assetReference.AssetGUID;
        
        // 캐시 확인
        if (_loadedAssets.TryGetValue(key, out object cached))
        {
            return cached as T;
        }
        
        // 로드
        var handle = Addressables.LoadAssetAsync<T>(assetReference);
        T asset = await handle.Task;
        
        if (asset != null)
        {
            _loadedAssets[key] = asset;
            _handles[key] = handle;
        }
        
        return asset;
    }
    
    // 동기 로드 (주의: Addressable은 기본 비동기)
    public T LoadAsset<T>(AssetReference assetReference) where T : Object
    {
        // 비동기 결과를 동기적으로 기다림 (주의 깊게 사용)
        var task = LoadAssetAsync<T>(assetReference);
        task.Wait();
        return task.Result;
    }
    
    // 프리팹 인스턴스화
    public async Task<GameObject> InstantiatePrefabAsync(AssetReferenceGameObject prefabReference, 
        Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var prefab = await LoadAssetAsync<GameObject>(prefabReference);
        if (prefab == null) return null;
        
        GameObject instance;
        if (parent != null)
        {
            instance = Instantiate(prefab, position, rotation, parent);
        }
        else
        {
            instance = Instantiate(prefab, position, rotation);
        }
        
        // Addressable 인스턴스 트래킹 (옵션)
        var tracker = instance.AddComponent<AddressableInstanceTracker>();
        tracker.Initialize(prefabReference.AssetGUID);
        
        return instance;
    }
    
    // 라벨로 일괄 로드
    public async Task PreloadAssetsByLabel(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = await locationsHandle.Task;
        
        var loadTasks = new List<Task>();
        foreach (var location in locations)
        {
            loadTasks.Add(LoadByLocation(location));
        }
        
        await Task.WhenAll(loadTasks);
        Addressables.Release(locationsHandle);
    }
    
    private async Task LoadByLocation(IResourceLocation location)
    {
        var handle = Addressables.LoadAssetAsync<Object>(location);
        Object asset = await handle.Task;
        
        if (asset != null)
        {
            _loadedAssets[location.PrimaryKey] = asset;
            _handles[location.PrimaryKey] = handle;
        }
    }
    
    // 에셋 해제
    public void ReleaseAsset(AssetReference assetReference)
    {
        if (assetReference == null) return;
        
        string key = assetReference.AssetGUID;
        
        if (_handles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle);
            _handles.Remove(key);
        }
        
        _loadedAssets.Remove(key);
    }
    
    // 모든 에셋 해제
    public void ReleaseAllAssets()
    {
        foreach (var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }
        
        _handles.Clear();
        _loadedAssets.Clear();
    }
    
    // 씬 전환 시 호출
    public void OnSceneChanged()
    {
        // 씬 전환 시 불필요한 에셋 해제
        // (필요한 경우에만)
    }
}

// 인스턴스 트래커 컴포넌트
public class AddressableInstanceTracker : MonoBehaviour
{
    public string AssetGUID { get; private set; }
    
    public void Initialize(string guid)
    {
        AssetGUID = guid;
    }
    
    private void OnDestroy()
    {
        // 파괴 시 Addressable 카운트 감소
        if (!string.IsNullOrEmpty(AssetGUID))
        {
            Addressables.ReleaseInstance(gameObject);
        }
    }
}
```

---

### 4. Excel Converter 수정 (1일)

#### GameDataConverterWindow 개선
```csharp
public class GameDataConverterWindow : EditorWindow
{
    // ... 기존 코드 ...
    
    [MenuItem("Tools/Game Data/Convert with Addressables Setup")]
    public static void ConvertWithAddressables()
    {
        // 1. 기존 변환 수행
        ConvertAll();
        
        // 2. Addressable 그룹 설정
        SetupAddressableGroups();
        
        // 3. 에셋 참조 업데이트
        UpdateAssetReferences();
    }
    
    private static void SetupAddressableGroups()
    {
        // Addressable 그룹 생성/설정
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        
        // Items 그룹
        CreateOrGetGroup(settings, "Items");
        
        // Buildings 그룹
        CreateOrGetGroup(settings, "Buildings");
        
        // Characters 그룹
        CreateOrGetGroup(settings, "Characters");
    }
    
    private static AddressableAssetGroup CreateOrGetGroup(AddressableAssetSettings settings, string groupName)
    {
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, false, null);
        }
        return group;
    }
    
    private static void UpdateAssetReferences()
    {
        // GameData.asset의 AssetReference 필드 업데이트
        var gameData = AssetDatabase.LoadAssetAtPath<GameDataContainer>(
            "Assets/ScriptableObjects/GameData/GameData.asset");
        
        if (gameData == null) return;
        
        // 각 아이템의 iconPath를 AssetReference로 변환
        foreach (var item in gameData.Items)
        {
            if (!string.IsNullOrEmpty(item.iconPath))
            {
                var iconGuid = AssetDatabase.AssetPathToGUID(item.iconPath);
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    item.iconReference = new AssetReferenceT<Sprite>(iconGuid);
                }
            }
        }
        
        // 변경사항 저장
        EditorUtility.SetDirty(gameData);
        AssetDatabase.SaveAssets();
    }
}
```

---

### 5. 런타임 로딩 패턴 (1일)

#### 데이터 로더
```csharp
public class GameDataLoader : MonoBehaviour
{
    [Inject] private AddressableResourceManager _resourceManager;
    
    // 아이템 데이터 로드
    public async Task<ItemData> LoadItemData(string itemId)
    {
        var gameData = await LoadGameDataAsync();
        var item = gameData.GetItem(itemId);
        
        if (item != null)
        {
            // 필요 시 아이콘 미리 로드
            await item.LoadIconAsync();
        }
        
        return item;
    }
    
    // 건설물 프리팹 생성
    public async Task<GameObject> CreateBuilding(string buildingId, Vector3 position)
    {
        var gameData = await LoadGameDataAsync();
        var building = gameData.GetBuilding(buildingId);
        
        if (building == null)
        {
            Debug.LogError($"Building not found: {buildingId}");
            return null;
        }
        
        return await _resourceManager.InstantiatePrefabAsync(
            building.prefabReference, position, Quaternion.identity);
    }
    
    // 몬스터 스폰
    public async Task<GameObject> SpawnMonster(string monsterId, Vector3 position)
    {
        var gameData = await LoadGameDataAsync();
        var monster = gameData.GetMonster(monsterId);
        
        if (monster == null) return null;
        
        return await _resourceManager.InstantiatePrefabAsync(
            monster.prefabReference, position, Quaternion.identity);
    }
    
    // 게임 데이터 비동기 로드
    private async Task<GameDataContainer> LoadGameDataAsync()
    {
        // GameData.asset도 Addressable로 관리
        var handle = Addressables.LoadAssetAsync<GameDataContainer>("GameData");
        return await handle.Task;
    }
}
```

#### 씬 전환 시 로딩
```csharp
public class SceneLoader : MonoBehaviour
{
    [Inject] private AddressableResourceManager _resourceManager;
    
    public async Task LoadScene(string sceneName)
    {
        // 1. 현재 씬 에셋 해제
        _resourceManager.ReleaseAllAssets();
        
        // 2. 다음 씬 필요 에셋 프리로드
        await PreloadSceneAssets(sceneName);
        
        // 3. 씬 로드
        await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single).Task;
    }
    
    private async Task PreloadSceneAssets(string sceneName)
    {
        // 씬별 필요 에셋 라벨
        string label = $"Scene_{sceneName}";
        await _resourceManager.PreloadAssetsByLabel(label);
    }
}
```

---

### 6. Addressable 빌드 설정 (0.5일)

#### 에셋 레이블 규칙
```
Label 규칙
├── Core (필수 항상 메모리)
│   ├── GameData
│   ├── Player
│   └── CoreUI
├── Items
│   ├── Icons (아이템 아이콘)
│   └── Prefabs (월드 아이템)
├── Buildings
│   ├── Housing
│   ├── Commerce
│   └── Tourism
├── Characters
│   ├── Player
│   ├── NPCs
│   └── Monsters
└── Audio
    ├── BGM
    └── SFX
```

#### 에셋 그룹 설정
```
Group: Core
- Bundle Mode: Pack Together
- Compression: LZ4

Group: Items
- Bundle Mode: Pack Together by Label
- Compression: LZ4

Group: Buildings
- Bundle Mode: Pack Separately
- Compression: LZ4

Group: Characters
- Bundle Mode: Pack Together by Label
- Compression: LZ4
```

---

## 개선 일정

| 작업 | 예상 시간 | 우선순위 |
|------|-----------|----------|
| Addressable 패키지 설치 및 설정 | 0.5일 | P0 |
| Addressable 그룹 구성 | 0.5일 | P0 |
| 데이터 클래스에 AssetReference 추가 | 1일 | P0 |
| AddressableResourceManager 구현 | 1일 | P0 |
| Excel Converter 수정 | 1일 | P1 |
| 런타임 로딩 패턴 구현 | 1일 | P1 |
| 기존 코드 마이그레이션 | 1일 | P1 |
| 테스트 및 디버깅 | 1일 | P1 |
| **총합** | **7일** | |

---

## 마이그레이션 가이드

### 기존 코드 → Addressable 코드

**변경 전**
```csharp
// 직접 참조
public Sprite icon;
public GameObject prefab;

// 사용
Instantiate(prefab);
```

**변경 후**
```csharp
// Addressable 참조
public AssetReferenceT<Sprite> iconReference;
public AssetReferenceGameObject prefabReference;

// 사용
var prefab = await prefabReference.LoadAssetAsync<GameObject>();
Instantiate(prefab);
```

**안전한 로딩 패턴**
```csharp
// null 체크 필수
if (data.iconReference != null && data.iconReference.RuntimeKeyIsValid())
{
    var sprite = await data.LoadIconAsync();
}
```

---

## 테스트 체크리스트

- [ ] Addressable 빌드 성공
- [ ] 에셋 비동기 로드 성공
- [ ] 프리팹 인스턴스화 성공
- [ ] 에셋 해제 시 메모리 정리 확인
- [ ] 씬 전환 시 에셋 로딩/해제 확인
- [ ] 중복 로드 시 캐싱 확인
- [ ] 빌드된 번들 크기 확인
