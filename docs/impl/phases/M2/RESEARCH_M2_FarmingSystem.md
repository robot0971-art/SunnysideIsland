# M2 리서치 문서: 농사 시스템 (FarmingSystem)

> **대상 시스템**: FarmingSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
작물 심기, 성장, 관리, 수확을 위한 농사 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 농장 터/작물 관리
  - 씨앗 심기
  - 물 주기
  - 성장 단계
  - 수확
  - 잡초/해충 처리
- **제외**: 
  - 농사 UI (M3에서 처리)
  - 비료 시스템 (M4에서 확장)

### 예상 산출물
- `FarmingManager.cs` - 농사 시스템 관리
- `FarmPlot.cs` - 개별 농장 터
- `Crop.cs` - 작물 컴포넌트
- `FarmingEvents.cs` - 농사 관련 이벤트
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 요구사항 정리

#### 기능적 요구사항

1. **FarmPlot (농장 터)**
   - 상태: Empty, Planted, Growing, Ready, Dead
   - 작물 정보 저장
   - 물 주기 상태
   - 잡초/해충 상태

2. **작물 심기**
   - 씨앗 아이템 필요
   - 계절 확인 (CropData.seasons)
   - 심기 가능 여부 확인

3. **성장 시스템**
   - 하루마다 성장 진행
   - 물이 있어야 성장
   - 성장 단계 (0% → 100%)

4. **관리**
   - 물 주기: 하루마다 필요
   - 잡초 제거: 도구 필요
   - 해충 처리: 도구 필요

5. **수확**
   - 성장 완료 시 수확 가능
   - CropData.yieldAmount만큼 획득
   - 작물 아이템 인벤토리에 추가

### 인터페이스 정의

```csharp
public interface IFarmPlot
{
    PlotState State { get; }
    string CropId { get; }
    float GrowthProgress { get; }
    bool IsWatered { get; }
    bool HasWeeds { get; }
    bool HasPests { get; }
    
    bool Plant(string seedItemId);
    bool Water();
    bool RemoveWeeds();
    bool RemovePests();
    bool Harvest();
    void OnDayPassed();
}

public enum PlotState
{
    Empty,      // 비어있음
    Planted,    // 심어짐 (아직 성장 안함)
    Growing,    // 성장 중
    Ready,      // 수확 가능
    Dead        // 죽음 (관리 실패)
}
```

---

## 3. 기존 코드 분석

### CropData 분석

**파일 경로**: `Assets/Scripts/GameData/CropData.cs`

**주요 필드**:
- cropId: 작물 ID
- growthDays: 성장에 필요한 일수
- yieldAmount: 수확량
- seasons: 재배 가능 계절
- seedItemId: 씨앗 아이템 ID
- cropItemId: 수확물 아이템 ID

**의견**: 작물 데이터 구조가 명확히 정의됨

---

### TimeEvents 분석

**파일 경로**: `Assets/Scripts/Events/TimeEvents.cs`

**관련 이벤트**:
- DayStartedEvent: 하루 시작 (성장 트리거)
- SeasonChangedEvent: 계절 변경

**의견**: DayStartedEvent 구독하여 성장 처리

---

## 4. 의존성 분석

### 의존하는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| EventBus | 강함 | 농사 이벤트 발행 |
| TimeManager | 강함 | 하루 경과 이벤트 |
| InventorySystem | 강함 | 씨앗 소모, 작물 획득 |
| CropData | 강함 | 작물 정보 |

---

## 5. 구현 방향 결정

### 클래스 설계

