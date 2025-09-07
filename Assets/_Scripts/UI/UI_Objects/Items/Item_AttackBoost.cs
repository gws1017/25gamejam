using UnityEngine;

public class Item_AttackBoost : Item
{
    protected override void Start()
    {
        itemEvents.OnAttackBoostBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnAttackBoostBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        var effects = PlayerCharacter.Instance?.PlayerItemEffects;

        if (effects == null) return;
        // 실제 증감은 PlayerItemEffects가 코루틴으로 처리
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}