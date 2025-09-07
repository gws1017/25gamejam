using UnityEngine;

public class BaseController : MonoBehaviour
{
    protected Rigidbody2D rigidBody2D;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D collide;

    protected bool isPlayer;

    protected virtual void Awake()
    {
        rigidBody2D =  GetComponent<Rigidbody2D>();
        spriteRenderer =  GetComponent<SpriteRenderer>();
        collide =  GetComponent<BoxCollider2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
