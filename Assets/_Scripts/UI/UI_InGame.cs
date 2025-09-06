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

    private void Awake()
    {
        hearts = GetComponentInChildren<Hearts>();
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();
    }


    private void SubscribeOnClickEvents()
    {
        shopButton.onClick.AddListener(() =>
        {
            
        });

        pauseButton.onClick.AddListener(() =>
        {
            
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
