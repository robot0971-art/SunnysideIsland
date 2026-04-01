# 오늘 작업 내용 및 남은 작업

## 날짜
2026-03-31

## 완료된 작업

### 1. RainEffect_Manual 위치 고정 문제 수정
- **파일**: `Assets/Scripts/Weather/RainEffect.cs`
- **변경사항**:
  - Y 오프셋: 12 → 2 (카메라보다 2 위에 위치)
  - `Application.isPlaying` 체크 추가로 에디터 모드에서 위치 자유롭게 변경 가능

### 2. 비가 올 때 FarmPlot 자동 물 주기
- **파일**: `Assets/Scripts/Farming/FarmingManager.cs`
- **변경사항**:
  - `WeatherChangedEvent` 구독 추가
  - 비(Rainy/Stormy)일 때 모든 FarmPlot에 `Water()` 자동 호출
  - using 추가: `SunnysideIsland.Weather`, `SunnysideIsland.GameData`

### 3. 작물 퀵슬롯 시스템 구현

#### 새 파일 생성
- `Assets/Scripts/Farming/CropSelectionSystem.cs`
  - `ICropSelectionSystem` 인터페이스 포함
  - 번호키 1-5 입력 처리
  - 5개 작물 슬롯 관리

- `Assets/Scripts/UI/Farming/CropQuickSlotUI.cs`
  - UI 슬롯 표시 및 선택 강조
  - DI로 `ICropSelectionSystem` 주입

#### 수정된 파일
- `Assets/Scripts/Player/PlayerController.cs`
  - `_potatoData` → `[Inject] ICropSelectionSystem` 변경
  - 선택된 작물로 심기 로직 수정

- `Assets/Scripts/DI/GameSceneInstaller.cs`
  - `_cropSelectionSystem` 필드 추가
  - DI 컨테이너에 자동 등록 (Inspector 연결 없이도 `FindObjectOfType`으로 찾음)
  - `ICropSelectionSystem` 인터페이스 등록

### 4. CropData 현황 확인 (Unity CLI)
```
[0] potato: 감자 (5일) ✅
[1] Carrot: 당근 (4일) ✅
[2] (없음) ❌
[3] (없음) ❌
[4] (없음) ❌
```

---

## 남은 작업 (TODO)

### 우선순위: 높음

#### 1. CropData ScriptableObject 생성 (Unity Editor)
**위치**: `Assets/CropData/`

| 작물 | 파일명 | 필요한 데이터 |
|------|--------|--------------|
| 호박 | PumpkinData.asset | cropId, cropName, growthDays, growthSprites(6단계) 등 |
| 양배추 | CabbageData.asset | 위와 동일 |
| 밀 | WheatData.asset | 위와 동일 |

**생성 방법**:
1. Project 창 → `Assets/CropData/` 우클릭
2. Create → Farming → Crop Data
3. Inspector에서 데이터 입력
4. growthSprites에 6단계 성장 이미지 할당

#### 2. UI Canvas 및 슬롯 생성
**위치**: `Assets/Prefabs/UI/CropQuickSlotPanel.prefab`

**구조**:
```
Canvas (Screen Space - Overlay)
└── CropQuickSlotPanel (Bottom Center anchor)
    ├── Slot0
    │   ├── Background (Image)
    │   ├── CropIcon (Image)
    │   ├── Number (Text) - "1"
    │   └── Name (Text) - "감자"
    ├── Slot1 (same structure)
    ├── Slot2
    ├── Slot3
    └── Slot4
```

**설정값**:
- Panel Size: 500 x 80
- Slot Size: 80 x 80
- Spacing: 10

#### 3. CropQuickSlotUI Inspector 연결
**GameObject**: Canvas/CropQuickSlotPanel

**할당 필요 항목**:
- Slot Backgrounds: [5개 Image 컴포넌트]
- Crop Icons: [5개 Image 컴포넌트]
- Slot Numbers: [5개 Text 컴포넌트]
- Crop Names: [5개 Text 컴포넌트]

#### 4. GameSceneInspector 수동 연결 (옵션)
자동 찾기가 작동하지 않을 경우:
- Hierarchy에서 GameSceneInstaller 선택
- Inspector → Production Systems
- Crop Selection System 필드에 CropSelectionSystem GameObject 드래그

---

### 우선순위: 중간

#### 5. 테스트 및 디버깅
- [ ] Play Mode에서 번호키 1-5로 작물 선택 확인
- [ ] E키로 FarmPlot에 선택된 작물 심기 확인
- [ ] UI에서 선택된 슬롯 강조 표시 확인
- [ ] 비가 올 때 FarmPlot 자동 물 주기 확인

#### 6. Save/Load 기능 (선택사항)
- CropSelectionSystem의 선택 상태 저장
- PlayerController의 선택 작물 저장

---

## 참고사항

### 사용된 키 입력
| 키 | 기능 |
|----|------|
| 1-5 | 작물 슬롯 선택 |
| E | FarmPlot에 선택된 작물 심기 |

### DI 연결 구조
```
GameSceneInstaller (Awake)
├── FindObjectOfType<CropSelectionSystem>() (자동)
├── Container.RegisterInstance<ICropSelectionSystem>()
│
├── PlayerController (Start)
│   └── [Inject] ICropSelectionSystem (자동 주입)
│
└── CropQuickSlotUI (Awake)
    └── [Inject] ICropSelectionSystem (자동 주입)
```

### 파일 변경 요약
```
수정됨:
- Assets/Scripts/Weather/RainEffect.cs
- Assets/Scripts/Farming/FarmingManager.cs
- Assets/Scripts/Player/PlayerController.cs
- Assets/Scripts/DI/GameSceneInstaller.cs

생성됨:
- Assets/Scripts/Farming/CropSelectionSystem.cs
- Assets/Scripts/UI/Farming/CropQuickSlotUI.cs
- Assets/Scripts/UI/Farming/ (새 폴더)
```
