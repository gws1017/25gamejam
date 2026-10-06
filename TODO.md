# TODO

## 예상 소요 기간

가정: 1인 풀타임, 하루 12시간 작업, 기획/아트/코드 전부 AI 활용.
(플레이테스트로 "느낌" 확인하는 부분은 AI로 못 줄어드니 그대로 반영)

| 섹션 | 예상 소요 |
|---|---|
| 스테이지 진행 & 엔딩 시스템 | 1.8~3.2일 |
| 보스 패턴 구현 (보스1) | 3~4.5일 |
| 스테이지 2 — 기획+에셋 | 1.3~2.3일 |
| 스테이지 2 — 구현 | **TBD** (패턴 기획 전까진 추정 불가, 최악의 경우 보스1 패턴2급 3~5일까지 가능) |
| 스테이지 3 — 기획+에셋 | 1.3~2.3일 |
| 스테이지 3 — 구현 | **TBD** (동일) |
| 스킬 시스템 | 1.8~2.8일 |
| 튜토리얼 개선 | 1.7~2.7일 |
| **소계 (TBD 제외)** | **약 11.9~18.8일** |
| 버퍼 (+25%) | +3~4.7일 |
| **소계 (버퍼 포함, TBD 제외)** | **약 15~23.5일** |

⚠️ 보스2("삼총사"라 개체 여러 마리일 수도), 보스3 구현은 패턴이 아직 하나도 안 정해져서
**기획 끝나기 전까지는 이슈 트래커에 시간 추정치 넣지 말고 "TBD"로 등록** 하는 걸 권장.
패턴 기획 후 실제 스펙 나오면 그때 재추정.

## 우선순위

- **P0** — 먼저 (구조 + 빠른 버그): 없으면 나머지가 얹힐 자리가 없거나, 방치하면 계속 쌓이는 것들
- **P1** — 핵심 콘텐츠 완성: 이미 있는 걸 끝내는 것, 스테이지 시스템과 무관하게 독립 진행 가능
- **P2** — 신규 콘텐츠: 스코프 제일 큼, P0(스테이지 시스템) 완료 후 실제로 붙여서 테스트 가능
- **P3** — 부가 시스템: 다른 것과 독립적, 없어도 게임 플레이 자체는 가능

---

## [P0] 스테이지 진행 & 엔딩 시스템 (스테이지 2/3 추가의 선행 작업)

원래 기획: **5스테이지마다 보스 스테이지** 등장, 무한모드는 별도 모드로 추가 예정이었음.
기존 구현은 그 둘 다 빠지고 "무한 서바이벌 스케일링"만 남아있었음 — 스테이지 개념 자체가 없고
플레이어 레벨이 난이도 카운터 역할을 겸하고 있었음(`Spawner`가 레벨 10마다 보스 재소환,
`SpawnBossMonster()`의 `lv`이 항상 0이라 `bossPrefabs[0]`만 영원히 재소환).

**→ 진행 카운터를 `StageManager`로 분리 완료.** 스테이지는 킬카운트로, 레벨은 경험치로 각각 진행하며
서로 독립적. `GameManager.GameState`엔 아직 승리/클리어 상태가 없어서 엔딩은 미구현.

- [x] 보스 순서 확정: 5스테이지=슬라임좀비(기존), 10스테이지=외계인 삼총사, 15스테이지=로봇악마
- [x] 종료 조건 확정: 기본(스토리) 모드는 보스3(15스테이지) 클리어 시 엔딩, 무한모드는 보스 3종 계속 순환
- [x] 진행 기준 확정: **킬카운트로 스테이지 진행 / 경험치·레벨은 분리해서 추후 스킬 시스템 전용으로 보존**
      - 일반 스테이지: 몬스터 40마리(인스펙터 조절) 처치 시 다음 스테이지, 킬카운트는 스테이지마다 리셋
      - 보스 스테이지(5의 배수): 잡몹 킬 무시, 보스를 잡아야만 통과
