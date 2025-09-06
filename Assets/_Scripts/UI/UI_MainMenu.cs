using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("MainMenu Config")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private UI_SoundSettings ui_SoundSettings;

    [Header("Buttons Config")]
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;

    [Header("Button Tweeners")]
    [SerializeField] DoTween_Button_Scale gameStartButtonTween;
    [SerializeField] DoTween_Button_Scale quitButtonTween;
    [SerializeField] DoTween_Button_Scale optionsButtonTween;

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

            SceneManager.LoadScene(SceneLoader.Scene.IntroScene.ToString());
        });

        quitButton.onClick.AddListener(() => 
        {
            // DoTween Button Click Animation
            quitButtonTween.OnButtonClick();

            Application.Quit();
        });

        optionsButton.onClick.AddListener(() =>
        {
            // DoTween Button Click Animation
            optionsButtonTween.OnButtonClick();

            ui_SoundSettings.Show();
            ui_SoundSettings.GetDoTweenPopup().Show();
        });
    }

    private void UnSubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }
    #endregion
}
