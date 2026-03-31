# M2 리서치 문서: 생존 시스템 (Survival Systems)

> **대상 시스템**: HungerSystem, HealthSystem, StaminaSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
M2 마일스톤의 핵심 생존 시스템(Hunger, Health, Stamina)을 구현하기 위한 기술적 설계 및 구현 방향을 결정합니다.

### 리서치 범위
- **포함**: 
  - HungerSystem: 허기 수치 관리 및 상태 효과
  - HealthSystem: 체력 관리 및 데미지/회복
  - StaminaSystem: 스태미나 관리 및 소모/회복
  - 세 시스템 간 상호작용
- **제외**: 
  - UI 구현 (M3에서 처리)
  - 사운드 효과

### 예상 산출물
- `HungerSystem.cs` - 허기 시스템
- `HealthSystem.cs` - 체력 시스템  
- `StaminaSystem.cs` - 스태미나 시스템
- `ISurvivalSystem.cs` - 공통 인터페이스
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 관련 기획 문서

| 문서명 | 경로 | 버전 | 상태 |
|--------|------|------|------|
| GDD | ../game-design/GDD.md | - | 참고 |
| CHECKLIST | CHECKLIST.md | - | 참고 |

### 요구사항 정리

#### 기능적 요구사항

**HungerSystem (허기 시스템)**
1. 허기 수치 관리 (0-100)
   - 상세: 시간에 따라 자연 감소
   - 우선순위: P0
   
2. 허기 상태 enum
   - Full (80-100): 정상
   - Normal (40-79): 정상
   - Hungry (20-39): 체력 자동 회복 불가
   - Starving (0-19): 체력 감소
   - 우선순위: P0

3. 음식 소비
   - 상세: 아이템 사용으로 허기 회복
   - 우선순위: P0

**HealthSystem (체력 시스템)**
1. 체력 수치 관리 (0-Max)
   - 상세: 데미지/회복 처리
   - 우선순위: P0

2. 자동 회복
   - 상세: 허기가 Normal 이상일 때 천천히 회복
   - 우선순위: P1

3. 사망 처리
   - 상세: 체력 0 시 사망 이벤트 발행
   - 우선순위: P0

**StaminaSystem (스태미나 시스템)**
1. 스태미나 수치 관리 (0-Max)
   - 상세: 행동에 따른 소모
   - 우선순위: P0

2. 소모 트리거
   - 상세: 달리기, 구르기, 공격 시 소모
   - 우선순위: P0

3. 자동 회복
   - 상세: 행동하지 않을 때 회복
   - 우선순위: P0

#### 비기능적 요구사항
1. **성능**: Update에서 직접 계산하지 않고 시간 기반 계산
2. **확장성**: 향후 버프/디버프 시스템과 연동 가능
3. **저장**: ISaveable 인터페이스 구현

### 인터페이스 정의

```csharp
// 생존 시스템 공통 인터페이스
public interface ISurvivalSystem
{
    float CurrentValue { get; }
    float MaxValue { get; }
    float Percentage { get; }
    
    void Modify(float amount);
    void SetValue(float value);
    void Reset();
}

// 허기 시스템 인터페이스
public interface IHungerSystem : ISurvivalSystem
{
    HungerState CurrentState { get; }
    void Eat(IFoodItem food);
}

// 체력 시스템 인터페이스
public interface IHealthSystem : ISurvivalSystem
{
    void TakeDamage(int amount, string source);
    void Heal(int amount, string source);
    bool IsDead { get; }
}

// 스태미나 시스템 인터페이스
public interface IStaminaSystem : ISurvivalSystem
{
    bool TryConsume(float amount);
    void Consume(float amount);
    void StopConsumption();
}
```

---

## 3. 기존 코드 분석

### 3.1 관련 클래스 분석

#### 클래스 1: PlayerController

**파일 경로**: `Assets/Scripts/Player/PlayerController.cs`

**분석 내용**:
- 목적: 플레이어 입력 및 이동 처리
- 주요 기능: 이동, 스프린트, 구르기
- 의존성: InputService, Animator
- 재사용 가능성: 스태미나 시스템과 연동 필요 (스프린트/구르기 시 소모)

