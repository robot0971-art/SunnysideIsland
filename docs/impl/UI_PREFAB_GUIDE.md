# Unity UI 프리팹 생성 가이드

이 문서는 Sunnyside Island 프로젝트의 UI 프리팹을 Unity 에디터에서 생성하는 방법을 설명합니다.

---

## 1. 개요

### 1.1 UI 시스템 구조

```
UI/
├── Core/                    # 핵심 시스템
│   ├── UIManager.cs         # UI 관리자 (싱글톤)
│   └── UIPanel.cs           # 패널 베이스 클래스
├── Components/              # 재사용 가능한 컴포넌트
│   ├── StatBar.cs           # 상태 바 (체력, 허기 등)
│   ├── SlotUI.cs            # 슬롯 UI (인벤토리, 퀵슬롯)
│   └── NotificationArea.cs  # 알림 영역
├── HUD/                     # 헤드업 디스플레이
│   └── HUDPanel.cs
├── Menu/                    # 메뉴 패널
│   ├── MainMenuPanel.cs
│   ├── PauseMenuPanel.cs
│   └── SettingsPanel.cs
├── Inventory/               # 인벤토리 관련
│   ├── InventoryPanel.cs
│   └── ItemDetailPanel.cs
├── Building/                # 건설 관련
│   ├── BuildingPanel.cs
│   ├── BuildingCard.cs
│   └── BuildingDetailPanel.cs
├── Shop/                    # 상점 관련
│   ├── ShopPanel.cs
│   └── ShopItemUI.cs
├── Dialogue/                # 대화 시스템
│   └── DialoguePanel.cs
└── Quest/                   # 퀘스트 시스템
    ├── QuestPanel.cs
    └── QuestDetailPanel.cs
```

### 1.2 프리팹 저장 위치

```
Assets/
└── Prefabs/
    └── UI/
        ├── Core/
        │   ├── UIManager.prefab
        │   └── EventSystem.prefab
        ├── Components/
        │   ├── StatBar.prefab
        │   ├── SlotUI.prefab
        │   ├── NotificationItem.prefab
        │   ├── QuestItem.prefab
        │   └── ObjectiveItem.prefab
        ├── Panels/
        │   ├── HUDPanel.prefab
        │   ├── MainMenuPanel.prefab
        │   ├── PauseMenuPanel.prefab
        │   ├── SettingsPanel.prefab
        │   ├── InventoryPanel.prefab
        │   ├── ItemDetailPanel.prefab
        │   ├── BuildingPanel.prefab
        │   ├── BuildingDetailPanel.prefab
        │   ├── BuildingCard.prefab
        │   ├── ShopPanel.prefab
        │   ├── ShopItemUI.prefab
        │   ├── DialoguePanel.prefab
        │   ├── QuestPanel.prefab
        │   └── QuestDetailPanel.prefab
        └── Choices/
            └── ChoiceButton.prefab
```

---

## 2. 공통 요구사항

### 2.1 Canvas 설정

모든 UI 패널은 다음 Canvas 설정을 사용합니다:

| 설정 | 값 |
|------|-----|
| Render Mode | Screen Space - Overlay |
| Pixel Perfect | false |
| Sort Order | 패널별 상이 (HUD: 0, 메뉴: 100, 모달: 200) |

**Canvas Scaler 설정:**
| 설정 | 값 |
|------|-----|
| UI Scale Mode | Scale With Screen Size |
| Reference Resolution | 1920 x 1080 |
| Screen Match Mode | Match Width Or Height |
| Match | 0.5 |

### 2.2 CanvasGroup 요구사항

모든 `UIPanel` 상속 클래스는 `CanvasGroup` 컴포넌트가 필요합니다:

```
UIPanel (GameObject)
├── CanvasGroup (Required)
│   ├── Alpha: 1
│   ├── Interactable: true
│   └── Blocks Raycasts: true
└── ... 자식 요소들
```

### 2.3 EventSystem 요구사항

씬에 반드시 하나의 `EventSystem`이 있어야 합니다:

```
EventSystem
├── EventSystem (Component)
└── StandaloneInputModule (Component)
```

---

## 3. 패널별 상세 명세

### 3.1 Core - UIManager

**파일:** `Assets/Prefabs/UI/Core/UIManager.prefab`

```
UIManager
├── Canvas
│   ├── Canvas (Screen Space - Overlay)
│   ├── Canvas Scaler (1920x1080)
│   ├── Graphic Raycaster
│   └── [ Panels Container ]
│       ├── HUDPanel
│       ├── MainMenuPanel
│       ├── PauseMenuPanel
│       ├── SettingsPanel
│       ├── InventoryPanel
│       ├── BuildingPanel
│       ├── ShopPanel
│       ├── DialoguePanel
│       └── QuestPanel
└── EventSystem
    ├── EventSystem
    └── StandaloneInputModule
```

