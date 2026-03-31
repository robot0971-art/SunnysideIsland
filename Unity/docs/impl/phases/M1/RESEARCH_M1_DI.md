# 리서치 문서: DI Container

> **대상 시스템**: DI (Dependency Injection) Container
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M1

---

## 1. 개요

### 리서치 목적
Sunnyside Island 프로젝트의 DI Container 시스템이 이미 구현되어 있으므로, 이를 분석하여 현재 상태를 파악하고 M1에서 필요한 보강 작업을 식별합니다.

### 리서치 범위
- **포함**: 기존 DI 시스템 코드 분석, 기능 검증, 확장 필요성 확인
- **제외**: DI 시스템의 완전한 재설계, 다른 DI 프레임워크 도입 검토

### 예상 산출물
- DI 시스템 현재 상태 보고서
- 보강 필요 기능 목록
- 테스트 계획

---

## 2. 기획 문서 분석

### 관련 기획 문서

| 문서명 | 경로 | 버전 | 상태 |
|--------|------|------|------|
| GDD | ../game-design/GDD.md | - | ✅ 참고 |
| 구현계획 | 02-core-systems.md | - | ✅ 참고 |
| 아키텍처 | 01-architecture.md | - | ✅ 참고 |
| AGENTS.md | ../../AGENTS.md | - | ✅ 참고 |

### 요구사항 정리

#### 기능적 요구사항
1. **인터페이스 기반 등록**
   - 상세: `Register<TInterface, TImplementation>()` 지원
   - 우선순위: P0

2. **인스턴스 등록**
   - 상세: `RegisterInstance<T>()` 지원
   - 우선순순위: P0

3. **자동 의존성 해결**
   - 상세: `[Inject]` 어트리뷰트를 통한 자동 주입
   - 우선순위: P0

4. **Scene별 Container**
   - 상세: 씬별로 독립적인 DI Container 지원
   - 우선순위: P0

5. **글로벌 Container**
   - 상세: 앱 전역에서 사용 가능한 싱글톤 Container
   - 우선순위: P0

#### 비기능적 요구사항
1. 성능: 런타임에 의존성 해결 시 성능 저하 최소화
2. 확장성: 새로운 서비스 추가가 용이해야 함
3. 테스트: DI를 통한 Mock 주입으로 테스트 가능해야 함

### 인터페이스 정의

```csharp
public interface IDIContainer
{
    void Register<TInterface, TImplementation>() where TImplementation : TInterface;
    void RegisterInstance<T>(T instance);
    T Resolve<T>();
    object Resolve(Type type);
}
```

---

## 3. 기존 코드 분석

### 3.1 DI 시스템 검색

#### 검색 결과
기존 DI 시스템은 `Assets/Scripts/DI/` 폴더에 구현되어 있음:

| 파일 | 위치 | 관련성 | 상태 |
|------|------|--------|------|
| DIContainer.cs | DI/ | 높음 | ✅ 재사용 가능 |
| InjectAttribute.cs | DI/ | 높음 | ✅ 재사용 가능 |
| Installer.cs | DI/ | 높음 | ✅ 재사용 가능 |
| SceneInstaller.cs | DI/ | 높음 | ✅ 재사용 가능 |
| GlobalInstaller.cs | DI/ | 높음 | ✅ 재사용 가능 |
| DIExamples.cs | DI/Examples/ | 중간 | ✅ 참고 가능 |

### 3.2 핵심 클래스 분석

#### 클래스 1: DIContainer

**파일 경로**: `Assets/Scripts/DI/DIContainer.cs`

**분석 내용**:
- **목적**: 의존성 주입 컨테이너의 핵심 구현
- **주요 기능**:
  - 싱글톤 글로벌 컨테이너 제공
  - 인터페이스-구현 등록
  - 인스턴스 등록
  - 의존성 해결
  - 자동 주입 (필드, 메서드, 프로퍼티)
