using System.Collections;
using UnityEngine;

/*****************************************************************************************
 * 파일: GreaterDevil.cs
 * 역할: 스테이지 3 엘리트 몬스터 — 대악마. 대부분의 시간을 공중에 떠 있다가 플레이어 위치로 내려찍는다.
 *       부유 → 전조(바닥 장판 + 이동) → 급강하 → 충격파 → 딜타임(스턴) → 상승 → 부유 …
 * 맞추기 어려운 이유:
 *   - 공중(발 기준 hoverHeight)에 떠 있으면 수평으로 나가는 스페이스 사격은 아래로 지나간다.
 *     마우스로 조준하는 로봇 정령 사격만 맞힌다. 따로 무적을 걸지 않고 "높이"가 곧 난이도다.
 *   - 내려찍고 착지한 딜타임(stunTime)에는 지면에 있어서 모든 공격이 맞는다.
 * 충격파 피해 (좀비 몸이 닿는 것과 같은 방식):
 *   - 착지 순간 전조 장판 크기의 트리거 콜라이더(stompColl)를 잠깐 켠다.
 *     플레이어가 그 콜라이더에 닿으면 PlayerCharacter의 기존 접촉 피해 경로를 그대로 탄다
 *     (몬스터 몸 접촉과 쿨타임을 공유하므로 몸과 충격파가 동시에 닿아도 한 번만 맞는다).
 *   - 플레이어 몸의 일부라도 빨간 장판 안에 들어 있으면 맞는다.
 *   - 땅 균열 + 흙먼지는 연출만 한다 (GroundSlamVFX).
 * 좌표 규칙:
 *   - transform.position = 발(피벗) 위치. groundPos(발 아래 지면 좌표) + altitude(지면에서 위로의 높이).
 *   - 지면 높이는 플레이어 발 높이를 따라간다 (플레이어는 좌우로만 움직인다).
 * 에디터 세팅:
 *   - Animator: GreaterDevil.controller (트리거 Fly / Windup / Dive / Land / Stun)
 *   - Rigidbody2D Kinematic, AIController 자리에 GreaterDevilController, spriteDir -1
 *   - shadow: 자식 그림자 오브젝트 (지면에 남고 높이에 따라 작아지고 흐려진다)
 *****************************************************************************************/

public class GreaterDevil : MonsterCharacter
{
    private static readonly int FlyTrigger = Animator.StringToHash("Fly");
    private static readonly int WindupTrigger = Animator.StringToHash("Windup");
    private static readonly int DiveTrigger = Animator.StringToHash("Dive");
    private static readonly int LandTrigger = Animator.StringToHash("Land");
    private static readonly int StunTrigger = Animator.StringToHash("Stun");

    private static readonly Vector2 PlayerFootOffset = new Vector2(0f, -0.59f);   // Player 프리팹 Shadow 위치 기준
    private const float GroundSquash = 0.35f;     // 바닥 장판 타원의 세로/가로 비율 (보스1 솟구침과 동일)
    private const float DeathFallTime = 0.4f;     // 공중에서 죽었을 때 떨어지는 시간
    private const float DeathFadeTime = 0.6f;     // 쓰러진 뒤 사라지는 시간

    [Header("Greater Devil — 부유")]
    [Tooltip("공중 부유 높이 (발 기준, 지면에서 위로). 높을수록 스페이스 사격이 확실히 비켜 간다")]
    [SerializeField] private float hoverHeight = 2.4f;
    [Tooltip("한 번 내려찍기 전까지 공중에서 떠 있는 시간(초)")]
    [SerializeField] private float hoverTime = 4f;
    [Tooltip("공중에서 플레이어 쪽으로 따라가는 속도 (월드 유닛/초)")]
    [SerializeField] private float hoverSpeed = 2.2f;
    [Tooltip("공중에서 위아래로 일렁이는 폭(월드 유닛)")]
    [SerializeField] private float bobAmp = 0.15f;
    [Tooltip("공중에서 위아래로 일렁이는 빠르기 (초당 횟수)")]
    [SerializeField] private float bobSpeed = 0.6f;
    [Tooltip("스폰 위치에서 플레이어 위 공중까지 날아오는 시간(초)")]
    [SerializeField] private float enterTime = 1.6f;

