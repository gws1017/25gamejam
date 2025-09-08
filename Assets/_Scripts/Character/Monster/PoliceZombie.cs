using UnityEngine;
using static AIController;

public class PoliceZombie : MonsterCharacter
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private AudioClip ShootFx;
    [SerializeField] private Transform firePoint;        // 총구 위치(자식 트랜스폼 할당)


    public override void Attack()
    {
        if (isAttacking) return;
        if (BulletPoolManager.Instance == null) return;
        base.Attack();
        isAttacking = true;

        // 플레이어 싱글톤이 없다면 안전 종료
        var player = PlayerCharacter.Instance;
        if (player == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[PoliceZombie] PlayerCharacter.Instance is null");
#endif
            isAttacking = false;
            return;
        }

        // 1) 발사 원점(반드시 적의 firePoint)
        Vector3 origin = (firePoint != null) ? firePoint.position : transform.position;

        // 2) "적 → 플레이어" 방향 계산
        Vector3 playerPos = player.transform.position;
        Vector2 dir = (playerPos - origin);
        if (dir.sqrMagnitude < 0.0001f)
        {
            // 혹시 완전히 겹쳐있거나 0벡터면 바라보는 방향으로 대체
            dir = transform.right;
        }
        dir.Normalize();

        // 3) 스폰 회전(스프라이트가 오른쪽을 기본으로 본다고 가정)
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.forward);

        // 4) 풀에서 탄환 꺼내기 → 초기화 → 자기 자신 무시 → 발사
        Bullet bullet = BulletPoolManager.Instance.Spawn(BulletType.Police, origin, rot);
        if (bullet != null)
        {
            bullet.Init(damage, gameObject);        // 데미지/사수 설정
            bullet.AddIgnoreObject(gameObject);     // 자기 자신 무시
            bullet.Fire(dir);                       // 이동 시작(위치 변경은 하지 않음)
        }

        // 5) 사운드 & 공격 딜레이
        SoundManager.Instance.PlaySoundFX(ShootFx, 0.1f);
        StartCoroutine(AttackDelayCorutine());
    }


}
