using UnityEngine;

public class Hearts : MonoBehaviour
{
    [SerializeField] private Heart[] hearts;

    public void Awake()
    {
        SetHearts();
    }

    public void SetHearts()
    {
        if (hearts != null && hearts.Length > 0) return;

        int heartsLength = this.gameObject.transform.childCount;

        hearts = new Heart[heartsLength];

        for (int i = 0; i < heartsLength; i++)
        {
             hearts[i] = this.gameObject.transform.GetChild(i).GetComponent<Heart>();
        }
    }

    public Heart[] GetHearts()
    {
        return hearts;
    }
}