**코드 예시**:
```csharp
// 스프린트와 구르기에서 스태미나 체크 필요
private void HandleSprint()
{
    _isSprinting = _inputService.IsSprintPressed && CanSprint();
    // 스태미나 시스템과 연동 예정
}
```

**의견**: PlayerController에서 스태미나 소모 호출하도록 수정 필요

---

#### 클래스 2: SurvivalEvents

**파일 경로**: `Assets/Scripts/Events/SurvivalEvents.cs`

**분석 내용**:
- 목적: 생존 관련 이벤트 정의
- 주요 기능: HungerChangedEvent, HealthChangedEvent, StaminaChangedEvent
- 의존성: 없음
- 재사용 가능성: ✅ 이미 정의됨, 직접 사용

**코드 예시**:
```csharp
public class HungerChangedEvent
{
    public float CurrentHunger { get; set; }
    public float MaxHunger { get; set; }
    public HungerState State { get; set; }
}
```

**의견**: 이벤트 클래스 이미 구현됨, 시스템에서 발행만 하면 됨

---

#### 클래스 3: PlayerEvents

**파일 경로**: `Assets/Scripts/Events/PlayerEvents.cs`

**분석 내용**:
- 목적: 플레이어 관련 이벤트 정의
- 주요 기능: PlayerDamagedEvent, PlayerDiedEvent 등
- 의존성: 없음
- 재사용 가능성: ✅ HealthSystem에서 사용

---

### 3.2 데이터 클래스 분석

#### FishData

**파일 경로**: `Assets/Scripts/GameData/FishData.cs`

**분석 내용**:
- 목적: 물고기 데이터 정의
- 주요 필드: hungerRestore (허기 회복량)
- 연관 데이터: ItemData

**의견**: 음식 아이템의 허기 회복량은 ItemData에 추가하거나 별도 음식 데이터 필요

---

## 4. 의존성 분석

### 4.1 의존하는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| EventBus | 강함 | 상태 변경 시 이벤트 발행 |
| TimeManager | 강함 | 시간 기반 자연 감소 |
| PlayerController | 강함 | 스태미나 소모 트리거 |

### 4.2 의존되는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| InventorySystem | 약함 | 음식 아이템 소비 |
| SaveSystem | 강함 | 저장/불러오기 |

### 4.3 의존성 그래프

```
SurvivalSystems
    ├── EventBus (Publish)
    ├── TimeManager (Subscribe: TimeChangedEvent)
    ├── PlayerController (Consume Stamina)
    └── SaveSystem (ISaveable)
```

---

## 5. 구현 방향 결정

### 5.1 아키텍처 결정

#### 결정 사항 1: 시스템 구성 방식

**선택지**:
1. **개별 MonoBehaviour**: HungerSystem, HealthSystem, StaminaSystem 각각 분리
   - 장점: 단일 책임, 테스트 용이
   - 단점: GameObject에 여러 컴포넌트 필요

2. **통합 SurvivalManager**: 하나의 클래스에서 모두 관리
   - 장점: 간단한 구조
   - 단점: 복잡해지면 관리 어려움

3. **하이브리드**: 인터페이스 기반 개별 구현 + 관리용 Manager
   - 장점: 유연성, 확장성
   - 단점: 구조 복잡

**결정**: 선택지 1 (개별 MonoBehaviour)

**사유**: 
- 각 시스템이 독립적으로 동작하며 명확한 책임 분리
- DI Container를 통한 주입이 용이함
- 테스트 및 디버깅이 쉬움
- M2 후에도 각 시스템별로 버프/디버프 추가가 쉬움

---

#### 결정 사항 2: 스태미나 소모 방식

**선택지**:
1. **Pull 방식**: PlayerController가 매 프레임 소모 요청
2. **Push 방식**: 스프린트/구르기 시작/종료 시에만 요청

**결정**: 선택지 2 (Push 방식)

**사유**: 효율적이며 코드가 간결함

---

#### 결정 사항 3: 자연 감소 계산 방식

**선택지**:
1. **Update 기반**: 매 프레임 감소
2. **시간 기반**: TimeManager 이벤트 기반

