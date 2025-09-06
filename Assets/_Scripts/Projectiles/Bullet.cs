using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour, IParryable
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 3f;

    [SerializeField] private float damage = 1f;
    [SerializeField] private List<GameObject> ignoreObjects = new List<GameObject>(); //충돌 처리 무시할 오브젝트 등록(본인, 무기등)

    public float Damage => damage;


    public bool CanBeParried { get; private set; } = true;

    public void Init(float dmg)
    {
        damage = dmg;
    }

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.right * speed,ForceMode2D.Impulse);
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

}
