using System.Collections;
using UnityEngine;

public class RobotSpirit : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Transform firePointTransform;   // 총구 위치(비워두면 현재 트랜스폼 사용)
    [SerializeField] private Camera targetCamera;            // 마우스 기준 카메라(비워두면 Camera.main)

    [Header("Combat Values")]
    [SerializeField] private int damageAmount = 5;           // 로봇 정령 탄환 데미지

    [Header("Parry")]
    [SerializeField] private float parryTimeSeconds = 0.2f;  // 패링 가능 시간
    [SerializeField] private bool isParrying = false;        // 패링 가능 상태

    private Coroutine parryCoroutine;
    public bool hasParried = false;                          // 패링 연속 입력 방지
    public bool detectedParryTarget = false;                 // 패링 중 대상 감지 여부

    public bool IsParrying => isParrying;
    public int Damage => damageAmount;

    // 공격: 마우스 월드 위치를 향해 발사 (angle 파라미터는 더 이상 사용하지 않음)
    public void Attack(float _ignoredAngle)
    {
        // 1) 총구 위치 계산
        Vector3 spawnWorldPosition = (firePointTransform != null ? firePointTransform.position : transform.position);

        // 2) 카메라 확보
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        // 3) 마우스 월드 좌표 계산
        Vector3 mouseScreenPosition = Input.mousePosition;
        // 화면 → 월드 변환용 z 깊이(카메라와의 거리) 맞추기
        mouseScreenPosition.z = cam.WorldToScreenPoint(spawnWorldPosition).z;
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);

        // 4) 발사 방향(마우스 방향) 계산
        Vector2 fireDirection = (mouseWorldPosition - spawnWorldPosition).normalized;

        // 5) 풀에서 탄환 꺼내 초기화 후 발사
        if (BulletPool.Instance == null) return;
        Bullet bullet = BulletPool.Instance.Spawn(spawnWorldPosition, Quaternion.identity);
        GameObject playerObject = transform.parent != null ? transform.parent.gameObject : gameObject;

        bullet.Init(damageAmount, playerObject);
        bullet.AddIgnoreObject(gameObject);    // 본인(로봇 정령) 무시
        if (transform.parent != null) bullet.AddIgnoreObject(playerObject); // 플레이어 무시

        bullet.Fire(fireDirection);            // 마우스 방향으로 직선 발사
    }

    // 패링 상태 초기화
    public void ClearParryFlags()
    {
        detectedParryTarget = false;
        hasParried = false;
    }

    // 패링 시작
    public void ActiveParry()
    {
        if (parryCoroutine != null) return;
        parryCoroutine = StartCoroutine(ActivateParryCoroutine());
    }

    private IEnumerator ActivateParryCoroutine()
    {
        isParrying = true;
        ClearParryFlags();

        yield return new WaitForSeconds(parryTimeSeconds);
        isParrying = false;

        // 주변에 패링 타겟이 있었으나 거리가 멀어 실패한 경우
        if (hasParried == false && detectedParryTarget)
        {
            Debug.Log("패링 실패");
            GetComponentInParent<PlayerCharacter>().ApplyDamage();
        }
        parryCoroutine = null;
    }
}
