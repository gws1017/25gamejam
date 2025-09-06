using UnityEngine;

public class PlayerCharacter : BaseCharacter
{
    //플레이어 정보
    [Header("PlayerStatus")]
    [SerializeField] private int level;
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp;

    private bool isDead = false;
    public int CurrentExp => currentExp;
    public int Level => level;
    public bool IsDead => isDead;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreamentExp(int exp)
    {
        currentExp += exp;

        LevelUp();
        currentExp = Mathf.Clamp(currentExp, 0, maxExp);

    }

    public void LevelUp()
    {
        if (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            level++;
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
        float applyDamage = 0f;

        //탄환 데미지 적용
        var bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            applyDamage += bullet.Damage;
        }
        //몬스터 근접공격 데미지 적용
        var monster = collision.GetComponent<MonsterCharacter>();
        if (monster != null)
        {
            applyDamage += monster.Damage;
        }

        currentHP -= applyDamage;
        currentHP = Mathf.Clamp(currentHP, 0,MaxHP);

        if (currentHP > 0)
        {
            Hit();
        }
        else
        {
            if(IsDead == false)
                Die();
        }
    }
}
