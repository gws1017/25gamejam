using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossAlienController))]
public class BossAlien : Boss
{
    // 각 개체는 상태만 보관한다. 공격 판단과 코루틴 실행은 모두 본체에서 처리한다.
    private class AlienState
    {
        public Transform target;
        public Sprite[] forms;
        public SpriteRenderer visual;
        public AlienForm form;
        public Vector2 home;
        public AttackTelegraph warning;
        public GameObject eruption;
    }

    private enum AlienForm { Basic, Tentacle, Dash, Shotgun }
    private static readonly Vector2 FootOffset = new Vector2(0f, -0.59f);
    [SerializeField] private Sprite[] blueForms;
    [SerializeField] private Sprite[] redForms;
    [SerializeField] private Vector2 arenaMin = new Vector2(-7f, -3.5f);
    [SerializeField] private Vector2 arenaMax = new Vector2(7f, 3.5f);
    [SerializeField] private GameObject partnerPrefab;
    [SerializeField] private AlienAttackSettings attackSettings = new AlienAttackSettings();

    private AlienState blue;
    private AlienState red;
    private GameObject partner;
    private bool joinPending; // 체력 조건을 만족하면 현재 공격 묶음 종료까지 합류를 예약한다.
    private bool started; // 스포너의 초기화 과정에서 공격 루프가 중복 시작되는 것을 막는다.

    public AlienAttackSettings AttackSettings => attackSettings;

    public override bool IsMeleeDamage => false;

    public Vector2 ArenaMin => arenaMin;
    public Vector2 ArenaMax => arenaMax;

    protected override void Awake()
    {
        base.Awake();
        phaseHpThresholds = new[] { 0.5f };
    }

    public override void Spawn()
    {
        base.Spawn();
        if (started)
        {
            return;
        }
        started = true;
        // 카메라 안에서 돌진 경로를 배치한다.
        var camera = Camera.main;
        if (camera != null && camera.orthographic)
        {
            Vector2 center = camera.transform.position;
            Vector2 extent = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
            arenaMin = center - extent + Vector2.one;
            arenaMax = center + extent - Vector2.one;
        }
        blue = CreateState(transform, blueForms, new Vector2((arenaMin.x + arenaMax.x) / 2f, arenaMax.y));
        blue.form = AlienForm.Basic;
        StartCoroutine(Battle(blue));
    }

    protected override void OnPhaseChanged(int phase)
    {
        if (phase >= 2 && partner == null)
        {
            joinPending = true;
        }
    }

    // 파랑의 공격과 장판 판정이 끝난 시점에 빨강을 합류시킨다.
    private void FinishBatch(AlienState actor)
    {
        if (!IsLive || !joinPending || actor != blue)
        {
            return;
        }
        joinPending = false;
        blue.home = new Vector2(Mathf.Lerp(arenaMin.x, arenaMax.x, 0.25f), arenaMax.y);
        blue.target.position = blue.home;
        partner = Instantiate(partnerPrefab);
        partner.GetComponent<ProjectileDamageRelay>().Initialize(this);
        red = CreateState(partner.transform, redForms, new Vector2(Mathf.Lerp(arenaMin.x, arenaMax.x, 0.75f), arenaMax.y));
        red.form = ChooseForm(red);
        StartCoroutine(Battle(red));
    }

    // 직전 폼과 상대의 금지 조합을 제외한다. 선택 후 대기 없이 현재 폼에 반영해야 한다.
    private AlienForm ChooseForm(AlienState actor)
    {
        var other = actor == blue ? red : blue;
        var candidates = new System.Collections.Generic.List<AlienForm>();
        for (int i = 0; i < 4; i++)
        {
            var form = (AlienForm)i;
            if (form == actor.form)
            {
                continue;
            }
            if (other != null && !Compatible(form, other.form))
            {
                continue;
            }
            candidates.Add(form);
        }
        return candidates[Random.Range(0, candidates.Count)];
    }

