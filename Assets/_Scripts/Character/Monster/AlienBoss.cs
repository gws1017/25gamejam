using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*****************************************************************************************
 * 파일: AlienBoss.cs (+ AlienBoss.Basic / Shotgun / Tentacle / Dash.cs)
 * 역할: 스테이지 2 보스 — 4가지 폼을 오가는 외계인. 체력 50%부터 파트너(빨강)가 합류해
 *       두 마리가 서로 다른 폼으로 동시에 공격한다. 기획서: STAGE2_BOSS_SPEC.md
 * 구조:
 *   - 체력·사망·스테이지 판정은 이 본체(파랑)만 가진다. 파트너는 MonsterCharacter가 아니라
 *     DamageRelay로 맞은 피해를 본체에 넘기기만 한다 → 체력바 1개, 스테이지는 한 번만 넘어감.
 *   - 두 마리 모두 이 클래스의 코루틴이 움직인다(AlienBody = 한 마리의 상태).
 *   - 폼별 공격은 partial 파일로 나눠 각 파일에서 한 폼만 보면 되게 했다.
 * 프리팹: 본체(파랑) _Prefabs/Character/BossAlienA, 파트너(빨강) BossAlienB. Spawner bossPrefabs[1]에 A 등록.
 * 에디터 세팅 (본체 프리팹):
 *   - SpriteRenderer / Rigidbody2D(Kinematic) / BoxCollider2D(Is Trigger) / AlienBossController / 이 컴포넌트
 *   - 태그 Enemy, Phase Hp Thresholds = {0.5}
 *   - blueForms / redForms: 폼 순서대로 4장 (0 기본형, 1 촉수형, 2 돌진형, 3 산탄형) = b1~b4 / r1~r4
 *   - partnerPrefab: SpriteRenderer / BoxCollider2D(Is Trigger) / Rigidbody2D(Kinematic) / DamageRelay, 태그 Enemy
 *****************************************************************************************/

public partial class AlienBoss : Boss
{
    public enum AlienForm { Basic = 0, Tentacle = 1, Dash = 2, Shotgun = 3 }

