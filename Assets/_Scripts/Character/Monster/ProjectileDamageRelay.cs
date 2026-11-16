using UnityEngine;

// 이 오브젝트에 맞은 투사체 피해를 지정된 몬스터의 체력에 적용한다.
// 피격 위치는 이 오브젝트를 기준으로 계산하며, 체력과 사망 처리는 대상이 담당한다.
public class ProjectileDamageRelay : MonoBehaviour
{
    private MonsterCharacter damageTarget;

    public void Initialize(MonsterCharacter target)
    {
        damageTarget = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTarget == null) return;

        damageTarget.ApplyProjectileDamage(collision, collision.ClosestPoint(transform.position));
    }
}