    [Header("Greater Devil — 내려찍기")]
    [Tooltip("전조 장판이 차오르는 시간(초). 이 동안 플레이어가 장판 밖으로 피할 수 있다 (플레이어 속도 4)")]
    [SerializeField] private float warnTime = 1f;
    [Tooltip("급강하에 걸리는 시간(초)")]
    [SerializeField] private float diveTime = 0.22f;
    [Tooltip("착지 직후 웅크린 채 있는 시간(초) — 스턴 자세로 넘어가기 전")]
    [SerializeField] private float landTime = 0.25f;
    [Tooltip("착지 후 축 처져 있는 시간(초) — 딜타임. 지면에 있어서 모든 공격이 맞는다")]
    [SerializeField] private float stunTime = 1.5f;
    [Tooltip("스턴 후 다시 부유 높이까지 떠오르는 시간(초)")]
    [SerializeField] private float riseTime = 0.6f;
    [Tooltip("내려찍기 피해 범위의 가로 반지름 (월드 유닛). 전조 장판, 피해 콜라이더, 링 크기가 모두 이 값을 쓴다")]
    [SerializeField] private float stompRadius = 2.4f;
    [Tooltip("착지 순간 피해 콜라이더를 켜 두는 시간(초). 물리 갱신이 한 번은 돌 만큼은 필요하다")]
    [SerializeField] private float stompActiveTime = 0.1f;

    [Header("Greater Devil — 충격 연출 (피해 판정 없음)")]
    [Tooltip("땅 균열 4단계 (groundCrack_0~3, 갈라지는 순서)")]
    [SerializeField] private Sprite[] crackFrames;
    [Tooltip("흙먼지 6프레임 (groundDust_0~5)")]
    [SerializeField] private Sprite[] dustFrames;

