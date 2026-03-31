# 2. 코어 시스템 구현 계획

## 2.1 의존성 주입 (DI) 시스템

### 구현 우선순위: P0 (필수)

#### DIContainer 구현
```csharp
public class DIContainer
{
    private static DIContainer _global;
    private readonly Dictionary<Type, object> _registrations;
    private readonly Dictionary<Type, Func<object>> _factories;
    
    public static DIContainer Global => _global ??= new DIContainer();
    
    public void Register<TInterface, TImplementation>() where TImplementation : TInterface;
    public void RegisterInstance<T>(T instance);
    public T Resolve<T>();
}
```

#### 주요 기능
- 싱글톤 대신 DI 사용
- 인터페이스 기반 등록
- 자동 의존성 해결
- Scene별 Container 지원

#### Installer 패턴
```csharp
public abstract class Installer : MonoBehaviour
{
    protected DIContainer Container { get; private set; }
    
    protected abstract void InstallBindings();
    
    protected void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
    protected void BindInstance<T>(T instance);
}
```

### 예상 개발 시간: 1일

---

## 2.2 이벤트 버스 시스템

### 구현 우선순위: P0 (필수)

#### EventBus 구현
```csharp
public interface IEventBus
{
    void Subscribe<TEvent>(Action<TEvent> handler);
    void Unsubscribe<TEvent>(Action<TEvent> handler);
    void Publish<TEvent>(TEvent eventData);
}

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers;
    
    public void Subscribe<TEvent>(Action<TEvent> handler);
    public void Unsubscribe<TEvent>(Action<TEvent> handler);
    public void Publish<TEvent>(TEvent eventData);
}
```

#### 이벤트 정의 예시
```csharp
public class TimeChangedEvent
{
    public int Hour { get; set; }
    public int Minute { get; set; }
}

public class PlayerDamagedEvent
{
    public int Damage { get; set; }
    public int RemainingHealth { get; set; }
}
```

### 예상 개발 시간: 0.5일

---

## 2.3 시간 관리 시스템

### 구현 우선순위: P0 (필수)

#### 요구사항
- 1초 = 1분 (게임 내)
- 하루 = 24분 (실제 시간)
- 28일 = 총 게임 기간
- 타임랩스 기능 지원

#### TimeManager 구현
```csharp
public class TimeManager : MonoBehaviour
{
    [Header("=== Time Settings ===")]
    [SerializeField] private float _secondsPerGameMinute = 1f;
    [SerializeField] private bool _isTimePaused;
    [SerializeField] private float _timeScale = 1f;
    
    public int CurrentDay { get; private set; }
    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    
    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnDayStarted;
    public event Action<int> OnDayEnded;
    
    public void SetTimeScale(float scale);
    public void PauseTime();
    public void ResumeTime();
}

public enum TimeOfDay
{
    Dawn,      // 04:00-06:00
    Morning,   // 06:00-09:00
    Noon,      // 09:00-12:00
    Afternoon, // 12:00-14:00
    Evening,   // 14:00-18:00
    Dusk,      // 18:00-21:00
    Night      // 21:00-04:00
}
```

#### 구현 단계
1. 기본 시간 흐름 구현
2. 시간대 분류 시스템
3. 이벤트 발행 연동
4. 타임랩스 기능
5. 저장/불러오기

### 예상 개발 시간: 1일

---

## 2.4 저장 시스템

### 구현 우선순위: P0 (필수)

#### SaveSystem 구현
```csharp
public interface ISaveSystem
{
    void SaveGame(string saveName);
    bool LoadGame(string saveName);
    void DeleteSave(string saveName);
    List<SaveMetadata> GetSaveList();
}

public class SaveSystem : ISaveSystem
{
    private readonly string _savePath;
    
    public void SaveGame(string saveName)
    {
        var saveData = CollectSaveData();
        var json = JsonUtility.ToJson(saveData);
        File.WriteAllText(GetSavePath(saveName), json);
    }
    
    public bool LoadGame(string saveName)
    {
        var path = GetSavePath(saveName);
        if (!File.Exists(path)) return false;
        
        var json = File.ReadAllText(path);
        var saveData = JsonUtility.FromJson<GameSaveData>(json);
        ApplySaveData(saveData);
        return true;
    }
}
```

#### 저장 가능 인터페이스
```csharp
public interface ISaveable
{
    string SaveId { get; }
    object GetSaveData();
    void LoadSaveData(object data);
}
```

#### 저장 데이터 구조
```csharp
[Serializable]
public class GameSaveData
{
    public string SaveId;
    public string SaveTime;
    public int TotalPlayTime;
    
    public PlayerSaveData Player;
    public WorldSaveData World;
    public QuestSaveData Quests;
    public EconomySaveData Economy;
}
```

### 예상 개발 시간: 1일

---

## 2.5 게임 매니저

### 구현 우선순위: P0 (필수)

#### GameManager 구현
```csharp
public class GameManager : MonoBehaviour
{
    [Inject] private ITimeManager _timeManager;
    [Inject] private ISaveSystem _saveSystem;
    [Inject] private IEventBus _eventBus;
    
    public GameState CurrentState { get; private set; }
    
    public void StartNewGame();
    public void LoadGame(string saveName);
    public void SaveGame(string saveName);
    public void PauseGame();
    public void ResumeGame();
    public void ReturnToMainMenu();
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Loading,
    GameOver
}
```

#### 게임 초기화 흐름
```
1. Bootstrap Scene 로드
2. DI Container 초기화
3. 전역 서비스 등록
4. Game Scene 로드
5. Scene Installer 실행
6. 게임 상태 Playing으로 변경
7. 게임 시작 이벤트 발행
```

### 예상 개발 시간: 0.5일

---

## 2.6 오브젝트 풀링 시스템

### 구현 우선순위: P1 (권장)

#### ObjectPool 구현
```csharp
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Queue<T> _pool;
    private readonly Transform _parent;
    
    public T Get();
    public void Return(T obj);
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    
    public ObjectPool<T> GetPool<T>(T prefab, int initialSize) where T : MonoBehaviour;
}
```

#### 풀링 대상
- 이펙트 (파티클)
- 투사체 (화살 등)
- 드랍 아이템
- 데미지 숫자

### 예상 개발 시간: 0.5일

---

## 2.7 로그 및 디버깅 시스템

### 구현 우선순위: P2 (선택)

#### DebugManager 구현
```csharp
public static class DebugLogger
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(string message, LogCategory category);
    
    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(string message, LogCategory category);
    
    [Conditional("UNITY_EDITOR")]
    public static void LogError(string message, LogCategory category);
}

public enum LogCategory
{
    Core,
    Player,
    Combat,
    Economy,
    Building,
    Quest,
    AI
}
```

### 예상 개발 시간: 0.25일

---

## 2.8 코어 시스템 개발 일정

| 시스템 | 우선순위 | 예상 시간 | 담당 |
|--------|----------|-----------|------|
| DI Container | P0 | 1일 | - |
| EventBus | P0 | 0.5일 | - |
| TimeManager | P0 | 1일 | - |
| SaveSystem | P0 | 1일 | - |
| GameManager | P0 | 0.5일 | - |
| ObjectPool | P1 | 0.5일 | - |
| DebugLogger | P2 | 0.25일 | - |
| **합계** | | **4.75일** | |
