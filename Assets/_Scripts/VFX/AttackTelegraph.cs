using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*****************************************************************************************
 * 파일: AttackTelegraph.cs
 * 역할: 공격 예고(텔레그래프) 공용 컴포넌트 — 테두리만 있다가 서서히 차오르고,
 *       다 차면 OnComplete 이벤트를 발행한다. 보스 패턴2(브레스/솟구침)에서 재사용.
 * 에디터 세팅:
 *   - World Space Canvas 밑에 Image를 놓고, Image.Type = Filled로 설정
 *     (솟구침: Fill Method = Radial 360 / 브레스: Fill Method = Horizontal)
 *   - fillImage 슬롯에 그 Image 연결
 *   - 색은 현재 단색 빨강 placeholder로 충분 (추후 실제 삽화로 교체 예정)
 * 사용 예:
 *   var tg = Instantiate(telegraphPrefab, position, Quaternion.identity);
 *   tg.Begin(1.2f);
 *   tg.OnComplete += () => { // 여기서 실제 공격 판정 처리 };
 *****************************************************************************************/

public class AttackTelegraph : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public event Action OnComplete;

    /// <summary>텔레그래프 시작. duration초 동안 fillAmount가 0→1로 채워진 뒤 OnComplete 발행.</summary>
    public void Begin(float duration)
    {
        StartCoroutine(FillRoutine(duration));
    }

    private IEnumerator FillRoutine(float duration)
    {
        float elapsed = 0f;
        fillImage.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fillImage.fillAmount = 1f;
        OnComplete?.Invoke();

        // 텔레그래프 자체는 공격 발동과 함께 사라짐 (실제 공격 VFX/판정은 호출자가 처리)
        Destroy(gameObject);
    }
}
