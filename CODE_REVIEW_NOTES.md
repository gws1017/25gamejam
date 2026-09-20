# 코드 리뷰 피드백 (미반영 항목)

작성일: 2026-09-17
반영 완료: `UI_StateManager`/`IToggleUI` 관련 Hide-Resume 분리 리팩토링 (커밋 `d59a057`, `dev`/`test` 브랜치)

아래는 그 외에 리뷰 중 발견됐지만 아직 코드에 적용하지 않은 항목입니다.

## 🔴 버그 후보 (우선 확인 필요)

### 1. `PlayerController.CheckUIInput()` — GetKey 대신 GetKeyDown 필요
- 파일: `Assets/_Scripts/Character/Player/PlayerController.cs:119-130`
- `Input.GetKey(KeyCode.E)` / `Input.GetKey(KeyCode.Escape)`를 사용 중이라, 키를 누르고 있는 동안 매 프레임 `UI_StateManager.SetState()`가 재호출됨.
- `SetState()`는 `HideAll()` + `Show()`를 매번 실행하므로, 키를 누르고 있으면 UI가 매 프레임 다시 열리는 셈 (연출/사운드 반복 재생 가능성).
- `Input.GetKeyDown`으로 변경 필요.

### 2. `Spawner.Spawn()` — null 체크 없음
- 파일: `Assets/_Scripts/Character/Spawner.cs:83-91`
- `PoolManager.Instance.Get(randKey)`가 잘못된 key(오타 등)일 경우 `null`을 반환하는데, 바로 `pool.Spawn(...)`을 호출해서 `NullReferenceException` 위험.
- `monsterKeys` 배열에 오타가 있거나 `PoolManager`에 해당 key 등록이 안 되어 있으면 즉시 터짐.

## 🟡 성능/불필요한 재조회

### 3. `PlayerController` — 매 프레임 GetComponent 재호출
- 파일: `Assets/_Scripts/Character/Player/PlayerController.cs:69, 93, 103`
- `Update()`/`FixedUpdate()`에서 `GetComponent<PlayerCharacter>()`, `GetComponentInChildren<RobotSpirit>()`를 매 프레임 다시 호출.
- 이미 `Awake()`에서 `playerCharacter`(44번째 줄), `robot`(52번째 줄) 필드로 캐싱해뒀으므로 캐시된 필드를 쓰면 됨.

## 🟢 정리하면 좋은 것들

### 4. 안 쓰는 using 제거
- `PlayerController.cs:2` — `using System.IO.Pipes;`
- `RobotSpirit.cs:3-4` — `using static UnityEngine.Rendering.DebugUI.Table;`, `using static UnityEngine.UI.Image;`
- 자동완성 잔재로 보이며 실제로 사용되지 않음.

### 5. `RobotSpirit.Attack()` 안 쓰는 파라미터
- 파일: `Assets/_Scripts/Weapon/RobotSpirit.cs:30`
- `Attack(float _ignoredAngle, bool auto = true)` — 주석에도 "더 이상 사용하지 않음"이라 적혀있는데 파라미터가 남아있음.
- 호출부(`PlayerController.cs:262, 266`)도 같이 정리하면 시그니처가 깔끔해짐.

### 6. `RobotSpirit.DeactiveParry()` — 죽은 코드 + 안전성
- 파일: `Assets/_Scripts/Weapon/RobotSpirit.cs:78-84`
- 현재 아무도 호출하지 않음 (`PlayerController.cs:231`에서 호출부가 주석 처리되어 있음).
- 쓸 계획이 없으면 삭제, 쓸 계획이면 `parryCoroutine == null` 체크를 추가해야 함 (안 그러면 이미 끝난 코루틴에 `StopCoroutine(null)` 호출 위험).

## 📎 참고 (설계 의도 확인 필요, 버그 아님)

### 7. `Spawner.SpawnBossMonster()` — 보스만 PoolManager 미사용
- 파일: `Assets/_Scripts/Character/Spawner.cs:94-102`
- 일반 몬스터는 전부 `PoolManager` 기반인데 보스만 `Instantiate()`를 직접 사용.
- 최근 "PoolManager 통합" 작업 방향과 일관성이 안 맞을 수 있음 — 의도된 예외(보스는 드물게 생성)인지 확인 필요.
