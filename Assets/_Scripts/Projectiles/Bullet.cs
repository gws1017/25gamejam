using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 3f;

    [SerializeField] private int damage = 1;
    [SerializeField] private List<GameObject> ignoreObjects = new List<GameObject>(); //충돌 처리 무시할 오브젝트 등록(본인, 무기등)

    public int Damage => damage;



    public void Init(int dmg)
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
