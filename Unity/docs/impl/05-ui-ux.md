# 5. UI/UX 시스템 구현 계획

## 5.1 UI 아키텍처

### UI 패턴
```
MVP (Model-View-Presenter)
├── Model: 게임 데이터 (PlayerStats, Inventory 등)
├── View: UI 컴포넌트 (Text, Image, Button 등)
└── Presenter: 중재자 (UIManager)
```

### UI Manager 구조
```csharp
public class UIManager : MonoBehaviour
{
    [Header("=== UI Panels ===")]
    [SerializeField] private HUDPanel _hudPanel;
    [SerializeField] private InventoryPanel _inventoryPanel;
    [SerializeField] private BuildingPanel _buildingPanel;
    [SerializeField] private DialoguePanel _dialoguePanel;
    [SerializeField] private PauseMenuPanel _pauseMenuPanel;
    [SerializeField] private ShopPanel _shopPanel;
    [SerializeField] private CraftingPanel _craftingPanel;
    
    [Inject] private IEventBus _eventBus;
    
    private Stack<UIPanel> _uiStack;
    
    public void OpenPanel(UIPanel panel);
    public void ClosePanel(UIPanel panel);
    public void CloseAllPanels();
    public void ShowHUD();
    public void HideHUD();
}

public abstract class UIPanel : MonoBehaviour
{
    public bool IsOpen { get; protected set; }
    public bool IsModal { get; protected set; }
    
    public virtual void Open();
    public virtual void Close();
    public virtual void OnBackButton();
}
```

---

## 5.2 HUD 구현

### 구현 우선순위: P0 (필수)

#### HUDPanel 구현
```csharp
public class HUDPanel : UIPanel
{
    [Header("=== Stat Bars ===")]
    [SerializeField] private StatBar _healthBar;
    [SerializeField] private StatBar _hungerBar;
    [SerializeField] private StatBar _staminaBar;
    
    [Header("=== Info Displays ===")]
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private Image _weatherIcon;
    
    [Header("=== Quick Slots ===")]
    [SerializeField] private QuickSlotUI[] _quickSlots;
    
    [Header("=== Notifications ===")]
    [SerializeField] private NotificationArea _notificationArea;
    
    [Inject] private IPlayerStats _playerStats;
    [Inject] private ITimeManager _timeManager;
    [Inject] private ICurrencySystem _currency;
    
    private void Update()
    {
        UpdateStatBars();
        UpdateTimeDisplay();
    }
    
    public void ShowNotification(string message, float duration = 3f);
    public void ShowDamageNumber(Vector3 position, int damage);
    public void ShowItemPickup(ItemData item, int quantity);
}

public class StatBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private TextMeshProUGUI _valueText;
    
    public void SetValue(float current, float max);
    public void SetColor(Color color);
}

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private TextMeshProUGUI _keyText;
    
    public void SetItem(ItemData item, int quantity);
    public void Clear();
}
```

#### HUD 요소
1. **체력 바**: 좌측 상단, 빨간색
2. **허기 바**: 체력 바 아래, 주황색
3. **스태미나 바**: 허기 바 아래, 노란색
4. **골드 표시**: 우측 상단
5. **시간/날짜**: 우측 상단 아래
6. **날씨 아이콘**: 시간 옆
7. **퀵슬롯**: 하단 중앙 (8개)
8. **알림 영역**: 좌측 하단

### 예상 개발 시간: 1일

---

## 5.3 인벤토리 UI

### 구현 우선순위: P0 (필수)

#### InventoryPanel 구현
```csharp
public class InventoryPanel : UIPanel
{
    [Header("=== Inventory Grid ===")]
    [SerializeField] private GridLayoutGroup _inventoryGrid;
    [SerializeField] private InventorySlotUI _slotPrefab;
    
    [Header("=== Equipment Slots ===")]
    [SerializeField] private EquipmentSlotUI _weaponSlot;
    [SerializeField] private EquipmentSlotUI _armorSlot;
    [SerializeField] private EquipmentSlotUI[] _accessorySlots;
    
    [Header("=== Item Detail ===")]
    [SerializeField] private ItemDetailPanel _itemDetailPanel;
    
    [Inject] private IPlayerInventory _inventory;
    
    private InventorySlotUI[] _slots;
    
    public void RefreshInventory();
    public void OnSlotClicked(InventorySlotUI slot);
    public void OnSlotRightClicked(InventorySlotUI slot);
    public void ShowItemDetail(ItemData item);
}

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _selectedOverlay;
    
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }
    
    public void SetItem(ItemData item, int quantity);
    public void SetSelected(bool selected);
    public void OnPointerClick(PointerEventData eventData);
}

public class ItemDetailPanel : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _itemStats;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _dropButton;
    
    public void ShowItem(ItemData item);
}
```

### 예상 개발 시간: 1일

---

## 5.4 건설 UI

### 구현 우선순위: P0 (필수)

#### BuildingPanel 구현
```csharp
public class BuildingPanel : UIPanel
{
    [Header("=== Category Tabs ===")]
    [SerializeField] private TabGroup _categoryTabs;
    
    [Header("=== Building Grid ===")]
    [SerializeField] private GridLayoutGroup _buildingGrid;
    [SerializeField] private BuildingCard _buildingCardPrefab;
    
    [Header("=== Detail Panel ===")]
    [SerializeField] private BuildingDetailPanel _detailPanel;
    
    [Inject] private IBuildingManager _buildingManager;
    [Inject] private IPlayerInventory _inventory;
    
    public void SelectCategory(BuildingCategory category);
    public void OnBuildingSelected(BuildingData building);
    public void OnBuildButtonClicked();
    public void UpdateResourceRequirements(BuildingData building);
}

public class BuildingCard : MonoBehaviour
{
    [SerializeField] private Image _buildingIcon;
    [SerializeField] private TextMeshProUGUI _buildingName;
    [SerializeField] private GameObject _lockedOverlay;
    
    public BuildingData Data { get; private set; }
    
    public void Setup(BuildingData data);
}

public class BuildingDetailPanel : MonoBehaviour
{
    [SerializeField] private Image _buildingPreview;
    [SerializeField] private TextMeshProUGUI _buildingName;
    [SerializeField] private TextMeshProUGUI _buildingDescription;
    [SerializeField] private Transform _costListContainer;
    [SerializeField] private BuildingCostItem _costItemPrefab;
    
    public void ShowBuilding(BuildingData building);
}
```

