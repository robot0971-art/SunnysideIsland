# 7. 테스트 계획

## 7.1 테스트 전략

### 테스트 레벨
```
Level 1: 단위 테스트 (Unit Tests)
  - 개별 클래스/메서드 테스트
  - 비즈니스 로직 검증
  
Level 2: 통합 테스트 (Integration Tests)
  - 시스템 간 연동 테스트
  - 데이터 흐름 검증
  
Level 3: 시스템 테스트 (System Tests)
  - 전체 게임 플로우 테스트
  - 시나리오 기반 테스트
  
Level 4: 플레이 테스트 (Play Tests)
  - 실제 플레이어 테스트
  - UX/밸런스 검증
```

---

## 7.2 단위 테스트

### 테스트 대상 시스템
- [ ] DI Container
- [ ] EventBus
- [ ] TimeManager
- [ ] SaveSystem
- [ ] CurrencySystem
- [ ] InventorySystem
- [ ] QuestSystem

### 테스트 예시
```csharp
[Test]
public void DIContainer_RegistersAndResolvesSingleton()
{
    var container = new DIContainer();
    container.Register<IService, ServiceImplementation>();
    
    var service1 = container.Resolve<IService>();
    var service2 = container.Resolve<IService>();
    
    Assert.AreSame(service1, service2);
}

[Test]
public void CurrencySystem_AddsAndRemovesGold()
{
    var currency = new CurrencySystem();
    currency.AddGold(100);
    
    Assert.AreEqual(100, currency.CurrentGold);
    Assert.IsTrue(currency.RemoveGold(50));
    Assert.AreEqual(50, currency.CurrentGold);
}

[Test]
public void InventorySystem_AddsAndRemovesItems()
{
    var inventory = new PlayerInventory();
    var item = ScriptableObject.CreateInstance<ItemData>();
    
    Assert.IsTrue(inventory.AddItem(item, 5));
    Assert.IsTrue(inventory.HasItem(item, 5));
    Assert.IsTrue(inventory.RemoveItem(item, 2));
    Assert.IsTrue(inventory.HasItem(item, 3));
}
```

### 단위 테스트 도구
- Unity Test Framework (NUnit)
- Code Coverage Package

---

## 7.3 통합 테스트

### 테스트 시나리오

#### 시나리오 1: 시간-생존 시스템 연동
```
Given: 게임 시작 상태
When: 시간이 흘러 밤이 되면
Then: 몬스터 생성 확률 증가
And: 플레이어 시야 감소
```

#### 시나리오 2: 건설-경제 시스템 연동
```
Given: 충분한 재료와 골드를 보유
When: 건설을 시작하면
Then: 재료와 골드가 소모됨
And: 건설 타이머가 시작됨
And: 완료 시 건물이 생성됨
```

#### 시나리오 3: 퀘스트-이벤트 연동
```
Given: "고블린 5마리 처치" 퀘스트가 활성화됨
When: 고블린을 처치하면
Then: 퀘스트 진행도가 증가함
And: 5마리 처치 시 퀘스트 완료됨
And: 보상이 지급됨
```

---

## 7.4 시스템 테스트

### 게임 플로우 테스트

#### 테스트 1: 새 게임 플로우
```
1. 메인 메뉴 → 새 게임 시작
2. 튜토리얼 표시 확인
3. 초기 인벤토리 확인 (기본 도구)
4. 첫 퀘스트 수락 확인
5. 시간 흐름 확인
```

#### 테스트 2: 저장/불러오기
```
1. 게임 진행 (자원 수집, 건설)
2. 게임 저장
3. 게임 종료 및 재시작
4. 저장된 게임 불러오기
5. 진행 상태 확인
```

#### 테스트 3: 28일 엔딩 플로우
```
1. 타임랩스로 시간 가속
2. 리소트 호텔 건설
3. 엔딩 시퀀스 확인
4. 자유 모드 해금 확인
```

---

## 7.5 밸런스 테스트

### 경제 밸런스 테스트

#### Week 1 밸런스
```
테스트 조건: 신규 플레이어, 난이도 Normal
목표: 텐트 → 오두막 건설 가능해야 함

검증 항목:
- [ ] 하루 수익 100~200 코인
- [ ] 생활비 50~70 코인
- [ ] 3일 내 오두막 건설 가능
- [ ] 음식 관리 가능
```

