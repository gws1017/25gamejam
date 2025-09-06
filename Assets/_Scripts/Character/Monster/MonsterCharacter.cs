using UnityEngine;

public class MonsterCharacter : BaseCharacter
{
    protected AIController controller;
    [SerializeField] protected float attackRange = 2f;


    public float AttackRange => attackRange;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<AIController>();
    }
    void Start()
    {
        
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
}
