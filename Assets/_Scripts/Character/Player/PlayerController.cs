using System.IO.Pipes;
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

    [Header("Player Shooting")]
    [SerializeField] private Transform firePoint;        // 총구 위치(자식 트랜스폼 할당)
    [SerializeField] private float fireCooldown = 0.001f; // 연사 간격(초)
    [SerializeField] private bool autoFire = true;       // true: 스페이스 꾹=연사, false: 단발
    private float fireTimer;                             // 쿨다운 타이머


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

    private PlayerItemEffects playerItemEffects;
    private PlayerCharacter playerCharacter;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        robot = GetComponentInChildren<RobotSpirit>();
        playerItemEffects = GetComponent<PlayerItemEffects>();
        playerCharacter = GetComponent<PlayerCharacter>();
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

        CountDownFireTimer();
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
        // 1) 수평/수직 입력 읽기
        horizontalAxis = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        verticalAxis = Input.GetAxisRaw("Vertical");   // -1, 0, 1

        // 2) 마우스 관련
        if (cam != null)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = cam.WorldToScreenPoint(transform.position).z;
            mouseWorld = cam.ScreenToWorldPoint(mouseScreenPosition);
            mouseDir = mouseWorld - transform.position;
            mouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        }

        // 3) 축 입력의 눌림/뗌 상태
        bool horizontalDown = Input.GetButtonDown("Horizontal");
        bool verticalDown = Input.GetButtonDown("Vertical");
        bool verticalUp = Input.GetButtonUp("Vertical");
        bool horizontalUp = Input.GetButtonUp("Horizontal");

        // 4) 최근에 시작된 이동 방향 축(필요 시 유지)
        if (horizontalDown)
            isHorizonMove = true;
        else if (verticalDown)
            isHorizonMove = false;
        else if (horizontalUp || verticalUp)
            isHorizonMove = horizontalAxis != 0;

        // 5) 바라보는 방향(좌우) 갱신: 수평 입력이 있을 때만 갱신
        if (horizontalAxis != 0)
        {
            lastHorzDir = (int)horizontalAxis; // -1 또는 1
            dirVec = lastHorzDir < 0 ? Vector3.left : Vector3.right;
        }

        // 6) 애니메이션 파라미터(수평)
        if (anim.GetInteger("horizonAxis") != (int)horizontalAxis)
            anim.SetInteger("horizonAxis", (int)horizontalAxis);

        // 7) 스프라이트 좌우 뒤집기(수직 이동만 할 때는 마지막 좌우를 유지)
        if (dirVec == Vector3.left)
            spriteRenderer.flipX = true;
        else if (dirVec == Vector3.right)
            spriteRenderer.flipX = false;

        // 8) 공격/패링 입력
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
            Debug.Log("Attack");
        }

        if (autoFire)
        {
            if (Input.GetKey(KeyCode.Space)) 
                TryShoot();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
                TryShoot();
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

    /// <summary>
    /// 탄환 발사 핵심
    /// </summary>
    private void TryShoot()
    {
        if (firePoint == null || BulletPool.Instance == null) return; // 세팅 안 되었으면 종료
        if (fireTimer > 0f) return;                                   // 쿨다운 중이면 무시

        if (playerCharacter == null) return;

        // 발사 방향 결정(수평만):
        // - lastHorzDir 은 기존 CheckInput()에서 갱신됨(좌:-1, 우:1)
        // - 정지 상태에서 쏘면 마지막 바라본 방향으로 발사
        int facingDirection = lastHorzDir != 0 ? 
                              lastHorzDir : (spriteRenderer != null && spriteRenderer.flipX ? 
                                                                            -1 : 1);
        Vector2 fireDirection = new Vector2(facingDirection, 0f);           // 수평 직선 방향(정규화 불필요: (+-1,0))

        // 발사 위치: firePoint.position 그대로 사용(트랜스폼 고정)
        Vector3 spawnPos = firePoint.position;

        float baseDamage = playerCharacter.Damage;                              // 탄환 데미지 = 플레이어 공격력
        float attackMultiplier = (playerItemEffects != null) ? 
                                 playerItemEffects.AttackMultiplier : 1f;
        float finalBulletDamage = baseDamage * attackMultiplier;

#if UNITY_EDITOR
        Debug.Log($"[Shoot] BaseDamage={baseDamage}, AttackMultiplier={attackMultiplier}, FinalBulletDamage={finalBulletDamage}");
#endif

        // 풀에서 탄환 꺼내기 → 초기화 → 발사
        Bullet bullet = BulletPool.Instance.Spawn(firePoint.position, Quaternion.identity);
        bullet.Init(finalBulletDamage, gameObject); // 사수 등록(자기 자신 피격 방지)
        bullet.Fire(fireDirection);

        fireTimer = fireCooldown;   // 쿨다운 재시작
    }

    private void CountDownFireTimer()
    {
        fireTimer -= Time.deltaTime;
    }
}
