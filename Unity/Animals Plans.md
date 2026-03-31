# Feature Plans - Sunnyside Island

이 파일은 향후 구현할 기능들의 계획을 저장하는 문서입니다.

---

## 1. 계란 부화 시스템 (Egg Hatching System)

### 개요
바닥에 방치된 계란에서 하루가 지나면 닭이 태어나는 시스템

### 상세 계획

**파일 수정:**
- `Assets/Scripts/Items/EggItem.cs` - 부화 타이머 및 닭 생성 로직 추가
- `Assets/Scripts/Core/TimeManager.cs` - 날짜 변경 이벤트 발행 (DayStartedEvent 이미 존재)

**구현 내용:**

1. **EggItem.cs 수정**
```csharp
[Header("=== Hatching Settings ===")]
[SerializeField] private int _hatchAfterDays = 1;  // 하루 후 부화
private int _spawnDay;  // 생성된 날짜

// DayStartedEvent 구독
// 바닥에 있고 하루가 지났으면 부화
private void OnDayStarted(DayStartedEvent evt)
{
    if (IsOnGround && evt.Day >= _spawnDay + _hatchAfterDays)
    {
        Hatch();
    }
}

private void Hatch()
{
    // 닭 프리팹 생성
    var chicken = Instantiate(chickenPrefab, transform.position, Quaternion.identity);
    var ai = chicken.GetComponent<ChickenAI>();
    ai.ChickenColor = Random.value > 0.5f ? ChickenColor.White : ChickenColor.Black;
    
    // Egg 제거
    Destroy(gameObject);
}
```

2. **닭 프리팹 참조**
- EggItem에 Chicken 프리팹 할당 필요
- 또는 Resources에서 로드

3. **고려사항**
- 인벤토리에 있는 계란은 부화하지 않음 (IsOnGround 체크)
- 최대 닭 수 제한 필요? (무한 생성 방지)
- 닭 비율: 50:50 랜덤

### 테스트 항목
- [ ] 계란이 바닥에 있을 때 하루 지나면 닭 생성
- [ ] 인벤토리에 있는 계란은 부화하지 않음
- [ ] 생성된 닭이 흰색/검은색 랜덤으로 나옴
- [ ] 생성된 닭이 정상적으로 활동함

---

## 2. Player 사냥 기능 (Player Hunting System)

### 개요
Player가 닭을 사냥할 수 있는 기능

### 상세 계획

**파일 생성/수정:**
- `Assets/Scripts/Player/PlayerHunting.cs` (신규) - 사냥 시스템
- `Assets/Scripts/Animal/AnimalAI.cs` - 사냥 당할 때 처리
- `Assets/Scripts/Items/RawMeat.cs` (신규) - 날고기 아이템

**구현 내용:**

1. **사냥 방식 (선택 필요)**
   - 옵션 A: 근접 공격 (칼로 때리기)
   - 옵션 B: 원거리 공격 (활/총)
   - 옵션 C: 덫 설치

2. **기본 메커니즘**
```csharp
// PlayerHunting.cs
public class PlayerHunting : MonoBehaviour
{
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private LayerMask _animalLayer;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 좌클릭
        {
            TryHunt();
        }
    }
    
    private void TryHunt()
    {
        // 범위 내 동물 탐지
        var hit = Physics2D.OverlapCircle(transform.position, _attackRange, _animalLayer);
        if (hit != null)
        {
            var animal = hit.GetComponent<AnimalAI>();
            if (animal != null)
            {
                animal.TakeDamage(_attackDamage);
            }
        }
    }
}
```

3. **AnimalAI 수정**
```csharp
public int Health = 1;  // 기본 체력

public void TakeDamage(int damage)
{
    Health -= damage;
    if (Health <= 0)
    {
        Die();
    }
}

private void Die()
{
    // 아이템 드랍 (날고기)
    Instantiate(rawMeatPrefab, transform.position, Quaternion.identity);
    
    // 사운드/이펙트
    
    // 제거
    Destroy(gameObject);
}
```

4. **아이템**
- RawMeat (날고기) - 요리 가능한 재료
- 요리 후 CookedMeat (구운 고기) - 회복 아이템

### 고려사항
- 닭이 도망가는 로직 이미 존재 (Flee 상태)
- 공격 시 닭이 도망가도록 할지, 바로 죽일지
- 요리 시스템과 연동
- 사냥 도구 (칼, 활 등) 필요성

### 질문사항
- [ ] 사냥 방식 선택 (근접/원거리/덫)
- [ ] 닭 체력 (1방? 여러 방?)
- [ ] 드랍 아이템 종류
- [ ] 요리 시스템 연동 여부

---

## 3. 기타 아이디어 (Backlog)

### 3.1 닭 성장 시스템
- 병아리 → 닭 성장
- 어린 닭은 알 못 낳음

### 3.2 닭 사료 시스템
- 사료를 주면 알 낳기 확률 증가
- 배고픔 시스템

### 3.3 닭 우리 / 축사
- 닭을 가둬두는 건물
- 자동으로 알 수집

### 3.4 닭 품종
- 흰닭 (알 생산)
- 검은닭 (고기용)
- 기타 특수 품종

---

## 수정 내역

| 날짜 | 내용 |
|------|------|
| 2026-03-27 | 파일 생성. 계란 부화, Player 사냥 기능 추가 |

---

## 참고 파일

- `Assets/Scripts/Animal/ChickenAI.cs` - 닭 AI
- `Assets/Scripts/Items/EggItem.cs` - 계란 아이템
- `Assets/Scripts/Core/TimeManager.cs` - 시간/날짜 관리
- `Assets/Scripts/Player/PlayerController.cs` - 플레이어 컨트롤러