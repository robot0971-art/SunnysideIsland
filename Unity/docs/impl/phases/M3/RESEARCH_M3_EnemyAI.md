# M3 리서치 문서: 적 AI 시스템 (EnemyAI)

> **대상 시스템**: EnemyAI, EnemySpawner
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M3

---

## 1. 개요

### 리서치 목적
적의 행동과 전투 AI를 구현합니다.

### 리서치 범위
- **포함**: 
  - 적 상태 머신
  - 기본 AI (고블린)
  - 적 스폰 시스템
- **제외**: 
  - 보스 AI (M5에서 구현)

### 예상 산출물
- `EnemyAI.cs` - 적 AI 기본 클래스
- `GoblinAI.cs` - 고블린 AI
- `EnemySpawner.cs` - 적 스포너

---

## 2. 요구사항 정리

### 기능적 요구사항

1. **상태 머신**
   - Idle: 대기
   - Patrol: 순찰
   - Chase: 추적
   - Attack: 공격
   - Stunned: 기절
   - Dead: 사망

2. **AI 행동**
   - 플레이어 감지 (시야 범위)
   - 추적 (플레이어 따라가기)
   - 공격 (근접 공격)
   - 순찰 (정해진 경로)

3. **적 스폰**
   - 스폰 지점 설정
   - 최대 적 수 관리
   - 리스폰 시간

### 인터페이스 정의

```csharp
public interface IEnemyAI
{
    EnemyState CurrentState { get; }
    float DetectionRange { get; }
    float AttackRange { get; }
    
    void ChangeState(EnemyState newState);
    void TakeDamage(int damage);
}

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Stunned,
    Dead
}
```

---

## 3. 기존 코드 분석

### MonsterData 분석

**파일 경로**: `Assets/Scripts/GameData/MonsterData.cs`

**주요 필드**:
- health: 체력
- damage: 데미지
- speed: 이동 속도
- detectionRange: 감지 범위
- attackRange: 공격 범위

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// EnemyAI.cs (추상 클래스)
public abstract class EnemyAI : MonoBehaviour, IDamageable, IEnemyAI
{
    [Header("=== AI Settings ===")]
    [SerializeField] protected MonsterData _monsterData;
    [SerializeField] protected float _detectionRange = 5f;
    [SerializeField] protected float _attackRange = 1f;
    
    protected EnemyState _currentState = EnemyState.Idle;
    protected Transform _target;
    protected int _currentHealth;
    
    public EnemyState CurrentState => _currentState;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _monsterData?.health ?? 50;
    public bool IsDead => _currentHealth <= 0;
    
    protected virtual void Start()
    {
        _currentHealth = MaxHealth;
        ChangeState(EnemyState.Idle);
    }
    
    protected virtual void Update()
    {
        UpdateState();
    }
    
    protected abstract void UpdateState();
    
    public virtual void ChangeState(EnemyState newState)
    {
        if (_currentState == newState) return;
        
        OnExitState(_currentState);
        _currentState = newState;
        OnEnterState(newState);
    }
    
    protected virtual void OnEnterState(EnemyState state) { }
    protected virtual void OnExitState(EnemyState state) { }
    
    public virtual void TakeDamage(int damage, string source)
    {
        if (IsDead) return;
        
        _currentHealth -= damage;
        
        if (IsDead)
        {
            Die();
        }
        else
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    protected virtual void Die()
    {
        ChangeState(EnemyState.Dead);
        // 아이템 드랍
        // 사망 애니메이션
        Destroy(gameObject, 2f);
    }
}

// GoblinAI.cs
public class GoblinAI : EnemyAI
{
    [Header("=== Goblin Settings ===")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _attackCooldown = 1f;
    
    private float _lastAttackTime;
    private Vector3 _patrolTarget;
    
    protected override void UpdateState()
    {
        switch (_currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }
    }
    
    private void UpdateIdle()
    {
        // 플레이어 감지
        if (DetectPlayer())
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    private void UpdateChase()
    {
        if (_target == null) return;
        
        float distance = Vector3.Distance(transform.position, _target.position);
        
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distance > _detectionRange * 1.5f)
        {
            // 플레이어가 멀어지면 Idle로
            _target = null;
            ChangeState(EnemyState.Idle);
        }
        else
        {
            // 플레이어 추적
            Vector3 direction = (_target.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
        }
    }
    
    private void UpdateAttack()
    {
        if (Time.time >= _lastAttackTime + _attackCooldown)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
        
        // 공격 범위 벗어나면 Chase로
        if (Vector3.Distance(transform.position, _target.position) > _attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    private void Attack()
    {
        // 플레이어에게 데미지
        var player = _target.GetComponent<IDamageable>();
        player?.TakeDamage(_monsterData?.damage ?? 10, gameObject.name);
    }
    
    private bool DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position, 
            _detectionRange, 
            LayerMask.GetMask("Player")
        );
        
        if (playerCollider != null)
        {
            _target = playerCollider.transform;
            return true;
        }
        
        return false;
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | EnemyAI 추상 클래스 | 2h |
| 2 | GoblinAI 구현 | 2h |
| 3 | EnemySpawner | 1h |
| 4 | 테스트 | 1h |

---

## 리서치 완료 확인

- [x] 요구사항 정리 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
