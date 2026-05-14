using System;
using System.Collections.Generic;
using DI;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.GameData;
using UnityEngine;
using Newtonsoft.Json.Linq;
using GameDataClass = SunnysideIsland.GameData.GameData;

namespace SunnysideIsland.Inventory
{
    /// <summary>
    /// ?몃깽?좊━ ?쒖뒪???명꽣?섏씠??
    /// </summary>
    public interface IInventorySystem
    {
        int Capacity { get; }
        int UsedSlots { get; }
        int EmptySlots { get; }


        bool AddItem(string itemId, int quantity = 1);
        bool RemoveItem(int slotIndex, int quantity = 1);
        bool RemoveItem(string itemId, int quantity = 1);
        bool MoveItem(int fromSlot, int toSlot);
        InventorySlot GetSlot(int index);
        int FindItem(string itemId);
        int CountItem(string itemId);
    }

    /// <summary>
    /// ?뚮젅?댁뼱 ?몃깽?좊━ ?쒖뒪??
    /// </summary>
    public class InventorySystem : MonoBehaviour, IInventorySystem, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private int _capacity = 40;
        [SerializeField] private int _quickSlotCount = 8;
        [Inject(Optional = true)]
        [SerializeField] private GameDataClass _gameData;


        private List<InventorySlot> _slots = default!;
        private int[] _quickSlots;


        public int Capacity => _capacity;
        public int UsedSlots => GetUsedSlotCount();
        public int EmptySlots => _capacity - UsedSlots;


        public string SaveKey => "Inventory";


        private void Awake()
        {
            InitializeSlots();
            if (DI.DIContainer.Global == null)
            {
                DI.DIContainer.InitializeGlobal();
            }
            DI.DIContainer.Global.RegisterInstance<IInventorySystem>(this);
        }


        private void Start()
        {
            // GameData媛 ?놁쑝硫?李얘린
            if (_gameData == null)
            {
                DIContainer.Inject(this);
                DIContainer.TryResolve(out _gameData);
                if (_gameData == null)
                {
                    Debug.LogWarning("[InventorySystem] GameData not found. Using default max stack size.");
                }
            }
            EnsureEssentialItems();
        }
        
        private void EnsureEssentialItems()
        {
            if (_gameData == null) return;
            
            // Ensure wood item exists
            if (_gameData.GetItem("wood") == null)
            {
                var wood = new ItemData
                {
                    itemId = "wood",
                    itemName = "?섎Т",
                    itemType = ItemType.Material,
                    maxStack = 99,
                    baseValue = 1,
                    canSell = true,
                    description = "諛곕? 留뚮뱶?????꾩슂???섎Т",
                    iconPath = ""
                };
                _gameData.items.Add(wood);
                Debug.Log("[InventorySystem] Added missing item: wood");
            }
            
            // Ensure boat item exists
            if (_gameData.GetItem("boat") == null)
            {
                var boat = new ItemData
                {
                    itemId = "boat",
                    itemName = "Boat",
                    itemType = ItemType.Valuable,
                    maxStack = 1,
                    baseValue = 1000,
                    canSell = false,
                    description = "A boat that can be used to escape the island.",
                    iconPath = ""
                };
                _gameData.items.Add(boat);
                Debug.Log("[InventorySystem] Added missing item: boat");
            }

            if (_gameData.GetItem("milk") == null)
            {
                var milk = new ItemData
                {
                    itemId = "milk",
                    itemName = "?곗쑀",
                    itemType = ItemType.Consumable,
                    maxStack = 20,
                    baseValue = 25,
                    canSell = true,
                    description = "?뚯뿉寃뚯꽌 ?살? ?곗쑀. ?덇린? 泥대젰??議곌툑 ?뚮났?쒕떎.",
                    iconPath = "",
                    hungerRestore = 5,
                    healthRestore = 10,
                    staminaRestore = 0f
                };
                _gameData.items.Add(milk);
                Debug.Log("[InventorySystem] Added missing item: milk");
            }

            if (_gameData.GetItem("pork") == null)
            {
                var pork = new ItemData
                {
                    itemId = "pork",
                    itemName = "?쇱?怨좉린",
                    itemType = ItemType.Consumable,
                    maxStack = 20,
                    baseValue = 35,
                    canSell = true,
                    description = "?쇱??먭쾶???살? 怨좉린?낅땲?? 諛곌퀬?붿쓣 ?뚮났?????덉뒿?덈떎.",
                    iconPath = "",
                    hungerRestore = 15,
                    healthRestore = 0,
                    staminaRestore = 0f
                };
                _gameData.items.Add(pork);
                Debug.Log("[InventorySystem] Added missing item: pork");
            }
        }


