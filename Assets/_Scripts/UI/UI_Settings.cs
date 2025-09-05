using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour, IToggleUI
{
    [Header("Options Config")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button exitButton;
    [SerializeField] private DoTween_Popup optionsPopup;

    // References
    private EventsManager eventsManager;

    private void Awake()
    {
        if (optionsPopup == null)
            optionsPopup = GetComponentInChildren<DoTween_Popup>(); 
    }

    private void OnEnable()
    {
        eventsManager = EventsManager.Instance;
    }

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
        exitButton.onClick.AddListener(() =>
        {
            optionsPopup.Hide(Hide);
        });
    }

    private void SubscribeToEvents()
    {
        if (eventsManager == null) return;

        eventsManager.Events_UI.OnOptionsButtonClicked += Events_UI_OnOptionsButtonClicked;
    }

    private void UnSubscribeFromEvents()
    {
        if (eventsManager == null) return;

        eventsManager.Events_UI.OnOptionsButtonClicked -= Events_UI_OnOptionsButtonClicked;
    }

    private void Events_UI_OnOptionsButtonClicked(object sender, System.EventArgs e)
    {
        optionsPopup.Show(Show);
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
