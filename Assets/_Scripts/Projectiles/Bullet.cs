using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IParryable
{
    [SerializeField] protected float speed = 8f;     // 탄환 속도
    [SerializeField] protected float lifetime = 3f;  // 탄환 생존 시간(초)
    [SerializeField] protected float damage = 1f;    // 탄환 기본 데미지

    // 충돌 처리 무시할 오브젝트 등록(본인, 무기등)
    [SerializeField] private List<GameObject> ignoreObjects = new List<GameObject>();

    [SerializeField] LayerMask ignoreMask; 

    public GameObject Causer { get; private set; } // 발사자
    protected Rigidbody2D rb;
    protected Coroutine lifeRoutine;

    public float Speed => speed;
    public float Damage => damage;
    public bool CanBeParried { get; private set; } = true; // 패링 가능 여부

    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start() { }

    protected void OnEnable()
    {
        // 풀에서 재사용될 때 상태 초기화
        CanBeParried = true;
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        lifeRoutine = StartCoroutine(LifeTimer()); // 타이머 시작
    }

    protected void OnDisable()
    {
        // 풀로 되돌아갈 때 깔끔히 리셋
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        rb.linearVelocity = Vector2.zero;
        ignoreObjects.Clear();
        Causer = null;
    }

    // 탄환 초기화(데미지/발사자 설정). 스폰 직후에 반드시 호출.
    public void Init(float dmg, GameObject causer)
    {
        damage = dmg;
        Causer = causer;
        AddIgnoreObject(causer); // 스폰 직후 발사자와는 충돌하지 않도록 등록
    }

    // 특정 방향(dir는 정규화 벡터)에 발사
    public void Fire(Vector2 dir)
    {
        rb.linearVelocity = dir * speed; // 물리 속도 직접 설정(일관된 이동)
        // 시각적 정렬(선택): 이동 방향을 바라보게 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // 생존 시간 경과 시 풀로 반환
    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        BulletPool.Instance.Despawn(this);
    }

    // 무시할 오브젝트(충돌 제외) 동적 등록
    public void AddIgnoreObject(GameObject obj)
    {
        if (!ignoreObjects.Contains(obj))
            ignoreObjects.Add(obj);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 투사체 끼리는 충돌체크 하지 말 것
        if (collision.CompareTag("Projectile")) return;

        // 무시할 레이어에 속한 오브젝트와의 충돌은 무시
        int playerLayer = LayerMask.NameToLayer("Player");
        if (collision.gameObject.layer == playerLayer) return; // 무시

        // 충돌 처리 무시 오브젝트 체크
        foreach (var ignore in ignoreObjects)
            if (ignore == collision.gameObject) return;

        // 데미지 계산/적용은 피격자 쪽에서 처리하고, 탄환은 여기서 수거
        BulletPool.Instance.Despawn(this);
    }

    // 패링 처리(기존 설계 유지): 반사 방향으로 재가속 + 수명 리셋 + 무시목록 초기화
    public void OnParried(Vector2 parryPos)
    {
        Vector2 incoming = rb.linearVelocity.normalized;                 // 기존 진행 방향
        Vector2 normal = (transform.position - (Vector3)parryPos).normalized; // 반사면 노멀
        Vector2 reflectDir = Vector2.Reflect(incoming, normal);          // 반사 방향 계산

        rb.linearVelocity = reflectDir * speed; // 반사 가속
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        lifeRoutine = StartCoroutine(LifeTimer()); // 수명 재시작
        ignoreObjects.Clear(); // 원래 사수도 다시 맞도록 초기화
    }
}

