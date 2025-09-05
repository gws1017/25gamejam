using UnityEngine;

public class PlayerController : BaseController
{
    [SerializeField] private float speed = 4f; // 플레이어 이동 속도

    [SerializeField] private float horizontalAxis;
    [SerializeField] private float verticalAxis;

    [SerializeField] private bool isHorizonMove;

    [SerializeField] private Vector3 dirVec;
    [SerializeField] private int lastHorzDir = 1; // 1: 오른쪽, -1: 왼쪽

    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    void Update()
    {
       CheckInput();
    }

    private void FixedUpdate()
    {
        //Vector2 moveVec = isHorizonMove ? new Vector2(horizontalAxis, 0) : new Vector2(0, verticalAxis);
        Vector2 moveVec = new Vector2(horizontalAxis, 0);
        rigidBody2D.linearVelocity = moveVec * speed;
        
    }

    void CheckInput()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        bool hDown = Input.GetButtonDown("Horizontal");
        bool vDown = Input.GetButtonDown("Vertical");
        bool vUp = Input.GetButtonUp("Vertical");
        bool hUp = Input.GetButtonUp("Horizontal");

        if (hDown)
            isHorizonMove = true;
        else if (vDown)
            isHorizonMove = false;
        else if (hUp || vUp)
            isHorizonMove = horizontalAxis != 0;

        if (horizontalAxis != 0)
        {
            lastHorzDir = (int)horizontalAxis;
            dirVec = lastHorzDir < 0 ? Vector3.left : Vector3.right;
        }

        //Animation
        if (anim.GetInteger("horizonAxis") != horizontalAxis)
        {
            anim.SetInteger("horizonAxis", (int)horizontalAxis);
        }

        if (dirVec == Vector3.left)
            spriteRenderer.flipX = true;
        else if (dirVec == Vector3.right)
            spriteRenderer.flipX = false;
    }

}