    // 같은 폼, 촉수+돌진은 동시에 사용할 수 없다.
    private static bool Compatible(AlienForm a, AlienForm b)
    {
        if (a == b)
        {
            return false;
        }
        return !((a == AlienForm.Tentacle && b == AlienForm.Dash) ||
                 (a == AlienForm.Dash && b == AlienForm.Tentacle));
    }

    // 공격 묶음을 끝까지 실행한 뒤 합류 확인과 다음 폼 선택을 진행한다.
    private IEnumerator Battle(AlienState actor)
    {
        while (IsLive && actor.target != null && actor.target.gameObject.activeInHierarchy)
        {
            actor.visual.sprite = actor.forms[(int)actor.form];
            actor.visual.flipX = false;
            var bounds = actor.visual.sprite.bounds;
            // 원본 이미지마다 크기가 달라 월드 높이를 동일하게 맞춘다.
            actor.target.localScale = Vector3.one * (1.8f / bounds.size.y);
            var hitbox = actor.target.GetComponent<BoxCollider2D>();
            hitbox.size = bounds.size * 0.75f;
            hitbox.offset = bounds.center;
            yield return new WaitForSeconds(attackSettings.formPause);
            switch (actor.form)
            {
                case AlienForm.Basic:
                    for (int i = 0; i < attackSettings.basicShots; i++)
                    {
                        Fire(actor, actor.target.position, PlayerPosition(actor));
                        yield return new WaitForSeconds(attackSettings.shotInterval);
                    }
                    break;
                case AlienForm.Shotgun:
                    for (int burst = 0; burst < attackSettings.shotgunBursts; burst++)
                    {
                        // 한 묶음의 조준점을 고정하고 왼쪽, 가운데, 오른쪽 눈 순서로 발사한다.
                        Vector2 target = PlayerPosition(actor);
                        for (int eye = -1; eye <= 1; eye++)
                        {
                            Fire(actor, (Vector2)actor.target.position + new Vector2(eye * 0.4f, -0.2f), target);
                            yield return new WaitForSeconds(attackSettings.eyeInterval);
                        }
                        yield return new WaitForSeconds(attackSettings.shotInterval);
                    }
                    break;
                case AlienForm.Tentacle:
                    yield return Tentacles(actor);
                    break;
                case AlienForm.Dash:
                    yield return Dash(actor);
                    break;
            }
            FinishBatch(actor);
            actor.form = ChooseForm(actor);
        }
    }

    private Vector2 PlayerPosition(AlienState actor)
    {
        var player = PlayerCharacter.Instance;
        return player != null ? (Vector2)player.transform.position : actor.home + Vector2.down;
    }

