using UnityEngine;

namespace SunnysideIsland.Building
{
    /// <summary>
    /// Campfire ?뚰떚???쒖뒪??    /// ?묎퀬 ?ъ꽭??遺덇퐙 ?④낵
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class CampfireParticles : MonoBehaviour
    {
        [Header("=== Particle Settings ===")]
        [Tooltip("遺덇퐙 ?됱긽 (?쒖옉)")]
        [SerializeField] private Color _startColorMin = new Color(1f, 0.6f, 0.1f, 1f); // ?몃옉-?ㅻ젋吏
        
        [Tooltip("遺덇퐙 ?됱긽 (??")]
        [SerializeField] private Color _startColorMax = new Color(1f, 0.3f, 0.05f, 1f); // ?ㅻ젋吏-鍮④컯
        
        [Tooltip("遺덇퐙 ?ш린")]
        [SerializeField] [Range(0.05f, 0.5f)] private float _particleSize = 0.15f;
        
        [Tooltip("遺덇퐙 ?띾룄")]
        [SerializeField] [Range(0.3f, 2f)] private float _particleSpeed = 0.8f;
        
        [Tooltip("遺덇퐙 ?섎챸(珥?")]
        [SerializeField] [Range(0.3f, 2f)] private float _particleLifetime = 0.8f;
        
        [Tooltip("?앹꽦 ?띾룄(媛?珥?")]
        [SerializeField] [Range(10f, 60f)] private float _emissionRate = 25f;
        
        [Tooltip("Maximum particles")]
        [SerializeField] [Range(20, 100)] private int _maxParticles = 40;
        
        [Header("=== Shape Settings ===")]
        [Tooltip("?뚰떚??紐⑥뼇 ?ъ슜 (false = ?먰삎, true = 遺덇퐙 紐⑥뼇)")]
        [SerializeField] private bool _useFlameShape = false;
        
        [Tooltip("?뚰떚??踰붿쐞(媛곷룄)")]
        [SerializeField] [Range(10f, 60f)] private float _coneAngle = 25f;
        
        [Tooltip("?뚰떚???믪씠")]
        [SerializeField] [Range(0.3f, 2f)] private float _fireHeight = 0.8f;
        
        private ParticleSystem _particleSystem;
        private bool _isPlaying = false;
        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem == null)
            {
                _particleSystem = gameObject.AddComponent<ParticleSystem>();
            }
            
            // 珥덇린 ?곹깭???뺤?
            StopFire();
        }
        
        /// <summary>
        /// ?뚰떚???ㅼ젙 ?곸슜
        /// </summary>
        private void ConfigureParticles()
        {
            if (_particleSystem == null) return;
            
            var main = _particleSystem.main;
            var emission = _particleSystem.emission;
            var shape = _particleSystem.shape;
            var velocityOverLifetime = _particleSystem.velocityOverLifetime;
            var colorOverLifetime = _particleSystem.colorOverLifetime;
            var sizeOverLifetime = _particleSystem.sizeOverLifetime;
            var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            
            // Main ?ㅼ젙
            main.startLifetime = new ParticleSystem.MinMaxCurve(_particleLifetime * 0.7f, _particleLifetime * 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(_particleSpeed * 0.5f, _particleSpeed * 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(_particleSize * 0.6f, _particleSize * 1.2f);
            main.maxParticles = _maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.2f; // ?꾨줈 ?좎삤瑜대뒗 ?④낵
            
            // ?됱긽 洹몃씪?곗씠??(?쒓컙???곕씪 蹂??
            var colorGradient = new ParticleSystem.MinMaxGradient();
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(_startColorMin, 0f),      // ?쒖옉: ?몃옉
                    new GradientColorKey(_startColorMax, 0.4f),    // 以묎컙: 二쇳솴
                    new GradientColorKey(new Color(0.8f, 0.1f, 0.05f), 0.8f), // ?? 遺됱???                    new GradientColorKey(new Color(0.3f, 0.05f, 0.02f), 1f)   // ?꾩쟾???대몢?뚯쭚
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0.3f, 0.9f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorGradient.mode = ParticleSystemGradientMode.Gradient;
            colorGradient.gradient = grad;
            main.startColor = colorGradient;
            
            // Emission ?ㅼ젙
            emission.enabled = true;
            emission.rateOverTime = _emissionRate;
            
            // Shape ?ㅼ젙 (Cone - ?꾨옒?먯꽌 ?꾨줈)
            shape.enabled = true;
            shape.shapeType = _useFlameShape ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Circle;
            shape.angle = _coneAngle;
            shape.radius = _useFlameShape ? 0.12f : 0.08f;
            shape.position = new Vector3(0, _fireHeight * 0.125f, 0);
            shape.rotation = new Vector3(-90f, 0, 0); // ?꾩そ???ν븯?꾨줉
            
            // Velocity Over Lifetime? ?ъ슜?섏? ?딆쓬 (Shape怨?StartSpeed濡??泥?
            velocityOverLifetime.enabled = false;
            
            // ???StartSpeed濡??꾨줈 ?щ씪媛???④낵
            
            // Color Over Lifetime - ?쒓컙???곕씪 ?됱긽 蹂??            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
            
            // Size Over Lifetime - ?묒븘吏???④낵
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(0.5f, 0.8f);
            sizeCurve.AddKey(0.8f, 0.4f);
            sizeCurve.AddKey(1f, 0.1f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Renderer ?ㅼ젙? ?몄뒪?숉꽣?먯꽌 ?ㅼ젙??媛??ъ슜 (肄붾뱶?먯꽌 ??뼱?곗? ?딆쓬)
            // Material怨?Texture???꾨━?뱀뿉 ?ㅼ젙??媛믪쓣 ?ъ슜
        }
        
        /// <summary>
        /// 遺?耳쒓린
        /// </summary>
        public void StartFire()
        {
            if (_particleSystem == null) return;
            
            _particleSystem.Play();
            _isPlaying = true;
            
        }
        
        /// <summary>
        /// 遺??꾧린
        /// </summary>
        public void StopFire()
        {
            if (_particleSystem == null) return;
            
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _isPlaying = false;
            
        }
        
        /// <summary>
        /// ?꾩옱 ?ъ깮 以묒씤吏 ?뺤씤
        /// </summary>
        public bool IsPlaying => _isPlaying;
        
        /// <summary>
        /// ?뚰떚?????뺤씤
        /// </summary>
        public int ParticleCount => _particleSystem?.particleCount ?? 0;
        
        /// <summary>
        /// ?뚰떚???ㅼ젙???ㅼ떆 ?곸슜 (Inspector?먯꽌 ?몄텧 媛??
        /// </summary>
        [ContextMenu("Reconfigure Particles")]
        public void Reconfigure()
        {
            ConfigureParticles();
            if (_isPlaying && _particleSystem != null)
            {
                _particleSystem.Play();
            }
        }
        
        /// <summary>
        /// ?뚯뒪?몄슜 - 遺?耳쒓린/?꾧린 ?좉?
        /// </summary>
        [ContextMenu("Toggle Fire")]
        public void ToggleFire()
        {
            if (_isPlaying)
                StopFire();
            else
                StartFire();
        }
    }
}
