using System;
using System.Diagnostics;
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

    [Header("Price Config")]
    [SerializeField] private int price_AddHeart = 100;
    [SerializeField] private int price_Invincibility = 100;
    [SerializeField] private int price_EnemySpeedDown = 100;
    [SerializeField] private int price_AttackBoost = 100;
    [SerializeField] private int price_EnemyDefensePowerDown = 100;
    [SerializeField] private int price_DefenseBoost = 100;

    private float nextPurchaseAllowedTime = 0f;
    private float itemPurchaseCooldown = 1f;
    private Button[] itemButtons;

    private ItemEvents itemEvents;
    private GameManager gameManager;
    private SoundEvents soundEvents;
    private PlayerWallet playerWallet;

    private void Start()
    {
        itemEvents = ItemEvents.Instance;
        gameManager = GameManager.Instance;
        soundEvents = SoundEvents.Instance;
        playerWallet = PlayerCharacter.Instance.PlayerWallet;  

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

    private void TryPurchase(int price, Action performPurchaseAndFireEvent)
    {
        if (!IsPurchaseAllowed())
        {
            // 오류 효과음 등
            return;
        }

        if (!playerWallet.TrySpend(price))
        {
            // 골드 부족 알림
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
            soundEvents.InvokeOnPlayButtonFx();
            doTweenPopup.Hide(Hide);
        });

        item_AddHeart.onClick.AddListener(() =>
            TryPurchase(price_AddHeart, () => {
                ExecutePurchaseProcess(itemEvents.InvokeOnAddHeartItemBought);

                // 플레이어의 하트가 꽉 차있다면 구매 불가
                if (PlayerCharacter.Instance.CurrentHP >= PlayerCharacter.Instance.MaxHP)
                {
                    // 골드 환불
                    playerWallet.AddGold(price_AddHeart);
                    UnityEngine.Debug.Log("AddHeart Item Purchase Failed - Full HP");
                    return;
                }

                PlayerCharacter.Instance.Heal(1);
                UnityEngine.Debug.Log("AddHeart Item Purchased");
            }));

        item_Invincibility.onClick.AddListener(() =>
            TryPurchase(price_Invincibility, () => {
                ExecutePurchaseProcess(itemEvents.InvokeOnInvincibilityBought);
                UnityEngine.Debug.Log("Invincibility Item Purchased");
            }));

        //item_EnemySpeedDown.onClick.AddListener(() =>
        //    TryPurchase(price_EnemySpeedDown, () => {
        //        ExecutePurchaseProcess(itemEvents.InvokeOnEnemySpeedDownBought);
        //        UnityEngine.Debug.Log("EnemySpeedDown Item Purchased");
        //    }));

        item_AttackBoost.onClick.AddListener(() =>
            TryPurchase(price_AttackBoost, () => {
                ExecutePurchaseProcess(itemEvents.InvokeOnAttackBoostBought);
                UnityEngine.Debug.Log("AttackBoost Item Purchased");
            }));

        //item_EnemyDefensePowerDown.onClick.AddListener(() =>
        //    TryPurchase(price_EnemyDefensePowerDown, () => {
        //        ExecutePurchaseProcess(itemEvents.InvokeOnEnemyDefenseDownBought);
        //        UnityEngine.Debug.Log("EnemyDefensePowerDown Item Purchased");
        //    }));

        item_DefenseBoost.onClick.AddListener(() =>
            TryPurchase(price_DefenseBoost, () => {
                ExecutePurchaseProcess(itemEvents.InvokeOnDefenseBoostBought);
                UnityEngine.Debug.Log("DefenseBoost Item Purchased");
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