**UIManager.cs SerializeField:**
| 필드 | 타입 | 설명 |
|------|------|------|
| `_panels` | `List<UIPanel>` | 모든 패널 참조 리스트 |
| `_usePanelStack` | `bool` | 패널 스택 사용 여부 |
| `_closeAllOnSceneChange` | `bool` | 씬 변경 시 모든 패널 닫기 |

---

### 3.2 Components - StatBar

**파일:** `Assets/Prefabs/UI/Components/StatBar.prefab`

```
StatBar
├── Background (Image)
│   └── Color: #333333 (어두운 회색)
├── Fill (Image)
│   ├── Image Type: Filled
│   ├── Fill Method: Horizontal
│   └── Fill Origin: Left
├── ValueText (TextMeshProUGUI)
│   ├── Font Size: 16
│   └── Alignment: Middle Center
└── MaxValueText (TextMeshProUGUI)
    ├── Text: "/ 100"
    ├── Font Size: 14
    └── Alignment: Middle Left
```

**StatBar.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_fillImage` | `Image` | O |
| `_backgroundImage` | `Image` | X |
| `_valueText` | `TextMeshProUGUI` | O |
| `_maxValueText` | `TextMeshProUGUI` | X |
| `_showValueText` | `bool` | - |
| `_showMaxValue` | `bool` | - |
| `_smoothFill` | `bool` | - |
| `_smoothSpeed` | `float` | - |
| `_normalColor` | `Color` | - |
| `_warningColor` | `Color` | - |
| `_criticalColor` | `Color` | - |
| `_warningThreshold` | `float` | - |
| `_criticalThreshold` | `float` | - |

---

### 3.3 Components - SlotUI

**파일:** `Assets/Prefabs/UI/Components/SlotUI.prefab`

```
SlotUI (64x64)
├── Background (Image)
│   └── Color: #4A4A4A
├── IconImage (Image)
│   ├── Preserve Aspect: true
│   └── Raycast Target: false
├── QuantityText (TextMeshProUGUI)
│   ├── Anchor: Bottom Right
│   ├── Font Size: 12
│   └── Alignment: Bottom Right
├── KeyText (TextMeshProUGUI)
│   ├── Anchor: Top Left
│   ├── Font Size: 10
│   └── Text: "1", "2", ... (퀵슬롯용)
├── SelectedOverlay (Image)
│   ├── Color: #00FF00 (반투명)
│   └── Enabled: false (기본값)
└── LockOverlay (Image)
    ├── Color: #000000 (반투명)
    ├── Enabled: false (기본값)
    └── 자식: LockIcon (Image)
```

**SlotUI.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_iconImage` | `Image` | O |
| `_backgroundImage` | `Image` | X |
| `_selectedOverlay` | `Image` | X |
| `_lockOverlay` | `Image` | X |
| `_quantityText` | `TextMeshProUGUI` | X |
| `_keyText` | `TextMeshProUGUI` | X |
| `_showQuantity` | `bool` | - |
| `_isDraggable` | `bool` | - |
| `_emptyColor` | `Color` | - |
| `_filledColor` | `Color` | - |

---

### 3.4 Components - NotificationArea

**파일:** `Assets/Prefabs/UI/Components/NotificationArea.prefab`

```
NotificationArea
├── RectTransform
│   ├── Anchor: Top Left
│   ├── Pivot: (0, 1)
│   └── Position: (20, -20, 0)
├── Vertical Layout Group
│   ├── Child Alignment: Upper Left
│   ├── Spacing: 5
│   └── Child Force Expand: X=false, Y=false
└── Content Size Fitter
    ├── Horizontal: Preferred Size
    └── Vertical: Preferred Size
```

**NotificationArea.cs SerializeField:**
| 필드 | 타입 | 설명 |
|------|------|------|
| `_maxNotifications` | `int` | 최대 표시 알림 수 (기본값: 5) |
| `_defaultDuration` | `float` | 기본 표시 시간 (기본값: 3초) |
| `_fadeDuration` | `float` | 페이드 시간 (기본값: 0.5초) |
| `_spacing` | `float` | 알림 간격 |
| `_notificationPrefab` | `GameObject` | NotificationItem 프리팹 |
| `_useAnimation` | `bool` | 애니메이션 사용 여부 |
| `_slideInDuration` | `float` | 슬라이드 인 시간 |
| `_slideInCurve` | `AnimationCurve` | 슬라이드 커브 |

---

### 3.5 Components - NotificationItem (프리팹)

**파일:** `Assets/Prefabs/UI/Components/NotificationItem.prefab`

```
NotificationItem
├── RectTransform
│   ├── Size: (400, 30)
│   └── Anchor: Top Left
├── Canvas Group
│   └── Alpha: 0 (애니메이션용)
├── Background (Image)
│   └── Color: #000000 (Alpha: 0.7)
└── Text (TextMeshProUGUI)
    ├── Anchor: Stretch
    ├── Offset: (10, -10, -5, 5)
    ├── Font Size: 14
    ├── Alignment: Left
    └── Color: #FFFFFF
```

---

### 3.6 HUD - HUDPanel

**파일:** `Assets/Prefabs/UI/Panels/HUDPanel.prefab`

