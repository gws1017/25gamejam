using UnityEngine;
using UnityEngine.UI;

public class UI_SoundSettings : MonoBehaviour, IToggleUI
{
    [Header("Options Config")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button exitButton;
    [SerializeField] private DoTween_Popup doTweenPopup;

    private void Awake()
    {
        if (doTweenPopup == null)
            doTweenPopup = GetComponentInChildren<DoTween_Popup>(); 
    }

    private void Start()
    {
        SubscribeOnClickEvents();

        Hide();
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();
    }

    public DoTween_Popup GetDoTweenPopup()
    {
        return doTweenPopup;
    }

    #region Internal Logic  
    // Interface Methods
    public void SubscribeOnClickEvents()
    {
        exitButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(Hide);
        });
    }

    public void UnsubscribeOnClickEvents()
    {
        exitButton.onClick.RemoveAllListeners();
    }

    public void Hide()
    {
        contentParent.SetActive(false);
    }

    public void Show()
    {
        contentParent.SetActive(true);
        doTweenPopup.Show();
        SoundEvents.Instance.InvokeOnPlayUIPopupFx();
    }
    #endregion
}