- [x] 스테이지 진행 상태 추가 (`StageManager` 신설 — 킬카운트 추적, 5스테이지마다 보스 스테이지 트리거)
- [x] 몬스터 사망 이벤트 추가 (`MonsterCharacter.OnMonsterDied` — 킬카운트/보스 클리어 판정 공용)
- [x] `Spawner`가 플레이어 레벨 대신 `StageManager.CurrentStage` 참조하도록 전환 (`bossPrefabs[0]` 고정 버그 해소)
- [x] 스테이지 번호 UI 표시 (`UI_InGame.SetStage`)
- [x] 기본(스토리) 모드 / 무한모드 모드 선택 진입점 추가 (`SceneLoader`에 `GameMode` enum + static 선택값 추가 —
      `targetScene`과 같은 패턴이라 별도 클래스 안 만들고 흡수. "시작하기" 클릭 시 기존 버튼(`ContentParents`) 숨기고
      모드 선택 패널(스토리/무한/뒤로가기) 노출 — 상시 버튼 2개 노출 대신 서브메뉴 방식)
      - [x] 에디터 세팅: MainMenuScene에 `ModeSelectPanel` 오브젝트 신설(기본 비활성화),
            버튼 3개(스토리/무한/뒤로가기) 배치 후 `UI_MainMenu` 슬롯 연결
      - [x] 버그 수정: `Title`이 `ContentParents` 안에 있어서 시작 버튼 누르면 같이 사라지던 문제 —
            `ContentParents` 밑에 `MainButtons` 서브그룹(버튼 3개만) 신설해서 그것만 토글하도록 변경
- [x] 무한모드 분리 (`StageManager.AdvanceStage()`에서 `SceneLoader.GetGameMode() == Story`일 때만
      `finalStage` 체크 → Victory. 무한모드는 그 조건을 안 타서 끝없이 진행, 보스도 계속 재소환됨)
- [x] `GameState`에 승리 상태 추가 + 엔딩 씬 제작 (`GameManager.GameState.Victory`, `UI_Ending.cs` — `UI_Intro.cs`와 대칭되는
      타이핑 연출 컷씬. 15스테이지 클리어 시 0.8초 대기 후 EndingScene 전환 → 완료 시 MainMenuScene 복귀)
      - [x] EndingScene 에디터 세팅 완료, `GameManager.VictoryState()`(ContextMenu) 단축키로 Victory→EndingScene 전환 테스트 완료
      - [x] Galmuri9 SDF 폰트 아틀라스 확장(4096) + Clear Dynamic Data 적용 — 한글 깨짐 해결 확인
      - [ ] `StageManager`가 실제로 보스 사망을 감지해서 Victory를 호출하는 경로는 아직 미검증
            (`Final Stage`/`Kills Per Stage`를 임시로 낮춰서 실제 킬로 확인 필요, 확인 후 15/40으로 복구)
      - [ ] 엔딩 삽화 2~4번 제작 (1번 `ending1.png`만 완성) 후 `endingSprites` 배열에 등록
- [ ] 스테이지 전환 연출 (배경/화면전환 등, 필요 시)
- [ ] 스테이지별로 다른 `monsterKeys` 사용 (현재는 전 스테이지 공통 배열 1개 — 스테이지 2/3 에셋 나온 뒤 작업)

### 경험치 밸런스 (스킬 시스템 붙이기 전까지 결정 필요)

현재 `PowerUp()`은 몬스터 HP/공격력/방어력만 올리고 `dropExp`는 안 올림
(`MonsterCharacter.cs`의 해당 줄이 주석 처리됨). `LevelUp()`도 `maxExp`를 100 고정으로 둬서
**후반으로 갈수록 몬스터는 단단해지는데 주는 경험치는 그대로 → 레벨업이 점점 느려짐**.
스테이지와 레벨을 분리한 지금은 이게 스킬 획득 속도가 후반에 말라붙는 형태로 드러남.

- [ ] `dropExp`도 `PowerUp` 배율 적용할지 (주석 해제) / `maxExp`를 레벨마다 올릴지 결정
- 참고 현재값: ZombieBasic 10, ZombiePolice 20, ZombieSlime(보스) 70 / `maxExp` 100 고정

## [P1] 보스 패턴 구현 (Stage 1 슬라임 좀비)

