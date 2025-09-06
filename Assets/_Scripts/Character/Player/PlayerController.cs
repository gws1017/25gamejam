using UnityEngine;

public class PlayerController : BaseController
{
    [Header("Component")]
    [SerializeField] private GameObject robotSpiritObject;
    [SerializeField] private Camera cam;

    [Header("Property")]
    [SerializeField] private float speed = 4f; // �÷��̾� �̵� �ӵ�
    [SerializeField] private float radius = 2f; // �κ� ���� ������

    [Header("Input Raw Data")]
    [SerializeField] private float horizontalAxis;
    [SerializeField] private float verticalAxis;
    [SerializeField] private Vector3 mouseWorld;
    [SerializeField] private Vector2 mouseDir;
    [SerializeField] private float mouseAngle;

    [SerializeField] private bool isHorizonMove;

    [SerializeField] private Vector3 dirVec;
    [SerializeField] private int lastHorzDir = 1; // 1: ������, -1: ����

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
        var player = GetComponent<PlayerCharacter>();
        if (player == null) return;
        if (player.IsDead == true) return;

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

    }

    //Ű�Է� üũ
    void CheckInput()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        //���콺 ������ǥ ���
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

    //�κ� ���� ȸ��
    void RotateRobot()
    {
        if (robotSpiritObject == null) return;
        if (cam == null)
        {
            Debug.Log("�÷��̾� ��Ʈ�ѷ��� ī�޶� Set �����ʾҽ��ϴ�.");
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