```
HUDPanel
├── CanvasGroup
├── StatBars (Vertical Layout)
│   ├── HealthBar (StatBar)
│   ├── HungerBar (StatBar)
│   └── StaminaBar (StatBar)
├── TimeInfo (Horizontal Layout)
│   ├── DayText (TextMeshProUGUI)
│   │   └── Text: "Day 1"
│   ├── TimeText (TextMeshProUGUI)
│   │   └── Text: "06:00"
│   ├── WeatherIcon (Image)
│   └── WeatherText (TextMeshProUGUI)
│       └── Text: "맑음"
├── GoldDisplay (Horizontal Layout)
│   ├── GoldIcon (Image)
│   └── GoldText (TextMeshProUGUI)
│       └── Text: "0 G"
├── QuickSlots (Horizontal Layout)
│   ├── SlotUI (1)
│   ├── SlotUI (2)
│   ├── SlotUI (3)
│   ├── SlotUI (4)
│   ├── SlotUI (5)
│   ├── SlotUI (6)
│   ├── SlotUI (7)
│   └── SlotUI (8)
└── NotificationArea
```

**HUDPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_healthBar` | `StatBar` | O |
| `_hungerBar` | `StatBar` | O |
| `_staminaBar` | `StatBar` | O |
| `_goldText` | `TextMeshProUGUI` | O |
| `_timeText` | `TextMeshProUGUI` | O |
| `_dayText` | `TextMeshProUGUI` | O |
| `_weatherIcon` | `Image` | O |
| `_weatherText` | `TextMeshProUGUI` | O |
| `_quickSlots` | `SlotUI[]` | O |
| `_notificationArea` | `NotificationArea` | O |
| `_sunnyIcon` | `Sprite` | O |
| `_cloudyIcon` | `Sprite` | O |
| `_rainyIcon` | `Sprite` | O |
| `_stormyIcon` | `Sprite` | O |
| `_rainbowIcon` | `Sprite` | O |

---

### 3.7 Menu - MainMenuPanel

**파일:** `Assets/Prefabs/UI/Panels/MainMenuPanel.prefab`

```
MainMenuPanel
├── CanvasGroup
├── Background (Image)
│   └── Source Image: 타이틀 배경
├── TitleText (TextMeshProUGUI)
│   ├── Text: "Sunnyside Island"
│   └── Font Size: 64
├── ButtonContainer (Vertical Layout)
│   ├── NewGameButton (Button)
│   │   └── Text: "새 게임"
│   ├── LoadGameButton (Button)
│   │   └── Text: "불러오기"
│   ├── SettingsButton (Button)
│   │   └── Text: "설정"
│   └── QuitButton (Button)
│       └── Text: "종료"
├── LoadGamePanel (GameObject, 비활성화)
│   └── ... 세이브 슬롯 목록
├── SettingsPanel (SettingsPanel)
└── VersionText (TextMeshProUGUI)
    └── Text: "v0.1.0"
```

**MainMenuPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_newGameButton` | `Button` | O |
| `_loadGameButton` | `Button` | O |
| `_settingsButton` | `Button` | O |
| `_quitButton` | `Button` | O |
| `_loadGamePanel` | `GameObject` | X |
| `_settingsPanel` | `SettingsPanel` | X |
| `_versionText` | `TextMeshProUGUI` | X |

---

### 3.8 Menu - PauseMenuPanel

**파일:** `Assets/Prefabs/UI/Panels/PauseMenuPanel.prefab`

```
PauseMenuPanel
├── CanvasGroup
├── Background (Image, 반투명)
│   └── Color: #000000 (Alpha: 0.5)
├── Panel (Image, 중앙 패널)
│   ├── TitleText (TextMeshProUGUI)
│   │   └── Text: "일시 정지"
│   ├── PlayTimeText (TextMeshProUGUI)
│   │   └── Text: "플레이 시간: 00:00:00"
│   ├── ButtonContainer (Vertical Layout)
│   │   ├── ResumeButton (Button)
│   │   │   └── Text: "계속하기"
│   │   ├── SaveButton (Button)
│   │   │   └── Text: "저장하기"
│   │   ├── LoadButton (Button)
│   │   │   └── Text: "불러오기"
│   │   ├── SettingsButton (Button)
│   │   │   └── Text: "설정"
│   │   └── MainMenuButton (Button)
│   │       └── Text: "메인 메뉴"
│   └── LoadGamePanel (GameObject, 비활성화)
└── SettingsPanel (SettingsPanel)
```

**PauseMenuPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_resumeButton` | `Button` | O |
| `_saveButton` | `Button` | O |
| `_loadButton` | `Button` | O |
| `_settingsButton` | `Button` | O |
| `_mainMenuButton` | `Button` | O |
| `_settingsPanel` | `SettingsPanel` | X |
| `_loadGamePanel` | `GameObject` | X |
| `_playTimeText` | `TextMeshProUGUI` | X |

---

### 3.9 Menu - SettingsPanel

**파일:** `Assets/Prefabs/UI/Panels/SettingsPanel.prefab`

```
SettingsPanel
├── CanvasGroup
├── Background (Image)
├── TitleText (TextMeshProUGUI)
│   └── Text: "설정"
├── AudioSection (Vertical Layout)
│   ├── SectionTitle (TextMeshProUGUI)
│   │   └── Text: "오디오"
│   ├── MasterVolume (Slider + Text)
│   ├── BGMVolume (Slider + Text)
│   └── SFXVolume (Slider + Text)
├── GraphicsSection (Vertical Layout)
│   ├── SectionTitle (TextMeshProUGUI)
│   │   └── Text: "그래픽"
│   ├── QualityDropdown (TMP_Dropdown)
│   ├── FullscreenToggle (Toggle)
│   └── ResolutionDropdown (TMP_Dropdown)
├── GameplaySection (Vertical Layout)
│   ├── SectionTitle (TextMeshProUGUI)
│   │   └── Text: "게임플레이"
│   ├── AutoSaveInterval (Slider + Text)
│   └── ShowTutorialToggle (Toggle)
└── ButtonContainer (Horizontal Layout)
    ├── ApplyButton (Button)
    │   └── Text: "적용"
    ├── ResetButton (Button)
    │   └── Text: "초기화"
    └── CloseButton (Button)
        └── Text: "닫기"
```

**SettingsPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_audioMixer` | `AudioMixer` | O |
| `_masterVolumeSlider` | `Slider` | O |
| `_bgmVolumeSlider` | `Slider` | O |
| `_sfxVolumeSlider` | `Slider` | O |
| `_qualityDropdown` | `TMP_Dropdown` | O |
| `_fullscreenToggle` | `Toggle` | O |
| `_resolutionDropdown` | `TMP_Dropdown` | O |
| `_autoSaveIntervalSlider` | `Slider` | O |
| `_autoSaveIntervalText` | `TextMeshProUGUI` | X |
| `_showTutorialToggle` | `Toggle` | O |
| `_applyButton` | `Button` | O |
| `_resetButton` | `Button` | O |
| `_closeButton` | `Button` | O |

---

### 3.10 Inventory - InventoryPanel

**파일:** `Assets/Prefabs/UI/Panels/InventoryPanel.prefab`

```
InventoryPanel
├── CanvasGroup
├── Background (Image)
├── TitleBar (Horizontal Layout)
│   ├── TitleText (TextMeshProUGUI)
│   │   └── Text: "인벤토리"
│   ├── CapacityText (TextMeshProUGUI)
│   │   └── Text: "0 / 24"
│   └── GoldText (TextMeshProUGUI)
│       └── Text: "0 G"
├── MainContent (Horizontal Layout)
│   ├── InventoryGrid (Grid Layout Group)
│   │   ├── Cell Size: (64, 64)
│   │   ├── Spacing: (5, 5)
│   │   └── Constraint: Fixed Column Count = 8
│   └── ItemDetailPanel (ItemDetailPanel)
├── ButtonContainer (Horizontal Layout)
│   ├── SortButton (Button)
│   │   └── Text: "정렬"
│   └── CloseButton (Button)
│       └── Text: "닫기"
└── SlotUI (프리팹, 런타임 생성용)
```

**InventoryPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_inventoryGrid` | `Transform` | O |
| `_slotPrefab` | `SlotUI` | O |
| `_gridColumns` | `int` | - |
| `_itemDetailPanel` | `ItemDetailPanel` | O |
| `_capacityText` | `TextMeshProUGUI` | X |
| `_goldText` | `TextMeshProUGUI` | X |
| `_closeButton` | `Button` | X |
| `_sortButton` | `Button` | X |

---

### 3.11 Inventory - ItemDetailPanel

**파일:** `Assets/Prefabs/UI/Panels/ItemDetailPanel.prefab`

```
ItemDetailPanel
├── Background (Image)
├── ItemInfo (Vertical Layout)
│   ├── ItemIcon (Image)
│   │   └── Size: (64, 64)
│   ├── ItemNameText (TextMeshProUGUI)
│   │   └── Font Size: 20
│   ├── ItemTypeText (TextMeshProUGUI)
│   │   └── Font Size: 14, Color: #AAAAAA
│   ├── RarityText (TextMeshProUGUI)
│   │   └── Font Size: 12
│   └── DescriptionText (TextMeshProUGUI)
│       └── Font Size: 14, Word Wrap: true
├── StatsPanel (Vertical Layout)
│   ├── ValueText (TextMeshProUGUI)
│   │   └── Text: "100 G"
│   └── StackText (TextMeshProUGUI)
│       └── Text: "최대 99개"
└── ActionButtons (Vertical Layout)
    ├── UseButton (Button)
    │   └── Text: "사용" / "장착"
    └── DropButton (Button)
        └── Text: "버리기"
