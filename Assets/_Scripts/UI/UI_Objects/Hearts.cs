using UnityEngine;

public class Hearts : MonoBehaviour
{
    [SerializeField] private Heart[] hearts;
    [SerializeField] private AudioClip heartFX;

    public void Awake() => SetHearts();

    public void SetHearts()
    {
        if (hearts != null && hearts.Length > 0) return;
        int len = transform.childCount;
        hearts = new Heart[len];
        for (int i = 0; i < len; i++)
            hearts[i] = transform.GetChild(i).GetComponent<Heart>();
    }

    public Heart[] GetHearts() => hearts;

    // 시작 시 Player HP로 하트 켜기
    public void SyncFromHP(int currentHP, int maxHP)
    {
        SetHearts();
        if (hearts == null || hearts.Length == 0) return;

        int total = Mathf.Clamp(maxHP, 0, hearts.Length);
        int on = Mathf.Clamp(currentHP, 0, total);

        for (int i = 0; i < hearts.Length; i++)
            if (hearts[i] != null) hearts[i].SetFilled(i < on);
    }

    // 왼쪽부터 첫 '켜진' 하트 끄기
    public bool TurnOffFirstOn()
    {
        if (hearts == null) return false;
        for (int i = 0; i < hearts.Length; i++)
        {
            var h = hearts[i];
            if (h != null && h.IsFilled())
            {
                h.SetFilled(false);
                return true;
            }
        }
        return false;
    }

    // 오른쪽부터 첫 '꺼진' 하트 켜기(자연스러운 복구)
    public bool TurnOnLastOff()
    {
        SoundManager.Instance.PlaySoundFX(heartFX);
        if (hearts == null) return false;
        for (int i = hearts.Length - 1; i >= 0; i--)
        {
            var h = hearts[i];
            if (h != null && !h.IsFilled())
            {
                h.SetFilled(true);
                return true;
            }
        }
        return false;
    }
}