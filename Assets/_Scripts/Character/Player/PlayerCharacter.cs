using System;
using System.Collections;
using UnityEngine;

public class PlayerCharacter : BaseCharacter
{
    #region [Singleton]
    public static PlayerCharacter Instance { get; private set; }

    private void SingleTon()
    {
        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

    //플레이어 정보
    [Header("PlayerStatus")]
    [SerializeField] private int level;
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp;

    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerItemEffects playerItemEffects;
    // lazy initialization
    public PlayerItemEffects ItemEffects => playerItemEffects ??= GetComponent<PlayerItemEffects>();

    [Space]
    [SerializeField] private Hearts hearts;

    public event Action<float, float> OnHealthChanged; // (currentHP, maxHP)
    private void NotifyHealthChanged() => OnHealthChanged?.Invoke(currentHP, maxHP);

    public bool IsInvincibleExternal { get; set; } = false;
    private int hitBuffer = 0; 

    private int maxLevel = 1000;
    private Coroutine DamageDelayCorutine;

    private bool isDead = false;
    public int CurrentExp => currentExp;
    public int Level => level;
    public bool IsDead => isDead;
    public PlayerWallet PlayerWallet => playerWallet;
    public PlayerController PlayerController => playerController;
    public PlayerItemEffects PlayerItemEffects => playerItemEffects;

    protected override void Awake()
    {
        SingleTon();

        if (playerWallet == null)
            playerWallet = GetComponentInChildren<PlayerWallet>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        EmptySingleton();
    }

    public void IncreamentExp(int exp)
    {
        currentExp += exp;

        LevelUp();
        currentExp = Mathf.Clamp(currentExp, 0, maxExp);

    }

    public void LevelUp()
    {
        if (currentExp >= maxExp && level <= maxLevel)
        {
            currentExp -= maxExp;
            level++;
        }
    }

    /// <summary>
    /// 체력을 회복한다. (양수만 허용, 오버힐 방지, 사망 시 무시)
    /// </summary>
       public void Heal(float amount)
        {
            if (IsDead) return;

            if (Mathf.Approximately(amount, 1f))
            {
                bool turnedOn = (hearts != null) && hearts.TurnOnLastOff();
                if (turnedOn) currentHP = Mathf.Clamp(currentHP + 1, 0, MaxHP);
                else currentHP = Mathf.Clamp(currentHP + 1, 0, MaxHP);

                NotifyHealthChanged();
                return;
            }

            // 특수 회복량은 기존 로직
            currentHP = Mathf.Clamp(currentHP + Mathf.RoundToInt(amount), 0, MaxHP);
            NotifyHealthChanged();
        }

    //데미지 처리함수
    public void ApplyDamage(float damage = 1f)
    {
        if(IsInvincibleExternal || IsDead) return;

        if (Mathf.Approximately(damage, 1f))
        {
            OnHit();             
            return;
        }

        float defenseMul = (ItemEffects != null) ? ItemEffects.DefenseMultiplier : 1f;
        float takenMul = 1f / Mathf.Max(0.0001f, defenseMul);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * takenMul));

        int before = (int)currentHP;
        currentHP = Mathf.Clamp(currentHP - finalDamage, 0, MaxHP);
        if (currentHP != before) NotifyHealthChanged();
        if (currentHP > 0) Hit(); else if (!IsDead) Die();
    }

    public override void Die()
    {
        isDead = true;
        base.Die();
        
        //애니메이션 호출
        GetComponent<Animator>().SetBool("isDead",isDead);
    }

    //데미지 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        float finalDamage = 0;

        if (collision.gameObject.name == "playerBullet(Clone)")
        {
            Debug.Log("Player Bullet Collision - Ignore");
            return;
        }

        //탄환 데미지 적용
        var bullet = collision.GetComponent<Bullet>();
        if (bullet != null && bullet.causerObject != gameObject)
        {
            finalDamage += bullet.Damage;
        }

        ApplyDamage(finalDamage);
    }

    private void OnHit()
    {
        bool twoHit = (ItemEffects != null && ItemEffects.IsTwoHitDefenseActive);

        if (twoHit)
        {
            hitBuffer++;
            if (hitBuffer < 2) return; // 첫 타 흡수(변화 없음)
            hitBuffer = 0;
        }

        bool turnedOff = (hearts != null) && hearts.TurnOffFirstOn();
        if (turnedOff) currentHP = Mathf.Clamp(currentHP - 1, 0, MaxHP);
        else currentHP = Mathf.Clamp(currentHP - 1, 0, MaxHP); // 안전

        NotifyHealthChanged();
        if (currentHP > 0) Hit(); else if (!IsDead) Die();
    }

    public void OnHealOne(Hearts hearts)
    {
        hearts.TurnOnLastOff();          // 마지막에 꺼진 칸 켜기
    }

    public void OnHealOne(Hearts hearts)
    {
        hearts.TurnOnLastOff();          // 마지막에 꺼진 칸 켜기
    }

    //근접 공격 데미지 처리 
    //각 근접 몬스터의 공격 쿨타임마다 데미지가 적용됨
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (DamageDelayCorutine == null)
        {
            var mob = collision.GetComponent<MonsterCharacter>();
            if (mob != null)
            {
                DamageDelayCorutine = StartCoroutine(DamageDelayCoroutine(mob.AttackCoolTime));
            }
        }
    }

    IEnumerator DamageDelayCoroutine(float time)
    {
        ApplyDamage();
        yield return new WaitForSeconds(time);
        DamageDelayCorutine = null;
    }

    [ContextMenu("Heal1")]
    public void Heal1()
    {
        Heal(1);
    }

    [ContextMenu("Hit Once")]
    public void CM_HitOnce() => ApplyDamage(1f);

    [ContextMenu("Hit x2")]
    public void CM_HitTwice() { ApplyDamage(1f); ApplyDamage(1f); }
}
