using UnityEngine;
using SunnysideIsland.Events;
using SunnysideIsland.Inventory;
using SunnysideIsland.Pool;
using DI;

namespace SunnysideIsland.Items
{
    public class PickableItem : MonoBehaviour
    {
        [Header("=== Settings ===")]
        [SerializeField] private string _itemId = "wood";
        [SerializeField] private int _quantity = 1;
        [SerializeField] private string _poolName = "Wood"; // 풀 이름 (기본값 Wood)
        
        public string ItemId => _itemId;
        public int Quantity => _quantity;

        [Inject] private IInventorySystem _inventory;
        [Inject] private IPoolManager _poolManager;
        
        private void Start()
        {
            DIContainer.Inject(this);
        }

        public void PickUp()
        {
            if (_inventory != null)
            {
                bool added = _inventory.AddItem(_itemId, _quantity);
                if (added)
                {
                    // 오브젝트 풀링 반환
                    if (_poolManager != null && _poolManager.GetPool(_poolName) != null)
                    {
                        _poolManager.Despawn(_poolName, gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning($"[PickableItem] Failed to add {_itemId} to inventory");
                }
            }
            else
            {
                Debug.LogWarning("[PickableItem] No InventorySystem injected");
            }
        }
    }
}