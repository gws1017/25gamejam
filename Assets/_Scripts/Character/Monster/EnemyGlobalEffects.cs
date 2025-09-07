using System.Collections;
using UnityEngine;

public class EnemyGlobalEffects : MonoBehaviour
{
    public static EnemyGlobalEffects Instance { get; private set; }

    [Header("Global Multipliers")]
    [SerializeField] private float enemySpeedMultiplier = 1f;   // 적 전체 속도에 곱해지는 값(1f = 기본, 0.7f = 30% 감소)
    [SerializeField] private float enemyDefenseMultiplier = 1f; // 적 전체 방어력에 곱해지는 값(1f = 기본, 0.7f = 30% 감소)

    private ItemEvents itemEvents;               // 상점 아이템 이벤트 허브(구매/만료 신호 전달)
    private Coroutine speedDownCoroutine;        // 속도 감소 효과를 관리하는 코루틴 핸들(중복 적용 방지)
    private Coroutine defenseDownCoroutine;      // 방어력 감소 효과를 관리하는 코루틴 핸들(중복 적용 방지)

    // 외부에서 읽기만 가능하게 노출(캡슐화 유지)
    public float EnemySpeedMultiplier => enemySpeedMultiplier;
    public float EnemyDefenseMultiplier => enemyDefenseMultiplier;

    private void Awake()
    {
        // 싱글톤 할당 (씬에 하나만 존재한다고 가정하는 간단 버전)
        Instance = this;
    }

    private void OnEnable()
    {
        // 이벤트 구독: 아이템이 구매되면 아래 핸들러들이 호출됨
        itemEvents = ItemEvents.Instance;
        itemEvents.OnEnemySpeedDownBought += HandleEnemySpeedDownBought;   // “적 속도 감소” 아이템 구매됨
        itemEvents.OnEnemyDefenseDownBought += HandleEnemyDefenseDownBought; // “적 방어력 감소” 아이템 구매됨
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제: 씬 변경/비활성화 시 중복 구독으로 인한 메모리 누수/중복 호출 방지
        if (itemEvents == null) return;
        itemEvents.OnEnemySpeedDownBought -= HandleEnemySpeedDownBought;
        itemEvents.OnEnemyDefenseDownBought -= HandleEnemyDefenseDownBought;
    }

    // “적 속도 감소” 아이템 구매 시 호출되는 콜백
    private void HandleEnemySpeedDownBought(object sender, System.EventArgs e)
    {
        // 이미 실행 중이면 먼저 정지하여 효과 시간을 리셋(중복 코루틴 방지)
        if (speedDownCoroutine != null) StopCoroutine(speedDownCoroutine);

        // 0.7f = 30% 감소, 5f초 지속
        speedDownCoroutine = StartCoroutine(EnemySpeedDownTimer(0.7f, 5f));
    }

    // 실제 효과를 적용/되돌리는 타이머
    private IEnumerator EnemySpeedDownTimer(float multiplier, float duration)
    {
        // 1) 적용: 현재 배수에 곱하기
        enemySpeedMultiplier *= multiplier;

        // 2) 지정 시간 동안 유지
        yield return new WaitForSeconds(duration);

        // 3) 해제: 원래대로 되돌리기(곱했던 값을 나눠서 복원)
        enemySpeedMultiplier /= multiplier;

        // 4) 만료 이벤트 발행(필요 시 UI, 사운드, 이펙트에서 이 신호를 활용)
        itemEvents.InvokeOnEnemySpeedDownExpired();

        // 5) 코루틴 핸들 정리
        speedDownCoroutine = null;
    }

    // ▶ “적 방어력 감소” 아이템 구매 시 호출되는 콜백
    private void HandleEnemyDefenseDownBought(object sender, System.EventArgs e)
    {
        if (defenseDownCoroutine != null) StopCoroutine(defenseDownCoroutine);

        // 0.7f = 30% 감소, 5f초 지속
        defenseDownCoroutine = StartCoroutine(EnemyDefenseDownTimer(0.7f, 5f));
    }

    private IEnumerator EnemyDefenseDownTimer(float multiplier, float duration)
    {
        enemyDefenseMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        enemyDefenseMultiplier /= multiplier;

        itemEvents.InvokeOnEnemyDefenseDownExpired();
        defenseDownCoroutine = null;
    }
}