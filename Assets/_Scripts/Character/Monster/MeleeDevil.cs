using System.Collections;
using UnityEngine;

/*****************************************************************************************
 * 파일: MeleeDevil.cs
 * 역할: 스테이지 3 근접 몬스터(도끼 악마 / 창병 악마) 공용 클래스.
 *       접근 → 공격 준비 → 타격(이때만 패링 가능) → 마무리 → 쿨타임 → 다시 접근.
 *       패링에 성공했을 때만 뒤로 넉백 + 스턴된다. 총에 맞아도 넉백/스턴/공격 끊김은 없다.
 * 타이밍은 Attack 클립의 애니메이션 이벤트가 정한다 (코드에 준비/판정/마무리 시간 없음):
 *   - OnParryWindowStart : 무기 판정 활성화 → 여기서부터 패링 가능 (이 구간엔 피해 없음)
 *   - OnStrikeImpact     : 무기가 꽂히는 순간. 패링 구간이 끝나고, 플레이어가 범위 안이면 피해 1
 *                          (구간 시작이 아니라 끝에 피해를 넣어야 내려찍는 그림을 보고 패링할 시간이 생긴다)
 *   - OnAttackEnd        : 공격 동작 끝. 쿨타임(attackCoolTime)을 기다린 뒤 다시 접근
 * 에디터 세팅:
 *   - Animator: MeleeDevil.controller (창병은 SpearDevil.overrideController)
 *     트리거 Attack / Stagger / Move / Idle, 상태 Idle(기본) / Attack / Stagger
 *   - strike: 자식 오브젝트의 MeleeStrikeHitbox
 *   - AIController.spriteDir = -1 (이미지가 오른쪽을 본다)
 * 몸에 닿았을 때의 피해는 다른 몹과 같은 규칙(PlayerCharacter의 접촉 피해)을 그대로 따른다.
 * 악마는 사거리에서 멈추므로 평소엔 닿지 않는다.
 *****************************************************************************************/

