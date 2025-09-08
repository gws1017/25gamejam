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
    [SerializeField] private Button tutorialButton;
    [Space]
    [SerializeField] private DoTween_Popup doTweenPopup;
    [SerializeField] private UI_SoundSettings ui_SoundSettings;
    [SerializeField] private UI_Tutorial ui_Tutorial;

    private GameManager gameManager;
    private SoundEvents soundEvents;

    private void Start()
    {
        gameManager = GameManager.Instance;
        soundEvents = SoundEvents.Instance;

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

        gameManager.ResumeGame();
    }

    public void Show()
    {
        contentParents.SetActive(true);
        doTweenPopup.Show();

        // Play popup open SFX
        soundEvents.InvokeOnPlayUIPopupFx();

        gameManager.PauseGameWithDelay();
    }

    public void SubscribeOnClickEvents()
    {
        exitButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(Hide);
        });

        resumeButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(Hide);
        });

        soundSettingsButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            Hide();

            ui_SoundSettings.Show();
        });

        tutorialButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();
            Hide();
            
            ui_Tutorial.Show();
        });

        quitGameButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            Hide();

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
