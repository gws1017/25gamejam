using System.Collections;
using UnityEngine;

/*****************************************************************************************
 * 파일: Burster.cs
 * 역할: 스테이지 2 근접 자폭 몬스터 — 접근 → 자폭 대기 → 4방향 X자 산개
 * 에디터 세팅:
 *   - bulletPoolKey: PoolManager에 등록한 키와 동일하게 입력
 *   - 프리팹 수치(기획): maxHP 8 / speed 2 / attackRange 1.2 / dropExp 15
 *   - 탄속은 Bullet 프리팹에서 설정한다(기획값 5)
 * 동작:
 *   1) 플레이어를 향해 이동 (AIController의 Move 상태)
 *   2) attackRange 도달 → AIController가 Attack 상태로 전환 → Attack() 호출
 *      = 자폭 대기 상태 진입. 이 시점부터 처치당해도 폭발한다
 *   3) armDuration 동안 팽창 + 점멸 (예고 장판 대신 이 연출이 "곧 터진다" 신호)
 *   4) 폭발: 반경 피격 판정 + 4방향 X자 탄 발사 + 자기 사망
 * 기획 결정:
 *   - 일반 몬스터에는 예고 장판(AttackTelegraph)을 쓰지 않는다. 예고는 보스 전용.
 *   - 접근 중 처치 → 폭발 없음 / 자폭 대기 중 처치 → 폭발 발동 (커밋된 공격)
 *****************************************************************************************/

public class Burster : MonsterCharacter
{
    [Header("Burster")]
    [SerializeField] private string bulletPoolKey = "BursterBullet";
    [SerializeField] private AudioClip explodeFx;
    [SerializeField] private GameObject explodeVFX;

    [Tooltip("자폭 대기 시간(초). 정지 → 폭발. 기획값 0.8")]
    [SerializeField] private float armDuration = 0.8f;
    [Tooltip("폭발 피격 반경. 기획값 1.5 (플레이어 폭 1.36보다 약간 큼 → 이동 강제)")]
    [SerializeField] private float explodeRadius = 1.5f;
    [Tooltip("폭발 시 발사할 탄 개수. 4 = X자")]
    [SerializeField] private int burstCount = 4;
    [Tooltip("첫 탄의 각도(도). 45면 X자 대각선(45/135/225/315)")]
    [SerializeField] private float burstStartAngle = 45f;

    [Header("자폭 대기 연출")]
    [Tooltip("점멸할 때 섞을 색")]
    [SerializeField] private Color armColor = new Color(1f, 0.55f, 0.1f);
    [Tooltip("점멸 간격(초). 대기 끝으로 갈수록 이 값보다 빨라진다")]
    [SerializeField] private float blinkInterval = 0.08f;
    [Tooltip("폭발 직전까지 부푸는 배율")]
    [SerializeField] private float maxScaleUp = 1.35f;

    private bool isArmed = false;      // 자폭 대기 상태에 진입했는가 (= 죽어도 터지는 상태)
    private bool hasExploded = false;  // 중복 폭발 방지

    private SpriteRenderer sr;
    private Color baseColor = Color.white;
    private Vector3 baseScale = Vector3.one;

    protected override void Awake()
    {
        base.Awake();

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;

        baseScale = transform.localScale;
    }

    /// <summary>풀에서 재사용되므로 스폰할 때마다 상태/연출을 되돌린다.</summary>
    public override void Spawn()
    {
        base.Spawn();
        RestoreVisual();
        isArmed = false;
        hasExploded = false;
    }

    public override void Attack()
    {
        if (isAttacking || hasExploded) return;
        base.Attack();

        // 자폭 대기 진입. AIController는 Attack 상태에서 이동하지 않으므로 자연히 정지한다.
        isAttacking = true;
        isArmed = true;
        StartCoroutine(ArmCoroutine());
    }

    public override void Die()
    {
        if (isLive == false) return;

        // 자폭 대기 중에 처치당한 경우 — 이미 발동한 공격이므로 폭발은 그대로 터진다
        if (isArmed && !hasExploded)
        {
            hasExploded = true;
            DoExplosion();
        }

        RestoreVisual();   // 풀로 돌아가기 전에 연출 원복
        base.Die();
    }

    #region Internal
    /// <summary>
    /// 자폭 대기 연출.
    /// 부풀기 + 점멸을 동시에 주고, 끝으로 갈수록 점멸을 빠르게 해서 임박했음을 알린다.
    /// </summary>
    private IEnumerator ArmCoroutine()
    {
        float elapsed = 0f;
        float nextBlink = 0f;
        bool blinkOn = false;

        while (elapsed < armDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / armDuration);

            transform.localScale = baseScale * Mathf.Lerp(1f, maxScaleUp, t);

            nextBlink -= Time.deltaTime;
            if (nextBlink <= 0f)
            {
                blinkOn = !blinkOn;
                if (sr != null) sr.color = blinkOn ? armColor : baseColor;
                // 처음엔 느리게, 폭발 직전엔 빠르게
                nextBlink = Mathf.Lerp(blinkInterval * 2f, blinkInterval * 0.4f, t);
            }

            yield return null;
        }

        if (hasExploded) yield break;
        hasExploded = true;

        DoExplosion();
        RestoreVisual();
        // 자폭이므로 AIController의 Dead 상태를 거치지 않는다 → 경험치는 지급되지 않음
        base.Die();
    }

    /// <summary>폭발 연출 + 피격 판정 + 방사탄. 사망 처리는 호출한 쪽에서 한다.</summary>
    private void DoExplosion()
    {
        if (explodeVFX != null)
            Instantiate(explodeVFX, transform.position, Quaternion.identity);

        if (explodeFx != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySoundFX(explodeFx);

        FireBurst();
        ApplyExplosionDamage();
    }

    /// <summary>burstCount 발을 균등 각도로 발사. 기본값(4, 45도)이면 X자가 된다.</summary>
    private void FireBurst()
    {
        if (PoolManager.Instance == null) return;

        Vector3 origin = transform.position;
        float step = 360f / Mathf.Max(1, burstCount); //발사 수만큼 각도를 나눔

        for (int i = 0; i < burstCount; i++)
        {
            float angle = burstStartAngle + step * i;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * Vector3.right;

            Bullet bullet = PoolManager.Instance.Spawn<Bullet>(origin, rot, bulletPoolKey);
            if (bullet == null) continue;

            bullet.Init(damage, gameObject);
            bullet.AddIgnoreObject(gameObject);
            bullet.Fire(dir);
        }
    }

    private void ApplyExplosionDamage()
    {
        var player = PlayerCharacter.Instance;
        if (player == null || player.IsDead) return;

        //반경 안인지 거리만 확인하여 피격 처리
        float dist = Vector2.Distance(player.transform.position, transform.position);
        if (dist <= explodeRadius)
            player.ApplyDamage(damage);
    }

    private void RestoreVisual()
    {
        transform.localScale = baseScale;
        if (sr != null) sr.color = baseColor;
    }
    #endregion

#if UNITY_EDITOR
    // 인스펙터에서 폭발 반경을 눈으로 확인하기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
#endif
}
