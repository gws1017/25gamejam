using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_SoundSettings : MonoBehaviour, IToggleUI
{
    private SceneState currentScene;
    public enum SceneState
    {
        MainMenuScene,
        GameScene,
    }

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
        CheckCurrentScene();

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

    private void CheckCurrentScene()
    {
        // 현재 플레이어가 위치해있는 씬의 이름을 체크해 currentScene 변수를 업데이트
        if (SceneManager.GetActiveScene().name == SceneState.MainMenuScene.ToString())
            currentScene = SceneState.MainMenuScene;
        else if (SceneManager.GetActiveScene().name == SceneState.GameScene.ToString())
            currentScene = SceneState.GameScene;
    }

    public void Hide()
    {
        contentParent.SetActive(false);

        // 현재 씬이 게임씬일 경우에만
        if (currentScene == SceneState.GameScene)
            GameManager.Instance.ResumeGame();
    }

    public void Show()
    {
        contentParent.SetActive(true);
        doTweenPopup.Show();
        SoundEvents.Instance.InvokeOnPlayUIPopupFx();

        // 현재 씬이 게임씬일 경우에만
        if (currentScene == SceneState.GameScene)
            GameManager.Instance.PauseGameWithDelay();
    }
    #endregion
}
