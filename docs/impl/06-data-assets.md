# 6. 데이터 및 에셋 계획

## 6.1 데이터 구조

### ScriptableObject 구조
```
Resources/Data/
├── Items/
│   ├── Tools/
│   ├── Consumables/
│   ├── Materials/
│   └── Equipment/
├── Buildings/
│   ├── Housing/
│   ├── Agriculture/
│   ├── Commerce/
│   └── Tourism/
├── Recipes/
│   ├── Crafting/
│   └── Cooking/
├── Quests/
│   ├── MainQuests/
│   └── SideQuests/
├── NPCs/
│   ├── Residents/
│   └── Tourists/
├── Enemies/
│   ├── Goblins/
│   ├── Skeletons/
│   └── Bosses/
└── Events/
    ├── RandomEvents/
    └── Festivals/
```

### ItemData 기본 구조
```csharp
[CreateAssetMenu(fileName = "ItemData", menuName = "Sunnyside/Item Data")]
public class ItemData : ScriptableObject
{
    public string ItemId;
    public string ItemName;
    public ItemType Type;
    public ItemRarity Rarity;
    public int MaxStack;
    public int BuyPrice;
    public int SellPrice;
    public Sprite Icon;
    public GameObject WorldPrefab;
    
    [TextArea(3, 5)]
    public string Description;
}

public enum ItemType
{
    Tool,
    Consumable,
    Material,
    Equipment,
    Quest
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
```

---

## 6.2 데이터 테이블

### 아이템 목록 (초기)

#### 도구 (Tools)
| ID | 이름 | 내구도 | 효과 |
|----|------|--------|------|
| tool_wood_axe | 나무 도끼 | 100 | 벌목 속도 기본 |
| tool_stone_pickaxe | 돌 곡괭이 | 150 | 채광 속도 기본 |
| tool_iron_fishingrod | 철 낚싯대 | 200 | 낚시 성공률 +20% |
| tool_iron_sword | 철 검 | 200 | 공격력 25 |
| tool_steel_sword | 강철 검 | 300 | 공격력 40 |

#### 소비품 (Consumables)
| ID | 이름 | 효과 | 판매가 |
|----|------|------|--------|
| food_carrot | 당근 | 허기 +15 | 5 |
| food_bread | 빵 | 허기 +40 | 15 |
| food_cooked_fish | 구운 생선 | 허기 +20 | 20 |
| food_stew | 스튜 | 허기 +60, 체력 +10 | 50 |
| potion_health | 체력 물약 | 체력 +50 | 100 |

#### 재료 (Materials)
| ID | 이름 | 희귀도 | 판매가 |
|----|------|--------|--------|
| mat_wood | 나무 | Common | 2 |
| mat_stone | 돌 | Common | 3 |
| mat_iron_ore | 철광석 | Uncommon | 15 |
| mat_mithril | 미스릴 | Rare | 100 |

---

## 6.3 아이템 에셋 목록

### 기본 아이템 아이콘 (필요 개수: 50+)
- [ ] 나무, 돌, 철광석, 미스릴
- [ ] 씨앗 (11종 작물)
- [ ] 농작물 (11종)
- [ ] 생선 (8종)
- [ ] 고기 (5종)
- [ ] 도구 (10종)
- [ ] 무기 (10종)
- [ ] 소비품 (20종)

### 아이템 스프라이트 크기
- 인벤토리 아이콘: 32x32
- 월드 드랍: 16x16
- 장비 착용: 32x32

---

## 6.4 건설물 에셋 목록

### 주거 시설 (5종)
- [ ] 텐트
- [ ] 오두막
- [ ] 집
- [ ] 큰 집
- [ ] 저택

### 농업 시설 (6종)
- [ ] 밭
- [ ] 큰 밭
- [ ] 비닐하우스
- [ ] 축사
- [ ] 저장고
- [ ] 풍차

### 상업 시설 (6종)
- [ ] 노점
- [ ] 식료품점
- [ ] 대장간
- [ ] 식당
- [ ] 여관
- [ ] 시장

### 관광 시설 (7종)
- [ ] 부두
- [ ] 등대
- [ ] 광장
- [ ] 공원
- [ ] 축제장
- [ ] 온천
- [ ] 리조트 호텔

### 건설물 스프라이트 크기
- 텐트: 32x32 (2x2 타일)
- 집: 64x64 (4x4 타일)
- 리조트: 128x96 (8x6 타일)

---

## 6.5 캐릭터 에셋 목록

### 플레이어 애니메이션
- [ ] Idle (4방향)
- [ ] Walk (4방향)
- [ ] Roll (4방향)
- [ ] Attack (4방향)
- [ ] Hurt
- [ ] Die

