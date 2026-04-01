using UnityEngine;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// 비 효과 (Particle System)
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
        
        private void Awake()
        {
            Debug.Log("[RainEffect] Awake called");
            CreateParticleSystem();
        }
        
        private void Update()
        {
            FollowCamera();
        }
        
        private void CreateParticleSystem()
        {
            // 파티클 시스템 추가
            _particleSystem = gameObject.AddComponent<ParticleSystem>();
            
            // 먼저 정지 상태로 설정
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // Main 모듈
            var main = _particleSystem.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = _fallSpeed;
            main.startSize = _particleSize;  // 0.2f로 증가
            main.startColor = _particleColor;  // 더 밝고 불투명하게
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _particleCount;
            
            Debug.Log($"[RainEffect] Main settings - Size: {main.startSize.constant}, Color: {main.startColor.color}");
            
            // Emission
            var emission = _particleSystem.emission;
            emission.rateOverTime = _particleCount;
            
            // Shape - Box (2D용)
            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 1f, 0f); // Z축 0으로 (2D)
            shape.position = new Vector3(0f, 15f, 0f); // Z축 0 (2D 평면 위)
            
            // Renderer - 2D용 설정
            var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 5f; // 더 길게 (비 효과)
            renderer.sortingLayerName = "Default"; // UI → Default로 변경
            renderer.sortingOrder = 100;
            
            // 머티리얼 설정 - 2D용 Sprites/Default
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 1f, 1f, 0.8f); // 흰색, 80% 불투명
            renderer.material = mat;
            Debug.Log("[RainEffect] Using Sprites/Default material for 2D");
            
            // Velocity over Lifetime - Y축으로만 이동 (2D용)
            var velocity = _particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(-_fallSpeed); // Y축으로만 아래로
            velocity.x = new ParticleSystem.MinMaxCurve(0f); // X축 0
            velocity.z = new ParticleSystem.MinMaxCurve(0f); // Z축 0 (2D에서는 Z축 이동 금지)
            
            Debug.Log($"[RainEffect] Velocity: X=0, Y={-_fallSpeed}, Z=0");
            
            Debug.Log($"[RainEffect] Particle system created with {_particleCount} particles");
        }
        
        private void FollowCamera()
        {
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
            
            // 카메라 위치 따라가기 (Z축은 0으로 고정 - 2D)
            Vector3 pos = _targetCamera.position;
            pos.y = 12f;
            pos.z = 0f; // Z축 0으로 고정 (2D)
            transform.position = pos;
        }
        
        public void Play()
        {
            if (_particleSystem != null && !_particleSystem.isPlaying)
            {
                _particleSystem.Play();
                Debug.Log("[RainEffect] Play() called - Particle system playing");
            }
            else
            {
                Debug.LogWarning($"[RainEffect] Cannot play - System: {_particleSystem != null}, Playing: {_particleSystem?.isPlaying}");
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
        /// 디버깅용: 현재 파티클 상태 반환
        /// </summary>
        public string GetStatus()
        {
            if (_particleSystem == null)
                return "ParticleSystem is null";
            
            return $"Playing: {_particleSystem.isPlaying}, ParticleCount: {_particleSystem.particleCount}, Position: {transform.position}";
        }
    }
}
