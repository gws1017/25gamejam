using UnityEngine;

// 기본 AI의 추적 이동만 차단한다. 이동은 BossAlien의 돌진 패턴에서 처리한다.
// AIController의 Dead 상태 처리는 유지하여 경험치 지급과 MonsterCharacter.Die를 실행한다.
public class BossAlienController : AIController
{
    protected override void CalculateAIMovement() { }
}
