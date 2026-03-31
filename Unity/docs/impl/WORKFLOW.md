# 개발 워크플로우 가이드

## 개요

이 문서는 Sunnyside Island 프로젝트의 개발 프로세스와 규칙을 정의합니다. 모든 개발 작업은 이 워크플로우를 따릅니다.

---

## Phase 기반 개발 프로세스

### Phase 구조

```
┌─────────────────────────────────────────────────────────────┐
│                        Phase Cycle                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌──────────┐    ┌──────────┐    ┌──────────┐            │
│   │ Research │───▶│  Plan    │───▶│  Build   │            │
│   │  (리서치) │    │  (계획)  │    │  (구현)  │            │
│   └──────────┘    └──────────┘    └────┬─────┘            │
│         ▲                              │                   │
│         │                              ▼                   │
│         │                         ┌──────────┐            │
│         │                         │  Verify  │            │
│         │                         │ (검증)   │            │
│         │                         └────┬─────┘            │
│         │                              │                   │
│         └──────────────────────────────┘                   │
│                   (피드백 루프)                              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Phase 상세

#### 1. Research (리서치)
**목적**: 구현 전 기존 코드와 기획 문서 분석

**산출물**: `RESEARCH_{Phase}_{Feature}.md`

**수행 작업**:
- [ ] 기존 코드 분석 (동일/유사 시스템 검색)
- [ ] 기획 문서 검토 (GDD, 구현계획)
- [ ] 의존성 분석 (필요한 다른 시스템 확인)
- [ ] 리스크 식별 (기술적, 일정적)
- [ ] 참고 자료 수집

**템플릿**: [RESEARCH_TEMPLATE.md](./RESEARCH_TEMPLATE.md)

---

#### 2. Plan (계획)
**목적**: 리서치 결과를 바탕으로 세부 구현 계획 수립

**산출물**: `PLAN_{Phase}_{Feature}.md`

**수행 작업**:
- [ ] 클래스 설계 (인터페이스, 의존성 정의)
- [ ] 파일 구조 계획
- [ ] 테스트 계획
- [ ] 예상 시간 산정
- [ ] 완료 조건 정의

---

#### 3. Build (구현)
**목적**: 계획에 따라 코드 작성

**수행 작업**:
- [ ] 인터페이스 정의
- [ ] 핵심 로직 구현
- [ ] 단위 테스트 작성
- [ ] 문서화 (주석, XML docs)

**코딩 규칙**:
- AGENTS.md 준수
- DI 패턴 사용 (FindObjectOfType 금지)
- EventBus로 이벤트 발행
- Interface 기반 설계

---

#### 4. Verify (검증)
**목적**: 구현 완료 및 품질 확인

**수행 작업**:
- [ ] 기능 테스트
- [ ] 코드 리뷰 (Self)
- [ ] 성능 체크
- [ ] 문서 업데이트
- [ ] 체크리스트 완료 표시

---

## 문서 구조

```
docs/
├── impl/                           # 구현 계획 문서
│   ├── README.md                   # 개요 및 인덱스
│   ├── WORKFLOW.md                 # 이 문서 (개발 프로세스)
│   ├── CHECKLIST.md                # 마일스톤별 체크리스트
│   ├── RESEARCH_TEMPLATE.md        # 리서치 문서 템플릿
│   │
│   ├── phases/                     # Phase별 계획 문서
│   │   ├── M0/                     # M0: 프로젝트 설정
│   │   │   ├── RESEARCH_M0.md
│   │   │   └── PLAN_M0.md
│   │   ├── M1/                     # M1: 코어 시스템
│   │   │   ├── RESEARCH_M1_DI.md
│   │   │   ├── PLAN_M1_DI.md
│   │   │   ├── RESEARCH_M1_EventBus.md
│   │   │   ├── PLAN_M1_EventBus.md
│   │   │   └── ...
│   │   ├── M2/                     # M2: 생존/생산 시스템
│   │   ├── M3/                     # M3: 전투/건설 시스템
│   │   ├── M4/                     # M4: 경제/콘텐츠 시스템
│   │   ├── M5/                     # M5: 콘텐츠 완성
│   │   ├── M6/                     # M6: 폴리싱
│   │   └── M7/                     # M7: 릴리즈
│   │
│   └── research/                   # 완료된 리서치 문서 아카이브
│       ├── M0/
│       ├── M1/
│       └── ...
│
├── game-design/                    # 게임 기획 문서
│   └── GDD.md
│
└── technical/                      # 기술 문서
    ├── architecture.md
    └── api-reference.md
