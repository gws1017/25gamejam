using UnityEngine;

public class UI_Skill : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;

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