```csharp
// FarmPlot.cs
public class FarmPlot : MonoBehaviour, IFarmPlot, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private SpriteRenderer _plotSprite;
    [SerializeField] private SpriteRenderer _cropSprite;
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private IInventory _inventory;
    
    [Inject]
    private IGameDataLoader _dataLoader;
    
    // 상태
    public PlotState State { get; private set; } = PlotState.Empty;
    public string CropId { get; private set; }
    public float GrowthProgress { get; private set; }
    public bool IsWatered { get; private set; }
    public bool HasWeeds { get; private set; }
    public bool HasPests { get; private set; }
    
    private CropData _cropData;
    private int _daysPlanted;
    
    public bool Plant(string seedItemId)
    {
        if (State != PlotState.Empty) return false;
        
        // 씨앗에서 작물 ID 찾기
        var cropData = FindCropBySeed(seedItemId);
        if (cropData == null) return false;
        
        // 계절 확인
        if (!IsValidSeason(cropData.seasons)) return false;
        
        // 씨앗 소모
        if (!_inventory.RemoveItem(seedItemId, 1)) return false;
        
        // 심기
        CropId = cropData.cropId;
        _cropData = cropData;
        State = PlotState.Planted;
        GrowthProgress = 0f;
        _daysPlanted = 0;
        IsWatered = false;
        
        UpdateVisuals();
        _eventBus.Publish(new CropPlantedEvent 
        { 
            PlotId = GetInstanceID(),
            CropId = CropId 
        });
        
        return true;
    }
    
    public bool Water()
    {
        if (State == PlotState.Empty || State == PlotState.Dead) return false;
        if (IsWatered) return false;
        
        IsWatered = true;
        UpdateVisuals();
        return true;
    }
    
    public void OnDayPassed()
    {
        if (State != PlotState.Planted && State != PlotState.Growing) return;
        
        // 물이 없으면 성장 안함
        if (!IsWatered)
        {
            // 3일 연속 물 없으면 죽음
            // ... (구현)
            return;
        }
        
        _daysPlanted++;
        GrowthProgress = (float)_daysPlanted / _cropData.growthDays;
        
        if (GrowthProgress >= 1f)
        {
            State = PlotState.Ready;
            _eventBus.Publish(new CropReadyEvent 
            { 
                PlotId = GetInstanceID(),
                CropId = CropId 
            });
        }
        else if (GrowthProgress > 0f)
        {
            State = PlotState.Growing;
        }
        
        IsWatered = false; // 다음날 물 필요
        
        // 잡초/해충 확률 발생
        TrySpawnWeedsOrPests();
        
        UpdateVisuals();
    }
    
    public bool Harvest()
    {
        if (State != PlotState.Ready) return false;
        
        // 작물 획득
        _inventory.AddItem(_cropData.cropItemId, _cropData.yieldAmount);
        
        _eventBus.Publish(new CropHarvestedEvent 
        { 
            PlotId = GetInstanceID(),
            CropId = CropId,
            Amount = _cropData.yieldAmount
        });
        
        // 터 초기화
        Reset();
        return true;
    }
    
    private void Reset()
    {
        State = PlotState.Empty;
        CropId = null;
        _cropData = null;
        GrowthProgress = 0f;
        IsWatered = false;
        HasWeeds = false;
        HasPests = false;
        UpdateVisuals();
    }
    
    public object CaptureState()
    {
        return new FarmPlotSaveData
        {
            State = State,
            CropId = CropId,
            GrowthProgress = GrowthProgress,
            IsWatered = IsWatered,
            HasWeeds = HasWeeds,
            HasPests = HasPests,
            DaysPlanted = _daysPlanted
        };
    }
    
    public void RestoreState(object state)
    {
        if (state is FarmPlotSaveData saveData)
        {
            State = saveData.State;
            CropId = saveData.CropId;
            GrowthProgress = saveData.GrowthProgress;
            IsWatered = saveData.IsWatered;
            HasWeeds = saveData.HasWeeds;
            HasPests = saveData.HasPests;
            _daysPlanted = saveData.DaysPlanted;
            
            if (!string.IsNullOrEmpty(CropId))
            {
                _cropData = _dataLoader.GetCropData(CropId);
            }
            
            UpdateVisuals();
        }
    }
}

// FarmingManager.cs
public class FarmingManager : MonoBehaviour
{
    [Inject]
    private IEventBus _eventBus;
    
    private List<FarmPlot> _plots = new List<FarmPlot>();
    
    private void Start()
    {
        _eventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        FindAllPlots();
    }
    
    private void FindAllPlots()
    {
        _plots.Clear();
        _plots.AddRange(FindObjectsOfType<FarmPlot>());
    }
    
    private void OnDayStarted(DayStartedEvent evt)
    {
        foreach (var plot in _plots)
        {
            plot.OnDayPassed();
        }
    }
    
    public void RegisterPlot(FarmPlot plot)
    {
        if (!_plots.Contains(plot))
        {
            _plots.Add(plot);
        }
    }
}
```

---

## 6. 구현 계획

| 단계 | 작업 내용 | 예상 시간 | 의존성 |
|------|-----------|-----------|--------|
| 1 | FarmPlot 기본 구현 | 2h | 없음 |
| 2 | 작물 심기/수확 | 2h | InventorySystem |
| 3 | 성장 시스템 | 2h | TimeManager |
| 4 | 관리 기능 (물/잡초/해충) | 2h | 없음 |
| 5 | FarmingManager | 1h | FarmPlot |
| 6 | 이벤트 정의 및 발행 | 1h | EventBus |
| 7 | 저장/불러오기 | 1h | SaveSystem |

### 완료 조건

- [ ] 씨앗 심기 가능
- [ ] 하루마다 성장 진행
- [ ] 물 주기 필요
- [ ] 성장 완료 시 수확 가능
- [ ] 인벤토리에 작물 추가
- [ ] 저장/불러오기 정상 작동

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 의존성 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
