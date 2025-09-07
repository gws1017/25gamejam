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

    public bool IsTwoHitDefenseActive { get; private set; }

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

    private void HandleDefenseBoostBought(object sender, System.EventArgs e)
    {
        if (defenseBoostCoroutine != null) StopCoroutine(defenseBoostCoroutine);
        defenseBoostCoroutine = StartCoroutine(DefenseTwoHitTimer(10f));
    }

    private IEnumerator DefenseTwoHitTimer(float duration)
    {
        // 시작: 두 번 맞아야 하트 1칸 모드 ON
        IsTwoHitDefenseActive = true;

        // 지정 시간 유지
        yield return new WaitForSeconds(duration);

        // 종료: 원복
        IsTwoHitDefenseActive = false;
        itemEvents.InvokeOnDefenseBoostExpired();   // 공격 버프와 동일한 '만료' 알림 패턴
        defenseBoostCoroutine = null;
    }
}