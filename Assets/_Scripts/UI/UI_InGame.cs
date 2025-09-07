using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    #region [Singleton]
    public static UI_InGame Instance { get; private set; }

    private void SingleTon()
    {
        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

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
        SingleTon();
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
        EmptySingleton();
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

    public void SetXPBar(int currentExp, int maxExp, bool animate = true)
    {
        if (xpBar == null)
        {
            Debug.LogWarning("XP bar Image is not assigned.");
            return;
        }

        float target = (maxExp <= 0) ? 0f : Mathf.Clamp01((float)currentExp / maxExp);

#if DOTWEEN_ENABLED
    if (animate)
    {
        xpBar.DOKill();                         // stop previous tweens if any
        xpBar.DOFillAmount(target, 0.25f)       // requires using DG.Tweening;
             .SetEase(Ease.OutCubic);
        return;
    }
#endif

        xpBar.fillAmount = target;
    }
}
