using System.Collections.Generic;
using UnityEngine;
using SunnysideIsland.Events;
using SunnysideIsland.Inventory;
using SunnysideIsland.Pool;
using DI;

namespace SunnysideIsland.Environment
{
    public class WoodDropManager : MonoBehaviour
    {
        [Header("=== Settings ===")]
        [SerializeField] private GameObject _woodPrefab;
        [SerializeField] private float _dropRadius = 0.5f;
        [SerializeField] private string _woodPoolName = "Wood";
        
        [Inject] private IInventorySystem _inventorySystem;
        [Inject] private IPoolManager _poolManager;
        
        private void Start()
        {
            DIContainer.Inject(this);
            EventBus.Subscribe<TreeChoppedEvent>(OnTreeChopped);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<TreeChoppedEvent>(OnTreeChopped);
        }
        
        private void OnTreeChopped(TreeChoppedEvent evt)
        {
            if (_woodPrefab == null) return;

            // 풀이 없으면 생성
            if (_poolManager != null && _poolManager.GetPool(_woodPoolName) == null)
            {
                _poolManager.CreatePool(_woodPoolName, _woodPrefab, 30, 100);
            }

            for (int i = 0; i < evt.WoodAmount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * _dropRadius;
                Vector3 spawnPos = evt.TreePosition + new Vector3(randomCircle.x, randomCircle.y, 0);
                
                if (_poolManager != null)
                {
                    _poolManager.Spawn(_woodPoolName, spawnPos, Quaternion.identity);
                }
                else
                {
                    Instantiate(_woodPrefab, spawnPos, Quaternion.identity);
                }
            }
            
            Debug.Log($"[WoodDropManager] Dropped {evt.WoodAmount} wood from pool");
        }
    }
}