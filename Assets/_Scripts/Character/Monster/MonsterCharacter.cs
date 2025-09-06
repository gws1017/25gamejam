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
        //���� ����� ������ �Լ� �ۼ�
    }

    public virtual void Attack()
    {
        //���� ���ݽ� ������ �Լ� �ۼ�
    }
}
