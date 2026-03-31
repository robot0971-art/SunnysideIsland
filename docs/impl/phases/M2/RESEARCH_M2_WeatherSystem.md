# M2 리서치 문서: 날씨 시스템 (WeatherSystem)

> **대상 시스템**: WeatherSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
날씨 변경 및 게임 플레이에 영향을 주는 날씨 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 날씨 타입 관리
  - 날씨 변경 로직
  - 게임 플레이 영향
  - 저장/불러오기
- **제외**: 
  - 비 파티클 효과
  - 날씨 UI (M3에서 처리)

### 예상 산출물
- `WeatherSystem.cs` - 날씨 관리
- `WeatherEvents.cs` - 날씨 관련 이벤트
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 요구사항 정리

#### 기능적 요구사항

1. **날씨 타입**
   - Sunny (맑음): 기본 상태
   - Cloudy (흐림): 농사 영향 없음
   - Rainy (비): 물 주기 불필요
   - Stormy (폭풍): 낚시 불가
   - Rainbow (무지개): 특별 이벤트

2. **날씨 변경**
   - 매일 아침 확률 기반 변경
   - 계절별 확률 차이
   - WeatherData.probability 기준

3. **게임 영향**
   - Rainy: 농사 자동 물 주기
   - Stormy: 낚시 불가
   - 다른 날씨: 기본 상태

4. **지속 시간**
   - 하루 단위 변경
   - 아침에 변경

### 인터페이스 정의

```csharp
public interface IWeatherSystem
{
    WeatherType CurrentWeather { get; }
    WeatherType PreviousWeather { get; }
    
    void ChangeWeather(WeatherType weather);
    void RandomizeWeather();
    bool IsFishingAllowed();
    bool IsWateringRequired();
}
```

---

## 3. 기존 코드 분석

### WeatherData 분석

**파일 경로**: `Assets/Scripts/GameData/WeatherData.cs`

**주요 필드**:
- weatherType: 날씨 타입
- probability: 발생 확률
- effect: 효과 설명

### WeatherType enum

```csharp
public enum WeatherType
{
    Sunny,
    Cloudy,
    Rainy,
    Stormy,
    Rainbow
}
```

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// WeatherSystem.cs
public class WeatherSystem : MonoBehaviour, IWeatherSystem, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private WeatherType _defaultWeather = WeatherType.Sunny;
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private IGameDataLoader _dataLoader;
    
    public WeatherType CurrentWeather { get; private set; }
    public WeatherType PreviousWeather { get; private set; }
    
    private void Start()
    {
        _eventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        
        // 초기 날씨 설정
        if (CurrentWeather == WeatherType.Sunny && PreviousWeather == WeatherType.Sunny)
        {
            RandomizeWeather();
        }
    }
    
    private void OnDayStarted(DayStartedEvent evt)
    {
        RandomizeWeather();
    }
    
    public void ChangeWeather(WeatherType weather)
    {
        if (CurrentWeather == weather) return;
        
        PreviousWeather = CurrentWeather;
        CurrentWeather = weather;
        
        _eventBus.Publish(new WeatherChangedEvent
        {
            PreviousWeather = PreviousWeather,
            CurrentWeather = CurrentWeather
        });
    }
    
    public void RandomizeWeather()
    {
        // 현재 계절에 따른 날씨 확률 계산
        var weatherData = _dataLoader.GetAllWeatherData();
        
        float totalProbability = 0f;
        foreach (var data in weatherData)
        {
            totalProbability += data.probability;
        }
        
        float random = Random.Range(0f, totalProbability);
        float current = 0f;
        
        foreach (var data in weatherData)
        {
            current += data.probability;
            if (random <= current)
            {
                ChangeWeather(data.weatherType);
                return;
            }
        }
        
        // 기본값
        ChangeWeather(WeatherType.Sunny);
    }
    
    public bool IsFishingAllowed()
    {
        return CurrentWeather != WeatherType.Stormy;
    }
    
    public bool IsWateringRequired()
    {
        // 비가 오면 물 주기 필요 없음
        return CurrentWeather != WeatherType.Rainy && CurrentWeather != WeatherType.Stormy;
    }
    
    public object CaptureState()
    {
        return new WeatherSaveData
        {
            CurrentWeather = CurrentWeather,
            PreviousWeather = PreviousWeather
        };
    }
    
    public void RestoreState(object state)
    {
        if (state is WeatherSaveData saveData)
        {
            CurrentWeather = saveData.CurrentWeather;
            PreviousWeather = saveData.PreviousWeather;
        }
    }
}

// 날씨 변경 이벤트 (WeatherEvents.cs에 추가)
public class WeatherChangedEvent
{
    public WeatherType PreviousWeather { get; set; }
    public WeatherType CurrentWeather { get; set; }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | WeatherSystem 기본 구조 | 1h |
| 2 | 날씨 변경 로직 | 1h |
| 3 | 게임 영향 함수 | 1h |
| 4 | 이벤트 발행 | 0.5h |
| 5 | 저장/불러오기 | 0.5h |

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
