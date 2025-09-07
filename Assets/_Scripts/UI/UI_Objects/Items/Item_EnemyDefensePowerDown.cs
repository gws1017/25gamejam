/* 4-6) 5초간 적 방어력 30% 감소 */
using UnityEngine;

public class Item_EnemyDefensePowerDown : Item
{
    protected override void Start()
    {
        itemEvents.OnEnemyDefenseDownBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnEnemyDefenseDownBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        if (EnemyGlobalEffects.Instance == null) return;
        // 실제 방어력 감소는 EnemyGlobalEffects에서 코루틴 처리
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}