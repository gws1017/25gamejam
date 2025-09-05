using UnityEngine;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour, IToggleUI
{
    [Header("Options Config")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        SubscribeToEvents();
        SubscribeOnClickEventListeners();

        Hide();
    }

    private void OnDestroy()
    {
        UnSubscribeFromEvents();
    }

    #region Internal Logic  
    // Events Methods
    private void SubscribeOnClickEventListeners()
    {
        exitButton.onClick.AddListener(() => Hide());
    }

    private void SubscribeToEvents()
    {
        EventsManager.Instance.Events_UI.OnOptionsButtonClicked += Events_UI_OnOptionsButtonClicked;
    }

    private void UnSubscribeFromEvents()
    {
        EventsManager.Instance.Events_UI.OnOptionsButtonClicked -= Events_UI_OnOptionsButtonClicked;
    }

    private void Events_UI_OnOptionsButtonClicked(object sender, System.EventArgs e)
    {
        Show();
    }


    // Interface Methods
    public void Hide()
    {
        contentParent.SetActive(false);
    }

    public void Show()
    {
        contentParent.SetActive(true);
    }
    #endregion
}
