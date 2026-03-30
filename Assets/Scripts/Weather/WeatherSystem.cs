using System;
using System.Collections.Generic;
using UnityEngine;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.GameData;
using DI;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// 날씨 시스템
    /// </summary>
    public class WeatherSystem : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private WeatherType _defaultWeather = WeatherType.Sunny;
        
        [Header("=== Effects ===")]
        [SerializeField] private RainEffect _rainEffectPrefab;
        
        private TimeManager _timeManager;
        
        public WeatherType CurrentWeather { get; private set; }
        public WeatherType PreviousWeather { get; private set; }
        
        public string SaveKey => "WeatherSystem";
        
        private RainEffect _rainEffect;
        
        private void Awake()
        {
            // RainEffect 생성 (Start보다 먼저)
            CreateRainEffect();
        }
        
        private void Start()
        {
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            
            // 1초 후 초기 날씨 설정 (다른 오브젝트들의 Awake/Start 완료 후)
            if (CurrentWeather == WeatherType.Sunny && PreviousWeather == WeatherType.Sunny)
            {
                Invoke(nameof(RandomizeWeather), 1f);
            }
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }
        
        private void CreateRainEffect()
        {
            if (_rainEffectPrefab != null)
            {
                _rainEffect = Instantiate(_rainEffectPrefab, transform);
                _rainEffect.gameObject.SetActive(false);
            }
            else
            {
                // 프리팹이 없으면 코드로 생성
                var rainObj = new GameObject("RainEffect");
                rainObj.transform.SetParent(transform);
                _rainEffect = rainObj.AddComponent<RainEffect>();
                _rainEffect.gameObject.SetActive(false);
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
            
            // 비 효과 제어
            UpdateWeatherEffects();
            
            EventBus.Publish(new WeatherChangedEvent
            {
                PreviousWeather = PreviousWeather,
                CurrentWeather = CurrentWeather
            });
        }
        
        private void UpdateWeatherEffects()
        {
            if (_rainEffect == null) return;
            
            bool isRainy = CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy;
            
            if (isRainy)
            {
                _rainEffect.gameObject.SetActive(true);
                _rainEffect.Play();
            }
            else
            {
                _rainEffect.Stop();
                _rainEffect.gameObject.SetActive(false);
            }
        }
        
        public void RandomizeWeather()
        {
            // TimeManager가 없으면 찾기
            if (_timeManager == null)
            {
                _timeManager = FindObjectOfType<TimeManager>();
            }
            
            Season currentSeason = _timeManager?.CurrentSeason ?? Season.Spring;
            
            var weatherWeights = GetSeasonWeatherWeights(currentSeason);
            
            float totalWeight = 0f;
            foreach (var weight in weatherWeights.Values)
            {
                totalWeight += weight;
            }
            
            float random = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            
            foreach (var kvp in weatherWeights)
            {
                cumulative += kvp.Value;
                if (random <= cumulative)
                {
                    ChangeWeather(kvp.Key);
                    return;
                }
            }
            
            ChangeWeather(WeatherType.Sunny);
        }
        
        private Dictionary<WeatherType, float> GetSeasonWeatherWeights(Season season)
        {
            var weights = new Dictionary<WeatherType, float>();
            
            switch (season)
            {
                case Season.Spring:
                    weights[WeatherType.Sunny] = 3f;
                    weights[WeatherType.Cloudy] = 3f;
                    weights[WeatherType.Rainy] = 3f;
                    weights[WeatherType.Stormy] = 0.5f;
                    weights[WeatherType.Rainbow] = 0.5f;
                    break;
                    
                case Season.Summer:
                    weights[WeatherType.Sunny] = 5f;
                    weights[WeatherType.Cloudy] = 2f;
                    weights[WeatherType.Rainy] = 2f;
                    weights[WeatherType.Stormy] = 1f;
                    weights[WeatherType.Rainbow] = 0.5f;
                    break;
                    
                case Season.Fall:
                    weights[WeatherType.Sunny] = 2f;
                    weights[WeatherType.Cloudy] = 4f;
                    weights[WeatherType.Rainy] = 3f;
                    weights[WeatherType.Stormy] = 0.5f;
                    weights[WeatherType.Rainbow] = 0.5f;
                    break;
                    
                case Season.Winter:
                    weights[WeatherType.Sunny] = 2f;
                    weights[WeatherType.Cloudy] = 4f;
                    weights[WeatherType.Rainy] = 3f;
                    weights[WeatherType.Stormy] = 1f;
                    weights[WeatherType.Rainbow] = 0.3f;
                    break;
                    
                default:
                    weights[WeatherType.Sunny] = 3f;
                    weights[WeatherType.Cloudy] = 2f;
                    weights[WeatherType.Rainy] = 1f;
                    break;
            }
            
            return weights;
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
        
        public object GetSaveData()
        {
            return new WeatherSaveData
            {
                CurrentWeather = CurrentWeather,
                PreviousWeather = PreviousWeather
            };
        }
        
        public void LoadSaveData(object state)
        {
            if (state is WeatherSaveData data)
            {
                CurrentWeather = data.CurrentWeather;
                PreviousWeather = data.PreviousWeather;
            }
        }
    }
    
    [Serializable]
    public class WeatherSaveData
    {
        public WeatherType CurrentWeather;
        public WeatherType PreviousWeather;
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
