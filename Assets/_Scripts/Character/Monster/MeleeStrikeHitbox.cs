using UnityEngine;

/*****************************************************************************************
 * 파일: MeleeStrikeHitbox.cs
 * 역할: 근접 몬스터의 무기 판정(자식 오브젝트). 타격 순간에만 활성화되고, 그동안 패링할 수 있다.
 * 왜 몸체와 분리했나:
 *   - 기존 패링(PlayerController.TryParry)은 Parryable 레이어의 콜라이더를 찾아 그 오브젝트의
 *     IParryable을 호출한다. 몸체를 패링 대상으로 하면 공격 중이 아닐 때도 패링되고,
 *     악마가 근처에 있기만 해도 "패링 실패(코인 차감)"가 걸린다.
 *   - 그래서 무기 앞쪽에 붙은 별도 오브젝트를 판정 구간에만 활성화한다 → 패링 코드는 수정 없이 동작.
 * 두 가지 배치 방식:
 *   1) 무기 따라가기 (도끼 악마, 창병 악마): 계층이 몸체 → StrikePivot → StrikeHitbox.
 *      Attack 클립이 StrikeHitbox의 위치와 박스 크기를 프레임마다 움직여 무기(도끼날 / 창끝)를 따라간다
 *      (오른쪽을 볼 때 기준으로 키를 찍는다). 좌우 반전은 StrikePivot의 scale.x를 ±1로 바꿔서 처리.
 *   2) 고정 범위: 몸체 바로 아래에 두고 offset 위치에 고정 박스를 놓는다 (현재 쓰는 몹은 없음).
 * 에디터 세팅 (자식 오브젝트):
 *   - 레이어 Parryable (총알과 같은 레이어 — 플레이어 패링 탐색 대상)
 *   - BoxCollider2D (Is Trigger) — 판정 크기
 *   - Rigidbody2D (Kinematic) — 몸체 Rigidbody에 합쳐지지 않게 분리 (안 하면 무기 판정에 닿은
 *     플레이어 탄이 몸체의 피격 처리로 들어간다)
 *****************************************************************************************/

[RequireComponent(typeof(BoxCollider2D))]
public class MeleeStrikeHitbox : MonoBehaviour, IParryable
{
    [Tooltip("고정 범위 방식에서만 사용: 몸 중심에서 판정 중심까지의 거리 (오른쪽을 볼 때 기준, 왼쪽을 보면 x가 반전된다)")]
    [SerializeField] private Vector2 offset = new Vector2(0.55f, 0f);

    private BoxCollider2D box;
    private MeleeDevil owner;
    private Transform facingPivot;   // 무기 따라가기 방식일 때만 있음 (부모가 몸체가 아닌 중간 오브젝트)

    /// <summary>무기 판정이 활성화돼 있는가 (= 패링 가능 구간). IParryable 구현.</summary>
    public bool CanBeParried { get; private set; }
    public GameObject Causer => owner != null ? owner.gameObject : null;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.enabled = false;
        owner = GetComponentInParent<MeleeDevil>();

        // 부모가 몸체(MeleeDevil)가 아니면 중간 오브젝트(StrikePivot)로 보고 좌우 반전에 쓴다
        Transform parent = transform.parent;
        if (parent != null && parent.GetComponent<MeleeDevil>() == null) facingPivot = parent;
    }

    /// <summary>
    /// 바라보는 방향(1 오른쪽 / -1 왼쪽)으로 판정을 맞춘다. 공격 시작 시 한 번 호출.
    /// 무기 따라가기 방식은 위치를 클립이 정하므로 좌우만 뒤집고, 고정 범위 방식은 앞쪽 위치에 놓는다.
    /// </summary>
    public void SetFacing(int facing)
    {
        if (facingPivot != null)
            facingPivot.localScale = new Vector3(facing, 1f, 1f);
        else
            transform.localPosition = new Vector3(offset.x * facing, offset.y, 0f);
    }

    /// <summary>무기 판정 활성화 — 여기서부터 패링 가능.</summary>
    public void ActivateHitbox()
    {
        CanBeParried = true;
        box.enabled = true;
    }

    /// <summary>무기 판정 비활성화 — 패링 불가.</summary>
    public void DeactivateHitbox()
    {
        CanBeParried = false;
        if (box != null) box.enabled = false;
    }

    /// <summary>
    /// 판정 박스가 플레이어 콜라이더와 겹치는가. 콜라이더를 끈 뒤에도 쓸 수 있게
    /// (꺼진 콜라이더의 bounds는 비어 있다) 박스 크기를 직접 계산한다.
    /// </summary>
    public bool CollidesWithPlayer(PlayerCharacter player)
    {
        if (player == null) return false;
        var playerColl = player.GetComponent<Collider2D>();
        if (playerColl == null) return false;

        Vector3 scale = transform.lossyScale;
        var strikeBounds = new Bounds(transform.TransformPoint(box.offset),
                                      new Vector3(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y), 1f));
        Bounds playerBounds = playerColl.bounds;
        playerBounds.size = new Vector3(playerBounds.size.x, playerBounds.size.y, 1f);
        strikeBounds.center = new Vector3(strikeBounds.center.x, strikeBounds.center.y, playerBounds.center.z);
        return strikeBounds.Intersects(playerBounds);
    }

    public void OnParried(Vector2 parryPos)
    {
        if (!CanBeParried) return;
        if (owner != null) owner.OnStrikeParried();
    }
}
