using System.Collections;
using UnityEngine;
using static AIController;

public class PoliceZombie : MonsterCharacter
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private float attackCoolTime = 1f;

    private bool isAttacking = false;

    public override void Attack()
    {
        if (isAttacking) return;
        if (controller == null) return;
        base.Attack();
        isAttacking = true;

        Vector2 targetDir = controller.TargetDir;
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        var bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward)).GetComponent<Bullet>();

        bulletObject.Init(damage, gameObject);
        bulletObject.AddIgnoreObject(gameObject); // 본인 무시
        bulletObject.GetComponent<SpriteRenderer>().sprite = bulletSprite;

        StartCoroutine(AttackDelayCorutine());
        
    }

    IEnumerator AttackDelayCorutine()
    {
        GetComponent<Animator>().SetTrigger("Idle");

        yield return new WaitForSeconds(attackCoolTime);

        isAttacking = false;

        controller.ChangeState(AIController.AIState.Move);
        GetComponent<Animator>().SetTrigger("Move");
    }
}
