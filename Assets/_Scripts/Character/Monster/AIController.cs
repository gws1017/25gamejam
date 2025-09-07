using UnityEngine;
public class AIController : BaseController
{
    //몬스터 공통 컨트롤러 클래스

    protected Rigidbody2D targetPlayer;
    protected MonsterCharacter owner;

    [SerializeField] protected Vector2 targetDir;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int spriteDir = 1; // 1 : left -1 : right
    public enum AIState
    {
        Move,
        Attack,
        Spawn,
        Dead,
        Hit
    }

    [SerializeField] protected AIState currentState;

    //Getter
    public Rigidbody2D TargetPlayer => targetPlayer;
    public Vector2 TargetDir => targetDir;
    public float MoveSpeed => moveSpeed;

    //Setter

    public float SetSpeed { set => moveSpeed = value;}

    protected override void Awake()
    {
        base.Awake();
        isPlayer = false;

        owner = GetComponent<MonsterCharacter>();
    }
    protected virtual void OnEnable()
    {
        targetPlayer = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        spriteRenderer.sortingOrder = 1;
        rigidBody2D.simulated = true;
        collide.enabled = true;
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

        targetDir = (targetPlayer.position - rigidBody2D.position);

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

        LookAtPlayer();
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
        Vector2 nextVec = targetDir.normalized * moveSpeed * Time.fixedDeltaTime;
        //공격사거리까지 이동
        if (targetDir.magnitude >= owner.AttackRange)
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
        switch(state)
        {
            case AIState.Spawn:
                owner.Spawn(); 
                break;
        }
    }
    protected virtual void OnStateExit(AIState state)
    {

    }
}
