# M2 리서치 문서: 채집 시스템 (GatheringSystem)

> **대상 시스템**: GatheringSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
나무, 광석, 약초 등 자원을 채집하는 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 채집 가능한 자원 관리
  - 상호작용 기반 채집
  - 도구 내구도
  - 자원 재생성
- **제외**: 
  - 채집 애니메이션
  - 파티클 효과

### 예상 산출물
- `GatherableResource.cs` - 추상 기본 클래스
- `TreeResource.cs` - 나무
- `RockResource.cs` - 광석
- `HerbResource.cs` - 약초
- `GatheringSystem.cs` - 관리자
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 요구사항 정리

#### 기능적 요구사항

1. **자원 유형**
   - 나무 (Tree): 도끼 필요, 목재 획득
   - 광석 (Rock): 곡괭이 필요, 광석 획득
   - 약초 (Herb): 손으로 채집, 약초 획득

2. **채집 프로세스**
   - 상호작용 시작
   - 진행바 표시 (M3 UI)
   - 채집 완료 시 아이템 획득
   - 자원 소멸 또는 재생성 대기

3. **도구 시스템**
   - 필요한 도구 타입 확인
   - 도구 내구도 감소
   - 내구도 0 시 파괴

4. **재생성 시스템**
   - ResourceSpawnData.regenDays 기준
   - TimeManager 연동
   - 재생성 시점에 복원

### 인터페이스 정의

```csharp
public interface IGatherableResource
{
    string ResourceId { get; }
    ResourceType Type { get; }
    bool IsAvailable { get; }
    
    bool CanGather(ToolType equippedTool);
    bool Gather(int damage);
    void OnResourceDepleted();
    void Respawn();
}

public enum ResourceType
{
    Tree,
    Rock,
    Herb
}
```

---

## 3. 기존 코드 분석

### ResourceSpawnData 분석

**파일 경로**: `Assets/Scripts/GameData/ResourceSpawnData.cs`

**주요 필드**:
- regenDays: 재생성에 필요한 일수
- maxQuantity: 최대 채집량

### ToolData 분석

**파일 경로**: `Assets/Scripts/GameData/ToolData.cs`

**주요 필드**:
- toolType: Axe, Pickaxe, Hoe, WateringCan, FishingRod
- durability: 내구도
- effectValue: 효과값 (채집량 등)

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// GatherableResource.cs (추상 클래스)
public abstract class GatherableResource : MonoBehaviour, IGatherableResource, ISaveable, IInteractable
{
    [Header("=== Settings ===")]
    [SerializeField] protected string _resourceId;
    [SerializeField] protected ResourceType _type;
    [SerializeField] protected int _health = 100;
    [SerializeField] protected int _gatherAmount = 1;
    [SerializeField] protected string _dropItemId;
    
    [Inject]
    protected IEventBus _eventBus;
    
    [Inject]
    protected IInventory _inventory;
    
    public string ResourceId => _resourceId;
    public ResourceType Type => _type;
    public bool IsAvailable => gameObject.activeSelf && _health > 0;
    
    protected int _currentHealth;
    protected int _daysSinceDepleted;
    protected bool _isDepleted;
    
    protected virtual void Awake()
    {
        _currentHealth = _health;
    }
    
    public abstract bool CanGather(ToolType equippedTool);
    
    public virtual bool Gather(int damage)
    {
        if (!IsAvailable) return false;
        
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            OnResourceDepleted();
            return true;
        }
        
        return false;
    }
    
    public virtual void OnResourceDepleted()
    {
        _isDepleted = true;
        _daysSinceDepleted = 0;
        
        // 아이템 드랍
        _inventory.AddItem(_dropItemId, _gatherAmount);
        
        _eventBus.Publish(new ResourceGatheredEvent
        {
            ResourceId = _resourceId,
            Type = _type,
            Amount = _gatherAmount
        });
        
        // 시각적 소멸
        gameObject.SetActive(false);
    }
    
    public virtual void Respawn()
    {
        _isDepleted = false;
        _currentHealth = _health;
        gameObject.SetActive(true);
        
        _eventBus.Publish(new ResourceRespawnedEvent
        {
            ResourceId = _resourceId
        });
    }
    
    public void OnDayPassed()
    {
        if (_isDepleted)
        {
            _daysSinceDepleted++;
            
            var spawnData = GetSpawnData();
            if (spawnData != null && _daysSinceDepleted >= spawnData.regenDays)
            {
                Respawn();
            }
        }
    }
    
    protected abstract ResourceSpawnData GetSpawnData();
    
    public abstract void Interact();
    
    public object CaptureState()
    {
        return new ResourceSaveData
        {
            ResourceId = _resourceId,
            IsDepleted = _isDepleted,
            DaysSinceDepleted = _daysSinceDepleted,
            CurrentHealth = _currentHealth
        };
    }
    
    public void RestoreState(object state)
    {
        if (state is ResourceSaveData saveData)
        {
            _isDepleted = saveData.IsDepleted;
            _daysSinceDepleted = saveData.DaysSinceDepleted;
            _currentHealth = saveData.CurrentHealth;
            
            gameObject.SetActive(!_isDepleted);
        }
    }
}

// TreeResource.cs
public class TreeResource : GatherableResource
{
    [Header("=== Tree Specific ===")]
    [SerializeField] private int _woodAmount = 3;
    [SerializeField] private string _woodItemId = "wood";
    
    protected override void Awake()
    {
        base.Awake();
        _type = ResourceType.Tree;
    }
    
    public override bool CanGather(ToolType equippedTool)
    {
        return equippedTool == ToolType.Axe;
    }
    
    public override void OnResourceDepleted()
    {
        _gatherAmount = _woodAmount;
        _dropItemId = _woodItemId;
        base.OnResourceDepleted();
    }
    
    protected override ResourceSpawnData GetSpawnData()
    {
        // GameDataLoader에서 조회
        return null;
    }
    
    public override void Interact()
    {
        // PlayerController에서 호출
        // 도구 확인 후 Gather 호출
    }
}

// GatheringSystem.cs
public class GatheringSystem : MonoBehaviour
{
    [Inject]
    private IEventBus _eventBus;
    
    private List<GatherableResource> _resources = new List<GatherableResource>();
    
    private void Start()
    {
        _eventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        FindAllResources();
    }
    
    private void FindAllResources()
    {
        _resources.Clear();
        _resources.AddRange(FindObjectsOfType<GatherableResource>());
    }
    
    private void OnDayStarted(DayStartedEvent evt)
    {
        foreach (var resource in _resources)
        {
            resource.OnDayPassed();
        }
    }
    
    public void RegisterResource(GatherableResource resource)
    {
        if (!_resources.Contains(resource))
        {
            _resources.Add(resource);
        }
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | GatherableResource 추상 클래스 | 2h |
| 2 | TreeResource 구현 | 1h |
| 3 | RockResource 구현 | 1h |
| 4 | HerbResource 구현 | 1h |
| 5 | GatheringSystem 관리자 | 1h |
| 6 | 재생성 시스템 | 1h |

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
