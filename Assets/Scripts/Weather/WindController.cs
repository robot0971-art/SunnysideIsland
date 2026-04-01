using UnityEngine;
using SunnysideIsland.GameData;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// 바람 강도를 관리하는 컨트롤러
    /// 싱글톤 패턴으로 구현하여 전역에서 접근 가능
    /// </summary>
    public class WindController : MonoBehaviour
    {
        public static WindController Instance { get; private set; }
        
        [Header("=== Wind Settings ===")]
        [SerializeField] private float _gentleWindStrength = 0.1f;
        [SerializeField] private float _strongWindStrength = 0.6f;
        [SerializeField] private float _windSpeed = 1.5f;
        
        [Header("=== Wind Events ===")]
        [SerializeField] private float _minEventInterval = 30f;
        [SerializeField] private float _maxEventInterval = 120f;
        [SerializeField] private float _minEventDuration = 10f;
        [SerializeField] private float _maxEventDuration = 30f;
        
        [Header("=== Weather Integration ===")]
        [SerializeField] private bool _strongWindDuringStorm = true;
        
        private float _currentWindStrength;
        private float _targetWindStrength;
        private float _windTransitionSpeed = 2f;
        private float _nextEventTime;
        private float _eventEndTime;
        private bool _isEventActive;
        
        public float CurrentWindStrength => _currentWindStrength;
        public float WindSpeed => _windSpeed;
        public bool IsStrongWind => _currentWindStrength > (_gentleWindStrength + _strongWindStrength) * 0.5f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _currentWindStrength = _gentleWindStrength;
            _targetWindStrength = _gentleWindStrength;
            ScheduleNextEvent();
        }
        
        private void Update()
        {
            UpdateWindStrength();
            CheckWindEvents();
        }
        
        private void UpdateWindStrength()
        {
            // 부드럽게 바람 세기 전환
            _currentWindStrength = Mathf.Lerp(_currentWindStrength, _targetWindStrength, _windTransitionSpeed * Time.deltaTime);
        }
        
        private void CheckWindEvents()
        {
            if (_isEventActive)
            {
                // 강풍 이벤트 종료
                if (Time.time >= _eventEndTime)
                {
                    EndWindEvent();
                }
            }
            else
            {
                // 다음 강풍 이벤트 시작
                if (Time.time >= _nextEventTime)
                {
                    StartWindEvent();
                }
            }
        }
        
        private void StartWindEvent()
        {
            _isEventActive = true;
            _targetWindStrength = _strongWindStrength;
            _eventEndTime = Time.time + Random.Range(_minEventDuration, _maxEventDuration);
            
            Debug.Log("[WindController] 강풍 시작! 세기: " + _strongWindStrength);
        }
        
        private void EndWindEvent()
        {
            _isEventActive = false;
            _targetWindStrength = _gentleWindStrength;
            ScheduleNextEvent();
            
            Debug.Log("[WindController] 강풍 종료. 약한 바람으로 전환");
        }
        
        private void ScheduleNextEvent()
        {
            _nextEventTime = Time.time + Random.Range(_minEventInterval, _maxEventInterval);
        }
        
        /// <summary>
        /// 날씨에 따라 바람 세기 조절 (WeatherSystem에서 호출)
        /// </summary>
        public void OnWeatherChanged(WeatherType weather)
        {
            if (!_strongWindDuringStorm) return;
            
            switch (weather)
            {
                case WeatherType.Stormy:
                    // 폭풍우 시 항상 강풍
                    _targetWindStrength = _strongWindStrength;
                    _isEventActive = true;
                    break;
                    
                case WeatherType.Rainy:
                    // 비 올 때 약간 강한 바람
                    _targetWindStrength = (_gentleWindStrength + _strongWindStrength) * 0.5f;
                    break;
                    
                default:
                    // 기본 날씨에서는 이벤트 시스템 따라감
                    if (!_isEventActive)
                    {
                        _targetWindStrength = _gentleWindStrength;
                    }
                    break;
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