public class MeleeDevil : MonsterCharacter
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int StaggerTrigger = Animator.StringToHash("Stagger");
    private static readonly int MoveTrigger = Animator.StringToHash("Move");
    private static readonly int DeathTrigger = Animator.StringToHash("Death");
    private const float DeathDisappearDelay = 3f;   // Death 클립 이벤트가 안 오더라도 이 시간 뒤엔 치운다

    [Header("Melee Devil")]
    [SerializeField] private MeleeStrikeHitbox strike;

    [Tooltip("패링당했을 때 뒤로 밀려나는 거리 (월드 유닛)")]
    [SerializeField] private float knockbackDistance = 1.5f;
    [Tooltip("패링당했을 때 밀려나는 데 걸리는 시간(초) — 넉백 연출")]
    [SerializeField] private float knockbackTime = 0.2f;
    [Tooltip("밀려난 뒤 멈춰 있는 시간(초) — 패링 보상 딜타임")]
    [SerializeField] private float stunTime = 0.7f;

    [Tooltip("사망 모션(Death 클립)이 있으면 켠다. 켜면 쓰러지는 모션이 끝난 뒤 사라지고, 끄면 죽는 즉시 사라진다")]
    [SerializeField] private bool hasDeathAnim = false;

    private Animator anim;
    private Collider2D bodyColl;
    private bool parried;              // 이번 공격이 패링당했는가 (남은 애니메이션 이벤트 무시용)
    private Coroutine cooldownRoutine;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        bodyColl = GetComponent<Collider2D>();
        if (strike == null) strike = GetComponentInChildren<MeleeStrikeHitbox>(true);
    }

    /// <summary>스폰(풀 재사용 포함)마다 전투 상태를 초기화한다. Spawner의 PowerUp에서 호출된다.</summary>
    public override void Spawn()
    {
        base.Spawn();
        ResetCombat();
    }

    private void ResetCombat()
    {
        isAttacking = false;
        parried = false;
        cooldownRoutine = null;
        if (strike != null) strike.DeactivateHitbox();
        if (controller != null) controller.SetLockOn(true);
    }

    /// <summary>AIController가 Attack 상태 동안 매 프레임 호출한다 → 공격은 한 번만 시작.</summary>
    public override void Attack()
    {
        if (isAttacking || !isLive) return;
        isAttacking = true;
        parried = false;

        controller.SetLockOn(false);                 // 공격 중엔 락온 해제 → 돌아보지 않는다 (판정 방향 고정)
        if (strike != null) strike.SetFacing(FacingSign());

        anim.ResetTrigger(MoveTrigger);
        anim.SetTrigger(AttackTrigger);
    }

    #region Animation Events (Attack 클립)
    /// <summary>패링 가능 구간 시작 — 무기 판정 활성화. 이 구간 동안엔 피해가 들어가지 않는다.</summary>
    public void OnParryWindowStart()
    {
        if (!isAttacking || parried || strike == null) return;
        strike.ActivateHitbox();
    }

    /// <summary>무기가 꽂히는 순간 — 패링 가능 구간이 끝나고, 플레이어가 범위 안이면 피해 1.</summary>
    public void OnStrikeImpact()
    {
        if (strike == null || !strike.CanBeParried) return;   // 패링당했으면 이미 비활성화돼 있다

        var player = PlayerCharacter.Instance;
        bool hit = player != null && !player.IsDead && strike.CollidesWithPlayer(player);
        strike.DeactivateHitbox();
        if (hit) player.ApplyDamage(damage);
    }

    public void OnAttackEnd()
    {
        if (!isAttacking || parried || !isLive) return;
        cooldownRoutine = StartCoroutine(CooldownThenMove());
    }
    #endregion

    private IEnumerator CooldownThenMove()
    {
        controller.SetLockOn(true);
        yield return new WaitForSeconds(attackCoolTime);
        cooldownRoutine = null;
        isAttacking = false;
        if (isLive) controller.ChangeState(AIController.AIState.Move);
    }

    /// <summary>무기 판정이 패링당했을 때 MeleeStrikeHitbox가 호출한다. 넉백 + 스턴.</summary>
    public void OnStrikeParried()
    {
        if (!isLive) return;

        parried = true;
        if (strike != null) strike.DeactivateHitbox();
        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = null;
        isAttacking = false;
        controller.SetLockOn(true);

        // AIController가 Move로 돌아갈 때마다 Move 트리거를 보내는데, Idle 상태에서는 쓰이지 않아 남아 있다.
        // 그대로 두면 Stagger에 들어가자마자 Move 트리거로 빠져나오므로 먼저 지운다.
        anim.ResetTrigger(MoveTrigger);
        anim.ResetTrigger(AttackTrigger);
        anim.SetTrigger(StaggerTrigger);

        Vector2 away = (Vector2)transform.position - PlayerPosition();
        if (away.sqrMagnitude < 0.0001f) away = new Vector2(-FacingSign(), 0f);
        controller.Knockback(away, knockbackDistance, knockbackTime, stunTime);
    }

    #region Death
    /// <summary>
    /// 사망 모션이 있으면 바로 치우지 않고 쓰러지는 모션을 재생한다.
    /// 경험치/골드/킬 카운트는 Die에서 이미 처리됐고, isLive가 false라 이동·공격·피격도 없다.
    /// </summary>
    protected override void Disappear()
    {
        if (!hasDeathAnim || anim == null)
        {
            base.Disappear();
            return;
        }

        if (strike != null) strike.DeactivateHitbox();
        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = null;
        if (bodyColl != null) bodyColl.enabled = false;   // 쓰러지는 동안 접촉 피해 없음 (재사용 시 AIController가 다시 켠다)

        anim.ResetTrigger(MoveTrigger);
        anim.ResetTrigger(AttackTrigger);
        anim.ResetTrigger(StaggerTrigger);
        anim.SetTrigger(DeathTrigger);
        StartCoroutine(DisappearAfterDelay());
    }

    /// <summary>Death 클립 끝 애니메이션 이벤트.</summary>
    public void OnDeathAnimEnd()
    {
        if (isLive || !gameObject.activeSelf) return;   // 이미 치워졌거나 재사용된 뒤 늦게 온 이벤트 무시
        base.Disappear();
    }

    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(DeathDisappearDelay);
        if (!isLive) base.Disappear();   // 이벤트가 먼저 왔다면 이미 비활성화돼서 여기까지 오지 않는다
    }
    #endregion

    /// <summary>플레이어가 있는 쪽 (1 오른쪽 / -1 왼쪽).</summary>
    private int FacingSign() => PlayerPosition().x >= transform.position.x ? 1 : -1;

    private Vector2 PlayerPosition()
    {
        var player = PlayerCharacter.Instance;
        return player != null ? (Vector2)player.transform.position : (Vector2)transform.position;
    }
}