    [Header("Greater Devil — 그림자")]
    [Tooltip("바닥 그림자(자식 오브젝트). 지면에 남고 높이에 따라 작아지고 흐려진다")]
    [SerializeField] private Transform shadow;

    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D bodyColl;
    private BoxCollider2D stompColl;       // 내려찍기 피해 범위 (전조 장판 크기). 착지 순간에만 켠다
    private SpriteRenderer shadowSr;
    private Vector3 baseScale;
    private Vector3 shadowBaseScale;
    private Vector2 groundPos;     // 발 아래 지면 좌표
    private float altitude;        // 지면에서 위로의 높이 (스폰 직후엔 음수일 수 있다)

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        bodyColl = GetComponent<BoxCollider2D>();
        BuildStompColl();
        baseScale = transform.localScale;
        if (shadow != null)
        {
            shadowSr = shadow.GetComponent<SpriteRenderer>();
            shadowBaseScale = shadow.localScale;
        }
    }

    /// <summary>스폰(풀 재사용 포함)마다 Spawner의 PowerUp에서 호출 → 상태를 초기화하고 전투 시작.</summary>
    public override void Spawn()
    {
        base.Spawn();
        StopAllCoroutines();
        ResetState();
        StartCoroutine(Loop());
    }

    /// <summary>
    /// 피해 범위 콜라이더를 만든다: 전조 장판(납작한 타원)이 차지하는 가로 2R × 세로 2R·GroundSquash 상자.
    /// 플레이어는 좌우로만 움직이고 장판 중심 줄에 서 있어서, 타원과 상자의 도달 거리는 같다.
    /// 반지름이 인스펙터 값(stompRadius)을 따라가도록 에디터가 아니라 시작할 때 만든다.
    /// 발(피벗)이 장판 중심에 착지하므로 로컬 원점이 상자 중심이다.
    /// </summary>
    private void BuildStompColl()
    {
        stompColl = gameObject.AddComponent<BoxCollider2D>();
        stompColl.isTrigger = true;
        stompColl.size = new Vector2(stompRadius * 2f, stompRadius * 2f * GroundSquash);
        stompColl.enabled = false;
    }

    /// <summary>
    /// 애니메이션 상태를 바꾼다. Any State 전환이라 이미 그 상태일 때 켠 트리거는 소비되지 않고 남아서,
    /// 나중에 엉뚱한 상태로 튀게 만든다. 그래서 새로 켜기 전에 전부 지운다.
    /// </summary>
    private void Play(int trigger)
    {
        anim.ResetTrigger(FlyTrigger);
        anim.ResetTrigger(WindupTrigger);
        anim.ResetTrigger(DiveTrigger);
        anim.ResetTrigger(LandTrigger);
        anim.ResetTrigger(StunTrigger);
        anim.SetTrigger(trigger);
    }

    private void ResetState()
    {
        anim.ResetTrigger(FlyTrigger);
        anim.ResetTrigger(WindupTrigger);
        anim.ResetTrigger(DiveTrigger);
        anim.ResetTrigger(LandTrigger);
        anim.ResetTrigger(StunTrigger);
        transform.localScale = baseScale;
        if (sr != null) sr.color = Color.white;
        if (bodyColl != null) bodyColl.enabled = true;
        if (stompColl != null) stompColl.enabled = false;
        if (shadowSr != null) shadowSr.color = Color.white;

        Vector2 feet = PlayerFeet();
        groundPos = new Vector2(transform.position.x, feet.y);
        altitude = transform.position.y - groundPos.y;   // 스폰 위치가 플레이어 행보다 위/아래일 수 있다
        Apply();
    }

    #region Battle Loop
    private IEnumerator Loop()
    {
        yield return Enter();
        while (isLive)
        {
            yield return Hover(hoverTime);
            yield return Stomp();
            yield return Rise();
        }
    }

    /// <summary>스폰 위치에서 플레이어 위 공중으로 날아온다.</summary>
    private IEnumerator Enter()
    {
        Play(FlyTrigger);
        Vector2 startGround = groundPos;
        float startAltitude = altitude;
        for (float t = 0f; t < enterTime && isLive; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / enterTime);
            Vector2 feet = PlayerFeet();
            groundPos = new Vector2(Mathf.Lerp(startGround.x, feet.x, k), Mathf.Lerp(startGround.y, feet.y, k));
            altitude = Mathf.Lerp(startAltitude, hoverHeight, k);
            Apply();
            yield return null;
        }
    }

    /// <summary>공중에서 플레이어를 느리게 따라다닌다. 위아래로 살짝 일렁인다.</summary>
    private IEnumerator Hover(float duration)
    {
        for (float t = 0f; t < duration && isLive; t += Time.deltaTime)
        {
            Vector2 feet = PlayerFeet();
            groundPos.x = Mathf.MoveTowards(groundPos.x, feet.x, hoverSpeed * Time.deltaTime);
            groundPos.y = Mathf.MoveTowards(groundPos.y, feet.y, hoverSpeed * Time.deltaTime);
            altitude = hoverHeight + Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmp;
            Apply();
            yield return null;
        }
    }

    /// <summary>전조 → 급강하 → 충격 → 착지 → 스턴(딜타임).</summary>
    private IEnumerator Stomp()
    {
        // 이 순간의 플레이어 발밑으로 고정한다. 장판 자체는 따라오지 않는다 (따라오면 피할 수 없다)
        Vector2 target = PlayerFeet();
        AttackTelegraph.CreateEllipse(target, stompRadius, GroundSquash, warnTime, transform);   // watch: 죽으면 장판도 사라진다
        Play(WindupTrigger);

        // 전조 동안 목표 위로 이동 (끝날 때 정확히 위에 도착)
        Vector2 from = groundPos;
        for (float t = 0f; t < warnTime && isLive; t += Time.deltaTime)
        {
            groundPos = Vector2.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / warnTime));
            altitude = hoverHeight + Mathf.Sin(t * 40f) * 0.03f;   // 벼르는 듯한 떨림
            Apply();
            yield return null;
        }
        if (!isLive) yield break;

        Play(DiveTrigger);
        float startAltitude = altitude;
        for (float t = 0f; t < diveTime; t += Time.deltaTime)
        {
            float k = t / diveTime;
            altitude = Mathf.Lerp(startAltitude, 0f, k * k);   // 점점 빨라진다
            groundPos = target;
            Apply();
            yield return null;
        }
        altitude = 0f;
        groundPos = target;
        Apply();

        Impact(target);
        Play(LandTrigger);
        yield return new WaitForSeconds(landTime);
        Play(StunTrigger);
        yield return new WaitForSeconds(stunTime);
    }

    /// <summary>땅 균열 + 흙먼지 연출, 피해 콜라이더를 잠깐 켠다 (닿은 플레이어는 기존 접촉 피해 경로로 맞는다).</summary>
    private void Impact(Vector2 center)
    {
        GroundSlamVFX.Play(center, stompRadius, crackFrames, dustFrames);
        StartCoroutine(StompHitRoutine());
    }

    private IEnumerator StompHitRoutine()
    {
        stompColl.enabled = true;
        yield return new WaitForSeconds(stompActiveTime);
        stompColl.enabled = false;
    }

    /// <summary>스턴에서 다시 부유 높이까지 떠오른다. 웅크렸다 튀어 오르는 모양을 크기로 연출한다.</summary>
    private IEnumerator Rise()
    {
        Play(FlyTrigger);
        float startAltitude = altitude;
        for (float t = 0f; t < riseTime && isLive; t += Time.deltaTime)
        {
            float k = t / riseTime;
            altitude = Mathf.Lerp(startAltitude, hoverHeight, Mathf.SmoothStep(0f, 1f, k));

            float crouch = Mathf.Max(0f, 1f - k / 0.2f);   // 처음 20%: 웅크림 1 → 0
            float stretch = Mathf.Sin(k * Mathf.PI);       // 올라가는 동안 위로 살짝 늘어남
            transform.localScale = new Vector3(baseScale.x * (1f + 0.08f * crouch - 0.06f * stretch),
                                               baseScale.y * (1f - 0.15f * crouch + 0.12f * stretch),
                                               baseScale.z);
            Apply();
            yield return null;
        }
        transform.localScale = baseScale;
    }
    #endregion

    #region Death
    /// <summary>
    /// 경험치/골드/킬 카운트는 Die에서 이미 처리됐다. 공중에서 죽었으면 먼저 떨어지고, 쓰러진 자세로 사라진다.
    /// 죽는 동안 isLive가 false라 이동·공격·피격은 없다.
    /// </summary>
    protected override void Disappear()
    {
        StopAllCoroutines();
        if (!isActiveAndEnabled)
        {
            base.Disappear();
            return;
        }
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (bodyColl != null) bodyColl.enabled = false;   // 쓰러지는 동안 접촉 피해 없음 (재사용 시 AIController가 다시 켠다)
        if (stompColl != null) stompColl.enabled = false;
        Play(StunTrigger);                     // 쓰러진 자세 (스턴 이미지 재사용)

        float startAltitude = altitude;
        if (startAltitude > 0.05f)
        {
            for (float t = 0f; t < DeathFallTime; t += Time.deltaTime)
            {
                float k = t / DeathFallTime;
                altitude = Mathf.Lerp(startAltitude, 0f, k * k);
                Apply();
                yield return null;
            }
        }
        altitude = 0f;
        Apply();

        for (float t = 0f; t < DeathFadeTime; t += Time.deltaTime)
        {
            float alpha = 1f - t / DeathFadeTime;
            if (sr != null) sr.color = new Color(1f, 1f, 1f, alpha);
            if (shadowSr != null) shadowSr.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        base.Disappear();   // 풀 반환 (다음 Spawn에서 색과 크기를 되돌린다)
    }
    #endregion

    #region Helpers
    /// <summary>groundPos + altitude를 실제 위치로 반영하고, 그림자를 지면에 맞춘다.</summary>
    private void Apply()
    {
        transform.position = new Vector3(groundPos.x, groundPos.y + altitude, transform.position.z);

        if (shadow == null) return;
        float h = Mathf.Clamp01(Mathf.Max(0f, altitude) / Mathf.Max(0.01f, hoverHeight));   // 0 지면 ~ 1 부유 높이
        shadow.position = new Vector3(groundPos.x, groundPos.y, shadow.position.z);
        shadow.localScale = shadowBaseScale * Mathf.Lerp(1f, 0.5f, h);
        if (shadowSr != null && sr != null && sr.color.a >= 1f)   // 죽어서 페이드 중에는 건드리지 않는다
            shadowSr.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.5f, h));
    }

    private static Vector2 PlayerFeet()
    {
        var player = PlayerCharacter.Instance;
        Vector2 position = player != null ? (Vector2)player.transform.position : Vector2.zero;
        return position + PlayerFootOffset;
    }
    #endregion
}
