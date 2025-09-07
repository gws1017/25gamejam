using UnityEngine;

public class BasicZombie : MonsterCharacter
{
    public override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackDelayCorutine());
    }
}