기획서 대비 현재는 패턴1만 축소 구현된 상태. 원래 스테이지 2/3와 보스를 각각 만들 계획이었으나
시간 부족으로 보스 1종만 넣고, `Spawner`/`PoolManager` 레벨 스케일링으로 무한히 강해지게 만들어
땜빵한 것으로 보임 (`BossZombie.PowerUp()`이 회전속도만 배율로 증가). 나머지 격차:

- [x] 패턴 전환(페이즈) 구조 추가 — HP% 기반. `MonsterCharacter`(일반몬스터 포함 공통 부모) 밑에
      `Boss`(신규) 중간 클래스를 추가해서 Phase 시스템은 전부 거기로 격리
      (`IsBoss=true` 고정, `phaseHpThresholds` + `virtual OnPhaseChanged(int)` 훅 — 일반 몬스터
      프리팹 인스펙터엔 Phase 필드가 안 보임, 추후 스테이지2/3 보스는 `Boss` 상속만 받으면 됨).
      `BossZombie : Boss`로 변경, `OnPhaseChanged()`에서:
        - Phase 2 진입 시: 패턴2(브레스/솟구침) 해금 플래그만 세움 — 실제 공격 루틴은 미구현(아래 항목)
        - Phase 3 진입 시: 패턴1(90도 간격)→패턴3(10도 간격)으로 `attackAngles` 교체 + 회전속도 배율
          (패턴1·3은 같은 회전+각도트리거 메커니즘이라 "교체"이지 "추가"가 아님 — 패턴2는 유지되는 "추가" 쪽.
          배율은 `phase3RotateSpeedMultiplier`로 인스펙터 노출, 기본 1배 — 10도 간격 자체로 이미 9배
          빈도라 처음엔 2배를 곱했다가 "기관총 수준"이라 1배로 수정, 추가 튜닝은 플레이테스트로)
      - [x] 버그 수정: `BossZombie.Attack()`이 죽은 구버전 `BulletPoolManager.Instance`로 null 가드하는 바람에
            항상 조기 리턴 → `Attack` 상태에 영원히 고정되어 서클링·발사체 둘 다 멈추던 문제. 실제 스폰에 쓰는
            `PoolManager.Instance`로 가드 교체
      - [x] 에디터 세팅: `ZombieSlime.prefab`의 `Phase Hp Thresholds` 배열에 `{0.66, 0.33}` 입력 완료
      - [x] 버그 수정: `BossZombieController.CanAttack()`이 쏜 각도를 `List<float>`에 계속 쌓기만 하던 방식이라,
            패턴3(10도 간격, ±5도 허용오차)는 판정 구간이 끊김 없이 이어져서 "리스트 리셋될 틈"이 영영
            안 생김 → 한 바퀴 돌고 나면 영구히 공격 불가 상태가 되던 버그. "직전에 쏜 각도 하나만" 비교하는
            방식(`lastFiredAngle`)으로 교체, 몇 바퀴를 돌아도 막히지 않음
      - [x] `phase3RotateSpeedMultiplier`를 매 프레임 재계산하도록 변경 — Play 중 Inspector 값 바꾸면
            즉시 반영됨 (기존엔 Phase3 진입 순간 1회성 곱셈이라 재스폰해야 테스트 가능했음)
      - [ ] 페이즈 전환 실제 플레이테스트 계속 (패턴3 속도 등 세부 밸런스)
      - [x] 디버그 단축키: `StageManager.JumpToNearestBossStage()` ([ContextMenu]) — 가장 가까운
            보스 스테이지로 즉시 점프, 플레이테스트용
      - [ ] 보스 등장 시 프레임 드랍 체감 리포트 있음 — 원인 미확정. Spine/풀링 등 검토했으나 결정적 증거
            못 찾음, Profiler로 재확인 필요