### NPC 애니메이션
- [ ] Idle (4방향)
- [ ] Walk (4방향)
- [ ] 작업 애니메이션

### 적(몬스터) 애니메이션
**고블린**
- [ ] Idle
- [ ] Walk
- [ ] Attack
- [ ] Hurt
- [ ] Die

**스켈레톤**
- [ ] Idle
- [ ] Walk
- [ ] Attack
- [ ] Hurt
- [ ] Die

**보스**
- [ ] 고블린 족장 (페이즈별 애니메이션)
- [ ] 고대 골렘

### 동물 애니메이션
- [ ] 토끼 (Idle, Run)
- [ ] 사슴 (Idle, Run)
- [ ] 여우 (Idle, Run)
- [ ] 멧돼지 (Idle, Walk, Attack)
- [ ] 늑대 (Idle, Walk, Attack)
- [ ] 곰 (Idle, Walk, Attack)

---

## 6.6 환경 에셋 목록

### 타일셋
- [ ] 흙/잔디 타일
- [ ] 물 타일
- [ ] 모래 타일
- [ ] 돌 타일
- [ ] 농장 흙
- [ ] 바닥재 (나무, 돌)

### 자원 오브젝트
- [ ] 나무 (3종 크기)
- [ ] 돌 (3종 크기)
- [ ] 광석 (철, 미스릴)
- [ ] 약초 (5종)
- [ ] 버섯 (3종)

### 환경 오브젝트
- [ ] 풀
- [ ] 꽃 (5종)
- [ ] 바위
- [ ] 나무 그루터기
- [ ] 동굴 입구

---

## 6.7 UI 에셋 목록

### 기본 UI
- [ ] 버튼 (Normal, Hover, Pressed, Disabled)
- [ ] 패널 배경
- [ ] 슬롯 배경
- [ ] 체크박스
- [ ] 슬라이더 (체력, 허기, 스태미나)
- [ ] 스크롤바
- [ ] 드롭다운

### 아이콘
- [ ] 골드 아이콘
- [ ] 시간 아이콘
- [ ] 날씨 아이콘 (5종)
- [ ] 상태 아이콘 (허기, 체력, 스태미나)

### 폰트
- [ ] Noto Sans KR (한글)
- [ ] Roboto Mono (숫자)

---

## 6.8 사운드 에셋 목록

### BGM
- [ ] 메인 메뉴
- [ ] 필드 (낮)
- [ ] 필드 (밤)
- [ ] 전투
- [ ] 보스 전투
- [ ] 상점
- [ ] 엔딩

### 효과음 (SFX)
**플레이어**
- [ ] 발걸음 (땅, 잔디, 돌)
- [ ] 공격 (검, 도끼, 활)
- [ ] 피격
- [ ] 구르기
- [ ] 아이템 획득
- [ ] 레벨업

**환경**
- [ ] 나무 벌목
- [ ] 돌 채광
- [ ] 물소리
- [ ] 바람
- [ ] 새 소리

**UI**
- [ ] 버튼 클릭
- [ ] 인벤토리 열기/닫기
- [ ] 아이템 장착
- [ ] 건설 완료
- [ ] 퀘스트 완료

**전투**
- [ ] 적 피격
- [ ] 적 사망
- [ ] 보스 공격

---

## 6.9 에셋 제작 우선순위

### P0 (필수)
1. 플레이어 스프라이트 및 애니메이션
2. 기본 타일셋 (흙, 잔디, 물)
3. 기본 자원 (나무, 돌)
4. 기본 UI 요소
5. 기본 아이템 아이콘 (20개)
6. 건설물 스프라이트 (핵심 10종)
7. 고블린 스프라이트
8. BGM 2~3곡
9. 필수 SFX 20개

### P1 (권장)
1. 추가 타일셋
2. 추가 자원
3. NPC 스프라이트
4. 추가 건설물
5. 추가 아이템 아이콘
6. 추가 몬스터
7. 추가 BGM
8. 추가 SFX

### P2 (선택)
1. 특수 효과
2. 추가 애니메이션
3. 펫/탈것
4. 특수 UI 스킨

---

## 6.10 외부 에셋 활용

### 사용 예정 에셋
**Sunnyside World** (danieldiggle)
- URL: https://danieldiggle.itch.io/sunnyside
- 라이선스: 상업적 사용 가능
- 포함 내용:
  - 캐릭터 스프라이트
  - 타일셋
  - 농작물 스프라이트
  - 몬스터 (고블린, 스켈레톤)
  - UI 요소

### 추가 제작 필요 에셋
- 야생 동물 (토끼, 사슴, 멧돼지, 늑대, 곰)
- 특수 건설물 (리조트 호텔, 온천 등)
- 추가 무기/장비
- 효과음 및 BGM
