using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [Header("Character Status")]
    [SerializeField] protected float damage;
    [SerializeField] protected float defense;
    [SerializeField] protected float speed;
    [SerializeField] protected float currentHP;
    [SerializeField] protected float maxHP;

    public float Defense => defense;
    public float Speed => speed;
    public float Damage => damage;
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

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
    public virtual void Hit(Vector2 HitPoint)
    {

    }

    public virtual void Die()
    {
        //몬스터 사망시 실행할 함수 작성

    }
}
