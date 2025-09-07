using UnityEngine;
using System.Collections;
using static AIController;

public class MonsterCharacter : BaseCharacter
{
    protected AIController controller;
    [SerializeField] protected int dropExp = 1;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCoolTime = 1f;
    [SerializeField] protected string attackTrigger = "Idle";
    [SerializeField] protected float powerUpMultiplier = 0.1f;
    protected bool isAttacking = false;
    protected bool isLive = true;

    public float AttackRange => attackRange;
    public float AttackCoolTime => attackCoolTime;
    public int DropExp => dropExp;
    public bool IsLive => isLive;

    public void  SetSpeed(int value)
    {
        speed = value;
        controller.SetSpeed = value;
    }

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<AIController>();
    }
    void Start()
    {
        //플레이어 한테 입히는 데미지 1로 고정
        //플레이어는 체력 3을 갖고, 3번 히트시 게임오버
        damage = 1;
    }

    private void OnEnable()
    {
        isLive = true;
        controller.enabled = true;
        currentHP = MaxHP;
    }

    public virtual void PowerUp(int count)
    {
        powerUpMultiplier *= count; //1~9 : 0 , 10 ~ 19 : 0.1 , 20 ~ 29 : 0.2

        float pm = 1 + powerUpMultiplier;

        controller.SetSpeed = controller.MoveSpeed * pm;
        maxHP *= pm;
        defense *= pm;
        //경험치도 늘리고싶으면 주석해제
        //dropExp = Mathf.CeilToInt(((float)dropExp * pm));

        currentHP = maxHP;
    }

    public virtual void Spawn()
    {
        //몬스터 등장시 실행할 함수 작성
    }

    public virtual void Attack()
    {
        //몬스터 공격시 실행할 함수 작성
    }

    public override void Die()
    {
        if (isLive == false) return;
        base.Die();
        isLive = false;
        gameObject.SetActive(false);
    }

    protected IEnumerator AttackDelayCorutine()
    {
        GetComponent<Animator>().SetTrigger(attackTrigger);

        yield return new WaitForSeconds(attackCoolTime);

        isAttacking = false;

        controller.ChangeState(AIController.AIState.Move);
        GetComponent<Animator>().SetTrigger(AIState.Move.ToString());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어 공격은 투사체 밖에 없다
        if (!collision.CompareTag("Projectile") || !isLive) return;
        if (collision.CompareTag("Enemy")) return; //적끼리 체크 X

        

        float applyDamage = 0;

        var bullet = collision.GetComponent<Bullet>();
        if(bullet != null && bullet.Causer != gameObject)
        {
            if (bullet.Causer != null && 
                bullet.Causer.CompareTag("Enemy")) return; //투사체인데, Enemy가 쏜 총알이라면 종료
            applyDamage += bullet.Damage;
        }
        currentHP -= applyDamage;

        if(currentHP > 0)
        {
            //
            Hit();
        }
        else
        {
            controller.ChangeState(AIController.AIState.Dead);
        }
    }
}
