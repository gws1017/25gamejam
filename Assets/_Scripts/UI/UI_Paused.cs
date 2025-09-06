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
    }

    public void SubscribeOnClickEvents()
    {
        exitButton.onClick.AddListener(() =>
        {
            // TODO : GameManager에서 Time.timeScale을 1로 설정하는 메서드 호출
            Hide();
        });

        resumeButton.onClick.AddListener(() =>
        {
            // TODO : GameManager에서 Time.timeScale을 1로 설정하는 메서드 호출
            Hide();
        });

        soundSettingsButton.onClick.AddListener(() =>
        {
            UI_StateManager.Instance.SetState(UI_StateManager.UIState.UI_SoundSettings);
        });

        quitGameButton.onClick.AddListener(() =>
        {
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