- [x] 보스 체력바 UI 추가 — `MonsterCharacter.OnMonsterSpawned`(신규, `OnMonsterDied`와 대칭) +
      `Boss.OnHealthChanged` 이벤트 + `UI_BossHealthBar.cs` 신규. 보스 스폰 시 자동으로 뜨고 사망 시 숨김
      - [x] 에디터 세팅: `UI_InGame` Canvas 상단에 `BossHPBar` 배치, `barRoot`/`fillImage` 슬롯 연결 완료
            (하단은 몹 스폰 지점이라 안 가리게 상단으로 결정)
      - [x] 버그 수정: `barRoot`에 스크립트 자신이 붙은 오브젝트를 넣으면 `SetActive(false)` 시
            스크립트 자신도 꺼져서 이벤트 구독이 영구히 끊기던 문제 — `SetActive` 대신 자식 `Image`들의
            `enabled`만 토글하도록 변경, 기존 Inspector 세팅 그대로 사용 가능
- [x] `AttackTelegraph.cs` 신설 — 테두리(윤곽)만 있다가 안쪽이 서서히 차오르고 다 차면 `OnComplete(위치)` 발행.
      당초 World Space Canvas+Image(Fill) 설계였으나, 프리팹/정렬 문제 없이 **코드에서 SpriteRenderer로 직접 생성**
      (`CreateCircle` / `CreateBeam`)하는 방식으로 변경. 색은 단색 빨강. 감시 대상(보스)이 죽으면 발동 없이 취소
- [x] `SpriteFlipbook.cs` 신설 — 슬라이스한 스프라이트 배열을 1회 재생하고 스스로 사라지는 일회성 VFX
      (Animator/클립/프리팹 불필요)
- [x] 패턴2(브레스+솟구침) 구현 — Phase2 진입 시 해금, Phase3에서도 유지되는 "추가" 패턴.
      **발동 시점**: 보스가 플레이어 기준 동·북·서·남에 도달할 때마다(`BossZombieController.OnCardinalReached`,
      `attackAngles`와 무관하게 항상 4방위 감지). 한 세트(`BossZombie.Pattern2Set()`)는 두 공격이 겹쳐 읽기 어려워서
      **시간차**를 둔다 (브레스 → 솟구침 순서. 솟구침을 먼저 하면 보스가 공전해서 브레스가 방위에서 벗어나므로):
        ①보스가 방위에 선 순간, 보스→플레이어 방향 직사각형 브레스 예고(1발) → `breathWarningDuration`(1.0초) 후 발동.
          **브레스를 모으는 동안에만** 보스가 공전을 멈춤(`pauseOrbitDuringPattern2`), VFX가 끝나면 다시 공전
        ②`gapBetweenPatterns`(0.3초) 뒤, 보스가 도는 중에 플레이어 **발밑** 스냅샷 지점의 **바닥에 눕힌 납작한 타원**
          솟구침 예고(그림자 크기, 원근감) → `warningDuration`(1.2초) 후 발동
      (브레스를 피해 옮긴 자리를 솟구침이 이어서 겨냥하는 연계). 예고 영역과 동일한 범위로 피해 판정(하트 1칸, 발동 순간 1회).
      (※ 초안은 "쿨다운마다 동서남북 4방향 동시 발사 + 큰 원" → 방위 도달 기반 → 동시 발동이 겹쳐 보여 시간차 → 솟구침 중엔 정지 불필요해서 순서 반전)
      - [x] 3페이즈 진입 후 패턴2가 안 나온다는 리포트 → 플레이테스트 재확인 결과 정상 동작 확인(원인 미확정, 진단 로그는 제거).
            재발하면 `Phase 3 Rotate Speed Multiplier`가 너무 낮아 보스가 방위에 못 가는 경우부터 의심
      인스펙터(필수 값만 노출): `breathWarningDuration`(1.0) / `warningDuration`(1.2) / `eruptionRadius`(0.6) /
      `breathLength`(10) / `breathWidth`(1.2) / `eruptionFrames` / `breathFrames`.
      나머지(`GapBetweenPatterns` 0.3, `VfxFps` 16, `GroundSquash` 0.35, `PlayerFootOffset` (0,-0.59))는 코드 상수로 내림 —
      튜닝이 필요해지면 `[SerializeField]`로 다시 올릴 것. 방위 N번째마다 쓰기 옵션/정지 끄기 옵션은 불필요해서 삭제.
      - [x] 에셋: `boss_effect_02_clean.png`(솟구침), `boss_effect_Breath_clean.png`(브레스) — 격자선 제거,
            444px 셀로 재배치, 브레스는 모든 프레임의 빔 뿌리를 (8,222)로 정렬 + 오른쪽 끝 페이드
      - [ ] 에디터 세팅 필요: 두 PNG를 Sprite/Multiple + Grid By Cell Size 444x444로 슬라이스
            (솟구침 pivot Custom (0.5,0.14) / 브레스 pivot Custom (0.018,0.5)), `ZombieSlime.prefab`의 `BossZombie`
            `Eruption Frames` / `Breath Frames` 배열에 슬라이스된 8장을 순서대로 연결 (비워두면 VFX 없이 예고+판정만 동작)
      - [ ] 플레이테스트 후 쿨다운/예고시간/크기 밸런스 조정
      - [x] 디버그 단축키: `Boss`의 [ContextMenu] `Debug: HP 60% (Phase2 진입)` / `HP 30% (Phase3 진입)`
