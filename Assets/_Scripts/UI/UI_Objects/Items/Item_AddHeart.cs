using UnityEngine;

public class Item_AddHeart : Item
{
    protected override void Start()
    {
        itemEvents.OnAddHeartItemBought += ItemEvents_OnAddHeartItemBought;
    }

    private void ItemEvents_OnAddHeartItemBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    protected override void Update()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDestroy()
    {
        itemEvents.OnAddHeartItemBought -= ItemEvents_OnAddHeartItemBought;
    }

    public override void ApplyItemEffects()
    {
        // player.Stats.IncreaseMaxHealth(1);
    }

    public override void RemoveItemEffects()
    {
        throw new System.NotImplementedException();
    }

    public override bool CheckItemPurchasability()
    {
        // 1. 플레이어 체력이 최대치가 아니어야 함
        // 2. 플레이어가

        return true;
    }
}
