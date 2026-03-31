# Unity 에디터 작업 가이드

> **작성일**: 2026-03-16
> 
> **목적**: 코드 구현 완료 후 Unity 에디터에서 수행해야 할 작업 가이드

---

## 1. 필수 프리팹 생성

### 1.1 UI 프리팹
상세 내용은 `docs/impl/UI_PREFAB_GUIDE.md` 참조

| 프리팹 | 경로 | 필수 컴포넌트 |
|--------|------|---------------|
| HUDPanel | Prefabs/UI/HUD/ | CanvasGroup, HUDPanel |
| MainMenuPanel | Prefabs/UI/Menu/ | CanvasGroup, MainMenuPanel |
| PauseMenuPanel | Prefabs/UI/Menu/ | CanvasGroup, PauseMenuPanel |
| SettingsPanel | Prefabs/UI/Menu/ | CanvasGroup, SettingsPanel |
| InventoryPanel | Prefabs/UI/Inventory/ | CanvasGroup, InventoryPanel |
| ItemDetailPanel | Prefabs/UI/Inventory/ | CanvasGroup, ItemDetailPanel |
| BuildingPanel | Prefabs/UI/Building/ | CanvasGroup, BuildingPanel |
| BuildingCard | Prefabs/UI/Building/ | Button, BuildingCard |
| BuildingDetailPanel | Prefabs/UI/Building/ | CanvasGroup, BuildingDetailPanel |
| ShopPanel | Prefabs/UI/Shop/ | CanvasGroup, ShopPanel |
| ShopItemUI | Prefabs/UI/Shop/ | Button, ShopItemUI |
| DialoguePanel | Prefabs/UI/Dialogue/ | CanvasGroup, DialoguePanel |
| QuestPanel | Prefabs/UI/Quest/ | CanvasGroup, QuestPanel |
| QuestDetailPanel | Prefabs/UI/Quest/ | CanvasGroup, QuestDetailPanel |
| TutorialUI | Prefabs/UI/Tutorial/ | CanvasGroup, TutorialUI |
| AchievementUI | Prefabs/UI/Achievement/ | CanvasGroup, AchievementUI |
| SlotUI | Prefabs/UI/Components/ | Button, SlotUI |
| StatBar | Prefabs/UI/Components/ | Image (Fill), StatBar |
| NotificationItem | Prefabs/UI/Components/ | Text, Image |

### 1.2 게임 오브젝트 프리팹

| 프리팛 | 경로 | 필수 컴포넌트 |
|--------|------|---------------|
| Player | Prefabs/Player/ | PlayerController, HealthSystem, HungerSystem, StaminaSystem |
| Tourist | Prefabs/NPC/ | TouristWander, NavMeshAgent |
| DroppedItem | Prefabs/Items/ | DroppedItem, SpriteRenderer, Collider2D |
| PooledEffect | Prefabs/Effects/ | PooledEffect, ParticleSystem |
| Goblin | Prefabs/Enemies/ | EnemyAI, HealthSystem, Collider2D |
| GoblinChief | Prefabs/Enemies/ | GoblinChiefAI, HealthSystem, Collider2D |
| FarmPlot | Prefabs/Building/ | FarmPlot, SpriteRenderer |
| GatherableTree | Prefabs/Resources/ | TreeResource, SpriteRenderer, Collider2D |
| GatherableRock | Prefabs/Resources/ | RockResource, SpriteRenderer, Collider2D |
| GatherableHerb | Prefabs/Resources/ | HerbResource, SpriteRenderer, Collider2D |

---

## 2. ScriptableObject 생성

### 2.1 데이터베이스
| 이름 | 경로 | CreateAssetMenu 경로 |
|------|------|---------------------|
| GameData | Data/GameData.asset | SunnysideIsland/Data/GameData |
| QuestDatabase | Data/QuestDatabase.asset | SunnysideIsland/Quest/Database |
| BuildingDatabase | Data/BuildingDatabase.asset | SunnysideIsland/Building/Database |
| SoundDatabase | Data/SoundDatabase.asset | SunnysideIsland/Audio/Database |
| BalanceData | Data/BalanceData.asset | SunnysideIsland/Data/BalanceData |

### 2.2 튜토리얼 데이터
| 이름 | 경로 |
|------|------|
| Tutorial_BasicControls | Data/Tutorials/Tutorial_BasicControls.asset |
| Tutorial_Survival | Data/Tutorials/Tutorial_Survival.asset |
| Tutorial_Farming | Data/Tutorials/Tutorial_Farming.asset |
| Tutorial_Building | Data/Tutorials/Tutorial_Building.asset |

### 2.3 업적 데이터
| 이름 | 경로 |
|------|------|
| Achievement_FirstFish | Data/Achievements/Achievement_FirstFish.asset |
| Achievement_FirstBuild | Data/Achievements/Achievement_FirstBuild.asset |
| Achievement_Survivor | Data/Achievements/Achievement_Survivor.asset |
| Achievement_TourismKing | Data/Achievements/Achievement_TourismKing.asset |

### 2.4 사운드 데이터
| 이름 | 경로 |
|------|------|
| BGM_MainMenu | Data/Sounds/BGM_MainMenu.asset |
| BGM_Gameplay | Data/Sounds/BGM_Gameplay.asset |
| SFX_ButtonClick | Data/Sounds/SFX_ButtonClick.asset |
| SFX_ItemPickup | Data/Sounds/SFX_ItemPickup.asset |
| SFX_Build | Data/Sounds/SFX_Build.asset |
| SFX_Hit | Data/Sounds/SFX_Hit.asset |

---

## 3. 씬 설정