- [ ] 패턴1 다듬기 — 조준 사격을 기획대로 고정 4방향 발사로 바꿀지 결정
- [ ] 패턴3 정박자 리듬/패링 밸런스 테스트 (회전속도 배율이 추정값이라 실제 체감 확인 필요)

## [P2] 스테이지 2 (외계인 촉수 컨셉)

에셋만 있고 기획 없음 — 일반몬스터/보스 패턴 새로 짜야 함

- [ ] 일반몬스터(촉수괴물) 패턴/스탯 기획
- [ ] 보스(외계인 삼총사 - 색상 변형) 패턴 기획
- [ ] 에셋 리터칭/재생성 (분위기 통일용, GPT 등 이미지 툴로 컨셉만 참고해서 새로 생성)
- [ ] 구현 (프리팹/애니메이션/AI 상태/PoolManager 등록)

## [P2] 스테이지 3 (악마·로봇 컨셉)

에셋만 있고 기획 없음 — 일반몬스터/보스 패턴 새로 짜야 함

- [ ] 일반몬스터(악마) 패턴/스탯 기획
- [ ] 보스(로봇악마) 패턴 기획
- [ ] 에셋 리터칭/재생성
- [ ] 구현

## [P3] 스킬 시스템 & 레벨업

아이콘 10개(icon-1~10.png)와 "레벨업! 스킬을 고르세요!" 선택창 UI 목업은 있으나 실제 로직 없음.
`UI_Skill.cs`는 빈 Show()/Hide()만 있고, `PlayerCharacter.LevelUp()`과 연결도 안 되어 있음 —
지금은 레벨업해도 스킬 선택창이 뜨지 않음.

- [ ] 레벨업 시 스킬 선택창 뜨도록 연결 (`PlayerCharacter.LevelUp()` → `UI_StateManager.SetState(UI_Skill)`)
- [ ] 스킬 카드 2~3개 중 선택하는 로직 구현 — 기획 문구가 "2개 중 선택"과 "3업마다 선택 창"으로 상충되니 규칙 확정 필요
- [ ] 스킬별 데이터(효과/설명) 정의 — 예전에 스킬별 설명이 있었던 것 같은데 지금 안 보임, 추가로 찾아볼 것
- [ ] 추천 스킬 아이콘 강조 이펙트

## [P3] 튜토리얼 개선 (인게임 컨텍스추얼 방식)

현재는 디자이너가 만든 PNG 한 장을 팝업으로 띄우고 닫기 버튼만 있는 구조(`UI_Tutorial.cs`).
원래 의도는 플레이 중 특정 타이밍에 잠깐 멈추고, 배워야 할 대상만 밝게(스포트라이트) 남기고
나머지는 딤 처리 후 "여기를 눌러보세요" 식 텍스트로 안내, 실제로 그 조작을 하면 다음 단계로
넘어가는 방식.

- [ ] 딤 오버레이 + 특정 영역만 밝게 보여주는 스포트라이트/마스크 이펙트
- [ ] 튜토리얼 스텝 시퀀스 관리자 (현재 스텝, 다음 스텝 진행 조건)
- [ ] 플레이어 입력 감지 → 다음 스텝 트리거 연결
- [ ] 스텝별 자동 일시정지/재개 타이밍 처리
- [ ] 기존 PNG 안내 텍스트를 스텝별 짧은 문구로 분리
