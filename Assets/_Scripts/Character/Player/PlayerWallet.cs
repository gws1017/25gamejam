using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int currentGold = 0;
    public int CurrentGold => currentGold;

    private void Start()
    {
        UI_InGame.Instance.SetCoin(currentGold);
    }

    public bool HasEnough(int price) => currentGold >= price;

    public bool TrySpend(int price)
    {
        if (currentGold < price) return false;
        currentGold -= price;

        if (UI_InGame.Instance != null)
            UI_InGame.Instance.SetCoin(currentGold);

        return true;
    }

    public void AddGold(int amount)
    {
        currentGold += Mathf.Max(0, amount);

        if (UI_InGame.Instance != null)
            UI_InGame.Instance.SetCoin(currentGold);
    }
}