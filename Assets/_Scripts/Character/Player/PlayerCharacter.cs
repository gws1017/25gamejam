using UnityEngine;

public class PlayerCharacter : BaseCharacter
{
    //플레이어 정보
    private int level;
    private int currentExp;
    private int maxExxp;

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
        currentExp = Mathf.Clamp(currentExp, 0, maxExxp);
        LevelUp();
    }

    public void LevelUp()
    {
        if (currentExp >= maxExxp)
        {
            currentExp = 0;
            level++;
        }
    }

    public override void Die()
    {
        base.Die();

        isDead = true;
        //애니메이션 호출
        GetComponent<Animator>().SetTrigger("Die");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        currentHP -= collision.GetComponent<Bullet>().Damage;

        if (currentHP > 0)
        {
            //
            Hit();
        }
        else
        {
            if(IsDead == false)
                Die();
        }
    }
}