**결정**: 선택지 2 (시간 기반)

**사유**: 
- 성능상 이점
- 일시정지/타임랩스와 자연스럽게 연동

---

### 5.2 클래스 설계 초안

```csharp
// HungerSystem.cs
public class HungerSystem : MonoBehaviour, IHungerSystem, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private float _maxHunger = 100f;
    [SerializeField] private float _decayPerHour = 5f; // 게임 시간 1시간당 감소량
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private ITimeManager _timeManager;
    
    private float _currentHunger;
    
    public float CurrentValue => _currentHunger;
    public float MaxValue => _maxHunger;
    public float Percentage => _currentHunger / _maxHunger;
    public HungerState CurrentState => GetHungerState();
    
    private void Start()
    {
        _eventBus.Subscribe<TimeChangedEvent>(OnTimeChanged);
    }
    
    public void Eat(IFoodItem food)
    {
        Modify(food.HungerRestore);
    }
    
    private void OnTimeChanged(TimeChangedEvent evt)
    {
        // 시간 경과에 따른 허기 감소
        if (evt.DayDelta > 0 || evt.HourDelta > 0)
        {
            float totalHours = evt.DayDelta * 24 + evt.HourDelta;
            Modify(-_decayPerHour * totalHours);
        }
    }
    
    public object CaptureState()
    {
        return new HungerSaveData { CurrentHunger = _currentHunger };
    }
    
    public void RestoreState(object state)
    {
        if (state is HungerSaveData data)
        {
            _currentHunger = data.CurrentHunger;
            PublishChangedEvent();
        }
    }
}

// HealthSystem.cs
public class HealthSystem : MonoBehaviour, IHealthSystem, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _regenPerHour = 2f; // 자연 회복량
    [SerializeField] private float _hungerRegenThreshold = 40f; // 회복 가능 허기 기준
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private IHungerSystem _hungerSystem;
    
    private int _currentHealth;
    
    public float CurrentValue => _currentHealth;
    public float MaxValue => _maxHealth;
    public float Percentage => (float)_currentHealth / _maxHealth;
    public bool IsDead => _currentHealth <= 0;
    
    public void TakeDamage(int amount, string source)
    {
        if (IsDead) return;
        
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        PublishChangedEvent(-amount);
        
        if (IsDead)
        {
            _eventBus.Publish(new PlayerDiedEvent { DeathReason = source });
        }
    }
    
    public void Heal(int amount, string source)
    {
        if (IsDead) return;
        
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        PublishChangedEvent(amount);
    }
}

// StaminaSystem.cs
public class StaminaSystem : MonoBehaviour, IStaminaSystem, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _sprintCost = 10f; // 초당 소모량
    [SerializeField] private float _rollCost = 25f; // 구르기 소모량
    [SerializeField] private float _regenRate = 15f; // 초당 회복량
    [SerializeField] private float _regenDelay = 1f; // 소모 후 회복 시작까지 지연
    
    [Inject]
    private IEventBus _eventBus;
    
    private float _currentStamina;
    private float _timeSinceLastConsumption;
    private bool _isConsuming;
    
    public float CurrentValue => _currentStamina;
    public float MaxValue => _maxStamina;
    public float Percentage => _currentStamina / _maxStamina;
    
    private void Update()
    {
        if (_isConsuming)
        {
            _timeSinceLastConsumption = 0f;
        }
        else
        {
            _timeSinceLastConsumption += Time.deltaTime;
            if (_timeSinceLastConsumption >= _regenDelay)
            {
                Regenerate();
            }
        }
        
        _isConsuming = false; // 매 프레임 리셋
    }
    
    public bool TryConsume(float amount)
    {
        if (_currentStamina >= amount)
        {
            Consume(amount);
            return true;
        }
        return false;
    }
    
    public void Consume(float amount)
    {
        _currentStamina = Mathf.Max(0, _currentStamina - amount);
        _isConsuming = true;
        PublishChangedEvent(-amount);
    }
    
    public void StopConsumption()
    {
        _isConsuming = false;
    }
    
    private void Regenerate()
    {
        if (_currentStamina < _maxStamina)
        {
            float oldValue = _currentStamina;
            _currentStamina = Mathf.Min(_maxStamina, _currentStamina + _regenRate * Time.deltaTime);
            PublishChangedEvent(_currentStamina - oldValue);
        }
    }
}
```

