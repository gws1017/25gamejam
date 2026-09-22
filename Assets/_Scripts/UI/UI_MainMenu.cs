using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("MainMenu Config")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private GameObject mainButtonsGroup;  // ContentParents 밑, Title 제외한 버튼 3개만 묶은 그룹
    [SerializeField] private UI_SoundSettings ui_SoundSettings;

    [Header("Buttons Config")]
    [SerializeField] private Button gameStartButton;   // "시작하기" — 모드 선택 패널을 염
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;

    [Header("Button Tweeners")]
    [SerializeField] DoTween_Button_Scale gameStartButtonTween;
    [SerializeField] DoTween_Button_Scale quitButtonTween;
    [SerializeField] DoTween_Button_Scale optionsButtonTween;

    [Header("Mode Select Panel")]
    [SerializeField] private GameObject modeSelectPanel;   // 기본 비활성화
    [SerializeField] private Button storyModeButton;
    [SerializeField] private Button infiniteModeButton;
    [SerializeField] private Button backButton;
    [SerializeField] DoTween_Button_Scale storyModeButtonTween;
    [SerializeField] DoTween_Button_Scale infiniteModeButtonTween;
    [SerializeField] DoTween_Button_Scale backButtonTween;

    private void Awake()
    {
        SubscribeOnClickEventListeners();
    }

    private void OnDestroy()
    {
        UnSubscribeOnClickEventListeners();
    }

    #region Internal Logic

    private void SubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.AddListener(() =>
        {
            // DoTween Button Click Animation
            gameStartButtonTween.OnButtonClick();

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            ShowModeSelectPanel();
        });

        storyModeButton.onClick.AddListener(() =>
        {
            storyModeButtonTween.OnButtonClick();
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            SceneLoader.SetGameMode(SceneLoader.GameMode.Story);
            SceneManager.LoadScene(SceneLoader.Scene.IntroScene.ToString());
        });

        infiniteModeButton.onClick.AddListener(() =>
        {
            infiniteModeButtonTween.OnButtonClick();
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            // 무한모드는 스토리 인트로 없이 바로 게임 시작
            SceneLoader.SetGameMode(SceneLoader.GameMode.Infinite);
            SceneManager.LoadScene(SceneLoader.Scene.GameScene.ToString());
        });

        backButton.onClick.AddListener(() =>
        {
            backButtonTween.OnButtonClick();
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            HideModeSelectPanel();
        });

        quitButton.onClick.AddListener(() =>
        {
            // DoTween Button Click Animation
            quitButtonTween.OnButtonClick();

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            Application.Quit();
        });

        optionsButton.onClick.AddListener(() =>
        {
            // DoTween Button Click Animation
            optionsButtonTween.OnButtonClick();

            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            ui_SoundSettings.Show();
        });
    }

    private void UnSubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.RemoveAllListeners();
        storyModeButton.onClick.RemoveAllListeners();
        infiniteModeButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }

    private void ShowModeSelectPanel()
    {
        // Title은 ContentParents 밑에 같이 있지만 MainButtons 밖에 있어서 계속 보임
        mainButtonsGroup.SetActive(false);
        modeSelectPanel.SetActive(true);
    }

    private void HideModeSelectPanel()
    {
        modeSelectPanel.SetActive(false);
        mainButtonsGroup.SetActive(true);
    }
    #endregion
}
