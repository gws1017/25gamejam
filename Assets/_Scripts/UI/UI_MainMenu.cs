using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("MainMenu Config")]
    [SerializeField] private GameObject contentParent;

    [Header("Buttons Config")]
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button optionsButton;

    // References
    private EventsManager eventsManager;


    private void Awake()
    {
        SubscribeOnClickEventListeners();
    }

    private void Start()
    {
        eventsManager = EventsManager.Instance;
    }

    private void OnDestroy()
    {
        UnSubscribeOnClickEventListeners();
    }

    #region Internal Logic

    private void SubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.Scene.GameScene));
        exitButton.onClick.AddListener(() => Application.Quit());
        optionsButton.onClick.AddListener(() => eventsManager.Events_UI.InvokeOnOptionsButtonsClicked());
    }

    private void UnSubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }
    #endregion
}
