using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [Header("Character Status")]
    [SerializeField] protected int damage;
    [SerializeField] protected int defense;
    [SerializeField] protected int speed;
    [SerializeField] protected int currentHP;
    [SerializeField] protected int maxHP;


    public int Damage => damage;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    protected virtual void Awake()
    {

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //필요시 파라미터 변경
    public virtual void Hit()
    {

    }

    public virtual void Die()
    {
        //몬스터 사망시 실행할 함수 작성

    }
}