---

### 5.3 파일 구조

```
Scripts/
├── Survival/
│   ├── HungerSystem.cs
│   ├── HealthSystem.cs
│   ├── StaminaSystem.cs
│   ├── Interfaces/
│   │   ├── ISurvivalSystem.cs
│   │   ├── IHungerSystem.cs
│   │   ├── IHealthSystem.cs
│   │   └── IStaminaSystem.cs
│   └── Data/
│       ├── HungerSaveData.cs
│       ├── HealthSaveData.cs
│       └── StaminaSaveData.cs
```

---

## 6. 리스크 분석

### 6.1 기술적 리스크

| 리스크 | 영향도 | 가능성 | 대응 방안 |
|--------|--------|--------|-----------|
| 세 시스템 간 순환 의존성 | 중간 | 낮음 | 인터페이스 기반 설계로 단방향 의존성 유지 |
| 부동소수점 오차 | 낮음 | 중간 | int 타입 사용 또는 Mathf.Approximately 사용 |

### 6.2 의존성 리스크

| 의존 대상 | 현재 상태 | 리스크 | 대응 방안 |
|-----------|-----------|--------|-----------|
| TimeManager | 완료 | 없음 | TimeChangedEvent 활용 |
| EventBus | 완료 | 없음 | 이벤트 클래스 이미 정의됨 |
| PlayerController | 완료 | 낮음 | 스태미나 소耗 메서드 추가 필요 |

---

## 7. 구현 계획

### 7.1 구현 단계

| 단계 | 작업 내용 | 예상 시간 | 의존성 |
|------|-----------|-----------|--------|
| 1 | 인터페이스 정의 | 0.5h | 없음 |
| 2 | HungerSystem 구현 | 2h | EventBus, TimeManager |
| 3 | HealthSystem 구현 | 2h | EventBus, HungerSystem |
| 4 | StaminaSystem 구현 | 2h | EventBus |
| 5 | PlayerController 연동 | 1h | StaminaSystem |
| 6 | 테스트 | 1h | 전체 |

### 7.2 테스트 계획

| 테스트 유형 | 테스트 항목 | 방법 |
|-------------|-------------|------|
| 단위 테스트 | 각 시스템 독립 동작 | Unity Test Runner |
| 통합 테스트 | 시스템 간 상호작용 | 수동 테스트 |
| 수동 테스트 | 저장/불러오기 | 인게임 테스트 |

### 7.3 완료 조건

- [ ] 허기가 시간에 따라 감소
- [ ] 음식 아이템 소비로 허기 회복
- [ ] 허기 상태에 따른 체력 회복 여부 결정
- [ ] 체력 0 시 사망 이벤트 발행
- [ ] 스태미나 소모 (달리기, 구르기)
- [ ] 스태미나 자동 회복
- [ ] 저장/불러오기 정상 작동

---

## 8. 질문 및 논의점

### 8.1 미해결 질문

1. **음식 아이템 데이터 구조**
   - 상황: FishData에 hungerRestore 필드가 있음
   - 필요한 정보: 모든 음식 아이템에 대한 허기 회복량 정의 방법

2. **사망 시 처리**
   - 상황: 체력 0 시 이벤트 발행 후 게임오버 처리
   - 필요한 정보: GameManager와의 연동 방식

### 8.2 결정 필요 사항

| 사항 | 선택지 | 기한 | 담당 |
|------|--------|------|------|
| 음식 데이터 관리 | ItemData에 추가 / 별도 FoodData | 즉시 | 개발자 |
| 스태미나 소모 실패 시 | 무시 / 경고 / 강제 정지 | 즉시 | 개발자 |

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 의존성 분석 완료
- [x] 리스크 식별 완료
- [x] 구현 방향 결정 완료
- [x] PLAN 문서 작성 준비 완료

**리서치 완료일**: 2026-03-13

**다음 단계**: PLAN_M2_SurvivalSystems.md 작성
