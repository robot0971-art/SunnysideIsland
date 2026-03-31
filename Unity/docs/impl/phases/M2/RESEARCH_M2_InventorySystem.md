# M2 리서치 문서: 인벤토리 시스템 (InventorySystem)

> **대상 시스템**: InventorySystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M2

---

## 1. 개요

### 리서치 목적
플레이어의 아이템 수집, 관리, 사용을 위한 인벤토리 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 인벤토리 슬롯 관리
  - 아이템 추가/제거/이동
  - 퀵슬롯 시스템
  - 장비 시스템 기반
  - 아이템 사용
- **제외**: 
  - 인벤토리 UI (M3에서 처리)
  - 드래그 앤 드롭 UI 구현

### 예상 산출물
- `InventorySystem.cs` - 메인 인벤토리 관리
- `InventorySlot.cs` - 개별 슬롯
- `PlayerInventory.cs` - 플레이어 인벤토리 컴포넌트
- `EquipmentSystem.cs` - 장비 관리
- `IInventory` 인터페이스
- 저장/불러오기 지원

---

## 2. 기획 문서 분석

### 요구사항 정리

#### 기능적 요구사항

1. **기본 인벤토리**
   - 40개 슬롯 (8x5 그리드)
   - 아이템 중첩 가능 (maxStack 기준)
   - 슬롯별 아이템 저장
   - 빈 슬롯 관리

2. **아이템 조작**
   - AddItem: 아이템 추가 (자동 중첩)
   - RemoveItem: 아이템 제거
   - MoveItem: 슬롯 간 이동
   - SwapItem: 슬롯 교환
   - UseItem: 아이템 사용

3. **퀵슬롯**
   - 8개 퀵슬롯 (숫자키 1-8)
   - 인벤토리와 연동
   - 퀵슬롯에서 아이템 사용

4. **장비 시스템**
   - 무기 슬롯
   - 도구 슬롯
   - 장비 착용/해제

### 인터페이스 정의

```csharp
public interface IInventory
{
    int Capacity { get; }
    int UsedSlots { get; }
    int EmptySlots { get; }
    
    bool AddItem(string itemId, int quantity = 1);
    bool RemoveItem(int slotIndex, int quantity = 1);
    bool RemoveItem(string itemId, int quantity = 1);
    bool MoveItem(int fromSlot, int toSlot);
    bool SwapItems(int slotA, int slotB);
    bool UseItem(int slotIndex);
    
    InventorySlot GetSlot(int index);
    int FindItem(string itemId);
    int CountItem(string itemId);
    void Clear();
}

public interface IInventorySlot
{
    string ItemId { get; }
    int Quantity { get; }
    int MaxStack { get; }
    bool IsEmpty { get; }
    bool IsFull { get; }
    
    bool CanAdd(string itemId, int quantity);
    bool Add(string itemId, int quantity);
    bool Remove(int quantity);
    void Clear();
}
```

---

## 3. 기존 코드 분석

### ItemData 분석

**파일 경로**: `Assets/Scripts/GameData/ItemData.cs`

**주요 필드**:
- itemId: 아이템 고유 ID
- itemName: 표시 이름
- itemType: Tool, Consumable, Material, Equipment, Valuable, Quest
- maxStack: 최대 중첩 수량
- canSell: 판매 가능 여부

**의견**: ItemData를 기반으로 인벤토리 관리 가능

---

### EconomyEvents 분석

**파일 경로**: `Assets/Scripts/Events/EconomyEvents.cs`

**관련 이벤트**:
- ItemPickedUpEvent
- ItemDroppedEvent
- ItemPurchasedEvent
- ItemSoldEvent

**의견**: 인벤토리 변경 시 적절한 이벤트 발행 필요

---

## 4. 의존성 분석

### 의존하는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| EventBus | 강함 | 인벤토리 변경 이벤트 발행 |
| ItemData | 강함 | 아이템 정보 조회 |
| SaveSystem | 강함 | 저장/불러오기 |

### 의존되는 시스템

| 시스템 | 의존성 유형 | 설명 |
|--------|-------------|------|
| HungerSystem | 강함 | 음식 아이템 사용 |
| HealthSystem | 강함 | 회복 아이템 사용 |
| ShopSystem | 강함 | 상품 구매/판매 |

---

## 5. 구현 방향 결정

### 클래스 설계

