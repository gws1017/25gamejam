using UnityEngine;
using UnityEngine.UI;

/*****************************************************************************************
 * 파일: UI_BossHealthBar.cs
 * 역할: 화면 상단 보스 체력바. 보스가 스폰되면 뜨고, 죽으면 숨는다.
 * 에디터 세팅:
 *   - barRoot: 평소엔 숨길 바의 자식(배경/Fill 이미지들)을 담은 오브젝트.
 *     이 스크립트가 붙은 오브젝트를 그대로 넣어도 된다 — SetActive가 아니라
 *     그 밑의 Image 컴포넌트들만 enabled를 꺼서 숨기므로, 스크립트 자신이
 *     꺼져서 이벤트를 못 받는 문제가 없다.
 *   - fillImage: Image.Type = Filled, Fill Method = Horizontal
 *****************************************************************************************/

public class UI_BossHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject barRoot;
    [SerializeField] private Image fillImage;

    private Boss currentBoss;

    private void OnEnable()
    {
        MonsterCharacter.OnMonsterSpawned += HandleSpawned;
        MonsterCharacter.OnMonsterDied += HandleDied;

        SetBarVisible(false);
    }

    private void OnDisable()
    {
        MonsterCharacter.OnMonsterSpawned -= HandleSpawned;
        MonsterCharacter.OnMonsterDied -= HandleDied;

        UnbindCurrentBoss();
    }

    #region Internal Logic
    private void HandleSpawned(MonsterCharacter monster)
    {
        if (monster is not Boss boss) return;

        UnbindCurrentBoss(); // 혹시 이전 보스가 안 치워졌으면 정리

        currentBoss = boss;
        currentBoss.OnHealthChanged += HandleHealthChanged;

        if (fillImage != null) fillImage.fillAmount = 1f;
        SetBarVisible(true);
    }

    private void HandleDied(MonsterCharacter monster)
    {
        if (monster == null || currentBoss == null) return;
        if (!ReferenceEquals(monster, currentBoss)) return;

        UnbindCurrentBoss();
        SetBarVisible(false);
    }

    /// <summary>barRoot(자신 포함 가능) 밑의 Image들만 enabled를 토글한다.
    /// barRoot 자체를 SetActive하면 스크립트가 붙은 오브젝트가 꺼질 때 이 스크립트도
    /// 같이 비활성화되어 다시는 이벤트를 못 받게 되므로, GameObject 활성 상태는 건드리지 않는다.</summary>
    private void SetBarVisible(bool visible)
    {
        if (barRoot == null) return;
        var images = barRoot.GetComponentsInChildren<Image>(true);
        foreach (var img in images) img.enabled = visible;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (fillImage == null || max <= 0f) return;
        fillImage.fillAmount = Mathf.Clamp01(current / max);
    }

    private void UnbindCurrentBoss()
    {
        if (currentBoss == null) return;
        currentBoss.OnHealthChanged -= HandleHealthChanged;
        currentBoss = null;
    }
    #endregion
}
