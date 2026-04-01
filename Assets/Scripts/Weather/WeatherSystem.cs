using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.GameData;
using DI;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// 날씨 시스템
    /// 맑음 → 흐림 → 랜덤(맑음/흐림/비) 순환
    /// </summary>
    public class WeatherSystem : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private WeatherType _defaultWeather = WeatherType.Sunny;
        
        [Header("=== Lighting ===")]
        [Tooltip("Global Light 2D (URP) - Inspector에서 연결 또는 자동 검색")]
        [SerializeField] private Light2D _globalLight;
        
        [Header("=== Lighting Intensity ===")]
        [Tooltip("맑음 상태의 조명 세기")]
        [SerializeField] [Range(0.1f, 2f)] private float _sunnyIntensity = 1.0f;
        
        [Tooltip("흐림 상태의 조명 세기")]
        [SerializeField] [Range(0.1f, 2f)] private float _cloudyIntensity = 0.6f;
        
        [Tooltip("비 상태의 조명 세기")]
        [SerializeField] [Range(0.1f, 2f)] private float _rainyIntensity = 0.4f;
        
        [Header("=== Weather Duration (seconds) ===")]
        [Tooltip("최소 날씨 지속 시간 (초)")]
        [SerializeField] private float _minWeatherDuration = 1800f; // 30분
        
        [Tooltip("최대 날씨 지속 시간 (초)")]
        [SerializeField] private float _maxWeatherDuration = 7200f; // 2시간
        
        [Header("=== Rain Effect ===")]
        [Tooltip("RainEffect_Manual 프리팹")]
        [SerializeField] private GameObject _rainEffectPrefab;
        
        [Header("=== Weather Transition ===")]
        [Tooltip("Cloudy 후 Sunny로 갈 확률")]
        [SerializeField] [Range(0f, 1f)] private float _cloudyToSunnyChance = 0.4f;
        
        [Tooltip("Cloudy 후 Cloudy 유지 확률")]
        [SerializeField] [Range(0f, 1f)] private float _cloudyToCloudyChance = 0.3f;
        
        [Tooltip("Cloudy 후 Rainy로 갈 확률")]
        [SerializeField] [Range(0f, 1f)] private float _cloudyToRainyChance = 0.3f;
        
        private TimeManager _timeManager;
        private GameObject _rainEffectInstance;
        
        public WeatherType CurrentWeather { get; private set; }
        public WeatherType PreviousWeather { get; private set; }
        
        public string SaveKey => "WeatherSystem";
        
        // 날씨 변경 타이머
        private float _nextWeatherChangeTime;
        private bool _isWeatherChangePending = false;
        
        private void Awake()
        {
            FindGlobalLight();
            InitializeWeather();
        }
        
        private void Start()
        {
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            
            // 초기 날씨 설정
            if (CurrentWeather == WeatherType.Sunny && PreviousWeather == WeatherType.Sunny)
            {
                ChangeWeather(_defaultWeather);
                ScheduleNextWeatherChange();
            }
        }
        
        private void Update()
        {
            // 시간 체크하여 자동 날씨 변경
            if (_isWeatherChangePending && Time.time >= _nextWeatherChangeTime)
            {
                ProgressWeather();
            }
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }
        
        /// <summary>
        /// Global Light 2D 찾기
        /// </summary>
        private void FindGlobalLight()
        {
            if (_globalLight == null)
            {
                // 씬에서 Light2D 찾기 (Global 타입)
                var lights = FindObjectsOfType<Light2D>();
                foreach (var light in lights)
                {
                    if (light.lightType == Light2D.LightType.Global)
                    {
                        _globalLight = light;
                        Debug.Log($"[WeatherSystem] Global Light 2D found: {light.name}");
                        break;
                    }
                }
                
                if (_globalLight == null)
                {
                    Debug.LogWarning("[WeatherSystem] No Global Light 2D found! Please assign in Inspector.");
                }
            }
        }
        
        /// <summary>
        /// 초기 날씨 설정
        /// </summary>
        private void InitializeWeather()
        {
            CurrentWeather = _defaultWeather;
            PreviousWeather = _defaultWeather;
            UpdateLighting();
        }
        
        /// <summary>
        /// 다음 날씨 변경 예약
        /// </summary>
        private void ScheduleNextWeatherChange()
        {
            float duration = UnityEngine.Random.Range(_minWeatherDuration, _maxWeatherDuration);
            _nextWeatherChangeTime = Time.time + duration;
            _isWeatherChangePending = true;
            
            Debug.Log($"[WeatherSystem] Next weather change in {duration / 60f:F1} minutes");
        }
        
        /// <summary>
        /// 날씨 진행 (Sunny → Cloudy → Random)
        /// </summary>
        private void ProgressWeather()
        {
            _isWeatherChangePending = false;
            
            switch (CurrentWeather)
            {
                case WeatherType.Sunny:
                    // 맑음 → 흐림
                    ChangeWeather(WeatherType.Cloudy);
                    break;
                    
                case WeatherType.Cloudy:
                    // 흐림 → 랜덤
                    WeatherType nextWeather = GetRandomWeatherFromCloudy();
                    ChangeWeather(nextWeather);
                    break;
                    
                case WeatherType.Rainy:
                case WeatherType.Stormy:
                    // 비 → 흐림 또는 맑음
                    ChangeWeather(UnityEngine.Random.value < 0.5f ? WeatherType.Cloudy : WeatherType.Sunny);
                    break;
                    
                default:
                    ChangeWeather(WeatherType.Sunny);
                    break;
            }
            
            ScheduleNextWeatherChange();
        }
        
        /// <summary>
        /// Cloudy 상태에서 다음 날씨 랜덤 선택
        /// </summary>
        private WeatherType GetRandomWeatherFromCloudy()
        {
            float random = UnityEngine.Random.value;
            float sunnyThreshold = _cloudyToSunnyChance;
            float cloudyThreshold = sunnyThreshold + _cloudyToCloudyChance;
            
            if (random < sunnyThreshold)
            {
                return WeatherType.Sunny;
            }
            else if (random < cloudyThreshold)
            {
                return WeatherType.Cloudy;
            }
            else
            {
                return WeatherType.Rainy;
            }
        }
        
        /// <summary>
        /// 날씨 변경
        /// </summary>
        public void ChangeWeather(WeatherType weather)
        {
            if (CurrentWeather == weather) return;
            
            PreviousWeather = CurrentWeather;
            CurrentWeather = weather;
            
            // 조명 업데이트
            UpdateLighting();
            
            // 비 효과 업데이트
            UpdateRainEffect();
            
            EventBus.Publish(new WeatherChangedEvent
            {
                PreviousWeather = PreviousWeather,
                CurrentWeather = CurrentWeather
            });
            
            Debug.Log($"[WeatherSystem] Weather changed: {PreviousWeather} → {CurrentWeather}");
        }
        
        /// <summary>
        /// 조명 업데이트
        /// </summary>
        private void UpdateLighting()
        {
            if (_globalLight == null) return;
            
            float targetIntensity = _sunnyIntensity;
            
            switch (CurrentWeather)
            {
                case WeatherType.Sunny:
                case WeatherType.Rainbow:
                    targetIntensity = _sunnyIntensity;
                    break;
                    
                case WeatherType.Cloudy:
                    targetIntensity = _cloudyIntensity;
                    break;
                    
                case WeatherType.Rainy:
                case WeatherType.Stormy:
                    targetIntensity = _rainyIntensity;
                    break;
            }
            
            _globalLight.intensity = targetIntensity;
            Debug.Log($"[WeatherSystem] Light intensity set to {targetIntensity} for {CurrentWeather}");
        }
        
        /// <summary>
        /// 비 효과 업데이트
        /// </summary>
        private void UpdateRainEffect()
        {
            bool isRainy = CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy;
            
            if (isRainy)
            {
                if (_rainEffectInstance == null && _rainEffectPrefab != null)
                {
                    _rainEffectInstance = Instantiate(_rainEffectPrefab, transform);
                    Debug.Log("[WeatherSystem] Rain effect instantiated");
                }
                
                if (_rainEffectInstance != null)
                {
                    _rainEffectInstance.SetActive(true);
                }
            }
            else
            {
                if (_rainEffectInstance != null)
                {
                    _rainEffectInstance.SetActive(false);
                }
            }
        }
        
        private void OnDayStarted(DayStartedEvent evt)
        {
            // 하루가 시작되면 날씨 변경 가능성 체크
            // (이미 Update에서 처리되므로 추가 작업 불필요)
        }
        
        /// <summary>
        /// 수동으로 날씨 변경 (테스트용)
        /// </summary>
        [ContextMenu("Force Change to Sunny")]
        private void ForceChangeToSunny()
        {
            ChangeWeather(WeatherType.Sunny);
            ScheduleNextWeatherChange();
        }
        
        [ContextMenu("Force Change to Cloudy")]
        private void ForceChangeToCloudy()
        {
            ChangeWeather(WeatherType.Cloudy);
            ScheduleNextWeatherChange();
        }
        
        [ContextMenu("Force Change to Rainy")]
        private void ForceChangeToRainy()
        {
            ChangeWeather(WeatherType.Rainy);
            ScheduleNextWeatherChange();
        }
        
        public bool IsFishingAllowed()
        {
            return CurrentWeather != WeatherType.Stormy;
        }
        
        public bool IsWateringRequired()
        {
            return CurrentWeather != WeatherType.Rainy && CurrentWeather != WeatherType.Stormy;
        }
        
        public object GetSaveData()
        {
            return new WeatherSaveData
            {
                CurrentWeather = CurrentWeather,
                PreviousWeather = PreviousWeather,
                NextWeatherChangeTime = _nextWeatherChangeTime - Time.time // 남은 시간 저장
            };
        }
        
        public void LoadSaveData(object state)
        {
            if (state is WeatherSaveData data)
            {
                CurrentWeather = data.CurrentWeather;
                PreviousWeather = data.PreviousWeather;
                _nextWeatherChangeTime = Time.time + data.NextWeatherChangeTime;
                _isWeatherChangePending = true;
                
                UpdateLighting();
                UpdateRainEffect();
            }
        }
    }
    
    [Serializable]
    public class WeatherSaveData
    {
        public WeatherType CurrentWeather;
        public WeatherType PreviousWeather;
        public float NextWeatherChangeTime;
    }
    
    /// <summary>
    /// 날씨 변경 이벤트
    /// </summary>
    public class WeatherChangedEvent
    {
        public WeatherType PreviousWeather { get; set; }
        public WeatherType CurrentWeather { get; set; }
    }
}
