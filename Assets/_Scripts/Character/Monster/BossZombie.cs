using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI.Table;

public class BossZombie : Boss
{

    [SerializeField] private GameObject zombieBulletPrefab;
    [SerializeField] private Transform firePoint;        // 총구 위치(자식 트랜스폼 할당)

    private bool pattern2Unlocked = false;

    [Tooltip("Phase3 회전속도 배율 (10도 간격이라 1.0이어도 공격 빈도가 9배 늘어남 — 밸런스용 조절 노출). " +
             "Play 중에도 매 프레임 재계산해서 바로 반영됨 — 테스트하면서 실시간으로 조절 가능.")]
    [SerializeField] private float phase3RotateSpeedMultiplier = 1f;

    // Phase3 진입 직전의 회전속도(배율 적용 전 기준값). 이 값에 매 프레임 배율을 곱해서 적용하므로
    // phase3RotateSpeedMultiplier를 Play 중에 바꿔도 다음 프레임부터 바로 반영된다.
    private float rotateSpeedBeforePhase3 = -1f;

    void Start()
    {
        attackRange = 3f;
    }

    void Update()
    {
        if (CurrentPhase == 3 && rotateSpeedBeforePhase3 >= 0f && controller is BossZombieController bc)
        {
            bc.RotateSpeed = rotateSpeedBeforePhase3 * phase3RotateSpeedMultiplier;
        }
    }

    protected override void OnPhaseChanged(int newPhase)
    {
        // Phase 2: 브레스/솟구침(패턴2) 추가 — 실제 공격 루틴은 별도 설계/구현 예정, 지금은 훅만
        if (newPhase >= 2 && !pattern2Unlocked)
        {
            pattern2Unlocked = true;
            // TODO: 패턴2(브레스+솟구침) 공격 루틴 시작 — AttackTelegraph 활용 예정
        }

        // Phase 3: 패턴1(90도 간격) → 패턴3(10도 간격, 정박자)로 전환. 같은 회전+각도 트리거
        // 메커니즘을 재사용하는 것이므로 "교체"이지 "추가"가 아님 — 간격이 촘촘해진 만큼
        // 회전속도도 올려야 리듬이 유지된다. 실제 배율 적용은 Update()에서 매 프레임 재계산.
        if (newPhase == 3 && controller is BossZombieController bc)
        {
            bc.SetAttackAngles(BuildTightAttackAngles());
            rotateSpeedBeforePhase3 = bc.RotateSpeed; // 배율 적용 전 기준값 캡처(한 번만)
        }
    }

    private List<float> BuildTightAttackAngles()
    {
        var angles = new List<float>();
        for (float a = 0f; a < 360f; a += 10f) angles.Add(a);
        return angles;
    }

    public override void PowerUp(int count)
    {
        base.PowerUp(count);

        float pm = 1 + powerUpMultiplier * 2;

        if (controller is not BossZombieController bc) return;
        bc.RotateSpeed *= pm;

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
        if (PoolManager.Instance == null) return;

        Vector2 origin = (firePoint != null) ? firePoint.position : transform.position;
        Vector2 playerPos = controller.TargetPlayer.transform.position;

        Vector2 targetDir = (playerPos - origin);
        if (targetDir.sqrMagnitude < 0.0001f)
        {
            // 혹시 완전히 겹쳐있거나 0벡터면 바라보는 방향으로 대체
            targetDir = transform.right;
        }
        targetDir.Normalize();

        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        Bullet bullet = PoolManager.Instance.Spawn<Bullet>(origin, rot, "BossBullet");
        if (bullet != null)
        {
            bullet.Init(damage, gameObject);
            bullet.AddIgnoreObject(gameObject); // 본인 무시
            bullet.Fire(targetDir);
        }

        controller.ChangeState(AIController.AIState.Move);
        GetComponent<Animator>().SetTrigger("Move");

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
        GetComponent<Animator>().SetTrigger("Move");
        BossZombieController bc = controller as BossZombieController;
        if (bc == null) yield break;
        Vector2 dir = (rb.position - targetPos);
        bc.RotateAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

}
