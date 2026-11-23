/*****************************************************************************************
 * 파일: GreaterDevilController.cs
 * 역할: 대악마용 AIController. 기본 추적 이동을 끈다.
 *       이동/공격은 GreaterDevil이 코루틴으로 직접 처리하고,
 *       사망 처리(경험치 지급 → Die)는 기존 AIController의 Dead 상태 흐름을 그대로 쓴다.
 *       플레이어 쪽을 바라보는 좌우 뒤집기는 기본 동작(spriteDir -1: 이미지가 오른쪽을 본다)을 유지한다.
 *****************************************************************************************/

public class GreaterDevilController : AIController
{
    protected override void CalculateAIMovement() { }
}
