using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 3f;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.right * speed,ForceMode2D.Impulse);
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
