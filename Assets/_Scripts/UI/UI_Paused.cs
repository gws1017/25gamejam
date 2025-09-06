using UnityEngine;

public class UI_Paused : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;

    public void Hide()
    {
        contentParents.SetActive(false);
    }

    public void Show()
    {
        contentParents.SetActive(true);
    }
}
