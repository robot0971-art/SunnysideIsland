# M4 리서치 문서: 경제 시스템 (CurrencySystem, ShopSystem)

> **대상 시스템**: CurrencySystem, ShopSystem
> 
> **리서치 일자**: 2026-03-13
> 
> **작성자**: AI
> 
> **관련 마일스톤**: M4

---

## 1. 개요

### 리서치 목적
게임 내 통화(골드) 관리와 상점 시스템을 구현합니다.

### 리서치 범위
- **포함**: 
  - 골드 획득/소비
  - 상점 구매/판매
  - 상품 관리
- **제외**: 
  - 상점 UI (M3-6에서 처리)

### 예상 산출물
- `CurrencySystem.cs` - 통화 관리
- `ShopSystem.cs` - 상점 관리
- `ShopSlot.cs` - 상품 슬롯

---

## 2. 요구사항 정리

### CurrencySystem (통화 시스템)

1. **골드 관리**
   - 골드 획득
   - 골드 소비
   - 잔액 확인

2. **수입/지출 추적**
   - 일일 수입
   - 일일 지출
   - 누적 골드

### ShopSystem (상점 시스템)

1. **상품 관리**
   - 상품 등록
   - 재고 관리
   - 가격 설정

2. **거래**
   - 구매
   - 판매
   - 가격 계산

### 인터페이스 정의

```csharp
public interface ICurrencySystem
{
    int CurrentGold { get; }
    
    bool CanAfford(int amount);
    bool TrySpend(int amount, string reason);
    void Earn(int amount, string reason);
}

public interface IShopSystem
{
    bool BuyItem(string itemId, int quantity);
    bool SellItem(string itemId, int quantity);
    int GetBuyPrice(string itemId);
    int GetSellPrice(string itemId);
}
```

---

## 3. 기존 코드 분석

### EconomyEvents 분석

**파일 경로**: `Assets/Scripts/Events/EconomyEvents.cs`

**관련 이벤트**:
- GoldChangedEvent
- ItemSoldEvent
- ItemPurchasedEvent

### ShopItemData 분석

**파일 경로**: `Assets/Scripts/GameData/ShopItemData.cs`

**주요 필드**:
- itemId: 아이템 ID
- basePrice: 기본 가격
- stock: 재고

---

## 4. 구현 방향 결정

### 클래스 설계

```csharp
// CurrencySystem.cs
public class CurrencySystem : MonoBehaviour, ICurrencySystem, ISaveable
{
    [Header("=== Settings ===")]
    [SerializeField] private int _startingGold = 100;
    
    private int _currentGold;
    private int _dailyIncome;
    private int _dailyExpense;
    
    public int CurrentGold => _currentGold;
    
    private void Start()
    {
        _currentGold = _startingGold;
    }
    
    public bool CanAfford(int amount)
    {
        return _currentGold >= amount;
    }
    
    public bool TrySpend(int amount, string reason)
    {
        if (!CanAfford(amount)) return false;
        
        _currentGold -= amount;
        _dailyExpense += amount;
        
        EventBus.Publish(new GoldChangedEvent
        {
            CurrentGold = _currentGold,
            ChangeAmount = -amount,
            Reason = reason
        });
        
        return true;
    }
    
    public void Earn(int amount, string reason)
    {
        _currentGold += amount;
        _dailyIncome += amount;
        
        EventBus.Publish(new GoldChangedEvent
        {
            CurrentGold = _currentGold,
            ChangeAmount = amount,
            Reason = reason
        });
    }
}

// ShopSystem.cs
public class ShopSystem : MonoBehaviour, IShopSystem
{
    [Header("=== Settings ===")]
    [SerializeField] private List<ShopSlot> _shopSlots = new List<ShopSlot>();
    
    private ICurrencySystem _currencySystem;
    
    private void Start()
    {
        _currencySystem = FindObjectOfType<CurrencySystem>();
    }
    
    public bool BuyItem(string itemId, int quantity)
    {
        var slot = FindSlot(itemId);
        if (slot == null) return false;
        if (slot.Stock < quantity) return false;
        
        int totalPrice = slot.BuyPrice * quantity;
        if (!_currencySystem.TrySpend(totalPrice, $"Buy {itemId}")) return false;
        
        slot.RemoveStock(quantity);
        
        EventBus.Publish(new ItemPurchasedEvent
        {
            ItemId = itemId,
            Quantity = quantity,
            TotalPrice = totalPrice
        });
        
        return true;
    }
    
    public bool SellItem(string itemId, int quantity)
    {
        // TODO: 인벤토리에서 아이템 확인 및 제거
        int price = GetSellPrice(itemId) * quantity;
        _currencySystem.Earn(price, $"Sell {itemId}");
        
        EventBus.Publish(new ItemSoldEvent
        {
            ItemId = itemId,
            Quantity = quantity,
            TotalPrice = price
        });
        
        return true;
    }
}

// ShopSlot.cs
[System.Serializable]
public class ShopSlot
{
    public string ItemId;
    public int BuyPrice;
    public int SellPrice;
    public int Stock;
    public int MaxStock;
    
    public void RemoveStock(int amount)
    {
        Stock = Mathf.Max(0, Stock - amount);
    }
    
    public void AddStock(int amount)
    {
        Stock = Mathf.Min(MaxStock, Stock + amount);
    }
}
```

---

## 5. 구현 계획

| 단계 | 작업 내용 | 예상 시간 |
|------|-----------|-----------|
| 1 | CurrencySystem | 1h |
| 2 | ShopSystem | 2h |
| 3 | ShopSlot | 0.5h |
| 4 | 테스트 | 1h |

---

## 리서치 완료 확인

- [x] 요구사항 정리 완료
- [x] 기존 코드 분석 완료
- [x] 구현 방향 결정 완료

**리서치 완료일**: 2026-03-13
