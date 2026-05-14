using System.Collections.Generic;
using UnityEngine;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.GameData;
using DI;
using GameDataClass = SunnysideIsland.GameData.GameData;

namespace SunnysideIsland.Fishing
{
    /// <summary>
    /// ?öÏãú ?ÅÌÉú
    /// </summary>
    public enum FishingState
    {
        Idle,
        Casting,
        Waiting,
        Biting,
        Reeling,
        Success,
        Fail
    }

    /// <summary>
    /// ?öÏãú ?úÏä§??
    /// </summary>
    public class FishingSystem : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private float _waitTimeMin = 3f;
        [SerializeField] private float _waitTimeMax = 10f;
        [SerializeField] private float _biteWindow = 2f;
        [SerializeField] private string _currentLocation = "Beach";
        
        [Inject]
        private GameDataClass _gameData = default!;
        [Inject]
        private Weather.WeatherSystem _weatherSystem = default!;
        [Inject]
        private TimeManager _timeManager = default!;
        
        public FishingState CurrentState { get; private set; } = FishingState.Idle;
        
        private string _currentRodId;
        private string _selectedFishId;
        private float _waitTimer;
        private float _biteTimer;
        
        public string SaveKey => "FishingSystem";
        
        public bool StartFishing(string fishingRodItemId)
        {
            if (CurrentState != FishingState.Idle) return false;
            
            _currentRodId = fishingRodItemId;
            CurrentState = FishingState.Casting;
            
            return true;
        }
        
        public void Cast(Vector3 position)
        {
            if (CurrentState != FishingState.Casting) return;
            
            CurrentState = FishingState.Waiting;
            _waitTimer = Random.Range(_waitTimeMin, _waitTimeMax);
            
            // Î¨ºÍ≥†Í∏??†ÌÉù
            _selectedFishId = SelectFish();
        }
        
        private void Update()
        {
            switch (CurrentState)
            {
                case FishingState.Waiting:
                    _waitTimer -= Time.deltaTime;
                    if (_waitTimer <= 0f)
                    {
                        TriggerBite();
                    }
                    break;
                    
                case FishingState.Biting:
                    _biteTimer -= Time.deltaTime;
                    if (_biteTimer <= 0f)
                    {
                        Fail("Î∞òÏùë ?úÍ∞Ñ Ï¥àÍ≥º");
                    }
                    break;
            }
        }
        
        private void TriggerBite()
        {
            if (CurrentState != FishingState.Waiting) return;
            
            CurrentState = FishingState.Biting;
            _biteTimer = _biteWindow;
            
            EventBus.Publish(new FishBiteEvent
            {
                FishId = _selectedFishId
            });
        }
        
        public void Reel()
        {
            if (CurrentState != FishingState.Biting) return;
            
            // ?±Í≥µ ?ïÎ•† Í≥ÑÏÇ∞
            float successChance = 0.7f; // Í∏∞Î≥∏ 70%
            
            if (Random.value <= successChance)
            {
                Success();
            }
            else
            {
                Fail("Î¨ºÍ≥†Í∏∞Í? ?ÑÎßùÍ∞îÏäµ?àÎã§");
            }
        }
        
        public void Cancel()
        {
            if (CurrentState == FishingState.Idle) return;
            
            CurrentState = FishingState.Idle;
            _currentRodId = null;
            _selectedFishId = null;
        }
        
        private void Success()
        {
            CurrentState = FishingState.Success;
            
            EventBus.Publish(new FishingSuccessEvent
            {
                FishId = _selectedFishId
            });
            
            // ?ÅÌÉú Î¶¨ÏÖã (ÏßßÏ? ?úÎ†à????
            Invoke(nameof(ResetState), 1f);
        }
        
        private void Fail(string reason)
        {
            CurrentState = FishingState.Fail;
            
            EventBus.Publish(new FishingFailEvent
            {
                Reason = reason
            });
            
            // ?ÅÌÉú Î¶¨ÏÖã (ÏßßÏ? ?úÎ†à????
            Invoke(nameof(ResetState), 1f);
        }
        
        private void ResetState()
        {
            CurrentState = FishingState.Idle;
            _currentRodId = null;
            _selectedFishId = null;
        }
        