#### Week 4 밸런스
```
테스트 조건: Day 22, 중급 장비 보유
목표: 리조트 호텔 건설 가능

검증 항목:
- [ ] 일일 수익 5000~10000 코인
- [ ] 50,000 코인 모금 가능
- [ ] 관광객 20~50명/일
- [ ] 보스전 승리 가능
```

### 전투 밸런스 테스트

#### 난이도별 테스트
| 난이도 | 몬스터 HP | 몬스터 공격력 | 플레이어 승리 시간 |
|--------|-----------|---------------|------------------|
| Easy | -20% | -20% | 고블린 3초, 족장 2분 |
| Normal | 표준 | 표준 | 고블린 5초, 족장 3분 |
| Hard | +30% | +30% | 고블린 8초, 족장 5분 |
| Hell | +100% | +50% | 고블린 12초, 족장 8분 |

---

## 7.6 성능 테스트

### 프레임 레이트 테스트
```
목표: 안정적인 60 FPS 유지

테스트 케이스:
1. 평온한 필드 (NPC 5명 이하)
2. 번화가 (NPC 20명+, 건물 30개+)
3. 전투 중 (몬스터 10마리+)
4. 축제 이벤트 (NPC 50명+)
```

### 메모리 테스트
```
목표: 메모리 누수 없음

체크리스트:
- [ ] 장면 전환 시 메모리 정리
- [ ] 에셋 언로드 확인
- [ ] 오브젝트 풀링 효율성
- [ ] 저장/불러오기 메모리 사용량
```

### 로딩 시간 테스트
```
목표: 각 로딩 3초 이내

측정 항목:
- [ ] 게임 시작 로딩
- [ ] 씬 전환 로딩
- [ ] 저장/불러오기 속도
- [ ] UI 열기/닫기 반응속도
```

---

## 7.7 플레이 테스트 계획

### 내부 플레이 테스트
**1차 (Alpha)**
- 대상: 개발팀 내부
- 기간: 1주일
- 목표: 치명적 버그 발견, 기본 플레이 가능성 확인

**2차 (Closed Beta)**
- 대상: 지인 10명
- 기간: 2주일
- 목표: UX 피드백, 밸런스 조정

**3차 (Open Beta)**
- 대상: 외부 테스터 50명
- 기간: 1주일
- 목표: 다양한 플레이 패턴 확인

### 피드백 수집 항목
- [ ] 첫 30분 플레이 경험
- [ ] UI 사용성 (1~5점)
- [ ] 전투 재미 (1~5점)
- [ ] 경제 밸런스 적절성 (1~5점)
- [ ] 28일 내 클리어 가능성
- [ ] 가장 재미있었던 부분
- [ ] 가장 불편했던 부분
- [ ] 버그 리포트

---

## 7.8 버그 추적

### 버그 심각도 분류
- **Critical**: 게임 진행 불가, 크래시
- **Major**: 주요 기능 불가, 데이터 손실
- **Minor**: 사소한 문제, UI 오류
- **Trivial**: 시각적 문제, 오타

### 버그 리포트 템플릿
```
제목: [버그 간단 설명]
심각도: Critical/Major/Minor/Trivial
환경: Windows/Mac, 해상도
재현 방법:
1. 
2. 
3. 

예상 결과: 
실제 결과: 
스크린샷: (첨부)
로그: (첨부)
```

---

## 7.9 테스트 일정

| 단계 | 기간 | 목표 |
|------|------|------|
| 단위 테스트 | 프로토타입부터 지속 | 코드 품질 확보 |
| 통합 테스트 | M2~M3 | 시스템 연동 검증 |
| 시스템 테스트 | M3~M4 | 전체 플로우 검증 |
| 성능 테스트 | M3~M4 | 최적화 확인 |
| 1차 플레이 테스트 | M3 | 알파 테스트 |
| 2차 플레이 테스트 | M4 | 베타 테스트 |
| 최종 검수 | M4 | 릴리즈 준비 |

---

## 7.10 테스트 자동화

### CI/CD 파이프라인
```yaml
# 예시 GitHub Actions workflow
test:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v2
    - name: Run Unit Tests
      run: |
        unity -runTests -testPlatform editmode
        unity -runTests -testPlatform playmode
    - name: Build Check
      run: unity -quit -batchmode -buildWindows64Player
```

### 자동화 테스트 항목
- [ ] 빌드 성공 여부
- [ ] 단위 테스트 통과
- [ ] 정적 분석 (SonarQube)
- [ ] 코드 커버리지 리포트