```

---

## 네이밍 규칙

### 문서 파일명
```
{TYPE}_{Phase}_{Feature}.md

예시:
- RESEARCH_M1_DI.md
- PLAN_M2_Inventory.md
- RESEARCH_M3_Combat.md
```

### 브랜치명 (Git)
```
feature/{Phase}-{feature}

예시:
- feature/M1-di-container
- feature/M2-inventory-system
```

### 커밋 메시지
```
[{Phase}] {type}: {description}

예시:
- [M1] feat: DI Container 구현
- [M2] fix: Inventory slot 버그 수정
- [M3] docs: Combat 시스템 문서화

Type:
- feat: 새 기능
- fix: 버그 수정
- docs: 문서 변경
- refactor: 리팩토링
- test: 테스트 추가/수정
- chore: 기타 변경
```

---

## 체크리스트 활용법

### 1. 현재 Phase 확인
```bash
# 진행 중인 마일스톤 확인
check docs/impl/CHECKLIST.md
```

### 2. 작업 시작
```
1. CHECKLIST.md에서 현재 Phase 확인
2. 해당 Phase 폴더 생성 (phases/M{X}/)
3. RESEARCH_{Phase}_{Feature}.md 작성
4. 리서치 완료 후 PLAN 문서 작성
5. 구현 시작
```

### 3. 작업 완료
```
1. 체크리스트 항목 체크
2. 검증 (테스트, 리뷰)
3. 다음 작업으로 이동
```

---

## 필수 참고 문서

### 개발 시 항상 참고
1. **[AGENTS.md](../../AGENTS.md)** - 코딩 표준 및 아키텍처 규칙
2. **[CHECKLIST.md](./CHECKLIST.md)** - 마일스톤별 작업 체크리스트
3. **[GDD.md](../game-design/GDD.md)** - 게임 기획 문서

### 리서치 시 참고
1. **기존 코드** - `Assets/Scripts/` 내 유사 시스템 검색
2. **기획 문서** - `docs/impl/` 내 해당 시스템 구현 계획
3. **외부 레퍼런스** - Unity 공식 문서, 관련 블로그 등

---

## 개발 순서 (우선순위)

### Phase 진행 순서
```
M0 (프로젝트 설정)
    ↓
M1 (코어 시스템)
    ├── DI Container
    ├── EventBus
    ├── TimeManager
    ├── SaveSystem
    └── GameManager
    ↓
M2 (생존/생산)
    ├── HungerSystem
    ├── HealthSystem
    ├── StaminaSystem
    ├── InventorySystem
    ├── FarmingSystem
    ├── FishingSystem
    └── GatheringSystem
    ↓
M3 (전투/건설)
    ├── CombatSystem
    ├── WeaponSystem
    ├── EnemyAI
    ├── BuildingSystem
    └── TileSystem
    ↓
M4 (경제/콘텐츠)
    ├── CurrencySystem
    ├── ShopSystem
    ├── TourismSystem
    ├── ResidentSystem
    ├── CraftingSystem
    └── QuestSystem
    ↓
M5 (콘텐츠 완성)
M6 (폴리싱)
M7 (릴리즈)
```

---

## 리서치 문서 작성 규칙

### 작성 원칙
1. **객관적**: 추측보다는 확인된 사실 기록
2. **구조적**: 템플릿 준수
3. **참조성**: 관련 파일 경로 명시
4. **의사결정**: 선택지와 결정 사유 기록

### 필수 포함 내용
- 분석 대상 시스템
- 기존 코드 위치 및 상태
- 의존성 목록
- 리스크 및 대응책
- 구현 방향 결정

---

## 코드 리뷰 체크리스트

### Self Review
- [ ] AGENTS.md 코딩 규칙 준수
- [ ] DI 패턴 적용 확인
- [ ] EventBus 이벤트 적절히 사용
- [ ] null 체크 구현
- [ ] 주석/XML 문서화 완료
- [ ] 테스트 코드 작성

### 완료 조건
- [ ] 기능이 기획서 요구사항 충족
- [ ] 버그 없음 (기본 시나리오)
- [ ] 성능 이슈 없음
- [ ] 문서 업데이트 완료

---

## 업데이트 로그

| 날짜 | 버전 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 2026-03-13 | 1.0 | 초안 작성 | AI |

---

## 참고

- 이 문서는 필요 시 업데이트됩니다.
- 프로세스 개선 제안은 언제든 환영합니다.
