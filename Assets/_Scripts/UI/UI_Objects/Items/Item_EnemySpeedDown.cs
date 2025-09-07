using UnityEngine;

public class Item_EnemySpeedDown : Item
{
    protected override void Start()
    {
        itemEvents.OnEnemySpeedDownBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnEnemySpeedDownBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        if (EnemyGlobalEffects.Instance == null) return;
        // 실제 감속은 EnemyGlobalEffects에서 코루틴 처리
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}