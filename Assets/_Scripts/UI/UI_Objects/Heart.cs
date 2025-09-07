// Heart.cs — 하트 UI(체력칸) 비주얼 연출 + 즉시 논리상태 캐시
// - 기능 요약:
//   1) 힐(회복): 페이드-인 + 컬러 플래시 + 바운스(팝) 스케일
//   2) 데미지: 스케일 펀치 + 회전 펀치 + 붉은 플래시 + 페이드-아웃 후 비활성화
//   3) isOnCached(논리 상태 캐시)로 애니메이션 중에도 다음 타격이 즉시 반영되도록 처리

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Heart : MonoBehaviour
{
    [Header("Heart parts")]
    [SerializeField] private GameObject heartFillObject; // 실제로 켜고/끄는 '채워진 하트' 오브젝트(비주얼 토글 대상)
    [SerializeField] private Graphic heartFillGraphic;   // 컬러/알파 트윈용 Graphic (Image/Text 등 Graphic 기반)

    [Header("DOTween Settings")]
    [SerializeField] private bool useDoTween = true;            // DOTween 사용 여부(끄면 즉시 on/off만 수행)
    [SerializeField] private float healPopDuration = 0.18f;     // 힐 팝(바운스) 시간
    [SerializeField] private float healSettleDuration = 0.10f;  // 팝 이후 정착 시간
    [SerializeField] private float damagePunchDuration = 0.14f; // 데미지 펀치 시간
    [SerializeField] private Vector2 damagePunchScale = new Vector2(-0.22f, -0.22f); // 움츠렸다가 복원(음수 = 줄었다가 복귀)
    [SerializeField] private float rotatePunchZ = 10f;          // 데미지 시 Z축 회전 펀치 강도(도 단위)
    [SerializeField] private float fadeInDuration = 0.10f;      // 힐 시 페이드-인 시간
    [SerializeField] private float fadeOutDuration = 0.12f;     // 데미지 시 페이드-아웃 시간
    [SerializeField] private float flashDuration = 0.06f;       // 컬러 플래시 지속 시간(힐/데미지 공통)
    [SerializeField] private Ease healEase = Ease.OutBack;      // 힐 스케일 이징
    [SerializeField] private Ease damageEase = Ease.OutCubic;   // 데미지 스케일 이징

    [Header("Flash Colors")]
    [SerializeField] private Color healFlashColor = new Color(1f, 0.95f, 0.6f, 1f); // 힐 플래시 컬러
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.4f, 0.4f, 1f);// 데미지 플래시 컬러

    private Sequence activeSequence;          // 현재 재생 중인 시퀀스(중복 트윈 방지)
    private RectTransform rectTransformCache; // RectTransform 캐시

    // 논리적 상태 캐시: activeSelf를 그대로 믿지 않고 'UI 논리 상태'를 즉시 기록
    // → 애니메이션이 끝나기 전에도 다음 타격 로직이 정확히 판단할 수 있게 함
    private bool isOnCached;

    private void Reset()
    {
        // 에디터에서 HeartFill/Fill 이름의 자식 자동 연결
        if (heartFillObject == null)
        {
            var t = transform.Find("HeartFill") ?? transform.Find("Fill");
            if (t != null) heartFillObject = t.gameObject;
        }
        AutoWireGraphic(); // Graphic도 자동 추적 시도
    }

    private void Awake()
    {
        rectTransformCache = transform as RectTransform;

        // heartFillGraphic 자동 추적(직접 할당 안 해도 동작)
        AutoWireGraphic();

        // 시작 시 비주얼 상태를 읽어 논리 캐시에 반영
        isOnCached = (heartFillObject != null && heartFillObject.activeSelf);
    }

    // heartFillGraphic 자동 배선: heartFillObject에 Graphic이 없으면 자식에서도 탐색
    private void AutoWireGraphic()
    {
        if (heartFillObject == null) return;
        if (heartFillGraphic == null)
        {
            heartFillGraphic = heartFillObject.GetComponent<Graphic>();
            if (heartFillGraphic == null)
                heartFillGraphic = heartFillObject.GetComponentInChildren<Graphic>(true);
        }
    }

    /// <summary> 현재 하트가 '켜져있는(채워진)' 논리 상태인지 반환 </summary>
    public bool IsFilled() => isOnCached;

    /// <summary> 동의어(가독성용) </summary>
    public bool IsOn => IsFilled();

    /// <summary>
    /// 안전 토글(상태가 다를 때만 바꾸기). 내부에서 애니/비주얼 처리까지 수행.
    /// </summary>
    public bool TrySetOn(bool on)
    {
        if (heartFillObject == null) return false;
        if (isOnCached == on) return false;  // 중복 토글 방지(논리 캐시 기준)
        SetFilled(on);
        return true;
    }

    // ====================== EFFECT CORE ======================
    /// <summary>
    /// 하트 켜기/끄기(애니 포함). instant=true면 즉시(on/off) 연출로 건너뜀.
    /// - isOnCached(논리 상태)를 먼저 갱신하여 연속 타격 시 정확한 하트 선택 유도.
    /// - useDoTween=false, Graphic 없음, RectTransform 없음 등 특수 상황은 즉시 토글.
    /// </summary>
    public void SetFilled(bool isFilled, bool instant = false)
    {
        if (heartFillObject == null) return;
        if (isOnCached == isFilled) return;

        // 1) 논리 상태를 즉시 갱신 → 다음 히트에서 올바른 하트가 선택됨
        isOnCached = isFilled;

        // 2) 기존 트윈 중단(중첩 방지). Kill(false): 완료 콜백은 호출 안 함
        if (activeSequence != null && activeSequence.IsActive())
            activeSequence.Kill(false);

        // 3) 트윈 사용 불가/비활성 플래그/즉시토글/Graphic 부재 → 즉시 on/off
        if (!useDoTween || rectTransformCache == null || instant || heartFillGraphic == null)
        {
            heartFillObject.SetActive(isFilled);
            rectTransformCache.localScale = Vector3.one;

            // Graphic 존재 시 알파도 즉시 반영(켜질 때 1, 꺼질 때 0)
            if (heartFillGraphic != null)
            {
                var color = heartFillGraphic.color;
                heartFillGraphic.color = new Color(color.r, color.g, color.b, isFilled ? 1f : 0f);
            }
            return;
        }

        // 원본 컬러 기억(플래시 후 복원 용도)
        var originalColor = heartFillGraphic.color;

        if (isFilled)
        {
            // ---------- HEAL FLOW ----------
            // 1) SetActive(true)로 먼저 켬(보여야 페이드-인이 의미가 있음)
            heartFillObject.SetActive(true);

            // 2) 초기에 알파 0으로 만들어 '서서히 차오르는' 느낌
            heartFillGraphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            // 3) 작게 시작해서 팝업(바운스) → 정착
            rectTransformCache.localScale = Vector3.one * 0.7f;

            activeSequence = DOTween.Sequence()
                .SetLink(gameObject) // 부모 GO가 비활성/파괴되면 트윈도 정리
                                     // 컬러 플래시 → 다시 원본톤(알파0)으로 되돌림
                .Append(heartFillGraphic.DOColor(healFlashColor, flashDuration))
                .Append(heartFillGraphic.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0f), flashDuration))
                // 페이드-인과 동시에 팝(바운스) 스케일
                .Join(heartFillGraphic.DOFade(1f, fadeInDuration))
                .Join(rectTransformCache.DOScale(1.16f, healPopDuration).SetEase(healEase))
                // 살짝 되돌아오며 정착
                .Append(rectTransformCache.DOScale(1.0f, healSettleDuration).SetEase(Ease.OutCubic));
            // .SetUpdate(true) // 타임스케일 0에서도 재생하려면 주석 해제
        }
        else
        {
            // ---------- DAMAGE FLOW ----------
            // 1) 기본 스케일로 맞춘 후 시작(연속 히트에도 안정적)
            rectTransformCache.localScale = Vector3.one;

            activeSequence = DOTween.Sequence()
                .SetLink(gameObject)
                // 스케일 펀치(움츠렸다 복원)
                .Join(rectTransformCache.DOPunchScale(
                        new Vector3(damagePunchScale.x, damagePunchScale.y, 0f),
                        damagePunchDuration, vibrato: 8, elasticity: 0.6f).SetEase(damageEase))
                // 회전 펀치로 타격감 보강
                .Join(rectTransformCache.DOPunchRotation(
                        new Vector3(0f, 0f, -rotatePunchZ),
                        damagePunchDuration * 0.95f, vibrato: 6, elasticity: 0.5f))
                // 붉은 플래시 → 원본 컬러로 복귀
                .Append(heartFillGraphic.DOColor(damageFlashColor, flashDuration))
                .Append(heartFillGraphic.DOColor(originalColor, flashDuration))
                // 페이드-아웃(사라지는 느낌)
                .Join(heartFillGraphic.DOFade(0f, fadeOutDuration))
                // 완료 시 실제 비주얼 비활성화 + 다음을 위한 상태 복원
                .AppendCallback(() =>
                {
                    heartFillObject.SetActive(false);
                    heartFillGraphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // 다음에 켤 때 바로 보이도록 알파=1 복원
                    rectTransformCache.localScale = Vector3.one;
                });
            // .SetUpdate(true) // 타임스케일 0에서도 재생하려면 주석 해제
        }
    }
}

// 튜닝 가이드
// - 더 강한 타격감: damagePunchScale (-0.28~-0.35), rotatePunchZ (12~18)
// - 더 부드러운 페이드: fadeIn/OutDuration 0.14~0.20
// - 여러 칸이 연속으로 줄어들 때는 Hearts 쪽에서 도미노(스태거) 호출로 0.03~0.06 간격 추천
// - 타임스케일 0(일시정지)에서 UI 재생하려면 각 시퀀스에 .SetUpdate(true) 추가
