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
        
        [Inject(Optional = true)] private IPoolManager _poolManager = default!;
        
        private void Start()
        {
            DIContainer.Inject(this);
            InitializeEggPool();
        }
        
        private void InitializeEggPool()
        {
            // ?ÑÎ¶¨?πÏù¥ ?†Îãπ?òÏ? ?äÏïò?ºÎ©¥ Resources?êÏÑú Ï∞æÍ∏∞
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
                // ?Ä???¥Î? Ï°¥Ïû¨?òÎäîÏßÄ ?ïÏù∏
                var existingPool = _poolManager.GetPool(_poolName);
                if (existingPool == null)
                {
                    // ???Ä ?ùÏÑ±
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
