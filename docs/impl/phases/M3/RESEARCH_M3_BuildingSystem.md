# M3 리서치 문서: 건설 시스템 (BuildingSystem)

> **대상 시스템**: BuildingSystem, Building
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M3

---

## 1. 개요

### 리서치 목적
건물 건설 및 관리 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 건물 배치
  - 건설 진행
  - 건물 완료/업그레이드/철거
- **제외**: 
  - 건설 UI (M3-6에서 처리)

### 예상 산출물
- `BuildingSystem.cs` - 건설 관리
- `Building.cs` - 건물 클래스
- `BuildingManager.cs` - 건물 관리자

---

## 2. 요구사항 정리

### 기능적 요구사항

1. **건설 모드**
   - 건설 모드 진입/종료
   - 건물 미리보기
   - 배치 가능 여부 확인

2. **건물 배치**
   - 타일 기준 배치
   - 건설 가능한 타입 확인
   - 자원 소모

3. **건설 진행**
   - 하루 단위 진행
   - 건설 시간
   - 자재 필요량

4. **건물 관리**
   - 완료 처리
   - 업그레이드
   - 철거

### 인터페이스 정의

```csharp
public interface IBuilding
{
    string BuildingId { get; }
    BuildingState State { get; }
    int CurrentLevel { get; }
    
    void Place(Vector3 position);
    void StartConstruction();
    void ProgressConstruction();
    void Complete();
    void Upgrade();
    void Demolish();
}

public enum BuildingState
{
    Preview,        // 미리보기
    Placed,         // 배치됨
    Constructing,   // 건설 중
    Completed,      // 완료
    Upgrading       // 업그레이드 중
}
```

---

## 3. 기존 코드 분석

### BuildingData 분석

**파일 경로**: `Assets/Scripts/GameData/BuildingData.cs`

**주요 필드**:
- buildingId: 건물 ID
- buildTime: 건설 시간
- cost: 건설 비용
- size: 크기
- maxLevel: 최대 레벨

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// Building.cs
public class Building : MonoBehaviour, IBuilding, ISaveable
{
    [Header("=== Building Data ===")]
    [SerializeField] private BuildingData _buildingData;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    public string BuildingId => _buildingData?.buildingId;
    public BuildingState State { get; private set; } = BuildingState.Preview;
    public int CurrentLevel { get; private set; } = 1;
    
    private int _constructionProgress; // 일수
    private Vector3Int _gridPosition;
    
    public string SaveKey => $"Building_{BuildingId}_{gameObject.GetInstanceID()}";
    
    public void Place(Vector3Int gridPosition)
    {
        _gridPosition = gridPosition;
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        State = BuildingState.Placed;
        
        EventBus.Publish(new BuildingPlacedEvent
        {
            BuildingId = BuildingId,
            Position = gridPosition
        });
    }
    
    public void StartConstruction()
    {
        if (State != BuildingState.Placed) return;
        
        State = BuildingState.Constructing;
        _constructionProgress = 0;
        
        EventBus.Publish(new ConstructionStartedEvent
        {
            BuildingId = BuildingId,
            TotalDays = _buildingData?.buildTime ?? 1
        });
    }
    
    public void ProgressConstruction()
    {
        if (State != BuildingState.Constructing) return;
        
        _constructionProgress++;
        
        if (_constructionProgress >= (_buildingData?.buildTime ?? 1))
        {
            Complete();
        }
        else
        {
            EventBus.Publish(new ConstructionProgressedEvent
            {
                BuildingId = BuildingId,
                Progress = _constructionProgress,
                Total = _buildingData?.buildTime ?? 1
            });
        }
    }
    
    public void Complete()
    {
        State = BuildingState.Completed;
        
        EventBus.Publish(new ConstructionCompletedEvent
        {
            BuildingId = BuildingId,
            Level = CurrentLevel
        });
    }
    
    public void Upgrade()
    {
        if (State != BuildingState.Completed) return;
        if (CurrentLevel >= (_buildingData?.maxLevel ?? 1)) return;
        
        State = BuildingState.Upgrading;
        // 업그레이드 로직
    }
    
    public void Demolish()
    {
        EventBus.Publish(new BuildingDemolishedEvent
        {
            BuildingId = BuildingId,
            Position = _gridPosition
        });
        
        Destroy(gameObject);
    }
    
    public object GetSaveData()
    {
        return new BuildingSaveData
        {
            BuildingId = BuildingId,
            State = State,
            CurrentLevel = CurrentLevel,
            ConstructionProgress = _constructionProgress,
            GridPosition = _gridPosition
        };
    }
    
    public void LoadSaveData(object state)
    {
        if (state is BuildingSaveData data)
        {
            State = data.State;
            CurrentLevel = data.CurrentLevel;
            _constructionProgress = data.ConstructionProgress;
            _gridPosition = data.GridPosition;
            
            transform.position = new Vector3(_gridPosition.x, _gridPosition.y, 0);
        }
    }
}

// BuildingManager.cs
public class BuildingManager : MonoBehaviour
{
    [Header("=== Settings ===")]
    [SerializeField] private List<Building> _buildings = new List<Building>();
    
    private void Start()
    {
        EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
    }
    
    private void OnDestroy()
    {
        EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
    }
    
    private void OnDayStarted(DayStartedEvent evt)
    {
        // 건설 중인 건물 진행
        foreach (var building in _buildings)
        {
            if (building.State == BuildingState.Constructing ||
                building.State == BuildingState.Upgrading)
            {
                building.ProgressConstruction();
            }
        }
    }
    
    public void RegisterBuilding(Building building)
    {
        if (building != null && !_buildings.Contains(building))
        {
            _buildings.Add(building);
        }
    }
    
    public void UnregisterBuilding(Building building)
    {
        _buildings.Remove(building);
    }
}

// BuildingSystem.cs
public class BuildingSystem : MonoBehaviour
{
    [Header("=== Settings ===")]
    [SerializeField] private bool _isBuildingMode = false;
    [SerializeField] private BuildingData _selectedBuilding;
    
    public bool IsBuildingMode => _isBuildingMode;
    
    public void EnterBuildMode(BuildingData buildingData)
    {
        _isBuildingMode = true;
        _selectedBuilding = buildingData;
    }
    
    public void ExitBuildMode()
    {
        _isBuildingMode = false;
        _selectedBuilding = null;
    }
    
    public bool CanBuild(Vector3Int position)
    {
        // TODO: 타일 확인, 자원 확인 등
        return true;
    }
    
    public void Build(Vector3Int position)
    {
        if (!CanBuild(position)) return;
        
        // 건물 프리팹 생성 (TODO: Addressables 로드)
        // Building building = Instantiate(buildingPrefab);
        // building.Place(position);
        // building.StartConstruction();
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | Building 클래스 | 2h |
| 2 | BuildingManager | 1h |
| 3 | BuildingSystem | 1h |
| 4 | 건설 이벤트 | 0.5h |
| 5 | 테스트 | 1h |

---

## 리서치 완료 확인

- [x] 요구사항 정리 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
