using UnityEngine;

public class Item_Invincibility : Item
{
    protected override void Start()
    {
        itemEvents.OnInvincibilityBought += OnBought;
    }

    protected override void Update() { }

    protected override void OnDestroy()
    {
        itemEvents.OnInvincibilityBought -= OnBought;
    }

    private void OnBought(object sender, System.EventArgs e)
    {
        ApplyItemEffects();
    }

    public override void ApplyItemEffects()
    {
        // PlayerItemEffects가 실제 무적 타이머를 처리
        var effects = PlayerCharacter.Instance?.PlayerItemEffects;

        if (effects == null) return;
        // 이벤트만 던지면 PlayerItemEffects가 구독해 처리함 (이미 연결됨)
        // 이 클래스에서는 별도 로직 없음
    }

    public override void RemoveItemEffects() { }

    public override bool CheckItemPurchasability() => true;
}