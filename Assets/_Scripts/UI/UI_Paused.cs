using UnityEngine;
using UnityEngine.UI;

public class UI_Paused : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button currentStateButton;
    [SerializeField] private Button soundSettingsButton;
    [SerializeField] private Button quitGameButton;
    [Space]
    [SerializeField] private DoTween_Popup doTweenPopup;


    private void Start()
    {
        SubscribeOnClickEvents();
        Debug.Log("UI_Paused started and click events subscribed.");
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
        doTweenPopup.Show();

        // Play popup open SFX
        SoundEvents.Instance.InvokeOnPlayUIPopupFx();
    }

    public void SubscribeOnClickEvents()
    {
        exitButton.onClick.AddListener(() =>
        {
            // TODO : GameManager에서 Time.timeScale을 1로 설정하는 메서드 호출

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(Hide);
        });

        resumeButton.onClick.AddListener(() =>
        {
            // TODO : GameManager에서 Time.timeScale을 1로 설정하는 메서드 호출

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            Hide();
        });

        soundSettingsButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            UI_StateManager.Instance.SetState(UI_StateManager.UIState.UI_SoundSettings);
        });

        quitGameButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });
    }

    public void UnsubscribeOnClickEvents()
    {
        exitButton.onClick.RemoveAllListeners();
        resumeButton.onClick.RemoveAllListeners();
        soundSettingsButton.onClick.RemoveAllListeners();
        quitGameButton.onClick.RemoveAllListeners();
    }
    #endregion
}
