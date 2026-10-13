using UnityEngine;

/*****************************************************************************************
 * 파일: Spitter.cs
 * 역할: 스테이지 2 원거리 몬스터 — 제자리에서 3way 부채꼴 산탄 발사
 * 에디터 세팅:
 *   - firePoint: 총구 위치(자식 트랜스폼). 비워두면 자기 위치에서 발사
 *   - bulletPoolKey: PoolManager에 등록한 키와 동일하게 입력
 *   - 프리팹 수치(기획): maxHP 18 / speed 0 / attackRange 6 / attackCoolTime 2.5 / dropExp 25
 * 메모:
 *   - 탄속은 Bullet 프리팹에서 설정한다(기획값 6). 기본 8보다 느리게 잡아야
 *     탄막이 화면에 쌓이고 패링 타이밍도 잡힌다.
 *   - Bullet.OnParried()가 이미 구현되어 있어 패링 반사가 동작한다.
 *****************************************************************************************/

public class Spitter : MonsterCharacter
{
    [Header("Spitter")]
    [SerializeField] private Transform firePoint;          // 총구 위치(자식 트랜스폼 할당)
    [SerializeField] private string bulletPoolKey = "SpitterBullet";
    [SerializeField] private AudioClip shootFx;

    [Tooltip("산탄 발수")]
    [SerializeField] private int pelletCount = 3;
    [Tooltip("인접한 탄 사이의 각도(도)")]
    [SerializeField] private float spreadAngle = 18f;

    public override void Attack()
    {
        if (isAttacking) return;
        if (PoolManager.Instance == null) return;
        base.Attack();

        var player = PlayerCharacter.Instance;
        if (player == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[Spitter] PlayerCharacter.Instance is null");
#endif
            isAttacking = false;
            return;
        }

        // 1) 발사 원점
        Vector3 origin = (firePoint != null) ? firePoint.position : transform.position;

        // 2) "적 → 플레이어" 기준 방향
        Vector2 baseDir = (Vector2)player.transform.position - (Vector2)origin;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = transform.right;   // 완전히 겹쳐있으면 바라보는 방향으로 대체
        baseDir.Normalize();

        // 3) 가운데 탄이 플레이어를 향하도록 좌우 대칭으로 각도를 배분
        //    (3way면 startOffset = -18 → -18 / 0 / +18)
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startOffset = -spreadAngle * (pelletCount - 1) * 0.5f;

        bool fired = false;
        for (int i = 0; i < pelletCount; i++)
        {
            float angle = baseAngle + startOffset + spreadAngle * i;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector2 dir = rot * Vector3.right;

            Bullet bullet = PoolManager.Instance.Spawn<Bullet>(origin, rot, bulletPoolKey);
            if (bullet == null) continue;

            bullet.Init(damage, gameObject);    // 데미지/사수 설정
            bullet.AddIgnoreObject(gameObject); // 자기 자신 무시
            bullet.Fire(dir);                   // 이동 시작
            fired = true;
        }

        // 풀이 비어 한 발도 못 나갔으면 쿨타임을 소모하지 않고 다음 프레임에 재시도
        if (!fired) return;

        isAttacking = true;
        SoundManager.Instance.PlaySoundFX(shootFx, 0.1f);
        StartCoroutine(AttackDelayCorutine());
    }
}
