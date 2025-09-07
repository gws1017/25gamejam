using UnityEngine;
using System.Collections;

public class PlayerCharacter : BaseCharacter
{
    //플레이어 정보
    [Header("PlayerStatus")]
    [SerializeField] private int level;
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp;
    private int maxLevel = 1000;
    private Coroutine DamageDelayCorutine;

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
        currentHP -= damage;
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

        ApplyDamage(finalDamage);
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
}