- **의존성**: 없음 (순수 C#)
- **재사용 가능성**: ✅ 완전히 재사용 가능

**코드 예시**:
```csharp
public sealed class DIContainer
{
    public static DIContainer Global { get; } = new DIContainer();
    
    public void Register<TInterface, TImplementation>() where TImplementation : TInterface;
    public void RegisterInstance<T>(T instance);
    public T Resolve<T>();
    public void InjectDependencies(object target);
}
```

**의견**: 
- 이미 완전히 구현되어 있음
- 필드 주입, 메서드 주입, 프로퍼티 주입 모두 지원
- 생명주기 관리 (Transient, Singleton) 지원
- 제네릭 메서드로 타입 안전성 보장

---

#### 클래스 2: InjectAttribute

**파일 경로**: `Assets/Scripts/DI/InjectAttribute.cs`

**분석 내용**:
- **목적**: 주입 대상 필드/메서드/프로퍼티 표시
- **주요 기능**:
  - `[Inject]` 어트리뷰트
  - `[Inject("name")]` 이름으로 주입
- **의존성**: 없음
- **재사용 가능성**: ✅ 완전히 재사용 가능

**코드 예시**:
```csharp
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InjectAttribute : Attribute
{
    public string Name { get; }
    public InjectAttribute(string name = null) { Name = name; }
}
```

**의견**: Unity 스타일의 Attribute 기반 주입 지원

---

#### 클래스 3: Installer

**파일 경로**: `Assets/Scripts/DI/Installer.cs`

**분석 내용**:
- **목적**: 씬별 의존성 등록을 위한 기반 클래스
- **주요 기능**:
  - 추상 클래스로 구현
  - InstallBindings 메서드 오버라이드
  - Container 접근 제공
- **의존성**: DIContainer
- **재사용 가능성**: ✅ 완전히 재사용 가능

**코드 예시**:
```csharp
public abstract class Installer : MonoBehaviour
{
    protected DIContainer Container { get; private set; }
    protected abstract void InstallBindings();
}
```

**의견**: 씬 시작 시 자동으로 InstallBindings 호출 필요 확인

---

#### 클래스 4: SceneInstaller

**파일 경로**: `Assets/Scripts/DI/SceneInstaller.cs`

**분석 내용**:
- **목적**: 씬별 DI Container 설정 및 생명주기 관리
- **주요 기능**:
  - 씬 진입 시 Container 생성
  - Bind, BindInstance 메서드 제공
  - 씬 오브젝트 자동 주입
- **의존성**: Installer, DIContainer
- **재사용 가능성**: ✅ 완전히 재사용 가능

**코드 예시**:
```csharp
public class SceneInstaller : Installer
{
    protected override void InstallBindings() { }
    protected void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
    protected void BindInstance<T>(T instance);
}
```

**의견**: 씬별로 Installer를 상속받아 구현하면 됨

---

#### 클래스 5: GlobalInstaller

**파일 경로**: `Assets/Scripts/DI/GlobalInstaller.cs`

**분석 내용**:
- **목적**: 글로벌 DI Container 초기화
- **주요 기능**:
  - 앱 시작 시 전역 서비스 등록
  - 싱글톤 패턴 사용
- **의존성**: DIContainer
- **재사용 가능성**: ✅ 완전히 재사용 가능

**의견**: Bootstrap 씬에서 호출하여 전역 서비스 등록

---

### 3.3 사용 예시 분석

**파일 경로**: `Assets/Scripts/DI/Examples/DIExamples.cs`

**분석 내용**:
- 서비스 등록 예시
- 주입 예시
- SceneInstaller 사용 예시

**코드 예시**:
```csharp
// 서비스 등록
Container.Register<IPlayerService, PlayerService>();
Container.RegisterInstance<IGameConfig>(config);

// 주입
public class PlayerController : MonoBehaviour
{
    [Inject] private IInputService _inputService;
    [Inject("PlayerConfig")] private PlayerConfig _config;
}

// SceneInstaller
public class GameSceneInstaller : SceneInstaller
{
    protected override void InstallBindings()
    {
        Bind<IEnemyManager, EnemyManager>();
        BindInstance<LevelData>(currentLevelData);
    }
}
```

---

## 4. 의존성 분석

### 4.1 의존하는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| 없음 | - | DI는 가장 기초적인 시스템으로 다른 시스템에 의존하지 않음 |

### 4.2 의존되는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| GameManager | 강함 | DI Container를 통해 서비스 주입받음 |
| TimeManager | 강함 | DI Container를 통해 서비스 주입받음 |
| SaveSystem | 강함 | DI Container를 통해 서비스 주입받음 |
| PlayerController | 강함 | DI Container를 통해 서비스 주입받음 |
| 모든 Manager | 강함 | DI Container를 통해 서비스 주입받음 |

### 4.3 의존성 그래프

```
DI Container (Core)
    ├── GameManager
    ├── TimeManager
    ├── SaveSystem
    ├── EventBus
    ├── PlayerController
    ├── InventorySystem
    ├── CombatSystem
    ├── BuildingSystem
    ├── EconomySystem
    └── ... (모든 시스템)
```

---

## 5. 외부 레퍼런스

### 5.1 Unity DI 프레임워크 비교

| 프레임워크 | 특징 | 우리 구현과 비교 |
|------------|------|------------------|
| Zenject | 완성도 높음, 복잡함 | 유사한 기능 제공, 더 간단함 |
| VContainer | 성능 중심, 현대적 | 유사한 구조 |
| Extenject | Zenject 포크 | 불필요 |

**결정**: 기존 DI 구현을 유지 (충분히 완성도 있음)

### 5.2 Unity 공식 문서

| 주제 | 링크 | 관련성 |
|------|------|--------|
| MonoBehaviour | Unity Docs | 중간 |
| Script Execution Order | Unity Docs | 중간 |

---

## 6. 리스크 분석

### 6.1 기술적 리스크

| 리스크 | 영향도 | 가능성 | 대응 방안 |
|--------|--------|--------|-----------|
| 순환 의존성 | 중간 | 낮음 | DIContainer에서 예외 처리 확인 |
| 메모리 누수 | 중간 | 낮음 | IDisposable 지원 검토 |
| 성능 저하 | 낮음 | 낮음 | 이미 구현되어 검증됨 |

### 6.2 의존성 리스크

| 의존 대상 | 현재 상태 | 리스크 | 대응 방안 |
|-----------|-----------|--------|-----------|
| DI Container | ✅ 완료 | 없음 | 추가 보강만 필요 |

---

## 7. 구현 방향 결정

### 7.1 아키텍처 결정

#### 결정 사항 1: 기존 DI 시스템 유지 vs 교체

**선택지**:
1. 기존 DI 시스템 유지 및 보강
   - 장점: 이미 구현됨, 코드 이해 필요 없음, 프로젝트에 맞게 설계됨
   - 단점: 외부 라이브러리에 비해 기능이 적을 수 있음

2. Zenject/VContainer 등 외부 프레임워크 도입
   - 장점: 검증된 프레임워크, 풍부한 기능
   - 단점: 학습 필요, 마이그레이션 비용 발생

**결정**: 1번 - 기존 DI 시스템 유지 및 보강

**사유**: 
- 이미 충분한 기능 구현됨
- 프로젝트 규모에 적합함
- 교체 비용이 이점보다 큼

---

#### 결정 사항 2: 보강 필요 기능

**식별된 보강 필요사항**:
1. **에러 처리 개선**
   - 등록되지 않은 타입 해결 시 명확한 예외 메시지
   - 순환 의존성 감지

2. **디버깅 지원**
   - 등록된 서비스 목록 조회
   - 의존성 그래프 출력

3. **생명주기 관리**
   - IDisposable 지원 (씬 언로드 시 리소스 해제)
   - Scoped 생명주기 (씬별)

4. **테스트 지원**
   - Mock 주입을 위한 테스트용 Container

---

### 7.2 클래스 설계 보강

```csharp
// 추가될 기능 예시
public sealed class DIContainer
{
    // 기존 기능들...
    
    // 보강: 등록된 타입 목록 조회
    public IEnumerable<Type> GetRegisteredTypes();
    
    // 보강: 특정 타입이 등록되었는지 확인
    public bool IsRegistered<T>();
    
    // 보강: 모든 등록 해제 (테스트용)
    public void Clear();
}

// SceneInstaller 보강
public abstract class Installer : MonoBehaviour
{
    // 기존 기능들...
    
    // 보강: 초기화 완료 이벤트
    public event Action OnBindingsInstalled;
}
```

---

### 7.3 파일 구조 (변경 없음)

```
Scripts/
├── DI/
│   ├── DIContainer.cs          (✅ 이미 존재)
│   ├── InjectAttribute.cs      (✅ 이미 존재)
│   ├── Installer.cs            (✅ 이미 존재)
│   ├── SceneInstaller.cs       (✅ 이미 존재)
│   ├── GlobalInstaller.cs      (✅ 이미 존재)
│   ├── Examples/
│   │   └── DIExamples.cs       (✅ 이미 존재)
│   └── Tests/
│       └── DIContainerTests.cs (🆕 추가 예정)
```

---

## 8. 구현 계획

### 8.1 구현 단계

| 단계 | 작업 내용 | 예상 시간 | 의존성 |
|------|-----------|-----------|--------|
| 1 | DI 코드 리뷰 및 문서화 | 2시간 | 없음 |
| 2 | 에러 처리 개선 | 2시간 | 없음 |
| 3 | 디버깅 기능 추가 | 2시간 | 없음 |
| 4 | IDisposable 지원 | 2시간 | 없음 |
| 5 | 단위 테스트 작성 | 4시간 | 없음 |
| **합계** | | **0.5일** | |

### 8.2 테스트 계획

| 테스트 유형 | 테스트 항목 | 방법 |
|-------------|-------------|------|
| 단위 테스트 | Register/Resolve 기본 기능 | NUnit |
| 단위 테스트 | InjectAttribute 자동 주입 | NUnit |
| 단위 테스트 | Scene별 Container 격리 | NUnit |
| 통합 테스트 | GameManager DI 주입 | 수동 테스트 |
| 통합 테스트 | Scene 전환 시 Container 생명주기 | 수동 테스트 |

### 8.3 완료 조건

- [ ] DI 코드 리뷰 완료
- [ ] 에러 처리 개선 (명확한 예외 메시지)
- [ ] 순환 의존성 감지
- [ ] IDisposable 지원
- [ ] 단위 테스트 100% 통과
- [ ] 기존 시스템과 통합 테스트 완료

---

## 9. 질문 및 논의점

### 9.1 미해결 질문

1. **Scene 전환 시 Container 생명주기**
   - 상황: 씬 전환 시 Scene Container는 어떻게 처리되는가?
   - 필요한 정보: SceneInstaller에서 OnDestroy 처리 확인 필요

2. **비동기 주입 지원**
   - 상황: Addressable 로드 후 의존성 주입은 어떻게 처리하는가?
   - 필요한 정보: 현재 구현에서 비동기 지원 여부 확인

### 9.2 결정 필요 사항

| 사항 | 선택지 | 기한 | 담당 |
|------|--------|------|------|
| IDisposable 지원 방식 | 자동/수동 | M1 완료 시 | 개발자 |
| 디버깅 로그 레벨 | Debug.Log/Warning/Error | M1 완료 시 | 개발자 |

---

## 10. 부록

### 10.1 기존 DI 사용 예시

```csharp
// 서비스 정의
public interface ITimeManager
{
    void Pause();
    void Resume();
}

// 서비스 구현
public class TimeManager : MonoBehaviour, ITimeManager
{
    public void Pause() { /* ... */ }
    public void Resume() { /* ... */ }
}

// GlobalInstaller에서 등록
public class GlobalInstaller : MonoBehaviour
{
    void Awake()
    {
        DIContainer.Global.Register<ITimeManager, TimeManager>();
    }
}

// 사용
public class GameManager : MonoBehaviour
{
    [Inject] private ITimeManager _timeManager;
    
    void Start()
    {
        DIContainer.Global.InjectDependencies(this);
    }
}
```

### 10.2 테스트 예시

```csharp
[Test]
public void Register_Resolve_ShouldReturnInstance()
{
    // Arrange
    var container = new DIContainer();
    
    // Act
    container.Register<ITestService, TestService>();
    var result = container.Resolve<ITestService>();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsInstanceOf<TestService>(result);
}
```

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 의존성 분석 완료
- [x] 리스크 식별 완료
- [x] 구현 방향 결정 완료
- [ ] PLAN 문서 작성 준비 완료

**리서치 완료일**: 2026-03-13

**다음 단계**: PLAN_M1_DI.md 작성

---

## 업데이트 로그

| 날짜 | 버전 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 2026-03-13 | 1.0 | 초안 작성 | AI |
