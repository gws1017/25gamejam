using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Image xpBar;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button pauseButton;

    // References
    private Hearts hearts;
    private GameManager gameManager;
    private SoundEvents soundEvents;    

    private void Awake()
    {
        hearts = GetComponentInChildren<Hearts>();
    }

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


    public void SubscribeOnClickEvents()
    {
        shopButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            UI_StateManager.Instance.SetState(UI_StateManager.UIState.UI_Shop);
        });

        pauseButton.onClick.AddListener(() =>
        {
            // Play Button Click SFX
            SoundEvents.Instance.InvokeOnPlayButtonFx();

            UI_StateManager.Instance.SetState(UI_StateManager.UIState.UI_Paused);
        });
    }

    private void UnsubscribeOnClickEvents()
    {
        shopButton.onClick.RemoveAllListeners();
        pauseButton.onClick.RemoveAllListeners();
    }

    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }

    public void SetCoin(int coin)
    {
        coinText.text = coin.ToString();
    }

    public void SetXPBar(float fillAmount)
    {
        xpBar.fillAmount = fillAmount;
    }
}
