using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int currentGold = 0;
    public int CurrentGold => currentGold;

    public bool HasEnough(int price) => currentGold >= price;

    public bool TrySpend(int price)
    {
        if (currentGold < price) return false;
        currentGold -= price;
        return true;
    }

    public void AddGold(int amount)
    {
        currentGold += Mathf.Max(0, amount);
    }
}