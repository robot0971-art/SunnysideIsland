# M3 리서치 문서: 전투 시스템 (CombatSystem)

> **대상 시스템**: CombatSystem, WeaponSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M3

---

## 1. 개요

### 리서치 목적
플레이어와 적의 전투를 처리하는 전투 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - IDamageable 인터페이스
  - 근접/원거리 공격
  - 데미지 계산
  - 무기 시스템
- **제외**: 
  - 적 AI (별도 문서)
  - 전투 UI

### 예상 산출물
- `CombatSystem.cs` - 전투 관리
- `IDamageable.cs` - 데미지 가능 인터페이스
- `Weapon.cs` - 무기 기본 클래스
- `MeleeWeapon.cs` - 근접 무기
- `RangedWeapon.cs` - 원거리 무기

---

## 2. 요구사항 정리

### 기능적 요구사항

1. **IDamageable 인터페이스**
   - TakeDamage(int damage, string source)
   - Heal(int amount)
   - IsDead 속성

2. **공격 시스템**
   - 근접 공격: 범위 내 적 공격
   - 원거리 공격: 투사체 발사
   - 공격 쿨다운
   - 스태미나 소모

3. **데미지 계산**
   - 기본 데미지
   - 무기 데미지
   - 크리티컬 확률
   - 방어 계산

4. **무기 시스템**
   - 무기 장착/해제
   - 내구도 관리
   - 무기별 공격 범위/속도

### 인터페이스 정의

```csharp
public interface IDamageable
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }
    
    void TakeDamage(int damage, string source);
    void Heal(int amount);
}

public interface IWeapon
{
    string WeaponId { get; }
    int BaseDamage { get; }
    float AttackSpeed { get; }
    float AttackRange { get; }
    
    void Attack(Vector3 direction);
    bool CanAttack();
}
```

---

## 3. 기존 코드 분석

### WeaponData 분석

**파일 경로**: `Assets/Scripts/GameData/WeaponData.cs`

**주요 필드**:
- damage: 기본 데미지
- attackSpeed: 공격 속도
- durability: 내구도
- range: 공격 범위
- isRanged: 원거리 무기 여부

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// CombatSystem.cs
public class CombatSystem : MonoBehaviour
{
    [Header("=== Settings ===")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Transform _attackPoint;
    
    private IWeapon _currentWeapon;
    private float _lastAttackTime;
    
    public void Attack(Vector3 direction)
    {
        if (_currentWeapon == null) return;
        if (!CanAttack()) return;
        
        _currentWeapon.Attack(direction);
        _lastAttackTime = Time.time;
    }
    
    public bool CanAttack()
    {
        if (_currentWeapon == null) return false;
        return Time.time >= _lastAttackTime + (1f / _currentWeapon.AttackSpeed);
    }
}

// Weapon.cs (추상 클래스)
public abstract class Weapon : MonoBehaviour, IWeapon
{
    [Header("=== Weapon Data ===")]
    [SerializeField] protected WeaponData _weaponData;
    
    public string WeaponId => _weaponData?.weaponId;
    public int BaseDamage => _weaponData?.damage ?? 10;
    public float AttackSpeed => _weaponData?.attackSpeed ?? 1f;
    public float AttackRange => _weaponData?.range ?? 1f;
    
    public abstract void Attack(Vector3 direction);
    public bool CanAttack() => true;
}

// MeleeWeapon.cs
public class MeleeWeapon : Weapon
{
    [Header("=== Melee Settings ===")]
    [SerializeField] private float _attackArc = 90f;
    [SerializeField] private LayerMask _targetLayer;
    
    public override void Attack(Vector3 direction)
    {
        // 범위 내 적 검색
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, 
            AttackRange, 
            _targetLayer
        );
        
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(BaseDamage, WeaponId);
            }
        }
    }
}

// RangedWeapon.cs
public class RangedWeapon : Weapon
{
    [Header("=== Ranged Settings ===")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    
    public override void Attack(Vector3 direction)
    {
        if (_projectilePrefab == null) return;
        
        var projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
        var projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(direction, BaseDamage, WeaponId);
        }
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | IDamageable 인터페이스 | 0.5h |
| 2 | Weapon 추상 클래스 | 1h |
| 3 | MeleeWeapon 구현 | 1h |
| 4 | RangedWeapon 구현 | 1h |
| 5 | CombatSystem | 1h |
| 6 | 테스트 | 1h |

---

## 리서치 완료 확인

- [x] 요구사항 정리 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
