using System;
using UnityEngine;
using System.Collections;
using static AIController;

public class MonsterCharacter : BaseCharacter,IPoolable
{
    /// <summary>
    /// 몬스터가 죽을 때 발행. StageManager가 구독해서 킬카운트/보스 클리어를 판정한다.
    /// 몬스터는 풀에서 계속 재사용되므로 인스턴스 단위 구독 대신 static으로 둔다.
    /// </summary>
    public static event Action<MonsterCharacter> OnMonsterDied;

    /// <summary>
    /// 몬스터가 스폰될 때 발행 (OnEnable → OnSpawn에서 1회만). UI_BossHealthBar가 구독해서
    /// 보스 등장 시 체력바를 띄운다. 일반 몬스터는 풀에서 재사용되며 그때마다 발행됨.
    /// </summary>
    public static event Action<MonsterCharacter> OnMonsterSpawned;

    /// <summary>보스 여부. 보스 클래스에서 override 할 것.</summary>
    public virtual bool IsBoss => false;

    protected AIController controller;
    [SerializeField] protected int dropExp = 1;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCoolTime = 1f;
    [SerializeField] protected string attackTrigger = "Idle";
    [SerializeField] protected float powerUpMultiplier = 0.1f;
    [SerializeField] protected GameObject hitVFX;
    [SerializeField] protected AudioClip monsterDieFX;
    protected bool isAttacking = false;
    protected bool isLive = false;

    public float AttackRange => attackRange;
    public float AttackCoolTime => attackCoolTime;
    public int DropExp => dropExp;
    public bool IsLive => isLive;

    public string poolKey { get; set; }

    public void SetSpeed(int value)
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
        if(controller)controller.ChangeState(AIState.Move);
    }

    private void OnEnable()
    {
        OnSpawn();
    }
    public void OnSpawn()
    {
        isLive = true;
        controller.enabled = true;
        controller.ChangeState(AIState.Move);
        currentHP = MaxHP;
        OnMonsterSpawned?.Invoke(this);
    }

    public void OnDespawn()
    {
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
        controller.UpdateRigidBodyPosition();
        Spawn();
    }

    public virtual void Spawn()
    {
        isLive = true;
        //몬스터 등장시 실행할 함수 작성
    }

    public virtual void Attack()
    {
        //몬스터 공격시 실행할 함수 작성
    }
    public override void Hit(Vector2 HitPoint)
    {
        base.Hit(HitPoint);
        if (isLive == false) return;
        Vector3 hit3 = new Vector3(HitPoint.x, HitPoint.y, 0);
        Instantiate(hitVFX, hit3, Quaternion.identity);
    }

    public override void Die()
    {
        if (isLive == false) return;
        base.Die();
        SoundManager.Instance.PlaySoundFX(monsterDieFX);
        isAttacking = false;
        isLive = false;
        PlayerCharacter.Instance.PlayerWallet.AddGold(10);

        // 비활성화 전에 발행 (구독자가 몬스터 상태를 읽을 수 있도록)
        OnMonsterDied?.Invoke(this);

        gameObject.SetActive(false);
    }

    protected IEnumerator AttackDelayCorutine()
    {
        GetComponent<Animator>().SetTrigger(attackTrigger);

        yield return new WaitForSeconds(attackCoolTime);

        isAttacking = false;

        controller.ChangeState(AIController.AIState.Move);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어 공격은 투사체 밖에 없다
        if (!collision.CompareTag("Projectile") || !isLive) return;
        if (collision.CompareTag("Enemy")) return; //적끼리 체크 X



        float applyDamage = 0;
        Vector2 hitPoint = collision.ClosestPoint(transform.position);

        var bullet = collision.GetComponent<Bullet>();
        if (bullet == null) return;

        if (bullet.Causer != null && bullet.Causer.CompareTag("Enemy")) return; //투사체인데, Enemy가 쏜 총알이라면 종료
        if( bullet.Causer != gameObject)
        {
            applyDamage += bullet.Damage;
        }

        if (applyDamage <= 0) return;
        currentHP -= applyDamage;

        if (currentHP > 0)
        {
            Hit(hitPoint);
        }
        else
        {
            controller.ChangeState(AIController.AIState.Dead);
        }
    }


}
