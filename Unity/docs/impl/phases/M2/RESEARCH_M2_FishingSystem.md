# M2 리서치 문서: 낚시 시스템 (FishingSystem)

> **대상 시스템**: FishingSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
낚시 미니게임과 물고기 획득을 위한 낚시 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 낚시 상태 관리
  - 낚시 미니게임
  - 물고기 확률 계산
  - 물고기 획득
- **제외**: 
  - 낚시 UI (M3에서 처리)
  - 낚싯대 시각화

### 예상 산출물
- `FishingSystem.cs` - 낚시 시스템 관리
- `FishingMiniGame.cs` - 미니게임 로직
- `FishingEvents.cs` - 낚시 관련 이벤트
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 요구사항 정리

#### 기능적 요구사항

1. **낚시 상태**
   - Idle: 대기 상태
   - Casting: 낚싯대 던지기
   - Waiting: 물고기 대기
   - Biting: 물고기가 물음
   - Reeling: 당기는 중
   - Success/Fail: 결과

2. **물고기 확률**
   - 장소별 (FishData.location)
   - 시간대별 (FishData.timeCondition)
   - 난이도별 (FishData.difficulty)
   - 낚싯대 영향 (FishingRodData.successRate, rareFishBonus)

3. **미니게임**
   - 물고기가 물었을 때 타이밍 게임
   - 진행바 관리
   - 성공/실패 조건

4. **보상**
   - 물고기 아이템 획득
   - 허기 회복 (FishData.hungerRestore)

### 인터페이스 정의

```csharp
public interface IFishingSystem
{
    FishingState CurrentState { get; }
    
    bool StartFishing(string fishingRodItemId);
    void Cast(Vector3 position);
    void OnBite();
    void Reel();
    void Cancel();
}

public enum FishingState
{
    Idle,
    Casting,
    Waiting,
    Biting,
    Reeling,
    Success,
    Fail
}
```

---

## 3. 기존 코드 분석

### FishData 분석

**파일 경로**: `Assets/Scripts/GameData/FishData.cs`

**주요 필드**:
- grade: 물고기 등급 (희귀도)
- location: 등장 장소
- timeCondition: 등장 시간대
- difficulty: 난이도
- sellPrice: 판매 가격
- hungerRestore: 허기 회복량

### FishingRodData 분석

**파일 경로**: `Assets/Scripts/GameData/FishingRodData.cs`

**주요 필드**:
- successRate: 성공 확률
- durability: 내구도
- rareFishBonus: 희귀 물고기 보너스

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// FishingSystem.cs
public class FishingSystem : MonoBehaviour, IFishingSystem
{
    [Header("=== Settings ===")]
    [SerializeField] private float _waitTimeMin = 3f;
    [SerializeField] private float _waitTimeMax = 10f;
    [SerializeField] private float _biteWindow = 2f; // 물었을 때 반응 시간
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private IInventory _inventory;
    
    [Inject]
    private IGameDataLoader _dataLoader;
    
    public FishingState CurrentState { get; private set; } = FishingState.Idle;
    
    private string _currentRodId;
    private string _selectedFishId;
    private float _waitTimer;
    private float _biteTimer;
    
    public bool StartFishing(string fishingRodItemId)
    {
        if (CurrentState != FishingState.Idle) return false;
        
        _currentRodId = fishingRodItemId;
        CurrentState = FishingState.Casting;
        
        _eventBus.Publish(new FishingStartedEvent 
        { 
            RodId = _currentRodId 
        });
        
        return true;
    }
    
    public void Cast(Vector3 position)
    {
        if (CurrentState != FishingState.Casting) return;
        
        CurrentState = FishingState.Waiting;
        _waitTimer = Random.Range(_waitTimeMin, _waitTimeMax);
        
        // 물고기 미리 선택 (확률 기반)
        _selectedFishId = SelectFish();
        
        _eventBus.Publish(new FishingCastEvent 
        { 
            Position = position 
        });
    }
    
    private void Update()
    {
        switch (CurrentState)
        {
            case FishingState.Waiting:
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    OnBite();
                }
                break;
                
            case FishingState.Biting:
                _biteTimer -= Time.deltaTime;
                if (_biteTimer <= 0f)
                {
                    Fail("반응 시간 초과");
                }
                break;
        }
    }
    
    public void OnBite()
    {
        if (CurrentState != FishingState.Waiting) return;
        
        CurrentState = FishingState.Biting;
        _biteTimer = _biteWindow;
        
        _eventBus.Publish(new FishBiteEvent 
        { 
            FishId = _selectedFishId 
        });
    }
    
    public void Reel()
    {
        if (CurrentState != FishingState.Biting) return;
        
        // 성공 확률 계산
        var fishData = _dataLoader.GetFishData(_selectedFishId);
        var rodData = _dataLoader.GetFishingRodData(_currentRodId);
        
        float successChance = CalculateSuccessChance(fishData, rodData);
        
        if (Random.value <= successChance)
        {
            Success();
        }
        else
        {
            Fail("물고기가 도망갔습니다");
        }
    }
    
    private void Success()
    {
        CurrentState = FishingState.Success;
        
        // 물고기 획득
        _inventory.AddItem(_selectedFishId, 1);
        
        // 낚싯대 내구도 감소
        // ...
        
        _eventBus.Publish(new FishingSuccessEvent 
        { 
            FishId = _selectedFishId 
        });
        
        CurrentState = FishingState.Idle;
    }
    
    private void Fail(string reason)
    {
        CurrentState = FishingState.Fail;
        
        _eventBus.Publish(new FishingFailEvent 
        { 
            Reason = reason 
        });
        
        CurrentState = FishingState.Idle;
    }
    
    private string SelectFish()
    {
        // 장소, 시간대, 확률 기반 물고기 선택
        // ...
        return "fish_basic";
    }
    
    private float CalculateSuccessChance(FishData fish, FishingRodData rod)
    {
        float baseChance = rod.successRate;
        
        // 난이도에 따른 조정
        switch (fish.difficulty)
        {
            case FishDifficulty.Easy: baseChance *= 1.2f; break;
            case FishDifficulty.Normal: baseChance *= 1.0f; break;
            case FishDifficulty.Hard: baseChance *= 0.8f; break;
            case FishDifficulty.VeryHard: baseChance *= 0.6f; break;
            case FishDifficulty.Extreme: baseChance *= 0.4f; break;
        }
        
        return Mathf.Clamp01(baseChance);
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | FishingSystem 기본 구조 | 2h |
| 2 | 상태 머신 구현 | 2h |
| 3 | 물고기 선택 로직 | 1h |
| 4 | 미니게임 구현 | 2h |
| 5 | 이벤트 발행 | 1h |

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
