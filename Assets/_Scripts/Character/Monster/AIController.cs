using System.Collections;
using UnityEngine;
public class AIController : BaseController
{
    //몬스터 공통 컨트롤러 클래스

    protected Rigidbody2D targetPlayer;
    protected MonsterCharacter owner;

    [SerializeField] protected Vector2 toTargetVector;
    [SerializeField] protected Vector2 targetDirection;
    [SerializeField] protected float targetDistance;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int spriteDir = 1; // 1 : left -1 : right
    public enum AIState
    {
        Move,
        Attack,
        Spawn,
        Dead,
        Hit,
        Stagger   // 패링당해 넉백 + 스턴 중 (MeleeDevil). 기존 프리팹 값이 밀리지 않게 맨 뒤에 추가
    }

    [SerializeField] protected AIState currentState;

    private bool isLockedOn = true;        // 평소엔 플레이어를 락온(계속 바라봄). 근접 공격 중엔 해제해서 판정 방향을 고정
    private Coroutine knockbackRoutine;

    //Getter
    public Rigidbody2D TargetPlayer => targetPlayer;
    public Vector2 TargetDir => targetDirection;
    public float MoveSpeed => moveSpeed;
    /// <summary>프리팹에 설정된 원래 이동 속도. 스테이지 강화는 매번 이 값에서 계산한다 (풀 재사용 시 누적 방지).</summary>
    public float BaseMoveSpeed => baseMoveSpeed;
    private float baseMoveSpeed;

    //Setter

    public float SetSpeed { set => moveSpeed = value;}


    public void UpdateRigidBodyPosition()
    {
        rigidBody2D.position = (Vector2)transform.position;
    }

    protected override void Awake()
    {
        base.Awake();
        isPlayer = false;
        baseMoveSpeed = moveSpeed;

        owner = GetComponent<MonsterCharacter>();
    }
    protected virtual void OnEnable()
    {
        targetPlayer = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        spriteRenderer.sortingOrder = 1;
        rigidBody2D.simulated = true;
        collide.enabled = true;
        isLockedOn = true;
        knockbackRoutine = null;   // 비활성화 때 코루틴은 이미 멈춰 있다 (풀 재사용 대비 초기화)
        ChangeState(AIState.Move);
    }
    private void OnDisable()
    {
        collide.enabled = false;
        rigidBody2D.simulated = false;
        spriteRenderer.sortingOrder = -1;
    }
    protected virtual void Start()
    {
        LookAtPlayer();
    }

    protected virtual void FixedUpdate()
    {
        if (targetPlayer == null) return;
        if(!owner.IsLive) return;
        if (gameObject.activeSelf == false) return;

        toTargetVector = (targetPlayer.position - rigidBody2D.position);
        targetDirection = toTargetVector.normalized;
        targetDistance = toTargetVector.magnitude;

        switch (currentState)
        {
            case AIState.Move:
                CalculateAIMovement();
                break;
            case AIState.Attack:
                owner.Attack();
                break;
             case AIState.Dead:
                targetPlayer.GetComponent<PlayerCharacter>().IncreamentExp(owner.DropExp);
                owner.Die();
                break;
        }
    }
    private void LateUpdate()
    {
        if (targetPlayer == null) return;
        if (!owner.IsLive) return;
        if (!isLockedOn) return;

        LookAtPlayer();
    }

    /// <summary>
    /// 플레이어 락온 설정. true(기본)면 매 프레임 플레이어 쪽을 바라보고,
    /// false면 지금 방향을 유지한다 (근접 공격 중 판정 방향 고정용).
    /// </summary>
    public void SetLockOn(bool lockOn)
    {
        isLockedOn = lockOn;
    }

    /// <summary>
    /// 넉백 + 스턴. direction 쪽으로 distance만큼 duration 동안 밀려난 뒤 stunTime 동안 멈췄다가 Move로 돌아간다.
    /// 패링 성공 시에만 호출된다 (MeleeDevil.OnStrikeParried). 그동안 상태는 Stagger라 이동/공격하지 않는다.
    /// </summary>
    public void Knockback(Vector2 direction, float distance, float duration, float stunTime)
    {
        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        ChangeState(AIState.Stagger);
        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, distance, duration, stunTime));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float distance, float duration, float stunTime)
    {
        Vector2 start = rigidBody2D.position;
        for (float t = 0f; t < duration; t += Time.fixedDeltaTime)
        {
            float k = t / duration;
            float eased = 1f - (1f - k) * (1f - k);   // 처음에 세게 밀리고 점점 느려진다
            rigidBody2D.MovePosition(start + direction * distance * eased);
            rigidBody2D.linearVelocity = Vector2.zero;
            yield return new WaitForFixedUpdate();
        }
        rigidBody2D.MovePosition(start + direction * distance);

        yield return new WaitForSeconds(stunTime);

        knockbackRoutine = null;
        if (owner.IsLive && currentState == AIState.Stagger) ChangeState(AIState.Move);
    }

    protected virtual void CalculateAIMovement()
    {
        MoveToTarget();
    }

    protected void LookAtPlayer()
    {
        if (spriteRenderer != null)
        {
            if (spriteDir == -1)
                spriteRenderer.flipX = targetPlayer.position.x < rigidBody2D.position.x;
            else if(spriteDir == 1)
                spriteRenderer.flipX = targetPlayer.position.x > rigidBody2D.position.x;
        }
    }

    private void MoveToTarget()
    {
        Vector2 nextVec = targetDirection * moveSpeed * Time.fixedDeltaTime;
        //공격사거리까지 이동
        if (targetDistance > owner.AttackRange)
        {
            rigidBody2D.MovePosition(rigidBody2D.position + nextVec);
            rigidBody2D.linearVelocity = Vector2.zero;
        }
        else
        {
            //공격가능한 거리면 
            ChangeState(AIState.Attack);
        }
    }

    public void ChangeState(AIState newState)
    {
        //Exit
        OnStateExit(currentState);
        currentState = newState;
        //Enter
        OnStateEnter(currentState);
    }
    protected virtual void OnStateEnter(AIState state)
    {
        var anim = GetComponent<Animator>();

        switch (state)
        {
            case AIState.Move:
                if (anim != null) anim.SetTrigger(AIState.Move.ToString());
                break;
            case AIState.Spawn:
                owner.Spawn();
                if (anim != null)
                {
                    anim.SetTrigger("Idle");
                }
                break;
        }
    }
    protected virtual void OnStateExit(AIState state)
    {

    }
}