    /// <summary>보스 한 마리(파랑 또는 빨강)의 상태.</summary>
    private class AlienBody
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public BoxCollider2D hitbox;
        public Sprite[] formSprites;
        public string bulletPoolKey;         // 몸마다 탄 색이 다름 (파랑/빨강)
        public Sprite[] tentacleFrames;      // 몸마다 촉수 색이 다름 (파랑/빨강)
        public Vector2 home;                // 공격 사이에 돌아가 있는 대기 위치 (화면 위쪽)
        public AlienForm? currentForm;       // 지금 쓰는 폼 (상대 폼 선택 시 조합 금지 판단에 쓰임)
        public AlienForm? lastForm;          // 직전 폼 (같은 폼 연속 방지)
        public readonly List<GameObject> spawnedWarnings = new List<GameObject>(); // 사망 시 정리할 예고 장판
    }

    private static readonly AlienForm[] AllForms = (AlienForm[])Enum.GetValues(typeof(AlienForm));
    private static readonly Vector2 PlayerFootOffset = new Vector2(0f, -0.59f); // Player 프리팹 Shadow 위치 기준
    private const float GroundSquash = 0.35f;   // 바닥 장판 타원의 세로/가로 비율 (보스1 솟구침과 동일)
    private const float ArenaMargin = 1f;       // 카메라 가장자리에서 안쪽으로 띄울 여백

    [Header("Alien Boss")]
    [SerializeField] private Sprite[] blueForms = new Sprite[4];
    [SerializeField] private Sprite[] redForms = new Sprite[4];
    [SerializeField] private GameObject partnerPrefab;
    [SerializeField] private AlienBossSettings settings = new AlienBossSettings();

    private AlienBody blue;
    private AlienBody red;
    private GameObject partner;
    private bool battleStarted = false;
    private bool partnerJoinPending = false;   // 50%를 넘으면 예약 → 파랑이 지금 폼을 마친 뒤 합류

    // 카메라를 못 읽을 때만 쓰는 기본 범위. 프리팹은 런타임에 생성되므로 씬 오브젝트를 참조할 수 없어 카메라로 계산한다.
    // view = 화면 전체, arena = 화면에서 ArenaMargin만큼 안쪽 (대기 위치 좌우 배치용)
    private Vector2 viewMin = new Vector2(-9.6f, -5.4f);
    private Vector2 viewMax = new Vector2(9.6f, 5.4f);
    private Vector2 arenaMin = new Vector2(-8.6f, -4.4f);
    private Vector2 arenaMax = new Vector2(8.6f, 4.4f);

    #region Lifecycle
    public override void Spawn()
    {
        base.Spawn();

        // Spawner의 초기화 과정에서 Spawn이 여러 번 불려도 전투는 한 번만 시작
        if (battleStarted) return;
        battleStarted = true;

        CalculateArena();

        blue = CreateBody(transform, blueForms, settings.blueBulletPoolKey, settings.tentacleFrames, TopPoint(0.5f));
        StartCoroutine(BattleLoop(blue));
    }

    protected override void OnPhaseChanged(int newPhase)
    {
        if (newPhase >= 2 && partner == null)
            partnerJoinPending = true;
    }

    public override void Die()
    {
        CancelEverything();
        base.Die();
    }

    private void OnDisable()
    {
        CancelEverything();
    }

    /// <summary>진행 중인 공격·예고 장판·파트너를 모두 정리한다.</summary>
    private void CancelEverything()
    {
        StopAllCoroutines();
        ClearWarnings(blue);
        ClearWarnings(red);
        if (partner != null) Destroy(partner);
        SetVisible(blue, 1f, true);   // 순간이동 도중에 끊겨도 본체가 투명한 채로 남지 않게
    }

    private static void ClearWarnings(AlienBody body)
    {
        if (body == null) return;
        foreach (var warning in body.spawnedWarnings)
            if (warning != null) Destroy(warning);
        body.spawnedWarnings.Clear();
    }
    #endregion

    #region Battle Loop
    /// <summary>
    /// 한 마리의 전투 순서: 대기 위치로 순간이동 → 폼 선택 → 이미지 교체 → 짧은 빈틈 → 폼 공격(정해진 횟수) → 반복.
    /// 폼 공격이 끝나야 다음 폼을 고르므로 공격 도중에 폼이 바뀌지 않는다 (공격 횟수 기준 전환).
    /// </summary>
    private IEnumerator BattleLoop(AlienBody body)
    {
        while (IsLive)
        {
            yield return TeleportTo(body, body.home);

            AlienForm form = ChooseForm(body);
            ApplyFormVisual(body, form);
            yield return new WaitForSeconds(settings.formSwitchPause);

            yield return RunForm(body, form);

            body.lastForm = form;
            body.currentForm = null;

            if (body == blue) TryJoinPartner();
        }
    }

    private IEnumerator RunForm(AlienBody body, AlienForm form)
    {
        switch (form)
        {
            case AlienForm.Basic:    yield return Basic(body);     break;   // AlienBoss.Basic.cs
            case AlienForm.Tentacle: yield return Tentacle(body);  break;   // AlienBoss.Tentacle.cs
            case AlienForm.Dash:     yield return Dash(body);      break;   // AlienBoss.Dash.cs
            case AlienForm.Shotgun:  yield return Shotgun(body);   break;   // AlienBoss.Shotgun.cs
        }
    }

    /// <summary>
    /// 다음 폼 선택 (두 마리의 조합 규칙은 전부 여기 한 곳에서 판단).
    /// - 본체의 첫 폼은 기본형
    /// - 직전 폼 제외
    /// - 상대가 지금 쓰는 폼과 같은 폼 금지
    /// 고르는 즉시 currentForm에 기록 → 상대가 바로 이어서 골라도 규칙이 지켜진다.
    /// </summary>
    private AlienForm ChooseForm(AlienBody body)
    {
        if (body == blue && body.lastForm == null)
        {
            body.currentForm = AlienForm.Basic;
            return AlienForm.Basic;
        }

        AlienBody other = (body == blue) ? red : blue;
        AlienForm? otherForm = other?.currentForm;

        var candidates = new List<AlienForm>(AllForms.Length);
        foreach (var form in AllForms)
        {
            if (form == body.lastForm) continue;
            if (otherForm.HasValue && !CanUseTogether(form, otherForm.Value)) continue;
            candidates.Add(form);
        }

        // 폼이 4개라 후보가 비는 일은 없지만, 만약을 대비해 탄막 폼으로 대체
        if (candidates.Count == 0)
            candidates.Add(otherForm == AlienForm.Basic ? AlienForm.Shotgun : AlienForm.Basic);

        AlienForm chosen = candidates[Random.Range(0, candidates.Count)];
        body.currentForm = chosen;
        return chosen;
    }

    /// <summary>
    /// 같은 폼 동시 금지만 남김. (촉수형+돌진형 금지는 해제 — 돌진 피해가 장판이 아니라 보스 몸 접촉이라
    /// 이미 지나간 줄로 피하면 둘 다 피할 수 있고, 금지하면 긴 돌진 동안 촉수형이 거의 안 나왔음)
    /// </summary>
    private static bool CanUseTogether(AlienForm a, AlienForm b) => a != b;
    #endregion

    #region Partner (Phase 2)
    /// <summary>
    /// 50%를 넘은 뒤 파랑이 폼을 마친 시점에 호출 — 빨강을 대기 위치에 투명하게 만들어 나타나게 하고, 둘을 좌우로 벌린다.
    /// (나중에 "빛기둥에서 내려오는 등장 + 그동안 공격 정지·무적" 연출을 넣을 자리)
    /// </summary>
    private void TryJoinPartner()
    {
        if (!partnerJoinPending || partner != null || !IsLive) return;
        partnerJoinPending = false;

        if (partnerPrefab == null)
        {
            Debug.LogWarning("[AlienBoss] partnerPrefab이 비어 있어 2페이즈 파트너가 등장하지 않습니다.");
            return;
        }

        blue.home = TopPoint(0.25f);   // 파랑은 다음 루프에서 좌상단으로 순간이동

        Vector2 redHome = TopPoint(0.75f);
        partner = Instantiate(partnerPrefab, redHome, Quaternion.identity);

        var relay = partner.GetComponent<DamageRelay>();
        if (relay != null) relay.SetTarget(this);
        else Debug.LogWarning("[AlienBoss] partnerPrefab에 DamageRelay가 없어 파트너를 때려도 피해가 들어가지 않습니다.");

        bool hasRedTentacle = settings.redTentacleFrames != null && settings.redTentacleFrames.Length > 0;
        red = CreateBody(partner.transform, redForms, settings.redBulletPoolKey,
                         hasRedTentacle ? settings.redTentacleFrames : settings.tentacleFrames, redHome);
        ApplyFormVisual(red, AlienForm.Basic); // 나타날 때 보일 이미지
        SetVisible(red, 0f, false);            // 투명 상태로 생성 → 첫 루프의 TeleportTo에서 나타남
        StartCoroutine(BattleLoop(red));
    }
    #endregion

    #region Helpers
    private AlienBody CreateBody(Transform target, Sprite[] forms, string bulletPoolKey, Sprite[] tentacleFrames, Vector2 home)
    {
        var body = new AlienBody
        {
            transform = target,
            renderer = target.GetComponent<SpriteRenderer>(),
            hitbox = target.GetComponent<BoxCollider2D>(),
            formSprites = forms,
            bulletPoolKey = bulletPoolKey,
            tentacleFrames = tentacleFrames,
            home = home,
        };
        if (body.renderer != null) body.renderer.sortingOrder = 2;
        return body;
    }

    /// <summary>폼 이미지로 교체하고, 이미지마다 크기가 달라서 화면상 높이와 피격 범위를 다시 맞춘다.</summary>
    private void ApplyFormVisual(AlienBody body, AlienForm form)
    {
        if (body.renderer == null || body.formSprites == null) return;

        int index = (int)form;
        if (index >= body.formSprites.Length || body.formSprites[index] == null) return;

        Sprite sprite = body.formSprites[index];
        body.renderer.sprite = sprite;

        Bounds bounds = sprite.bounds;
        if (bounds.size.y > 0f)
            body.transform.localScale = Vector3.one * (settings.bodyHeight / bounds.size.y);

        if (body.hitbox != null)
        {
            body.hitbox.size = bounds.size * 0.75f;   // 이미지 여백만큼 살짝 작게
            body.hitbox.offset = bounds.center;
        }
    }

    private IEnumerator MoveTo(AlienBody body, Vector2 destination, float speed)
    {
        while (body.transform != null && Vector2.Distance(body.transform.position, destination) > 0.01f)
        {
            body.transform.position = Vector2.MoveTowards(body.transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }
        if (body.transform != null) body.transform.position = destination;
    }

    /// <summary>
    /// 순간이동: 사라짐 → 위치 이동 → 나타남. 사라지기 시작할 때부터 다 나타날 때까지 피격·접촉 판정을 끈다.
    /// 이미 투명한 몸(막 생성된 파트너)은 사라지는 단계를 건너뛰고, 이미 그 자리에 보이는 상태면 아무것도 안 한다.
    /// 나중에 입자 분해/재구성 이펙트는 두 Fade 자리에 넣는다 (폼 체인지에도 재사용 예정).
    /// </summary>
    private IEnumerator TeleportTo(AlienBody body, Vector2 destination) => TeleportTo(body, destination, settings.teleportFadeTime);

    /// <param name="fadeTime">사라지는 시간 = 나타나는 시간. 돌진 줄 사이 이동은 더 짧게 쓴다.</param>
    private IEnumerator TeleportTo(AlienBody body, Vector2 destination, float fadeTime)
    {
        if (body.transform == null) yield break;

        bool visible = body.renderer == null || body.renderer.color.a > 0f;
        bool alreadyThere = ((Vector2)body.transform.position - destination).sqrMagnitude < 0.0001f;
        if (visible && alreadyThere) yield break;

        if (body.hitbox != null) body.hitbox.enabled = false;
        if (visible) yield return Fade(body, 0f, fadeTime);   // ← 입자 분해 이펙트 자리

        if (body.transform == null) yield break;
        body.transform.position = destination;

        yield return Fade(body, 1f, fadeTime);                 // ← 입자 재구성 이펙트 자리
        if (body.hitbox != null) body.hitbox.enabled = true;
    }

    private IEnumerator Fade(AlienBody body, float targetAlpha, float duration)
    {
        if (body.renderer == null) yield break;

        float startAlpha = body.renderer.color.a;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (body.renderer == null) yield break;
            SetVisible(body, Mathf.Lerp(startAlpha, targetAlpha, t / duration), null);
            yield return null;
        }
        SetVisible(body, targetAlpha, null);
    }

    /// <summary>몸의 투명도를 바꾼다. hitboxEnabled를 주면 피격 판정도 같이 켜고 끈다.</summary>
    private static void SetVisible(AlienBody body, float alpha, bool? hitboxEnabled)
    {
        if (body == null) return;
        if (body.renderer != null)
        {
            Color color = body.renderer.color;
            color.a = alpha;
            body.renderer.color = color;
        }
        if (hitboxEnabled.HasValue && body.hitbox != null) body.hitbox.enabled = hitboxEnabled.Value;
    }

    private void Fire(AlienBody body, Vector2 origin, Vector2 target)
    {
        if (!IsLive || PoolManager.Instance == null) return;

        Vector2 direction = target - origin;
        direction = direction.sqrMagnitude < 0.0001f ? Vector2.down : direction.normalized;
        Quaternion rotation = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward);

        Bullet bullet = PoolManager.Instance.Spawn<Bullet>(origin, rotation, body.bulletPoolKey);
        if (bullet == null) return;

        bullet.Init(damage, gameObject);                     // 사수는 본체 (빨강이 쏴도 Enemy 태그라 아군 오사 없음)
        bullet.AddIgnoreObject(body.transform.gameObject);
        bullet.Fire(direction);
    }

    private static Vector2 PlayerPosition()
    {
        var player = PlayerCharacter.Instance;
        return player != null ? (Vector2)player.transform.position : Vector2.zero;
    }

    private static Vector2 PlayerFeet() => PlayerPosition() + PlayerFootOffset;

    private void CalculateArena()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        Vector2 center = cam.transform.position;
        Vector2 extent = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        viewMin = center - extent;
        viewMax = center + extent;
        arenaMin = viewMin + Vector2.one * ArenaMargin;
        arenaMax = viewMax - Vector2.one * ArenaMargin;
    }

    /// <summary>화면 높이 비율(0 맨 아래 ~ 1 맨 위)을 월드 y로.</summary>
    private float ViewportY(float ratio) => Mathf.Lerp(viewMin.y, viewMax.y, ratio);

    /// <summary>대기 높이(settings.homeViewportY)에서 가로 비율(0 왼쪽 ~ 1 오른쪽) 지점.</summary>
    private Vector2 TopPoint(float xRatio) => new Vector2(Mathf.Lerp(arenaMin.x, arenaMax.x, xRatio), ViewportY(settings.homeViewportY));
    #endregion
}
