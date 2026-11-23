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

    // 프리팹에 설정된 원래 값. 스테이지 강화는 매번 이 값에서 계산한다.
    // (강화된 값에 또 곱하면 풀에서 재사용될 때마다 강화가 누적된다)
    private float baseMaxHP;
    private float baseDefense;
    private float basePowerUpMultiplier;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<AIController>();
        baseMaxHP = maxHP;
        baseDefense = defense;
        basePowerUpMultiplier = powerUpMultiplier;
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

    /// <summary>
    /// 현재 스테이지 강화 단계(count)에 맞는 세기로 맞춘다. 스폰마다 Spawner가 호출.
    /// 항상 프리팹 원래 값에서 계산하므로 몇 번 재사용돼도 같은 스테이지면 같은 세기다.
    /// </summary>
    public virtual void PowerUp(int count)
    {
        powerUpMultiplier = basePowerUpMultiplier * count; // 강화 0단계: 0 / 1단계: 0.1 / 2단계: 0.2 ...

        float pm = 1 + powerUpMultiplier;

        controller.SetSpeed = controller.BaseMoveSpeed * pm;
        maxHP = baseMaxHP * pm;
        defense = baseDefense * pm;
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

        // 풀에서 나온 몬스터는 풀로 반환해야 다음 스폰 때 재사용된다.
        // (전에는 끄기만 해서 매 스폰마다 새로 생성됐다)
        // 보스처럼 Instantiate로 만든 몬스터는 poolKey가 없으므로 지금처럼 끄기만 한다.
        var pool = (PoolManager.Instance != null && !string.IsNullOrEmpty(poolKey))
                   ? PoolManager.Instance.Get(poolKey) : null;
        if (pool != null) pool.Despawn(this);
        else gameObject.SetActive(false);
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
        if (!isLive) return;
        if (!TryConsumePlayerBullet(collision, gameObject, out float damageAmount)) return;

        TakeDamage(damageAmount, collision.ClosestPoint(transform.position));
    }

    /// <summary>
    /// 피해를 적용한다. 자기 콜라이더에 맞았을 때뿐 아니라, 다른 오브젝트가 대신 맞고
    /// 넘겨줄 때(DamageRelay — 몸이 여러 개인 보스)도 이 함수를 통한다.
    /// </summary>
    public void TakeDamage(float amount, Vector2 hitPoint)
    {
        if (!isLive || amount <= 0f) return;

        currentHP -= amount;

        if (currentHP > 0)
        {
            Hit(hitPoint);
        }
        else
        {
            controller.ChangeState(AIController.AIState.Dead);
        }
    }

    /// <summary>
    /// 플레이어가 쏜 탄에 맞은 것인지 판별하고, 맞았다면 그 탄의 피해를 "사용 처리"한다.
    /// 사용 처리 덕분에 탄 하나가 같은 프레임에 콜라이더 두 개(예: 보스 본체와 파트너)에 동시에
    /// 닿아도 피해는 한 번만 들어간다.
    /// </summary>
    public static bool TryConsumePlayerBullet(Collider2D collision, GameObject self, out float damageAmount)
    {
        damageAmount = 0f;

        //플레이어 공격은 투사체 밖에 없다
        if (!collision.CompareTag("Projectile")) return false;
        if (collision.CompareTag("Enemy")) return false; //적끼리 체크 X

        var bullet = collision.GetComponent<Bullet>();
        if (bullet == null) return false;

        if (bullet.Causer != null && bullet.Causer.CompareTag("Enemy")) return false; //Enemy가 쏜 총알이면 무시
        if (bullet.Causer == self) return false;
        if (bullet.Damage <= 0f) return false;
        if (!bullet.TryConsumeHit()) return false;

        damageAmount = bullet.Damage;
        return true;
    }


}
