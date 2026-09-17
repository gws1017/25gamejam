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
    // resumeGame=true : 정상 종료(게임 재개), resumeGame=false: 다른 UI로 전환 시 재개 금지
    public void Hide(bool resumeGame)
    {
        contentParents.SetActive(false);

        if (resumeGame) gameManager.ResumeGame();
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

            doTweenPopup.Hide(() => Hide(true));
        });

        resumeButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(() => Hide(true));
        });

        soundSettingsButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            Hide(false); // 재개 X (틈 방지)

            ui_SoundSettings.Show();
        });

        tutorialButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();
            Hide(false); // 재개 X (틈 방지)

            ui_Tutorial.Show();
        });

        quitGameButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            Hide(true);

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
