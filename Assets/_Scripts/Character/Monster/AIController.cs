using UnityEngine;
public class AIController : BaseController
{
    //���� ���� ��Ʈ�ѷ� Ŭ����

    protected Rigidbody2D targetPlayer;
    protected MonsterCharacter owner;

    [SerializeField] protected Vector2 targetDir;
    [SerializeField] protected float moveSpeed = 2f;
    public enum AIState
    {
        Move,
        Attack,
        Spawn,
        Dead,
        Hit
    }

    protected AIState currentState;

    public Rigidbody2D TargetPlayer => targetPlayer;
    public Vector2 TargetDir => targetDir;
    public float MoveSpeed => moveSpeed;

    protected override void Awake()
    {
        base.Awake();
        isPlayer = false;

        owner = GetComponent<MonsterCharacter>();
        targetPlayer = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        ChangeState(AIState.Spawn);
    }

    protected virtual void FixedUpdate()
    {
        if (targetPlayer == null) return;
        targetDir = (targetPlayer.position - rigidBody2D.position).normalized;

        switch (currentState)
        {
            case AIState.Move:
                CalculateAIMovement();
                break;
            case AIState.Attack:
                owner.Attack();
                break;
             case AIState.Dead:
                break;
        }
    }

    protected virtual void CalculateAIMovement()
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector2 nextVec = targetDir * moveSpeed * Time.fixedDeltaTime;
        //���ݻ�Ÿ����� �̵�
        if (targetDir.magnitude >= owner.AttackRange)
        {
            rigidBody2D.MovePosition(rigidBody2D.position + nextVec);
            rigidBody2D.linearVelocity = Vector2.zero;
        }
        else
        {
            //���ݰ����� �Ÿ��� 
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
