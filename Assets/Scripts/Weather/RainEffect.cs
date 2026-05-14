using UnityEngine;
using DI;
using SunnysideIsland.Events;
using SunnysideIsland.GameData;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// Îπ??®Í≥º (Particle System)
    /// </summary>
    public class RainEffect : MonoBehaviour
    {
        [Header("=== Settings ===")]
        [SerializeField] private int _particleCount = 500;
        [SerializeField] private float _fallSpeed = 15f;
        [SerializeField] private float _particleSize = 0.2f;
        [SerializeField] private Color _particleColor = new Color(1f, 1f, 1f, 0.8f);
        
        private ParticleSystem _particleSystem;
        private Transform _targetCamera;

        [Inject(Optional = true)]
        private WeatherSystem _weatherSystem = default!;
        
        private void Awake()
        {
            Debug.Log("[RainEffect] Awake called");
            CreateParticleSystem();
        }

        private void Start()
        {
            DIContainer.Inject(this);
            // ?¥Î≤§??Íµ¨ÎèÖ
            EventBus.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            
            // Ï¥àÍ∏∞ ?†Ïî® Ï≤¥ÌÅ¨
            CheckInitialWeather();
        }

        private void OnDestroy()
        {
            // ?¥Î≤§??Íµ¨ÎèÖ ?¥Ï†ú
            EventBus.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
        }
        
        private void Update()
        {
            FollowCamera();
        }

        private void CheckInitialWeather()
        {
            if (_weatherSystem == null)
            {
                DIContainer.TryResolve(out _weatherSystem);
            }

            if (_weatherSystem != null)
            {
                UpdateByWeather(_weatherSystem.CurrentWeather);
            }
        }

        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            UpdateByWeather(evt.CurrentWeather);
        }

        private void UpdateByWeather(WeatherType weather)
        {
            bool isRainy = weather == WeatherType.Rainy || weather == WeatherType.Stormy;
            if (isRainy)
                Play();
            else
                Stop();
        }
        
        private void CreateParticleSystem()
        {
            // ?åÌã∞???úÏä§??Ï∂îÍ?
            _particleSystem = gameObject.AddComponent<ParticleSystem>();
            
            // Î®ºÏ? ?ïÏ? ?ÅÌÉúÎ°??§Ï†ï
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // Main Î™®Îìà
            var main = _particleSystem.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = _fallSpeed;
            main.startSize = _particleSize;  // 0.2fÎ°?Ï¶ùÍ?
            main.startColor = _particleColor;  // ??Î∞ùÍ≥† Î∂àÌà¨Î™ÖÌïòÍ≤?
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _particleCount;
            
            Debug.Log($"[RainEffect] Main settings - Size: {main.startSize.constant}, Color: {main.startColor.color}");
            
            // Emission
            var emission = _particleSystem.emission;
            emission.rateOverTime = _particleCount;
            
            // Shape - Box (2D??
            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 1f, 0f); // ZÏ∂?0?ºÎ°ú (2D)
            shape.position = new Vector3(0f, 15f, 0f); // ZÏ∂?0 (2D ?âÎ©¥ ??
            
            // Renderer - 2D???§Ï†ï
            var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 5f; // ??Í∏∏Í≤å (Îπ??®Í≥º)
            renderer.sortingLayerName = "Default"; // UI ??DefaultÎ°?Î≥ÄÍ≤?
            renderer.sortingOrder = 100;
            
            // Î®∏Ìã∞Î¶¨Ïñº ?§Ï†ï - 2D??Sprites/Default
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 1f, 1f, 0.8f); // ?∞ÏÉâ, 80% Î∂àÌà¨Î™?
            renderer.material = mat;
            Debug.Log("[RainEffect] Using Sprites/Default material for 2D");
            
            // Velocity over Lifetime - YÏ∂ïÏúºÎ°úÎßå ?¥Îèô (2D??
            var velocity = _particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(-_fallSpeed); // YÏ∂ïÏúºÎ°úÎßå ?ÑÎûòÎ°?
            velocity.x = new ParticleSystem.MinMaxCurve(0f); // XÏ∂?0
            velocity.z = new ParticleSystem.MinMaxCurve(0f); // ZÏ∂?0 (2D?êÏÑú??ZÏ∂??¥Îèô Í∏àÏ?)
            
            Debug.Log($"[RainEffect] Velocity: X=0, Y={-_fallSpeed}, Z=0");
            
            Debug.Log($"[RainEffect] Particle system created with {_particleCount} particles");
        }
        
        private void FollowCamera()
        {
            if (!Application.isPlaying) return; // ?êÎîî??Î™®Îìú?êÏÑú???êÎèô ?¥Îèô Ï§ëÏ?

            if (_targetCamera == null)
            {
                var cam = UnityEngine.Camera.main;
                if (cam != null)
                {
                    _targetCamera = cam.transform;
                    Debug.Log($"[RainEffect] Camera found: {_targetCamera.name}");
                }
                return;
            }
            
            // Ïπ¥Î©î???ÑÏπò ?∞ÎùºÍ∞ÄÍ∏?(ZÏ∂ïÏ? 0?ºÎ°ú Í≥†Ï†ï - 2D)
            Vector3 pos = _targetCamera.position;
            pos.y += 2f; // Ïπ¥Î©î?ºÎ≥¥??2 ?ÑÏóê ?ÑÏπò (12?êÏÑú ?òÏ†ï)
            pos.z = 0f; // ZÏ∂?0?ºÎ°ú Í≥†Ï†ï (2D)
            transform.position = pos;
        }
        
        public void Play()
        {
            if (_particleSystem != null && !_particleSystem.isPlaying)
            {
                _particleSystem.Play();
                Debug.Log("[RainEffect] Play() called - Particle system playing");
            }
        }
        
        public void Stop()
        {
            if (_particleSystem != null && _particleSystem.isPlaying)
            {
                _particleSystem.Stop();
                Debug.Log("[RainEffect] Stop() called");
            }
        }
        
        /// <summary>
        /// ?îÎ≤ÑÍπÖÏö©: ?ÑÏû¨ ?åÌã∞???ÅÌÉú Î∞òÌôò
        /// </summary>
        public string GetStatus()
        {
            if (_particleSystem == null)
                return "ParticleSystem is null";
            
            return $"Playing: {_particleSystem.isPlaying}, ParticleCount: {_particleSystem.particleCount}, Position: {transform.position}";
        }
    }
}
