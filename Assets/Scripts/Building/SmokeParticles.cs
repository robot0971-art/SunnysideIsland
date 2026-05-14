using UnityEngine;

namespace SunnysideIsland.Building
{
    /// <summary>
    /// ?곌린 ?뚰떚???앹꽦湲?    /// 遺덉뿉???щ씪媛??遺?쒕윭???곌린
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class SmokeParticles : MonoBehaviour
    {
        [Header("=== Smoke Settings ===")]
        [Tooltip("?곌린 ?됱긽 (?쒖옉)")]
        [SerializeField] private Color _smokeColorStart = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        
        [Tooltip("?곌린 ?됱긽 (??")]
        [SerializeField] private Color _smokeColorEnd = new Color(0.5f, 0.5f, 0.5f, 0f);
        
        [Tooltip("?곌린 ?앹꽦 媛꾧꺽")]
        [SerializeField] [Range(0.05f, 0.5f)] private float _emissionRate = 0.15f;
        
        [Tooltip("?곌린 ?ш린")]
        [SerializeField] [Range(0.1f, 0.5f)] private float _smokeSize = 0.25f;
        
        [Tooltip("?곌린 ?섎챸")]
        [SerializeField] [Range(1f, 4f)] private float _lifetime = 2.5f;
        
        [Tooltip("?곌린 ?곸듅 ?띾룄")]
        [SerializeField] [Range(0.3f, 1.5f)] private float _riseSpeed = 0.6f;
        
        [Tooltip("諛붾엺 ?곹뼢")]
        [SerializeField] [Range(0f, 0.5f)] private float _windEffect = 0.15f;
        
        private ParticleSystem _particleSystem;
        private bool _isPlaying = false;
        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            ConfigureSmoke();
        }
        
        private void ConfigureSmoke()
        {
            if (_particleSystem == null) return;
            
            var main = _particleSystem.main;
            var emission = _particleSystem.emission;
            var shape = _particleSystem.shape;
            var velocityOverLifetime = _particleSystem.velocityOverLifetime;
            var colorOverLifetime = _particleSystem.colorOverLifetime;
            var sizeOverLifetime = _particleSystem.sizeOverLifetime;
            var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            
            // Main
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(_lifetime * 0.8f, _lifetime * 1.2f);
            main.startSpeed = 0f;
            main.startSize = _smokeSize;
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f; // 泥쒖쿇???꾨줈
            
            // ?됱긽 洹몃씪?곗씠??            Gradient colorGrad = new Gradient();
            Gradient colorGrad = new Gradient();
            colorGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(_smokeColorStart, 0f),
                    new GradientColorKey(new Color(0.4f, 0.4f, 0.4f, 0.2f), 0.6f),
                    new GradientColorKey(_smokeColorEnd, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.3f, 0f),
                    new GradientAlphaKey(0.15f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(colorGrad);
            
            // Emission
            emission.enabled = true;
            emission.rateOverTime = _emissionRate;
            
            // Shape - 遺??꾩뿉??            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.position = new Vector3(0, 0.3f, 0);
            shape.rotation = new Vector3(0, 0, 0);
            
            // Velocity Over Lifetime? ?ъ슜?섏? ?딆쓬 (臾몄젣 諛⑹?)
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(_riseSpeed * 0.7f, _riseSpeed * 1.3f);
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-_windEffect, _windEffect);
            
            // Color over lifetime
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorGrad);
            
            // Size over lifetime - 而ㅼ?硫댁꽌 ?⑹뼱吏?            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(0.3f, 0.8f);
            sizeCurve.AddKey(0.7f, 1.2f);
            sizeCurve.AddKey(1f, 1.8f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;
            renderer.sortingOrder = 5; // 遺덈낫???꾩뿉
            
            // ?뚰봽???먰삎 ?띿뒪泥?            if (renderer.material == null)
            {
                Texture2D tex = ParticleTextureGenerator.CreateCircleTexture(64, true);
                renderer.material = ParticleTextureGenerator.CreateSimpleParticleMaterial(tex);
            }
            
            _particleSystem.Stop();
        }
        
        public void StartSmoke()
        {
            if (_particleSystem == null || _isPlaying) return;
            
            _particleSystem.Play();
            _isPlaying = true;
        }
        
        public void StopSmoke()
        {
            if (_particleSystem == null || !_isPlaying) return;
            
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _isPlaying = false;
        }
        
        /// <summary>
        /// 諛붾엺 諛⑺뼢 ?ㅼ젙
        /// </summary>
        public void SetWindDirection(float windX)
        {
            if (_particleSystem == null) return;
            
            var velocityOverLifetime = _particleSystem.velocityOverLifetime;
            velocityOverLifetime.x = windX;
        }
    }
}
