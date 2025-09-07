using UnityEngine;

public class PlayerController : BaseController
{
    [Header("Component")]
    [SerializeField] private GameObject robotSpiritObject;
    [SerializeField] private Camera cam;
    [SerializeField] private RobotSpirit robot;

    [Header("Property")]
    [SerializeField] private float speed = 4f; // 플레이어 이동속도
    [SerializeField] private float radius = 2f; // 로봇 정령 회전 반지름
    [SerializeField] private float parryDistance = 1f; // 패링 허용 거리
    [SerializeField] private float parryDistanceOffset = 2f; // 패링 실패 오프셋값
    [SerializeField] private LayerMask parryLayerMask; //패링 객체 탐색용 마스크


    [Header("Input Raw Data")]
    [SerializeField] private float horizontalAxis;
    [SerializeField] private float verticalAxis;
    [SerializeField] private Vector3 mouseWorld;
    [SerializeField] private Vector2 mouseDir;
    [SerializeField] private float mouseAngle;

    [SerializeField] private bool isHorizonMove;

    [SerializeField] private Vector3 dirVec;
    [SerializeField] private int lastHorzDir = 1;


    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        robot = GetComponentInChildren<RobotSpirit>();
    }
    void Start()
    {

    }

    void Update()
    {
        var player = GetComponent<PlayerCharacter>();
        if (player == null) return;
        if (player.IsDead == true)
        {
            if(robot != null)
                robot.ClearParryFlags();
            rigidBody2D.linearVelocity = Vector3.zero;
            return;
        }
        CheckInput();

        RotateRobot();
    }

    private void FixedUpdate()
    {
        var player = GetComponent<PlayerCharacter>();
        if (player == null) return;
        if (player.IsDead == true) return;

        //Vector2 moveVec = isHorizonMove ? new Vector2(horizontalAxis, 0) : new Vector2(0, verticalAxis);
        Vector2 moveVec = new Vector2(horizontalAxis, 0);
        rigidBody2D.linearVelocity = moveVec * speed;
        
        var robot = GetComponentInChildren<RobotSpirit>();
        if (robot != null && robot.IsParrying)
        {
            CanParry();
        }
    }
    //키입력 체크
    void CheckInput()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        //마우스 입력 체크
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
        CheckParryKey();
    }


    private void CanParry()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, parryDistance + parryDistanceOffset, parryLayerMask);
        var parrySuccess = Physics2D.OverlapCircleAll(transform.position, parryDistance, parryLayerMask);

        robot.detectedParryTarget = hits.Length > 0; //패링가능한 타겟 감지

        foreach (var hit in parrySuccess)
        {
            var parryable = hit.GetComponent<IParryable>();
            if (parryable != null && parryable.CanBeParried && robot.hasParried == false)
            {
                Debug.Log("패링 성공");
                robot.hasParried = true;
                parryable.OnParried(hit.ClosestPoint(transform.position));

                //투사체 패링이면 발사자 변경
                if (parryable is not Bullet bullet) return;
                bullet.Init(robot.Damage, gameObject);
            }
        }
    }

    private void CheckParryKey()
    {
        if(Input.GetButtonDown("MouseR"))
        {
            if(robot != null)
                robot.ActiveParry();
        }
    }

    private void CheckAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            robot.Attack(mouseAngle);
        }
    }

    //로봇 정령 회전
    void RotateRobot()
    {
        if (robotSpiritObject == null) return;
        if (cam == null)
        {
            Debug.LogWarning("MainCam is not Set!");
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
