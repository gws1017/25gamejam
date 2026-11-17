/*****************************************************************************************
 * 파일: AlienBossController.cs
 * 역할: 보스2용 AIController. 기본 추적 이동과 좌우 뒤집기만 끈다.
 *       이동/공격은 AlienBoss가 폼별로 직접 처리하고,
 *       사망 처리(경험치 지급 → Die)는 기존 AIController의 Dead 상태 흐름을 그대로 쓴다.
 *****************************************************************************************/

public class AlienBossController : AIController
{
    protected override void Awake()
    {
        base.Awake();

        // spriteDir 0 = 플레이어 쪽을 보도록 좌우 뒤집지 않음.
        // 파트너(빨강)는 컨트롤러가 없어 안 뒤집히므로, 두 마리 모습을 맞추기 위해 본체도 끈다.
        spriteDir = 0;
    }

    protected override void CalculateAIMovement() { }
}
