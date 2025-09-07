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

    public bool IsInvincibleExternal { get; set; } = false;

    private int maxLevel = 1000;

    private bool isDead = false;
    public int CurrentExp => currentExp;
    public int Level => level;
    public bool IsDead => isDead;
    public PlayerWallet PlayerWallet => playerWallet;
    public PlayerController PlayerController => playerController;
    public PlayerItemEffects PlayerItemEffects => playerItemEffects;

    private void Awake()
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
        if (currentExp >= maxExp || level <= maxLevel)
        {
            currentExp -= maxExp;
            level++;
        }
    }

    public float FinalDamage()
    {
        return 0.0f;
    }

    //데미지 처리함수
    public void ApplyDamage(float damage = 1f)
    {
        // (무적이면 데미지 무시)
        if (IsInvincibleExternal) return;

        // 방어 배수 반영: 들어온 피해 × (1 / DefenseMultiplier)
        float defenseMultiplier = (ItemEffects != null) ? 
                                   ItemEffects.DefenseMultiplier : 1f;
        float damageTakenMultiplier = 1f / Mathf.Max(0.0001f, defenseMultiplier); // 예: 1.4배 방어 ⇒ 약 0.714배 피해
        float finalDamage = damage * damageTakenMultiplier;

        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0, MaxHP);

        if (currentHP > 0)
        {
            Hit();
        }
        else
        {
            if (IsDead == false)
                Die();
        }
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

        //탄환 데미지 적용
        var bullet = collision.GetComponent<Bullet>();
        if (bullet != null && bullet.causerObject != gameObject)
        {
            finalDamage += bullet.Damage;
        }
        //몬스터 근접공격 데미지 적용
        var monster = collision.GetComponent<MonsterCharacter>();
        if (monster != null)
        {
            finalDamage += monster.Damage ;
        }

        ApplyDamage(finalDamage);
    }
}
