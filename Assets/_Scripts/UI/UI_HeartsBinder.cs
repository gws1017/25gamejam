using UnityEngine;

/// <summary>
/// PlayerCharacter의 체력 변경 이벤트를 구독해
/// 하트 칸을 '켜고/끄는' 방식으로 갱신한다.
/// </summary>
[RequireComponent(typeof(Hearts))]
public class UI_HeartsBinder : MonoBehaviour
{
    [SerializeField] private int totalHeartCount = 3; // 하트 칸 수(요구: 3)
    [SerializeField] PlayerCharacter playerCharacter; 

    private Hearts hearts;
    private Heart[] heartArray;

    private void Awake()
    {
        hearts = GetComponent<Hearts>();
    }

    private void OnEnable()
    {
        if (hearts != null)
        {
            hearts.SetHearts();
            heartArray = hearts.GetHearts();
        }
    }

    private void OnDisable()
    {
        if (playerCharacter != null)
            playerCharacter.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float currentHP, float maxHP)
    {
        if (heartArray == null || heartArray.Length == 0) return;

        int heartCount = Mathf.Max(1, totalHeartCount);
        float hpPerHeart = maxHP / heartCount;

        // 채워져야 하는 하트 ‘개수’를 정수로 계산
        // - 0.0001f 더해서 경계값(예: 2.0)이 안전하게 해당 칸으로 들어가게 함
        int filledCount = Mathf.Clamp(
            Mathf.FloorToInt((currentHP + 0.0001f) / hpPerHeart),
            0, heartCount
        );

        // i번째 하트는 i < filledCount이면 켠다
        for (int i = 0; i < heartArray.Length; i++)
        {
            if (heartArray[i] == null) continue;
            bool shouldBeFilled = (i < filledCount);
            heartArray[i].SetFilled(shouldBeFilled);
        }
    }
}
