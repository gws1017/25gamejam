using System;
using UnityEngine;

/*****************************************************************************************
 * 파일: Boss.cs
 * 역할: 모든 보스의 공통 부모 — 스테이지별 보스(BossZombie, 추후 BossAlienTrio/BossRobotDevil 등)가
 *       상속한다. 일반 몬스터(ZombieBasic/ZombiePolice 등)는 MonsterCharacter를 그대로 상속하고,
 *       보스만 이 클래스를 거치므로 Phase 시스템이 일반 몬스터 인스펙터에는 노출되지 않는다.
 * 에디터 세팅:
 *   - 각 보스 프리팹의 Phase Hp Thresholds에 HP% 임계값을 오름차순으로 입력
 *     예: {0.66, 0.33} → 66% 밑이면 Phase2, 33% 밑이면 Phase3
 * 사용법:
 *   보스 서브클래스에서 protected override void OnPhaseChanged(int newPhase)를 구현해서
 *   페이즈 전환 시 패턴 교체/추가 로직을 넣는다.
 *****************************************************************************************/

public class Boss : MonsterCharacter
{
    public override bool IsBoss => true;

    [Header("Phase")]
    [Tooltip("HP% 임계값 오름차순. 예: {0.66, 0.33} → 66% 밑이면 Phase2, 33% 밑이면 Phase3")]
    [SerializeField] protected float[] phaseHpThresholds = new float[0];

    public int CurrentPhase { get; private set; } = 1;

    /// <summary>체력 변화 시 발행 (현재HP, 최대HP). UI_BossHealthBar가 구독.</summary>
    public event Action<float, float> OnHealthChanged;

    /// <summary>페이즈가 바뀔 때 호출됨. 보스 서브클래스에서 override해서 패턴 전환/추가 로직을 넣는다.</summary>
    protected virtual void OnPhaseChanged(int newPhase) { }

    public override void Hit(Vector2 HitPoint)
    {
        base.Hit(HitPoint);
        OnHealthChanged?.Invoke(currentHP, maxHP);
        if (!IsLive) return;
        UpdatePhase();
    }

    private void UpdatePhase()
    {
        if (phaseHpThresholds == null || phaseHpThresholds.Length == 0) return;

        float hpRatio = currentHP / maxHP;
        int newPhase = 1;
        for (int i = 0; i < phaseHpThresholds.Length; i++)
            if (hpRatio <= phaseHpThresholds[i]) newPhase = i + 2;

        if (newPhase == CurrentPhase) return;
        CurrentPhase = newPhase;
        OnPhaseChanged(newPhase);
    }
}
