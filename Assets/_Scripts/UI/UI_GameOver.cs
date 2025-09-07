using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;

    public void Hide()
    {
        contentParents.SetActive(false);

        GameManager.Instance.ResumeGame();
    }

    public void Show()
    {
        contentParents.SetActive(true);

        GameManager.Instance.PauseGameWithDelay();
    }

    public void SubscribeOnClickEvents()
    {
    
    }

    public void UnsubscribeOnClickEvents()
    {
    
    }
}