        private string SelectFish()
        {
            if (_gameData == null || _gameData.fishData == null || _gameData.fishData.Count == 0)
            {
                return "fish_basic";
            }
            
            var candidateFish = new List<FishData>();
            var weights = new List<float>();
            
            int currentHour = _timeManager?.CurrentHour ?? 12;
            WeatherType currentWeather = _weatherSystem?.CurrentWeather ?? WeatherType.Sunny;
            
            foreach (var fish in _gameData.fishData)
            {
                if (!IsLocationMatch(fish.location))
                    continue;
                
                if (!IsTimeMatch(fish.timeCondition, currentHour))
                    continue;
                
                float weight = CalculateFishWeight(fish, currentWeather);
                
                if (weight > 0)
                {
                    candidateFish.Add(fish);
                    weights.Add(weight);
                }
            }
            
            if (candidateFish.Count == 0)
            {
                return "fish_basic";
            }
            
            float totalWeight = 0f;
            foreach (var w in weights)
            {
                totalWeight += w;
            }
            
            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            
            for (int i = 0; i < candidateFish.Count; i++)
            {
                cumulative += weights[i];
                if (random <= cumulative)
                {
                    return candidateFish[i].fishId;
                }
            }
            
            return candidateFish[0].fishId;
        }
        
        private bool IsLocationMatch(string fishLocation)
        {
            if (string.IsNullOrEmpty(fishLocation) || fishLocation == "Any")
                return true;
            
            return fishLocation.ToLower() == _currentLocation.ToLower();
        }
        
        private bool IsTimeMatch(string timeCondition, int currentHour)
        {
            if (string.IsNullOrEmpty(timeCondition) || timeCondition == "Any")
                return true;
            
            switch (timeCondition.ToLower())
            {
                case "dawn":
                    return currentHour >= 5 && currentHour < 7;
                case "morning":
                    return currentHour >= 7 && currentHour < 12;
                case "afternoon":
                    return currentHour >= 12 && currentHour < 18;
                case "evening":
                    return currentHour >= 18 && currentHour < 20;
                case "night":
                    return currentHour >= 20 || currentHour < 5;
                default:
                    return true;
            }
        }
        
        private float CalculateFishWeight(FishData fish, WeatherType weather)
        {
            float baseWeight = 1f;
            
            switch (fish.difficulty)
            {
                case FishDifficulty.Easy:
                    baseWeight = 10f;
                    break;
                case FishDifficulty.Normal:
                    baseWeight = 5f;
                    break;
                case FishDifficulty.Hard:
                    baseWeight = 2f;
                    break;
                case FishDifficulty.VeryHard:
                    baseWeight = 0.5f;
                    break;
                case FishDifficulty.Extreme:
                    baseWeight = 0.1f;
                    break;
            }
            
            if (weather == WeatherType.Rainy)
            {
                baseWeight *= 1.5f;
            }
            else if (weather == WeatherType.Stormy)
            {
                baseWeight *= 0.5f;
            }
            
            return baseWeight;
        }
        
        public void SetLocation(string location)
        {
            _currentLocation = location;
        }
        
        public object GetSaveData()
        {
            // ?öÏãú Ï§ëÏóê???Ä?•ÌïòÏßÄ ?äÏùå
            return null;
        }
        
        public void LoadSaveData(object state)
        {
            // ?öÏãú ?ÅÌÉú???Ä??Î≥µÏõê?òÏ? ?äÏùå
        }
    }
    
    /// <summary>
    /// Î¨ºÍ≥†Í∏?Î¨ºÏóà?????¥Î≤§??
    /// </summary>
    public class FishBiteEvent
    {
        public string FishId { get; set; }
    }
    
    /// <summary>
    /// ?öÏãú ?±Í≥µ ?¥Î≤§??
    /// </summary>
    public class FishingSuccessEvent
    {
        public string FishId { get; set; }
    }
    
    /// <summary>
    /// ?öÏãú ?§Ìå® ?¥Î≤§??
    /// </summary>
    public class FishingFailEvent
    {
        public string Reason { get; set; }
    }
}
