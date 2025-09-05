using UnityEngine;

public class BaseController : MonoBehaviour
{
    protected Rigidbody2D rigidBody2D;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        rigidBody2D =  GetComponent<Rigidbody2D>();
        spriteRenderer =  GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