```csharp
// InventorySlot.cs
[Serializable]
public class InventorySlot : IInventorySlot
{
    public string ItemId { get; private set; }
    public int Quantity { get; private set; }
    
    private int _maxStack;
    
    public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Quantity <= 0;
    public bool IsFull => Quantity >= _maxStack;
    public int MaxStack => _maxStack;
    
    public bool CanAdd(string itemId, int quantity)
    {
        if (IsEmpty) return true;
        return ItemId == itemId && Quantity + quantity <= _maxStack;
    }
    
    public bool Add(string itemId, int quantity, int maxStack)
    {
        if (!CanAdd(itemId, quantity)) return false;
        
        if (IsEmpty)
        {
            ItemId = itemId;
            _maxStack = maxStack;
        }
        
        Quantity += quantity;
        return true;
    }
    
    public bool Remove(int quantity)
    {
        if (Quantity < quantity) return false;
        
        Quantity -= quantity;
        if (Quantity <= 0)
        {
            Clear();
        }
        return true;
    }
    
    public void Clear()
    {
        ItemId = null;
        Quantity = 0;
        _maxStack = 0;
    }
}

// InventorySystem.cs
public class InventorySystem : MonoBehaviour, IInventory, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private int _capacity = 40;
    [SerializeField] private int _quickSlotCount = 8;
    
    [Inject]
    private IEventBus _eventBus;
    
    [Inject]
    private IGameDataLoader _dataLoader;
    
    private List<InventorySlot> _slots;
    private int[] _quickSlots; // 퀵슬롯이 참조하는 인벤토리 인덱스
    
    public int Capacity => _capacity;
    public int UsedSlots => _slots.Count(s => !s.IsEmpty);
    public int EmptySlots => _capacity - UsedSlots;
    
    private void Awake()
    {
        InitializeSlots();
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
            _quickSlots[i] = -1; // -1 = 비어있음
        }
    }
    
    public bool AddItem(string itemId, int quantity = 1)
    {
        var itemData = _dataLoader.GetItemData(itemId);
        if (itemData == null) return false;
        
        int remaining = quantity;
        
        // 기존 슬롯에 중첩 시도
        for (int i = 0; i < _capacity && remaining > 0; i++)
        {
            if (_slots[i].ItemId == itemId && !_slots[i].IsFull)
            {
                int canAdd = Mathf.Min(remaining, itemData.maxStack - _slots[i].Quantity);
                _slots[i].Add(itemId, canAdd, itemData.maxStack);
                remaining -= canAdd;
            }
        }
        
        // 빈 슬롯에 추가
        for (int i = 0; i < _capacity && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty)
            {
                int canAdd = Mathf.Min(remaining, itemData.maxStack);
                _slots[i].Add(itemId, canAdd, itemData.maxStack);
                remaining -= canAdd;
            }
        }
        
        int added = quantity - remaining;
        if (added > 0)
        {
            _eventBus.Publish(new ItemPickedUpEvent 
            { 
                ItemId = itemId, 
                Quantity = added,
                TotalQuantity = CountItem(itemId)
            });
            return true;
        }
        
        return false; // 인벤토리 가득참
    }
    
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _capacity) return false;
        if (_slots[slotIndex].IsEmpty) return false;
        
        var itemData = _dataLoader.GetItemData(_slots[slotIndex].ItemId);
        if (itemData == null) return false;
        
        // 아이템 타입별 사용 처리
        switch (itemData.itemType)
        {
            case ItemType.Consumable:
                // HungerSystem에 연동하여 음식 사용
                break;
            case ItemType.Tool:
                // 장착
                break;
            case ItemType.Equipment:
                // 장착
                break;
        }
        
        // 소모품은 사용 후 제거
        if (itemData.itemType == ItemType.Consumable)
        {
            _slots[slotIndex].Remove(1);
        }
        
        return true;
    }
    
    public object CaptureState()
    {
        var saveData = new InventorySaveData
        {
            Slots = _slots.Select(s => new SlotSaveData
            {
                ItemId = s.ItemId,
                Quantity = s.Quantity
            }).ToList(),
            QuickSlots = _quickSlots.ToList()
        };
        return saveData;
    }
    
    public void RestoreState(object state)
    {
        if (state is InventorySaveData saveData)
        {
            InitializeSlots();
            
            for (int i = 0; i < saveData.Slots.Count && i < _capacity; i++)
            {
                if (!string.IsNullOrEmpty(saveData.Slots[i].ItemId))
                {
                    var itemData = _dataLoader.GetItemData(saveData.Slots[i].ItemId);
                    if (itemData != null)
                    {
                        _slots[i].Add(saveData.Slots[i].ItemId, 
                            saveData.Slots[i].Quantity, itemData.maxStack);
                    }
                }
            }
            
            for (int i = 0; i < saveData.QuickSlots.Count && i < _quickSlotCount; i++)
            {
                _quickSlots[i] = saveData.QuickSlots[i];
            }
        }
    }
}
```

---

## 6. 구현 계획

| 단계 | 작업 내용 | 예상 시간 | 의존성 |
|------|-----------|-----------|--------|
| 1 | InventorySlot 구현 | 1h | 없음 |
| 2 | InventorySystem 기본 구현 | 3h | EventBus, GameDataLoader |
| 3 | 퀵슬롯 시스템 | 1h | InventorySystem |
| 4 | 아이템 사용 연동 | 1h | HungerSystem |
| 5 | 저장/불러오기 | 1h | SaveSystem |
| 6 | 테스트 | 1h | 전체 |

### 완료 조건

- [ ] 아이템 추가 시 자동 중첩
- [ ] 인벤토리 가득 시 추가 불가
- [ ] 아이템 사용 시 소모품 제거
- [ ] 퀵슬롯과 인벤토리 연동
- [ ] 저장/불러오기 정상 작동

---

## 리서치 완료 확인

- [x] 기획 문서 검토 완료
- [x] 기존 코드 분석 완료
- [x] 의존성 분석 완료
- [x] 구현 방향 결정 완료
- [x] PLAN 문서 작성 준비 완료

**리서치 완료일**: 2026-03-13
