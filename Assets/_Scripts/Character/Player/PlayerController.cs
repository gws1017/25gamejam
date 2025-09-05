using UnityEngine;

public class PlayerController : BaseController
{
    [Header("Component")]
    [SerializeField] private GameObject robotSpiritObject;
    [SerializeField] private Camera cam;

    [Header("Property")]
    [SerializeField] private float speed = 4f; // 플레이어 이동 속도
    [SerializeField] private float radius = 2f; // 로봇 정령 반지름

    [Header("Input Raw Data")]
    [SerializeField] private float horizontalAxis;
    [SerializeField] private float verticalAxis;
    [SerializeField] private Vector3 mouseWorld;
    [SerializeField] private Vector2 mouseDir;
    [SerializeField] private float mouseAngle;

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

       RotateRobot();
    }

    private void FixedUpdate()
    {
        //Vector2 moveVec = isHorizonMove ? new Vector2(horizontalAxis, 0) : new Vector2(0, verticalAxis);
        Vector2 moveVec = new Vector2(horizontalAxis, 0);
        rigidBody2D.linearVelocity = moveVec * speed;
        
    }

    //키입력 체크
    void CheckInput()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        //마우스 월드좌표 계산
        if (cam != null)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = cam.WorldToScreenPoint(transform.position).z;
            mouseWorld = cam.ScreenToWorldPoint(mouseScreenPos);
            mouseDir = mouseWorld - transform.position;
            mouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        }

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

        CheckAttack();
    }

    private void CheckAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RobotSpirit robot = robotSpiritObject.GetComponent<RobotSpirit>();
            robot.Attack(mouseAngle);
        }
    }

    //로봇 정령 회전
    void RotateRobot()
    {
        if (robotSpiritObject == null) return;
        if(cam == null)
        {
            Debug.Log("플레이어 컨트롤러에 카메라가 Set 되지않았습니다.");
            return;
        }

        Vector3 RobotPosition = new Vector3(
            Mathf.Cos(mouseAngle * Mathf.Deg2Rad),
            Mathf.Sin(mouseAngle * Mathf.Deg2Rad),
            0f
        ) * radius;

        robotSpiritObject.transform.position = transform.position + RobotPosition;
    }

}