```

**ItemDetailPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_itemIcon` | `Image` | O |
| `_itemNameText` | `TextMeshProUGUI` | O |
| `_itemTypeText` | `TextMeshProUGUI` | X |
| `_rarityText` | `TextMeshProUGUI` | X |
| `_descriptionText` | `TextMeshProUGUI` | O |
| `_statsPanel` | `GameObject` | X |
| `_valueText` | `TextMeshProUGUI` | X |
| `_stackText` | `TextMeshProUGUI` | X |
| `_useButton` | `Button` | X |
| `_dropButton` | `Button` | X |
| `_useButtonText` | `TextMeshProUGUI` | X |

---

### 3.12 Building - BuildingPanel

**파일:** `Assets/Prefabs/UI/Panels/BuildingPanel.prefab`

```
BuildingPanel
├── CanvasGroup
├── Background (Image)
├── TitleBar
│   └── TitleText (TextMeshProUGUI)
│       └── Text: "건설"
├── CategoryTabs (Horizontal Layout)
│   ├── TabPrefab (Button) - 런타임 생성
│   └── ... 카테고리별 탭
├── MainContent (Horizontal Layout)
│   ├── BuildingGrid (Grid Layout Group)
│   │   ├── Cell Size: (150, 180)
│   │   └── Spacing: (10, 10)
│   └── BuildingDetailPanel (BuildingDetailPanel)
└── BottomBar
    └── BuildButton (Button)
        ├── Text: "건설"
        └── Interactable: false (기본값)
```

**BuildingPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_categoryTabContainer` | `Transform` | O |
| `_categoryTabPrefab` | `Button` | O |
| `_selectedTabColor` | `Color` | - |
| `_unselectedTabColor` | `Color` | - |
| `_buildingGrid` | `Transform` | O |
| `_buildingCardPrefab` | `BuildingCard` | O |
| `_detailPanel` | `BuildingDetailPanel` | O |
| `_buildButton` | `Button` | O |
| `_buildButtonText` | `TextMeshProUGUI` | X |

---

### 3.13 Building - BuildingCard

**파일:** `Assets/Prefabs/UI/Panels/BuildingCard.prefab`

```
BuildingCard (150x180)
├── Button
├── BuildingIcon (Image)
│   └── Size: (128, 128)
├── BuildingNameText (TextMeshProUGUI)
│   ├── Font Size: 14
│   └── Alignment: Center
├── LockedOverlay (GameObject)
│   ├── Image (Color: #000000, Alpha: 0.7)
│   └── LockIcon (Image)
└── SelectionOverlay (Image)
    ├── Color: #00FF00 (Alpha: 0.3)
    └── Enabled: false (기본값)
```

**BuildingCard.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_buildingIcon` | `Image` | O |
| `_buildingNameText` | `TextMeshProUGUI` | O |
| `_lockedOverlay` | `GameObject` | X |
| `_selectionOverlay` | `Image` | X |
| `_button` | `Button` | O |

---

### 3.14 Building - BuildingDetailPanel

**파일:** `Assets/Prefabs/UI/Panels/BuildingDetailPanel.prefab`

```
BuildingDetailPanel
├── Background (Image)
├── BuildingPreview (Image)
│   └── Size: (200, 150)
├── BuildingNameText (TextMeshProUGUI)
│   └── Font Size: 24
├── DescriptionText (TextMeshProUGUI)
│   └── Font Size: 14, Word Wrap: true
├── BuildTimeText (TextMeshProUGUI)
│   └── Text: "건설 시간: 1일"
├── EffectsSection (Vertical Layout)
│   ├── Title (TextMeshProUGUI)
│   │   └── Text: "효과"
│   └── EffectsContainer (Vertical Layout)
│       └── EffectItem (런타임 생성)
├── CostSection (Vertical Layout)
│   ├── Title (TextMeshProUGUI)
│   │   └── Text: "비용"
│   └── CostListContainer (Vertical Layout)
│       └── CostItem (런타임 생성)
└── RequirementsPanel (GameObject)
    └── RequirementsText (TextMeshProUGUI)
        └── Text: "퀘스트 완료 필요: ..."
```

**BuildingDetailPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_buildingPreview` | `Image` | O |
| `_buildingNameText` | `TextMeshProUGUI` | O |
| `_descriptionText` | `TextMeshProUGUI` | O |
| `_buildTimeText` | `TextMeshProUGUI` | X |
| `_effectsContainer` | `Transform` | O |
| `_effectItemPrefab` | `GameObject` | O |
| `_costListContainer` | `Transform` | O |
| `_costItemPrefab` | `GameObject` | O |
| `_requirementsPanel` | `GameObject` | X |
| `_requirementsText` | `TextMeshProUGUI` | X |

---

### 3.15 Shop - ShopPanel

**파일:** `Assets/Prefabs/UI/Panels/ShopPanel.prefab`

```
ShopPanel
├── CanvasGroup
├── Background (Image)
├── TitleBar
│   ├── ShopNameText (TextMeshProUGUI)
│   │   └── Text: "상점 이름"
│   └── PlayerGoldText (TextMeshProUGUI)
│       └── Text: "0 G"
├── TabContainer (Horizontal Layout)
│   ├── BuyTab (Button)
│   │   └── Text: "구매"
│   └── SellTab (Button)
│       └── Text: "판매"
├── ItemGrid (Grid Layout Group)
│   ├── Cell Size: (180, 80)
│   └── Spacing: (10, 10)
├── QuantitySelector (GameObject, 비활성화)
│   ├── Background (Image)
│   ├── QuantityText (TextMeshProUGUI)
│   │   └── Text: "1"
│   ├── QuantityButtons (Horizontal Layout)
│   │   ├── DecreaseButton (Button)
│   │   │   └── Text: "-"
│   │   └── IncreaseButton (Button)
│   │       └── Text: "+"
│   ├── TotalPriceText (TextMeshProUGUI)
│   │   └── Text: "총 0 G"
│   └── ActionButtons (Horizontal Layout)
│       ├── ConfirmButton (Button)
│       │   └── Text: "확인"
│       └── CancelButton (Button)
│           └── Text: "취소"
└── ShopItemUI (프리팹, 런타임 생성용)
```

**ShopPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_shopNameText` | `TextMeshProUGUI` | O |
| `_itemGrid` | `Transform` | O |
| `_shopItemPrefab` | `GameObject` | O |
| `_playerGoldText` | `TextMeshProUGUI` | O |
| `_buyTab` | `Button` | O |
| `_sellTab` | `Button` | O |
| `_selectedTabColor` | `Color` | - |
| `_unselectedTabColor` | `Color` | - |
| `_quantitySelectorPanel` | `GameObject` | O |
| `_quantityText` | `TextMeshProUGUI` | O |
| `_quantityDecreaseButton` | `Button` | O |
| `_quantityIncreaseButton` | `Button` | O |
| `_confirmButton` | `Button` | O |
| `_cancelButton` | `Button` | O |
| `_totalPriceText` | `TextMeshProUGUI` | O |

---

### 3.16 Shop - ShopItemUI

**파일:** `Assets/Prefabs/UI/Panels/ShopItemUI.prefab`

```
ShopItemUI (180x80)
├── Button
├── ItemIcon (Image)
│   └── Size: (48, 48)
├── InfoContainer (Vertical Layout)
│   ├── ItemNameText (TextMeshProUGUI)
│   │   └── Font Size: 14
│   └── PriceText (TextMeshProUGUI)
│       ├── Font Size: 12
│       └── Color: #FFD700 (골드색)
└── StockText (TextMeshProUGUI)
    ├── Anchor: Bottom Right
    ├── Font Size: 10
    └── Text: "x10"
```

**ShopItemUI.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_itemIcon` | `Image` | X |
| `_itemNameText` | `TextMeshProUGUI` | O |
| `_priceText` | `TextMeshProUGUI` | O |
| `_stockText` | `TextMeshProUGUI` | X |
| `_button` | `Button` | O |

---

### 3.17 Dialogue - DialoguePanel

**파일:** `Assets/Prefabs/UI/Panels/DialoguePanel.prefab`

```
DialoguePanel
├── CanvasGroup
├── Background (Image, 반투명)
│   └── Color: #000000 (Alpha: 0.5)
├── DialogueBox (Image, 하단 중앙)
│   ├── SpeakerSection (Horizontal Layout)
│   │   ├── PortraitImage (Image)
│   │   │   └── Size: (100, 100)
│   │   └── SpeakerNameText (TextMeshProUGUI)
│   │       └── Font Size: 18
│   ├── DialogueText (TextMeshProUGUI)
│   │   ├── Font Size: 16
│   │   └── Word Wrap: true
│   └── ContinueIndicator (GameObject)
│       └── Image (화살표 애니메이션)
├── ChoiceContainer (Vertical Layout)
│   └── ChoiceButton (런타임 생성)
└── SkipButton (Button)
    └── Text: "스킵"
```

**DialoguePanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_portraitImage` | `Image` | X |
| `_speakerNameText` | `TextMeshProUGUI` | O |
| `_dialogueText` | `TextMeshProUGUI` | O |
| `_typewriterSpeed` | `float` | - |
| `_continueIndicator` | `GameObject` | X |
| `_choiceContainer` | `Transform` | O |
| `_choiceButtonPrefab` | `GameObject` | O |
| `_skipButton` | `Button` | X |

---

### 3.18 Quest - QuestPanel

**파일:** `Assets/Prefabs/UI/Panels/QuestPanel.prefab`

```
QuestPanel
├── CanvasGroup
├── Background (Image)
├── TitleBar
│   ├── TitleText (TextMeshProUGUI)
│   │   └── Text: "퀘스트"
│   └── QuestCountText (TextMeshProUGUI)
│       └── Text: "(0)"
├── TabContainer (Horizontal Layout)
│   ├── ActiveTabButton (Button)
│   │   └── Text: "진행 중"
│   └── CompletedTabButton (Button)
│       └── Text: "완료"
├── MainContent (Horizontal Layout)
│   ├── QuestListContainer (Vertical Layout)
│   │   └── QuestItem (런타임 생성)
│   └── QuestDetailPanel (QuestDetailPanel)
└── CloseButton (Button)
    └── Text: "닫기"
```

**QuestPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_questListContainer` | `Transform` | O |
| `_questItemPrefab` | `GameObject` | O |
| `_activeTabButton` | `Button` | O |
| `_completedTabButton` | `Button` | O |
| `_selectedTabColor` | `Color` | - |
| `_unselectedTabColor` | `Color` | - |
| `_questCountText` | `TextMeshProUGUI` | X |
| `_questDetailPanel` | `QuestDetailPanel` | O |
| `_closeButton` | `Button` | X |

---

### 3.19 Quest - QuestItem (프리팹)

**파일:** `Assets/Prefabs/UI/Components/QuestItem.prefab`

```
QuestItem (400x60)
├── Button
├── Background (Image)
├── Selected (Image)
│   └── Enabled: false (기본값)
├── QuestName (TextMeshProUGUI)
│   ├── Font Size: 16
│   └── Alignment: Left
├── Chapter (TextMeshProUGUI)
│   ├── Font Size: 12
│   ├── Color: #AAAAAA
│   └── Text: "챕터 1"
└── MainQuestIndicator (GameObject)
    └── Image (별 아이콘 등)
```

---

### 3.20 Quest - QuestDetailPanel

**파일:** `Assets/Prefabs/UI/Panels/QuestDetailPanel.prefab`

```
QuestDetailPanel
├── Background (Image)
├── Header
│   ├── QuestTitleText (TextMeshProUGUI)
│   │   └── Font Size: 24
│   ├── ChapterText (TextMeshProUGUI)
│   │   └── Text: "챕터 1"
│   └── MainQuestIndicator (GameObject)
│       └── Image (별 아이콘)
├── DescriptionText (TextMeshProUGUI)
│   └── Font Size: 14, Word Wrap: true
├── ObjectivesSection (Vertical Layout)
│   ├── Title (TextMeshProUGUI)
│   │   └── Text: "목표"
│   └── ObjectiveContainer (Vertical Layout)
│       └── ObjectiveItem (런타임 생성)
├── RewardsSection (Vertical Layout)
│   ├── Title (TextMeshProUGUI)
│   │   └── Text: "보상"
│   └── RewardContainer (Vertical Layout)
│       └── RewardItem (런타임 생성)
├── ActionButtons (Horizontal Layout)
│   ├── ClaimRewardButton (Button)
│   │   └── Text: "보상 받기"
│   └── AbandonButton (Button)
│       └── Text: "포기"
└── CompletedIndicator (GameObject)
    └── Text: "완료"
```

**QuestDetailPanel.cs SerializeField:**
| 필드 | 타입 | 필수 |
|------|------|------|
| `_questTitleText` | `TextMeshProUGUI` | O |
| `_questDescriptionText` | `TextMeshProUGUI` | O |
| `_chapterText` | `TextMeshProUGUI` | X |
| `_mainQuestIndicator` | `GameObject` | X |
| `_objectiveContainer` | `Transform` | O |
| `_objectiveItemPrefab` | `GameObject` | O |
| `_rewardContainer` | `Transform` | O |
| `_rewardItemPrefab` | `GameObject` | O |
| `_claimRewardButton` | `Button` | X |
| `_abandonButton` | `Button` | X |
| `_completedIndicator` | `GameObject` | X |

---

### 3.21 ObjectiveItem (프리팹)

**파일:** `Assets/Prefabs/UI/Components/ObjectiveItem.prefab`

```
ObjectiveItem (350x30)
├── Checkmark (GameObject)
│   └── Image (체크 아이콘)
├── ObjectiveText (TextMeshProUGUI)
│   ├── Font Size: 14
│   └── Alignment: Left
└── ProgressText (TextMeshProUGUI)
    ├── Font Size: 12
    ├── Color: #AAAAAA
    └── Text: "0 / 5"
```

---

## 4. 주요 프리팹 생성 단계별 가이드

### 4.1 SlotUI 프리팹 생성

1. **GameObject 생성**
   - Hierarchy에서 우클릭 > UI > Image
   - 이름을 "SlotUI"로 변경
   - RectTransform: Size (64, 64)

2. **필요한 컴포넌트 추가**
   - Add Component > SlotUI (스크립트)

3. **자식 요소 생성**
   ```
   SlotUI
   ├── Background (Image)
   ├── IconImage (Image)
   ├── QuantityText (TextMeshPro - Text)
   ├── KeyText (TextMeshPro - Text)
   ├── SelectedOverlay (Image)
   └── LockOverlay (Image)
       └── LockIcon (Image)
   ```

4. **각 요소 설정**
   - Background: Color #4A4A4A
   - IconImage: Preserve Aspect = true, Raycast Target = false
   - QuantityText: Anchor = Bottom Right, Font Size = 12
   - KeyText: Anchor = Top Left, Font Size = 10
   - SelectedOverlay: Color #00FF00 (Alpha 0.5), Enabled = false
   - LockOverlay: Color #000000 (Alpha 0.7), Enabled = false

5. **SerializeField 할당**
   - SlotUI 컴포넌트의 각 필드에 드래그

6. **프리팹 생성**
   - Project 창에서 Assets/Prefabs/UI/Components 폴더 생성
   - SlotUI를 폴더로 드래그하여 프리팹 생성
   - Hierarchy에서 삭제

---

### 4.2 NotificationItem 프리팹 생성

1. **GameObject 생성**
   - Create Empty > 이름: "NotificationItem"
   - Add Component > RectTransform
   - Add Component > Canvas Group
   - Add Component > Image (Background)

2. **RectTransform 설정**
   - Size: (400, 30)
   - Anchor: Top Left
   - Pivot: (0, 1)

3. **Background 설정**
   - Color: #000000 (Alpha: 0.7)

4. **Text 자식 생성**
   - 우클릭 > UI > Text - TextMeshPro
   - 이름: "Text"
   - Anchor: Stretch
   - Offset Min: (10, 5)
   - Offset Max: (-10, -5)
   - Font Size: 14
   - Alignment: Left

5. **Canvas Group 설정**
   - Alpha: 0 (애니메이션 시작용)

6. **프리팹 생성**
   - Assets/Prefabs/UI/Components 폴더로 드래그

---

### 4.3 QuestItem 프리팹 생성

1. **GameObject 생성**
   - Create Empty > 이름: "QuestItem"
   - Add Component > RectTransform
   - Add Component > Image (Background)
   - Add Component > Button

2. **RectTransform 설정**
   - Size: (400, 60)

3. **자식 요소 생성**
   ```
   QuestItem
   ├── Selected (Image)
   ├── QuestName (TextMeshPro)
   ├── Chapter (TextMeshPro)
   └── MainQuestIndicator (GameObject)
       └── Image
   ```

4. **각 요소 설정**
   - Selected: Enabled = false, Color = #FFFFFF (Alpha 0.3)
   - QuestName: Font Size = 16, Anchor = Left
   - Chapter: Font Size = 12, Color = #AAAAAA
   - MainQuestIndicator: 위치 우측, 별 아이콘

5. **프리팹 생성**

---

### 4.4 ObjectiveItem 프리팹 생성

1. **GameObject 생성**
   - Create Empty > 이름: "ObjectiveItem"
   - Add Component > RectTransform
   - Size: (350, 30)

2. **자식 요소 생성**
   ```
   ObjectiveItem
   ├── Checkmark (GameObject)
   │   └── Image (체크 아이콘)
   ├── ObjectiveText (TextMeshPro)
   └── ProgressText (TextMeshPro)
   ```

3. **각 요소 설정**
   - Checkmark: 위치 좌측, Size (20, 20)
   - ObjectiveText: Font Size = 14, Alignment = Left
   - ProgressText: Font Size = 12, Color = #AAAAAA, Anchor = Right

4. **프리팹 생성**

---

## 5. 네이밍 컨벤션

### 5.1 GameObject 이름

| 타입 | 규칙 | 예시 |
|------|------|------|
| 패널 | PascalCase + Panel | HUDPanel, InventoryPanel |
| 버튼 | 기능 + Button | NewGameButton, CloseButton |
| 텍스트 | 용도 + Text | TitleText, GoldText |
| 이미지 | 용도 + Image/Icon | BackgroundImage, ItemIcon |
| 컨테이너 | 용도 + Container/Grid | ButtonContainer, ItemGrid |
| 오버레이 | 상태 + Overlay | SelectedOverlay, LockOverlay |

### 5.2 SerializeField 변수명

| 규칙 | 예시 |
|------|------|
| private 필드 | `_camelCase` |
| public 프로퍼티 | `PascalCase` |
| 컬렉션 | 복수형 `_slots`, `_items` |
| 프리팹 | `+ Prefab` `_slotPrefab` |

### 5.3 프리팹 파일명

- 스크립트 이름과 일치: `InventoryPanel.prefab`
- 컴포넌트 프리팹은 스크립트 이름 사용: `SlotUI.prefab`

---

## 6. 체크리스트

### 새 패널 생성 시 확인사항

- [ ] CanvasGroup 컴포넌트 추가
- [ ] Canvas Scaler 설정 (1920x1080)
- [ ] SerializeField 모두 할당
- [ ] 자식 요소 이름 규칙 준수
- [ ] 프리팹으로 저장
- [ ] UIManager의 _panels 리스트에 추가
- [ ] EventSystem 존재 확인

### 컴포넌트 프리팹 생성 시 확인사항

- [ ] 필요한 스크립트 컴포넌트 추가
- [ ] 모든 SerializeField 할당
- [ ] 기본값 설정 (Enabled = false 등)
- [ ] 적절한 크기 설정
- [ ] 프리팹으로 저장

---

## 7. 참고 자료

- [AGENTS.md](../AGENTS.md) - 프로젝트 코딩 컨벤션
- [05-ui-ux.md](./05-ui-ux.md) - UI/UX 구현 계획
- Unity UI 최적화 가이드: https://docs.unity3d.com/Manual/UISystem.html