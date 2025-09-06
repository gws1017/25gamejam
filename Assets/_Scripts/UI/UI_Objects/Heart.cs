using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [SerializeField] private Image heartFill;

    public void SetFillAmount(float amount)
    {
        heartFill.fillAmount = amount;
    }

    public bool IsFull()
    {
        return heartFill.fillAmount >= 1f;
    }

    public bool IsEmpty()
    {
        return heartFill.fillAmount <= 0f;
    }
}
