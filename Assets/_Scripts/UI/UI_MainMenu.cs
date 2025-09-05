using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] Button gameStartButton;
    [SerializeField] Button exitButton;

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
        gameStartButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.Scene.GameScene));
        exitButton.onClick.AddListener(() => Application.Quit());
    }

    private void UnSubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }
    #endregion
}
