using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour, IToggleUI
{
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Button Tweeners")]
    [SerializeField] DoTween_Button_Scale restartButtonTween;
    [SerializeField] DoTween_Button_Scale quitButtonTween;

    private void OnEnable()
    {
        GameManager.Instance.OnStateChanged += Instance_OnStateChanged;

        SubscribeOnClickEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();

        if (GameManager.Instance == null) return;
        
        GameManager.Instance.OnStateChanged -= Instance_OnStateChanged;

    }

    private void Instance_OnStateChanged(object sender, GameManager.OnStateChangedEventArgs e)
    {
        if (e.GetCurrentGameState() == GameManager.GameState.GameOver)
            Show();
    }

    public void Hide()
    {
        GameManager.Instance.ResumeGame(); // 게임 재개
        contentParents.SetActive(false);
    }

    public void Show()
    {
        contentParents.SetActive(true);
    }

    public void SubscribeOnClickEvents()
    {
        quitButton.onClick.AddListener(() =>
        {
            quitButtonTween.OnButtonClick();

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            Hide();

            // 메인메뉴로 이동
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });

        restartButton.onClick.AddListener(() =>
        {
            restartButtonTween.OnButtonClick(); 

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            Hide();

            // 게임씬 재로딩
            SceneManager.LoadScene(SceneLoader.Scene.GameScene.ToString());
        });
    }

    public void UnsubscribeOnClickEvents()
    {
        
    }
}
