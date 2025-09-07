using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerCharacter))]
public class PlayerItemEffects : MonoBehaviour
{
    [Header("Multipliers")]
    [SerializeField] private float attackMultiplier = 1f;
    [SerializeField] private float defenseMultiplier = 1f;

    [Header("States")]
    [SerializeField] private bool isInvincible = false;

    [Space]
    [SerializeField] private ItemEvents itemEvents;

    private PlayerCharacter playerCharacter;

    private Coroutine invincibilityCoroutine;
    private Coroutine attackBoostCoroutine;
    private Coroutine defenseBoostCoroutine;

    public float AttackMultiplier => attackMultiplier;  
    public float DefenseMultiplier => defenseMultiplier;

    private void Awake()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
    }

    private void Start()
    {
        itemEvents = ItemEvents.Instance;

        itemEvents.OnInvincibilityBought += HandleInvincibilityBought;
        itemEvents.OnAttackBoostBought += HandleAttackBoostBought;
        itemEvents.OnDefenseBoostBought += HandleDefenseBoostBought;
    }

    private void OnDestroy()
    {
        if (itemEvents == null) return;

        itemEvents.OnInvincibilityBought -= HandleInvincibilityBought;
        itemEvents.OnAttackBoostBought -= HandleAttackBoostBought;
        itemEvents.OnDefenseBoostBought -= HandleDefenseBoostBought;
    }

    // 무적 3초 (아이템에서 지속시간을 전달하므로 여기서는 상수 사용 안 함)
    private void HandleInvincibilityBought(object sender, System.EventArgs e)
    {
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityTimer(3f));
    }

    private IEnumerator InvincibilityTimer(float duration)
    {
        // 내부 상태
        isInvincible = true;

        // 실제 데미지 가드: PlayerCharacter의 외부 무적 플래그 ON
        if (PlayerCharacter.Instance != null)
            PlayerCharacter.Instance.IsInvincibleExternal = true;

        yield return new WaitForSeconds(duration);
   

        isInvincible = false;

        // 데미지 가드 OFF
        if (PlayerCharacter.Instance != null)
            PlayerCharacter.Instance.IsInvincibleExternal = false;

        itemEvents.InvokeOnInvincibilityExpired();
        invincibilityCoroutine = null;
    }

    // 공격력 40% 상승 10초
    private void HandleAttackBoostBought(object sender, System.EventArgs e)
    {
        if (attackBoostCoroutine != null) StopCoroutine(attackBoostCoroutine);
        attackBoostCoroutine = StartCoroutine(AttackBoostTimer(1.4f, 10f));
    }

    private IEnumerator AttackBoostTimer(float multiplier, float duration)
    {
        attackMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        attackMultiplier /= multiplier;
        itemEvents.InvokeOnAttackBoostExpired();
    }

    // 방어력 40% 상승 10초
    private void HandleDefenseBoostBought(object sender, System.EventArgs e)
    {
        if (defenseBoostCoroutine != null) StopCoroutine(defenseBoostCoroutine);
        defenseBoostCoroutine = StartCoroutine(DefenseBoostTimer(1.4f, 10f));
    }

    private IEnumerator DefenseBoostTimer(float multiplier, float duration)
    {
        defenseMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        defenseMultiplier /= multiplier;
        itemEvents.InvokeOnDefenseBoostExpired();
    }
}