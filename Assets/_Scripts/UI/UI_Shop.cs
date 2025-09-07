using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop : MonoBehaviour, IToggleUI
{
    [Header("Shop Config")]
    [SerializeField] private GameObject contentParents;
    [SerializeField] private Button exitButton;
    [Space]
    [SerializeField] private DoTween_Popup doTweenPopup;

    [Header("Items Config")]
    [SerializeField] private Button item_AddHeart;
    [SerializeField] private Button item_Invincibility;
    [SerializeField] private Button item_EnemySpeedDown;
    [SerializeField] private Button item_AttackBoost;
    [SerializeField] private Button item_EnemyDefensePowerDown;
    [SerializeField] private Button item_DefenseBoost;

    private float nextPurchaseAllowedTime = 0f;
    private float itemPurchaseCooldown = 5f;
    private Button[] itemButtons;

    private ItemEvents itemEvents;
    private GameManager gameManager;
    private SoundEvents soundEvents;    

    private void Start()
    {
        itemEvents = ItemEvents.Instance;
        gameManager = GameManager.Instance;
        soundEvents = SoundEvents.Instance;

        SubscribeOnClickEvents();
        InitalizeButtonsArray();
    }

    private void Update()
    {
        UpdatePurchaseCoolDown();
    }

    private void OnDestroy()
    {
        UnsubscribeOnClickEvents();
    }
    public void BuyItem()
    {
        // 플레이어 골드 현황 체크
            // 부족할 시 골드부족 문구 띄우기
            // 충분할시 아이템 구매
                // 골드 차감
                // 아이템 효과 적용 (이벤트 발동)
                // 상점창 닫기
    }

    #region Internal Logic
    private void InitalizeButtonsArray()
    {
        itemButtons = new Button[]
        {
            item_AddHeart,
            item_Invincibility,
            item_EnemySpeedDown,
            item_AttackBoost,
            item_EnemyDefensePowerDown,
            item_DefenseBoost
        };
    }

    private void UpdatePurchaseCoolDown()
    {
        bool onCoolDown = !IsPurchaseAllowed();

        // Toggle Interactivity for all itemButtons
        if (itemButtons != null)
        {
            for (int i = 0; i < itemButtons.Length; i++)
                if (itemButtons[i])
                    itemButtons[i].interactable = !onCoolDown;
        }
    }

    private void TryPurchase(Action performPurchaseAndFireEvent)
    {
        if (!IsPurchaseAllowed())
        {
            // TODO: play error sound or show message

            return;
        }

        // Do gold check here
        
        performPurchaseAndFireEvent?.Invoke();
        StartPurchaseCooldown();
        doTweenPopup.Hide(Hide);
    }

    private void ExecutePurchaseProcess(Action itemEventAction)
    {
        // Play Button Click SFX
        soundEvents.InvokeOnPlayButtonFx();

        BuyItem();

        itemEventAction?.Invoke();
    }

    private bool IsPurchaseAllowed() => Time.unscaledTime >= nextPurchaseAllowedTime;
    private void StartPurchaseCooldown() => nextPurchaseAllowedTime = Time.unscaledTime + itemPurchaseCooldown;

    #region Interface Logic
    public void Hide()
    {
        contentParents.SetActive(false);

        gameManager.ResumeGame();
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
            // Play Button Click SFX
            soundEvents.InvokeOnPlayButtonFx();

            doTweenPopup.Hide(Hide);
        });

        // 아이템 버튼 onClick 이벤트 등록
        // Each button: wrap in TryPurchase so cooldown is enforced
        item_AddHeart.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnAddHeartItemBought);
                Debug.Log("AddHeart Item Purchased");
            }));

        item_Invincibility.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnInvincibilityBought);
                Debug.Log("Invincibility Item Purchased");
            }));

        item_EnemySpeedDown.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnEnemySpeedDownBought);
                Debug.Log("EnemySpeedDown Item Purchased");
            }));

        item_AttackBoost.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnAttackBoostBought);
                Debug.Log("AttackBoost Item Purchased");
            }));

        item_EnemyDefensePowerDown.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnEnemyDefenseDownBought);
                Debug.Log("EnemyDefensePowerDown Item Purchased");
            }));

        item_DefenseBoost.onClick.AddListener(() =>
            TryPurchase(() => {
                ExecutePurchaseProcess(itemEvents.InvokeOnDefenseBoostBought);
                Debug.Log("DefenseBoost Item Purchased");
            }));
    }

    public void UnsubscribeOnClickEvents()
    {
        exitButton.onClick.RemoveAllListeners();

        item_AddHeart.onClick.RemoveAllListeners();
        item_Invincibility.onClick.RemoveAllListeners();
        item_EnemySpeedDown.onClick.RemoveAllListeners();
        item_AttackBoost.onClick.RemoveAllListeners();
        item_EnemyDefensePowerDown.onClick.RemoveAllListeners();
        item_DefenseBoost.onClick.RemoveAllListeners();
    }
    #endregion
    #endregion
}