    private void Fire(AlienState actor, Vector2 origin, Vector2 target)
    {
        if (!IsLive || PoolManager.Instance == null)
        {
            return;
        }
        Vector2 direction = (target - origin).normalized;
        var bullet = PoolManager.Instance.Spawn<Bullet>(origin, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), "BossBullet");
        if (bullet == null)
        {
            return;
        }
        bullet.Init(damage, gameObject);
        bullet.AddIgnoreObject(actor.target.gameObject);
        bullet.Fire(direction);
    }

    private IEnumerator Tentacles(AlienState actor)
    {
        for (int i = 0; i < attackSettings.tentacleCount; i++)
        {
            // 매 솟구침마다 위치를 새로 잡되, 전조가 뜬 뒤에는 따라가지 않는다.
            Vector2 center = PlayerPosition(actor) + FootOffset;
            bool ready = false;
            actor.warning = AttackTelegraph.CreateEllipse(center, attackSettings.tentacleRadius, 0.35f, attackSettings.warningTime, transform);
            actor.warning.OnComplete += _ => ready = true;
            yield return new WaitUntil(() => ready || !IsLive);
            if (!IsLive)
            {
                yield break;
            }
            actor.warning = null;
            var player = PlayerCharacter.Instance;
            Vector2 offset = PlayerPosition(actor) + FootOffset - center;
            // 장판과 같은 타원 내부인지 확인한다.
            float nx = offset.x / attackSettings.tentacleRadius;
            float ny = offset.y / (attackSettings.tentacleRadius * 0.35f);
            if (player != null && nx * nx + ny * ny <= 1f)
            {
                player.ApplyDamage(damage);
            }
            // 전용 솟구침 시트가 준비될 때까지 촉수형 이미지를 사용한다.
            actor.eruption = new GameObject("AlienTentacle", typeof(SpriteRenderer));
            actor.eruption.transform.position = center + Vector2.up * 0.6f;
            var sprite = actor.eruption.GetComponent<SpriteRenderer>();
            sprite.sprite = actor.forms[(int)AlienForm.Tentacle];
            sprite.sortingOrder = 3;
            actor.eruption.transform.localScale = Vector3.one * (attackSettings.tentacleRadius * 2f / sprite.sprite.bounds.size.x);
            yield return new WaitForSeconds(0.35f);
            Destroy(actor.eruption);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Dash(AlienState actor)
    {
        // 전체 너비를 균등하게 나누고 왼쪽 또는 오른쪽 끝부터 한 줄씩 진행한다.
        int count = Mathf.Max(1, Mathf.CeilToInt((arenaMax.x - arenaMin.x) / attackSettings.laneWidth));
        float width = (arenaMax.x - arenaMin.x) / count;
        bool reverse = Random.value < 0.5f;
        for (int i = 0; i < count; i++)
        {
            int lane = reverse ? count - 1 - i : i;
            float x = arenaMin.x + (lane + 0.5f) * width;
            bool downward = i % 2 == 0;
            Vector2 start = new Vector2(x, downward ? arenaMax.y : arenaMin.y);
            Vector2 end = new Vector2(x, downward ? arenaMin.y : arenaMax.y);
            actor.target.position = start;
            bool ready = false;
            actor.warning = AttackTelegraph.CreateBeam(start, (end - start).normalized, Vector2.Distance(start, end), width, attackSettings.warningTime);
            actor.warning.OnComplete += _ => ready = true;
            yield return new WaitUntil(() => ready || !IsLive);
            if (!IsLive)
            {
                yield break;
            }
            actor.warning = null;
            bool isHit = false;
            while (Vector2.Distance(actor.target.position, end) > 0.01f)
            {
                Vector2 previous = actor.target.position;
                Vector2 next = Vector2.MoveTowards(previous, end, attackSettings.dashSpeed * Time.deltaTime);
                actor.target.position = next;
                Vector2 player = PlayerPosition(actor);
                // 프레임 사이에 통과한 구간도 검사한다. 한 줄에서는 한 번만 피해를 준다.
                if (!isHit && Mathf.Abs(player.x - x) <= width / 2f &&
                    player.y >= Mathf.Min(previous.y, next.y) - 0.4f && player.y <= Mathf.Max(previous.y, next.y) + 0.4f)
                {
                    if (PlayerCharacter.Instance != null)
                    {
                        PlayerCharacter.Instance.ApplyDamage(damage);
                    }
                    isHit = true;
                }
                yield return null;
            }
        }
        actor.target.position = actor.home;
    }
    private AlienState CreateState(Transform target, Sprite[] forms, Vector2 home)
    {
        var actor = new AlienState
        {
            target = target,
            forms = forms,
            home = home,
            visual = target.GetComponent<SpriteRenderer>()
        };
        target.position = home;
        actor.visual.sortingOrder = 2;
        return actor;
    }

    // 코루틴을 먼저 중단한 뒤 두 개체의 예약 장판과 임시 효과를 정리한다.
    private void CancelAttacks()
    {
        StopAllCoroutines();
        ClearEffects(blue);
        ClearEffects(red);
    }

    private void ClearEffects(AlienState actor)
    {
        if (actor == null) return;
        if (actor.warning != null) Destroy(actor.warning.gameObject);
        if (actor.eruption != null) Destroy(actor.eruption);
    }

    public override void Die()
    {
        CancelAttacks();
        if (partner != null) Destroy(partner);
        base.Die();
    }

    private void OnDisable()
    {
        CancelAttacks();
        if (partner != null) Destroy(partner);
    }
}