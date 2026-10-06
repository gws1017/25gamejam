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

    [Header("Pattern 2 (Phase2+) — 보스가 플레이어 기준 동서남북에 설 때마다: 보스→플레이어 방향 브레스 1발 + 발밑 솟구침")]
    [Tooltip("브레스 예고가 차오르는 시간(초) — 이 안에 선을 벗어나야 함")]
    [SerializeField] private float breathWarningDuration = 1.0f;
    [Tooltip("솟구침 예고(빨간 장판)가 차오르는 시간(초) — 이 안에 피해야 함")]
    [SerializeField] private float warningDuration = 1.2f;
    [Tooltip("솟구침 가로 반지름 (월드 유닛). 플레이어 그림자(가로 1.36) 정도로 작게")]
    [SerializeField] private float eruptionRadius = 0.6f;
    [Tooltip("브레스 길이 / 두께 (월드 유닛). 예고 장판과 판정이 같은 크기")]
    [SerializeField] private float breathLength = 10f;
    [SerializeField] private float breathWidth = 1.2f;
    [Tooltip("boss_effect_02_clean 을 슬라이스한 8장을 순서대로 (솟구침). 비워두면 VFX 없이 예고+판정만 동작")]
    [SerializeField] private Sprite[] eruptionFrames;
    [Tooltip("boss_effect_Breath_clean 을 슬라이스한 8장을 순서대로 (브레스). 비워두면 VFX 없이 예고+판정만 동작")]
    [SerializeField] private Sprite[] breathFrames;

    // 스프라이트 시트에서 측정한 실제 효과 크기(px). 인스펙터의 반경/길이/두께에 맞춰 VFX 스케일을 자동 계산하는 데 사용
    private const float EruptionPeakDiameterPx = 335f;
    private const float BreathPeakLengthPx = 428f;
    private const float BreathPeakThicknessPx = 145f;

    // 한 번 정하면 거의 안 바꾸는 값들 (바꿀 일이 생기면 [SerializeField]로 올릴 것)
    private const float GapBetweenPatterns = 0.3f;   // 브레스 VFX 종료 후 솟구침 예고 시작까지 간격(초). 두 패턴이 겹쳐 보이지 않게
    private const float VfxFps = 16f;                // VFX 재생 속도. 8프레임이면 0.5초
    private const float GroundSquash = 0.35f;        // 솟구침 타원의 세로/가로 비율 (바닥에 눕힌 원근감). 플레이어 그림자는 약 0.27
    private static readonly Vector2 PlayerFootOffset = new Vector2(0f, -0.59f); // 플레이어 중심 → 발밑(그림자). Player 프리팹의 Shadow 위치 기준

    private bool isPattern2Busy = false;

    void Start()
    {
        attackRange = 3f;

        // 보스가 플레이어 기준 동서남북에 도달할 때마다 알림을 받는다 (패턴2 발동 시점)
        if (controller is BossZombieController bc)
            bc.OnCardinalReached += HandleCardinalReached;
    }

    private void OnDestroy()
    {
        if (controller is BossZombieController bc)
            bc.OnCardinalReached -= HandleCardinalReached;
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
        // Phase 2: 브레스/솟구침(패턴2) 추가 — 한 번 해금되면 Phase3에서도 계속 유지되는 "추가" 패턴
        if (newPhase >= 2 && !pattern2Unlocked)
        {
            pattern2Unlocked = true; // 이후 방위 도달 신호(HandleCardinalReached)부터 실제로 발동
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

    #region Pattern 2
    /// <summary>보스가 플레이어 기준 동/북/서/남에 도달했을 때 호출됨.</summary>
    private void HandleCardinalReached(float angle)
    {
        if (!pattern2Unlocked || isPattern2Busy || !IsLive || controller == null) return;

        StartCoroutine(Pattern2Set());
    }

    /// <summary>
    /// 한 세트: 두 공격이 겹쳐 보이지 않도록 시간차를 둔다. (코루틴은 보스가 죽어 비활성화되면 자동 종료)
    ///  ① 브레스: 보스가 방위에 선 이 순간, 보스→플레이어 방향 직사각형 예고 → breathWarningDuration 후 발동.
    ///           예고~VFX가 끝날 때까지만 보스가 제자리에 선다.
    ///  ② GapBetweenPatterns 뒤, 솟구침: 보스는 다시 공전 중. 이 시점 플레이어 발밑 타원 예고 → warningDuration 후 발동.
    /// 브레스를 피해 옮긴 자리를 이어서 솟구침이 겨냥하는 연계 구조.
    /// </summary>
    private IEnumerator Pattern2Set()
    {
        isPattern2Busy = true;
        var bc = controller as BossZombieController;

        // 브레스: 지금 보스 위치에서 플레이어 방향으로 고정 
        Vector2 bossPos = transform.position;
        Vector2 dir = (Vector2)controller.TargetPlayer.position - bossPos;
        dir = dir.sqrMagnitude < 0.0001f ? Vector2.left : dir.normalized;

        if (bc != null) bc.SetOrbitPaused(true);

        var beamTelegraph = AttackTelegraph.CreateBeam(bossPos, dir, breathLength, breathWidth, breathWarningDuration, transform);
        beamTelegraph.OnComplete += origin => Breathe(origin, dir);

        // 예고 + VFX 재생이 끝날 때까지 보스는 제자리, 끝나면 다시 공전
        float vfxTime = (breathFrames != null && breathFrames.Length > 0) ? breathFrames.Length / VfxFps : 0.5f;
        yield return new WaitForSeconds(breathWarningDuration + vfxTime);
        if (bc != null) bc.SetOrbitPaused(false);

        yield return new WaitForSeconds(GapBetweenPatterns);
        if (!IsLive) yield break;

        //솟구침: 이 시점 플레이어 "발밑"을 스냅샷 (실시간 추적하면 피할 수 없음). 보스는 공전 계속.
        Vector2 eruptionCenter = (Vector2)controller.TargetPlayer.position + PlayerFootOffset;
        var eruptionTelegraph = AttackTelegraph.CreateEllipse(eruptionCenter, eruptionRadius, GroundSquash, warningDuration, transform);
        eruptionTelegraph.OnComplete += center => Erupt(center);

        yield return new WaitForSeconds(warningDuration);
        isPattern2Busy = false;
    }

    private void Erupt(Vector2 center)
    {
        if (eruptionFrames != null && eruptionFrames.Length > 0)
        {
            float ppu = eruptionFrames[0].pixelsPerUnit;
            float s = (eruptionRadius * 2f) / (EruptionPeakDiameterPx / ppu);
            SpriteFlipbook.Play(eruptionFrames, center, 0f, new Vector3(s, s, 1f), VfxFps);
        }

        var player = PlayerCharacter.Instance;
        if (player == null || player.IsDead) return;

        // 플레이어 발밑 예고 
        Vector2 feet = (Vector2)player.transform.position + PlayerFootOffset;
        float nx = (feet.x - center.x) / eruptionRadius;
        float ny = (feet.y - center.y) / (eruptionRadius * GroundSquash);
        if (nx * nx + ny * ny <= 1f)
            player.ApplyDamage(damage);
    }

    private void Breathe(Vector2 origin, Vector2 dir)
    {
        if (breathFrames != null && breathFrames.Length > 0)
        {
            float ppu = breathFrames[0].pixelsPerUnit;
            float sx = breathLength / (BreathPeakLengthPx / ppu);
            float sy = breathWidth / (BreathPeakThicknessPx / ppu);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            SpriteFlipbook.Play(breathFrames, origin, angle, new Vector3(sx, sy, 1f), VfxFps);
        }

        var player = PlayerCharacter.Instance;
        if (player == null || player.IsDead) return;

        // 예고 장판(직사각형)과 동일한 영역 안에 있으면 피해 — 보이는 것과 판정을 일치시킨다
        Vector2 rel = (Vector2)player.transform.position - origin;
        float along = Vector2.Dot(rel, dir);
        if (along < 0f || along > breathLength) return;
        float across = Vector2.Dot(rel, new Vector2(-dir.y, dir.x));
        if (Mathf.Abs(across) <= breathWidth * 0.5f)
            player.ApplyDamage(damage);
    }
    #endregion

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