        private void InitializeSlots()
        {
            _slots = new List<InventorySlot>(_capacity);
            for (int i = 0; i < _capacity; i++)
            {
                _slots.Add(new InventorySlot());
            }


            _quickSlots = new int[_quickSlotCount];
            for (int i = 0; i < _quickSlotCount; i++)
            {
                _quickSlots[i] = -1;
            }
        }


        private int GetUsedSlotCount()
        {
            int count = 0;
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty) count++;
            }
            return count;
        }


public bool AddItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

            int maxStack = GetMaxStack(itemId);
            int remaining = quantity;

            // 湲곗〈 ?щ’??以묒꺽 ?쒕룄

            for (int i = 0; i < _capacity && remaining > 0; i++)
            {
                if (_slots[i].ItemId == itemId && !_slots[i].IsFull)
                {
                    int canAdd = Mathf.Min(remaining, maxStack - _slots[i].Quantity);
                    if (_slots[i].Add(itemId, canAdd, maxStack))
                    {
                        remaining -= canAdd;
                    }
                }
            }

            // 鍮??щ’??異붽?

            for (int i = 0; i < _capacity && remaining > 0; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    int canAdd = Mathf.Min(remaining, maxStack);
                    if (_slots[i].Add(itemId, canAdd, maxStack))
                    {
                        remaining -= canAdd;
                    }
                }
            }


            int added = quantity - remaining;
            
            if (added > 0)
            {
                EventBus.Publish(new ItemPickedUpEvent
                {
                    ItemId = itemId,
                    Quantity = added,
                    TotalQuantity = CountItem(itemId)
                });
                return true;
            }


            return false;
        }


        public bool RemoveItem(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= _capacity) return false;
            var slot = _slots[slotIndex];
            if (slot.IsEmpty)
            {
                return false;
            }

            string itemId = slot.ItemId;
            int canRemove = Mathf.Min(quantity, slot.Quantity);
            bool removed = slot.Remove(quantity);
            if (removed)
            {
                EventBus.Publish(new ItemRemovedEvent
                {
                    ItemId = itemId,
                    Quantity = canRemove,
                    TotalQuantity = CountItem(itemId)
                });
            }

            return removed;
        }


        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;


            int remaining = quantity;


            for (int i = 0; i < _capacity && remaining > 0; i++)
            {
                if (_slots[i].ItemId == itemId)
                {
                    int canRemove = Mathf.Min(remaining, _slots[i].Quantity);
                    if (_slots[i].Remove(canRemove))
                    {
                        remaining -= canRemove;
                    }
                }
            }

            int removed = quantity - remaining;
            if (removed > 0)
            {
                EventBus.Publish(new ItemRemovedEvent
                {
                    ItemId = itemId,
                    Quantity = removed,
                    TotalQuantity = CountItem(itemId)
                });
            }

            return remaining == 0;
        }


        public bool MoveItem(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= _capacity) return false;
            if (toSlot < 0 || toSlot >= _capacity) return false;
            if (fromSlot == toSlot) return false;
            if (_slots[fromSlot].IsEmpty) return false;


            var fromItem = _slots[fromSlot];
            var toItem = _slots[toSlot];


            if (toItem.IsEmpty)
            {
                toItem.Add(fromItem.ItemId, fromItem.Quantity, fromItem.MaxStack);
                fromItem.Clear();
            }
            else if (fromItem.ItemId == toItem.ItemId)
            {
                int canAdd = Mathf.Min(fromItem.Quantity, toItem.MaxStack - toItem.Quantity);
                if (canAdd > 0)
                {
                    toItem.Add(fromItem.ItemId, canAdd, toItem.MaxStack);
                    fromItem.Remove(canAdd);
                }
            }
            else
            {
                string tempId = toItem.ItemId;
                int tempQty = toItem.Quantity;
                int tempMax = toItem.MaxStack;


                toItem.Clear();
                toItem.Add(fromItem.ItemId, fromItem.Quantity, fromItem.MaxStack);


                fromItem.Clear();
                fromItem.Add(tempId, tempQty, tempMax);
            }


            EventBus.Publish(new ItemMovedEvent
            {
                FromSlot = fromSlot,
                ToSlot = toSlot,
                ItemId = _slots[toSlot].ItemId
            });


            return true;
        }


        public InventorySlot GetSlot(int index)
        {
            if (_slots == null)
            {
                InitializeSlots();
            }
            if (index < 0 || index >= _capacity) return null;
            return _slots[index];
        }


        public int FindItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return -1;


            for (int i = 0; i < _capacity; i++)
            {
                if (_slots[i].ItemId == itemId)
                {
                    return i;
                }
            }
            return -1;
        }


        public int CountItem(string itemId)
        {
            if (_slots == null) return 0;

            int count = 0;
            for (int i = 0; i < _capacity && i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].ItemId == itemId)
                {
                    count += _slots[i].Quantity;
                }
            }
            return count;
        }



        private int GetMaxStack(string itemId)
        {
            if (_gameData != null)
            {
                var itemData = _gameData.GetItem(itemId);
                if (itemData != null)
                {
                    return itemData.maxStack;
                }
            }
            return 99;
        }


        public object GetSaveData()
        {
            var slotData = new List<SlotSaveData>();
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty)
                {
                    slotData.Add(new SlotSaveData
                    {
                        ItemId = slot.ItemId,
                        Quantity = slot.Quantity
                    });
                }
                else
                {
                    slotData.Add(null);
                }
            }


            return new InventorySaveData
            {
                Slots = slotData,
                QuickSlots = _quickSlots
            };
        }


        public void LoadSaveData(object state)
        {
            Debug.Log($"[InventorySystem] LoadSaveData called.");
            InventorySaveData saveData = null;

            if (state is InventorySaveData data)
            {
                saveData = data;
            }
            else if (state is JObject jObject)
            {
                saveData = jObject.ToObject<InventorySaveData>();
            }

            if (saveData != null)
            {
                InitializeSlots();

                if (saveData.Slots != null)
                {
                    int loadedCount = 0;
                    for (int i = 0; i < saveData.Slots.Count && i < _capacity; i++)
                    {
                        if (saveData.Slots[i] != null)
                        {
                            int maxStack = GetMaxStack(saveData.Slots[i].ItemId);
                            _slots[i].Add(saveData.Slots[i].ItemId, saveData.Slots[i].Quantity, maxStack);
                            loadedCount++;
                        }
                    }
                    Debug.Log($"[InventorySystem] Loaded {loadedCount} slots from save data");
                }

                if (saveData.QuickSlots != null)
                {
                    for (int i = 0; i < saveData.QuickSlots.Length && i < _quickSlotCount; i++)
                    {
                        _quickSlots[i] = saveData.QuickSlots[i];
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] LoadSaveData: saveData is null or invalid");
            }
        }
    }


    [Serializable]
    public class SlotSaveData
    {
        public string ItemId;
        public int Quantity;
    }


    [Serializable]
    public class InventorySaveData
    {
        public List<SlotSaveData> Slots;
        public int[] QuickSlots;
    }
}
