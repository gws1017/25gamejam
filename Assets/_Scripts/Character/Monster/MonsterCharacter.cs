using UnityEngine;

public class MonsterCharacter : BaseCharacter
{
    protected AIController controller;
    [SerializeField] protected int dropExp = 1;
    [SerializeField] protected float attackRange = 2f;

    public float AttackRange => attackRange;
    public int DropExp => dropExp;

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

    // Update is called once per frame
    void Update()
    {
        
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
        base.Die();
        //오브젝트 풀링 사용시 변경 필요

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어 공격은 투사체 밖에 없다
        if (!collision.CompareTag("Projectile")) return;
        float applyDamage = 0;

        var bullet = collision.GetComponent<Bullet>();
        if(bullet != null && bullet.causerObject != gameObject)
        {
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