### 3.1 필수 씬
| 씬 이름 | 경로 | 용도 |
|---------|------|------|
| MainMenu | Scenes/MainMenu.unity | 메인 메뉴 |
| GameScene | Scenes/GameScene.unity | 메인 게임플레이 |
| Loading | Scenes/Loading.unity | 로딩 화면 (선택) |

### 3.2 MainMenu 씬 구성
```
MainMenu
├── Main Camera
├── EventSystem
├── Canvas
│   └── MainMenuPanel (Prefab)
├── AudioManager
└── GameManager
```

### 3.3 GameScene 씬 구성
```
GameScene
├── Main Camera (Cinemachine 가상 카메라 권장)
├── EventSystem
├── Canvas
│   ├── UIManager
│   ├── HUDPanel (Prefab)
│   ├── InventoryPanel (Prefab)
│   ├── BuildingPanel (Prefab)
│   ├── ShopPanel (Prefab)
│   ├── DialoguePanel (Prefab)
│   ├── QuestPanel (Prefab)
│   ├── PauseMenuPanel (Prefab)
│   ├── TutorialUI (Prefab)
│   └── AchievementUI (Prefab)
├── Managers
│   ├── GameManager
│   ├── TimeManager
│   ├── SaveSystem
│   ├── PoolManager
│   ├── AudioManager
│   └── UpdateManager
├── Player (Prefab)
├── World
│   ├── Ground (Tilemap)
│   ├── Resources (Tree, Rock, Herb 등)
│   └── Buildings
└── Lighting
    └── Directional Light
```

---

## 4. DI Container 설정

### 4.1 GameManager 인스펙터 설정
GameManager 컴포넌트에 다음 서비스들 등록:

```
Installers:
- SceneInstaller (씬 범위 서비스)
```

### 4.2 SceneInstaller 구성 예시
```
Bindings:
- ITimeManager -> TimeManager
- ISaveSystem -> SaveSystem
- IInventorySystem -> InventorySystem
- ICurrencySystem -> CurrencySystem
- IShopSystem -> ShopSystem
- IQuestSystem -> QuestSystem
- ITourismSystem -> TourismSystem
- IPoolManager -> PoolManager
- IAudioManager -> AudioManager
```

---

## 5. PoolManager 설정

### 5.1 프리페인 풀 설정
PoolManager 인스펙터에서:

```
Predefined Pools:
- PoolName: "DroppedItem", Prefab: DroppedItem, InitialSize: 20, MaxSize: 100
- PoolName: "HitEffect", Prefab: PooledEffect, InitialSize: 10, MaxSize: 50
- PoolName: "PickupEffect", Prefab: PooledEffect, InitialSize: 10, MaxSize: 50
```

---

## 6. 입력 시스템 설정

### 6.1 Input Actions 에셋 생성
경로: `Controls/GameControls.inputactions`

```
Action Maps:
- Gameplay
  - Move (2D Vector, Keyboard WASD, Gamepad Left Stick)
  - Sprint (Button, Left Shift, Gamepad Left Shoulder)
  - Roll (Button, Space, Gamepad A)
  - Interact (Button, E, Gamepad X)
  - Inventory (Button, I, Gamepad Start)
  - Pause (Button, Escape, Gamepad Menu)
  
- UI
  - Navigate (2D Vector, Arrow Keys, Gamepad D-Pad)
  - Submit (Button, Enter, Gamepad A)
  - Cancel (Button, Escape, Gamepad B)
```

---

## 7. Tag 및 Layer 설정

### 7.1 Tags
```
Player
Enemy
NPC
Resource
Building
Interactable
Item
```

### 7.2 Layers
```
Layer 8: Player
Layer 9: Enemy
Layer 10: NPC
Layer 11: Resource
Layer 12: Building
Layer 13: Item
Layer 14: UI
Layer 15: PostProcessing
```

---

## 8. 프로젝트 설정

### 8.1 Physics 2D
```
Layer Collision Matrix:
- Player ↔ Enemy: 체크
- Player ↔ Resource: 체크
- Player ↔ Building: 체크
- Enemy ↔ Building: 체크
```

### 8.2 Audio
```
Virtual Voice Count: 64
Real Voice Count: 32
```

### 8.3 Quality
```
VSync Count: Every V Blank
Anti Aliasing: 4x Multi Sampling
```

---

## 9. 빌드 설정

### 9.1 Scenes in Build
```
0: Scenes/MainMenu
1: Scenes/GameScene
```

### 9.2 빌드 타겟
```
Platform: Windows
Architecture: x86_64
```

---

## 10. 체크리스트

### 10.1 프리팹 생성
- [ ] 모든 UI 프리팹 생성
- [ ] 모든 게임 오브젝트 프리팹 생성
- [ ] SerializeField 참조 연결

### 10.2 데이터 설정
- [ ] GameData ScriptableObject 생성
- [ ] QuestDatabase 생성 및 퀘스트 데이터 입력
- [ ] BuildingDatabase 생성 및 건물 데이터 입력
- [ ] SoundDatabase 생성
- [ ] Tutorial 데이터 생성
- [ ] Achievement 데이터 생성

### 10.3 씬 구성
- [ ] MainMenu 씬 구성
- [ ] GameScene 씬 구성
- [ ] DI Container 연결

### 10.4 시스템 연동
- [ ] PoolManager 풀 설정
- [ ] Input System 설정
- [ ] Tag/Layer 설정

### 10.5 테스트
- [ ] 메인 메뉴 → 게임 시작
- [ ] 플레이어 이동 테스트
- [ ] 저장/불러오기 테스트
- [ ] 각 시스템 기능 테스트

---

**작성자**: AI  
**업데이트**: 2026-03-16