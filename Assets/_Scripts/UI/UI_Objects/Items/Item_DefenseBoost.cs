/* 4-4) 10초간 방어력 40% 상승 */
using UnityEngine;

public class Item_DefenseBoost : Item
{
    protected override void Start()
    {
        itemEvents.OnDefenseBoostBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnDefenseBoostBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        var effects = PlayerCharacter.Instance?.PlayerItemEffects;
        if (effects == null) return;
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}