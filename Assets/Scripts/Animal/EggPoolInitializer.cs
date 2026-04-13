using DI;
using SunnysideIsland.Pool;
using UnityEngine;

namespace SunnysideIsland.Animal
{
    public class EggPoolInitializer : MonoBehaviour
    {
        [Header("=== Egg Pool Settings ===")]
        [SerializeField] private GameObject _eggPrefab;
        [SerializeField] private string _poolName = "Egg";
        [SerializeField] private int _initialSize = 10;
        [SerializeField] private int _maxSize = 50;
        
        [Inject(Optional = true)] private IPoolManager _poolManager;
        
        private void Start()
        {
            DIContainer.Inject(this);
            InitializeEggPool();
        }
        
        private void InitializeEggPool()
        {
            // 프리팹이 할당되지 않았으면 Resources에서 찾기
            if (_eggPrefab == null)
            {
                _eggPrefab = Resources.Load<GameObject>("Prefabs/Egg");
                if (_eggPrefab == null)
                    _eggPrefab = Resources.Load<GameObject>("Egg");
            }
            
            if (_eggPrefab == null)
            {
                Debug.LogError("[EggPoolInitializer] Egg prefab is not assigned and not found in Resources!");
                return;
            }
            
            if (_poolManager == null)
            {
                DIContainer.TryResolve(out _poolManager);
            }

            if (_poolManager != null)
            {
                // 풀이 이미 존재하는지 확인
                var existingPool = _poolManager.GetPool(_poolName);
                if (existingPool == null)
                {
                    // 새 풀 생성
                    _poolManager.CreatePool(_poolName, _eggPrefab, _initialSize, _maxSize);
                }
            }
            else
            {
                Debug.LogError("[EggPoolInitializer] PoolManager instance not found!");
            }
        }
    }
}
