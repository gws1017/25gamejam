using UnityEngine;

/*****************************************************************************************
 * 파일: DamageRelay.cs
 * 역할: 이 오브젝트에 맞은 플레이어 탄의 피해를 다른 몬스터에게 넘겨주는 부품.
 *       자기 체력은 없다. 몸이 여러 개인데 체력은 하나를 공유하는 보스에 붙인다.
 *       (예: 보스2 빨강 파트너 → 파랑 본체 체력이 깎임)
 *       플레이어 쪽에서도 Target을 보고 이 몸을 그 몬스터로 취급해 접촉 피해를 받는다.
 * 에디터 세팅:
 *   - 같은 오브젝트에 Collider2D(Is Trigger) 필요
 *   - 태그는 Enemy (보스가 쏜 탄이 이 오브젝트를 맞히지 않도록)
 *   - target은 런타임에 SetTarget으로 연결한다 (프리팹끼리는 씬의 인스턴스를 미리 참조할 수 없음)
 *****************************************************************************************/

public class DamageRelay : MonoBehaviour
{
    private MonsterCharacter target;

    /// <summary>이 몸이 대신하는 몬스터. 플레이어 접촉 피해 판정에서 사용.</summary>
    public MonsterCharacter Target => target;

    public void SetTarget(MonsterCharacter newTarget)
    {
        target = newTarget;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (target == null || !target.IsLive) return;
        if (!MonsterCharacter.TryConsumePlayerBullet(collision, gameObject, out float damageAmount)) return;

        // 피격 이펙트는 실제로 맞은 이 오브젝트 위치에 나오도록 여기 기준으로 계산
        target.TakeDamage(damageAmount, collision.ClosestPoint(transform.position));
    }
}
