using UnityEngine;
using UnityEngine.UI;

public class UI_Shop : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        SubscribeOnClickEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();
    }

    #region Internal Logic
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
        exitButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void UnsubscribeOnClickEvents()
    {
        exitButton.onClick.RemoveAllListeners();
    }
    #endregion
}
