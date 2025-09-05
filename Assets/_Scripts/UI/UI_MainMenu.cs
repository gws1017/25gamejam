using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Buttons Config")]
    [SerializeField] Button gameStartButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button optionsButton;

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
        optionsButton.onClick.AddListener(() => SceneManager.LoadSceneAsync(SceneLoader.GetSceneName(SceneLoader.Scene.UI_Options), LoadSceneMode.Additive));
    }

    private void UnSubscribeOnClickEventListeners()
    {
        gameStartButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
    }
    #endregion
}