### 예상 개발 시간: 0.75일

---

## 5.5 상점 UI

### 구현 우선순위: P0 (필수)

#### ShopPanel 구현
```csharp
public class ShopPanel : UIPanel
{
    [Header("=== Shop UI ===")]
    [SerializeField] private TextMeshProUGUI _shopNameText;
    [SerializeField] private GridLayoutGroup _itemGrid;
    [SerializeField] private ShopItemUI _shopItemPrefab;
    
    [Header("=== Player Gold ===")]
    [SerializeField] private TextMeshProUGUI _playerGoldText;
    
    [Header("=== Tabs ===")]
    [SerializeField] private TabButton _buyTab;
    [SerializeField] private TabButton _sellTab;
    
    [Inject] private IShopSystem _shopSystem;
    [Inject] private ICurrencySystem _currency;
    
    private ShopMode _currentMode;
    
    public void OpenShop(IShopSystem shop);
    public void SwitchMode(ShopMode mode);
    public void OnItemSelected(ItemData item);
    public void OnBuySellButtonClicked(ItemData item, int quantity);
}

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _stockText;
    
    public void Setup(ItemData item, int price, int stock);
}
```

### 예상 개발 시간: 0.75일

---

## 5.6 대화 시스템 UI

### 구현 우선순위: P1 (권장)

#### DialoguePanel 구현
```csharp
public class DialoguePanel : UIPanel
{
    [Header("=== Dialogue UI ===")]
    [SerializeField] private Image _npcPortrait;
    [SerializeField] private TextMeshProUGUI _npcNameText;
    [SerializeField] private TypewriterText _dialogueText;
    
    [Header("=== Choice Buttons ===")]
    [SerializeField] private Transform _choiceContainer;
    [SerializeField] private ChoiceButton _choiceButtonPrefab;
    
    public void StartDialogue(NPC npc, DialogueData dialogue);
    public void ShowNextLine();
    public void ShowChoices(List<DialogueChoice> choices);
    public void OnChoiceSelected(DialogueChoice choice);
}

public class TypewriterText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textComponent;
    [SerializeField] private float _typeSpeed = 0.05f;
    
    public void StartTyping(string text);
    public void SkipTyping();
}
```

### 예상 개발 시간: 0.5일

---

## 5.7 조합 UI

### 구현 우선순위: P1 (권장)

#### CraftingPanel 구현
```csharp
public class CraftingPanel : UIPanel
{
    [Header("=== Crafting UI ===")]
    [SerializeField] private TabGroup _categoryTabs;
    [SerializeField] private GridLayoutGroup _recipeGrid;
    [SerializeField] private RecipeCard _recipeCardPrefab;
    
    [Header("=== Recipe Detail ===")]
    [SerializeField] private RecipeDetailPanel _recipeDetailPanel;
    
    [Inject] private ICraftingSystem _craftingSystem;
    
    public void SelectCategory(CraftingCategory category);
    public void OnRecipeSelected(RecipeData recipe);
    public void OnCraftButtonClicked(RecipeData recipe, int quantity);
}
```

### 예상 개발 시간: 0.5일

---

## 5.8 메뉴 UI

### 구현 우선순위: P0 (필수)

#### MainMenu 구현
```csharp
public class MainMenuPanel : UIPanel
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _loadGameButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    
    public void OnNewGameClicked();
    public void OnLoadGameClicked();
    public void OnSettingsClicked();
    public void OnQuitClicked();
}
```

#### PauseMenu 구현
```csharp
public class PauseMenuPanel : UIPanel
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;
    
    public void OnResumeClicked();
    public void OnSaveClicked();
    public void OnLoadClicked();
    public void OnSettingsClicked();
    public void OnMainMenuClicked();
}
```

### 예상 개발 시간: 0.5일

---

## 5.9 UI 스타일 가이드

### 색상 팔레트
```
기본 배경: #2C3E50 (어두운 파랑)
강조색: #E74C3C (빨강)
보조색: #27AE60 (초록)
경고색: #F39C12 (주황)
텍스트 기본: #ECF0F1 (흰색)
텍스트 어두움: #95A5A6 (회색)
```

### 폰트
- 제목: Noto Sans KR Bold
- 본문: Noto Sans KR Regular
- 숫자: Roboto Mono

### UI 요소 크기
- 버튼: 160 x 50
- 슬롯: 64 x 64
- 패널 패딩: 20px
- 간격: 10px

---

## 5.10 UI/UX 개발 일정

| UI | 우선순위 | 예상 시간 | 담당 |
|-----|----------|-----------|------|
| HUD | P0 | 1일 | - |
| Inventory | P0 | 1일 | - |
| Building | P0 | 0.75일 | - |
| Shop | P0 | 0.75일 | - |
| Dialogue | P1 | 0.5일 | - |
| Crafting | P1 | 0.5일 | - |
| Menus | P0 | 0.5일 | - |
| **합계** | | **5일** | |
