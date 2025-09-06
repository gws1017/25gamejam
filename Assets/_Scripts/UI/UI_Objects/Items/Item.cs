using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [Header("Item Config")]
    [SerializeField] protected float itemDuration;
    [SerializeField] protected float itemCurrentDuration; 
    [SerializeField] protected int itemPrice;
    [SerializeField] protected bool canBePurchased;

    protected ItemEvents itemEvents;

    protected virtual void OnEnable()
    {
        itemEvents = ItemEvents.Instance;
    }

    protected abstract void Start();

    protected abstract void Update();

    protected abstract void OnDestroy();

    public abstract void ApplyItemEffects();

    public abstract void RemoveItemEffects();

    public abstract bool CheckItemPurchasability();

    public virtual bool CanBePurchased() => canBePurchased;

    public virtual void CheckItemPurchaseCooldown()
    {

    }
}
