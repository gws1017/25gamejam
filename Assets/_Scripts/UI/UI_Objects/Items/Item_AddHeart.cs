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
        // 구매 버튼 클릭 → 이벤트 수신 → 효과 적용
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        var player = PlayerCharacter.Instance;
        if (player == null) return;

        // 풀피면 구매 불가라면 여기서 리턴
        if (player.CurrentHP >= player.MaxHP) return;

        // 하트 1칸 회복 (PlayerCharacter가 하트 토글/HP갱신 처리)
        player.Heal(1f);
    }

    public override void RemoveItemEffects() { }

    // “하트가 닳았을 때만” 구매 가능
    public override bool CheckItemPurchasability()
    {
        var p = PlayerCharacter.Instance;
        return p != null && p.CurrentHP < p.MaxHP;
    }
}
