using UnityEngine;

public class UI_Skill : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;

    public void Hide(bool resumeGame)
    {
        contentParents.SetActive(false);

        if (resumeGame) GameManager.Instance.ResumeGame();
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
