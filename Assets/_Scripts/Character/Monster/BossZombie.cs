using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BossZombie : MonsterCharacter
{
    
    [SerializeField] private GameObject zombieBulletPrefab;

    void Start()
    {
        attackRange = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Spawn()
    {
        base.Spawn();

        if (controller == null) return;
        StartCoroutine(SpawnCorutine());
    }

    public override void Attack()
    {
        base.Attack();

        if (controller == null) return;

        Vector2 targetDir = controller.TargetDir;
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Instantiate(zombieBulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));

        controller.ChangeState(AIController.AIState.Move);
    }

    //등장시 이동과 스케일을 서서히 늘린다
    IEnumerator SpawnCorutine()
    {
        var rb = GetComponent<Rigidbody2D>();
        Vector2 targetPos = controller.TargetPlayer.position;
        Vector2 targetDir = (rb.position - targetPos).normalized;
        
        Vector2 spawnPos = targetPos + targetDir * attackRange;

        Vector3 initialScale = Vector3.one * 0.2f;
        Vector3 targetScale = Vector3.one;

        float elapsed = 0f;
        float duration = 1f;
        while ((rb.position - spawnPos).sqrMagnitude > 0.01f || elapsed < duration)
        {
            Vector2 toTarget = spawnPos - rb.position;
            float step = controller.MoveSpeed * Time.fixedDeltaTime;
            Vector2 next = (toTarget.magnitude > step) ? rb.position + toTarget.normalized * step : spawnPos;
            rb.MovePosition(next);

            //보스몬스터 scale이 0.2 부터 1.0까지 점점 커진다
            if (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                elapsed += Time.deltaTime;
            }

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(spawnPos);
        transform.localScale = targetScale;
        controller.ChangeState(AIController.AIState.Move);

        BossZombieController bc = controller as BossZombieController;
        if (bc == null) yield break;
        Vector2 dir = (rb.position - targetPos);
        bc.RotateAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

}
