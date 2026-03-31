using UnityEngine;

namespace SunnysideIsland.Animal
{
    public class SheepAI : AnimalBaseAI
    {
        protected override void Awake()
        {
            base.Awake();
            // 양 특성 설정
            _wanderRadius = 18f;
            _wanderInterval = 3.5f;
            _moveSpeed = 1.8f;
            _idleTime = 2.5f;
            _fleeRange = 3.5f;
            _fleeSpeed = 3.5f;
        }
        
        protected override void Start()
        {
            base.Start();
            // Grass 레이어 설정
            if (_groundLayer == 0)
                _groundLayer = LayerMask.GetMask("Grass", "Ground");
        }
    }
}
