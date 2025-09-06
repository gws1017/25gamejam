using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour, IParryable
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 3f;

    [SerializeField] private int damage = 1;
    [SerializeField] private List<GameObject> ignoreObjects = new List<GameObject>(); //충돌 처리 무시할 오브젝트 등록(본인, 무기등)

    public GameObject causerObject;
    private Rigidbody2D rb;

    public int Damage => damage;


    public bool CanBeParried { get; private set; } = true;

    public void Init(int dmg, GameObject causer)
    {
        damage = dmg;
        causerObject = causer;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.right * speed,ForceMode2D.Impulse);
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIgnoreObject(GameObject obj)
    {
        ignoreObjects.Add(obj);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //투사체 끼리는 충돌체크 하지 말 것
        if (collision.CompareTag("Projectile")) return;

        //충돌 처리 무시 오브젝트 체크
        foreach (var ignoreObject in ignoreObjects)
        {
            if (ignoreObject == collision.gameObject) return;
        }

        Destroy(gameObject);
    }

    public void OnParried(Vector2 parryPos)
    {
        Vector2 incomingVec = rb.linearVelocity.normalized; //기존 진행 방향
        Vector2 normalVec = (transform.position - (Vector3)parryPos).normalized; //반사면 노멀

        Vector2 reflectDir = Vector2.Reflect(incomingVec, normalVec);

        //방향 전환
        rb.linearVelocity = reflectDir * speed;
        //값 초기화
        lifetime = 3f;
        ignoreObjects.Clear();
    }

}
