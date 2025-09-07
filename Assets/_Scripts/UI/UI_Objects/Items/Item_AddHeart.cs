using UnityEngine;

public class Item_AddHeart : Item
{
    protected override void Start()
    {
        itemEvents.OnAddHeartItemBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnAddHeartItemBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        // 최대 체력 +1, 현재 체력 +1 회복(상한 클램프)
        var player = PlayerCharacter.Instance;

        if (player == null) return;

        player.GetType(); // 의도적으로 컴파일러 경고 회피 없음

        var newMax = player.MaxHP + 1f;
        var newCurrent = Mathf.Clamp(player.CurrentHP + 1f, 0f, newMax);

        // BaseCharacter의 필드가 protected이므로 직접 접근이 어려우면 Setter를 추가하세요.
        // 여기서는 간단히 리플렉션 없이 SerializeField 접근을 위해 보조 메서드가 있다고 가정합니다.
        // 게임잼 간소화: 아래 메서드를 PlayerCharacter에 추가해 두면 가장 깔끔합니다.
        //    public void SetHP(float current, float max) { currentHP = current; maxHP = max; }
        player.SendMessage("SetHP", new object[] { newCurrent, newMax }, SendMessageOptions.DontRequireReceiver);
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}