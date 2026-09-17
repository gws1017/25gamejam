using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tutorial : MonoBehaviour, IToggleUI 
{
    [Header("Shop Config")]
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;
    [Space]
    [SerializeField] private DoTween_Popup doTweenPopup;

    [SerializeField]  private GameManager gameManager;
    [SerializeField]  private SoundEvents soundEvents;

    private void Start()
    {
        gameManager = GameManager.Instance;
        soundEvents = SoundEvents.Instance;
        SubscribeOnClickEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();
    }

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
            soundEvents.InvokeOnPlayButtonFx();
            doTweenPopup.Hide(() => Hide(true));

            Debug.Log("Tutorial Exit Button Clicked.");
        });
    }

    public void UnsubscribeOnClickEvents()
    {
        exitButton.onClick.RemoveAllListeners();
    }
}
