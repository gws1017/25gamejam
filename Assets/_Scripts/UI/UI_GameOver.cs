using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;

    public void Hide()
    {
        contentParents.SetActive(false);
    }

    public void Show()
    {
        contentParents.SetActive(true);
    }

    public void SubscribeOnClickEvents()
    {
    
    }

    public void UnsubscribeOnClickEvents()
    {
    
    }
}
