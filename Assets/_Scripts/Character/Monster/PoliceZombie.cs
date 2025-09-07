using UnityEngine;
using static AIController;

public class PoliceZombie : MonsterCharacter
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private AudioClip ShootFx;


    public override void Attack()
    {
        if (isAttacking) return;
        if (controller == null) return;                     // 타겟 방향 계산용 컨트롤러 필요
        if (BulletPool.Instance == null) return;            // 풀 준비 안 됨
        base.Attack();
        isAttacking = true;

        // 1) 발사 방향: "적 → 플레이어" (controller.TargetDir이 그 벡터를 제공한다고 가정)
        Vector2 targetDir = controller.TargetDir;
        if (targetDir.sqrMagnitude < 0.0001f)
        {
            // 안전장치: 타겟 방향이 0이면, 바라보는 방향/오른쪽 등 기본값 사용
            targetDir = transform.right;
        }
        targetDir.Normalize();

        // 2) 스폰 위치/회전: 반드시 "적의 firePoint" 기준
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
        float angleDeg = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.forward);

        // 3) 풀에서 탄환 꺼내기 → 초기화 → 자기 자신 무시 → 발사
        Bullet bullet = BulletPool.Instance.Spawn(poolIndexForEnemyBullet, spawnPos, rot);
        if (bullet != null)
        {
            bullet.Init(damage, gameObject);       // 사수/데미지 설정
            bullet.AddIgnoreObject(gameObject);    // 발사자 충돌 무시
            bullet.Fire(targetDir);                // 방향/속도 부여(탄환은 위치를 바꾸지 않음)
        }

        // 4) 사운드 & 쿨다운
        SoundManager.Instance.PlaySoundFX(ShootFx, 0.1f);
        StartCoroutine(AttackDelayCorutine());
    }


}
