using System;
using UnityEngine;

/*****************************************************************************************
 * 파일: StageManager.cs
 * 역할: 스테이지 진행 추적 (킬카운트 기반) + 보스 스테이지 판정
 * 에디터 세팅:
 *   - GameScene에 빈 GameObject를 만들고 이 스크립트를 붙일 것
 *   - killsPerStage: 일반 스테이지 통과에 필요한 몬스터 처치 수
 *   - bossStageInterval: 몇 스테이지마다 보스가 나오는지 (기본 5)
 *   - finalStage: 스토리 모드 종료 스테이지 (기본 15)
 * 동작:
 *   - 일반 스테이지: 몬스터 killsPerStage 마리 처치 시 다음 스테이지
 *   - 보스 스테이지: 잡몹 킬은 무시, 보스를 잡아야만 다음 스테이지
 * 주의:
 *   - 플레이어의 exp/level과는 완전히 분리되어 있음 (level은 추후 스킬 시스템용)
 *****************************************************************************************/

public class StageManager : MonoBehaviour
{
    #region [Singleton]
    public static StageManager Instance { get; private set; }

    private void SingleTon()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance == this)
            Instance = null;
    }
    #endregion

    [SerializeField] private int killsPerStage = 40;
    [SerializeField] private int bossStageInterval = 5;
    [SerializeField] private int finalStage = 15;

    private int currentStage = 1;
    private int killCount = 0;

    public int CurrentStage => currentStage;
    public int KillCount => killCount;
    public int KillsPerStage => killsPerStage;
    public int FinalStage => finalStage;

    /// <summary>현재 스테이지가 보스 스테이지인지 (5, 10, 15...)</summary>
    public bool IsBossStage => currentStage % bossStageInterval == 0;

    /// <summary>보스 스테이지일 때 bossPrefabs 배열의 인덱스 (5→0, 10→1, 15→2)</summary>
    public int BossIndex => currentStage / bossStageInterval - 1;

    public event Action<int> OnStageChanged;
    public event Action OnAllStagesCleared;

    private void Awake()
    {
        SingleTon();
    }

    private void OnEnable()
    {
        MonsterCharacter.OnMonsterDied += HandleMonsterDied;
    }

    private void OnDisable()
    {
        MonsterCharacter.OnMonsterDied -= HandleMonsterDied;
    }

    private void OnDestroy()
    {
        EmptySingleton();
    }

    #region Internal Logic
    private void HandleMonsterDied(MonsterCharacter monster)
    {
        if (monster == null) return;

        if (IsBossStage)
        {
            // 보스 스테이지는 보스를 잡아야만 통과. 잡몹 킬은 집계하지 않는다.
            if (monster.IsBoss) AdvanceStage();
            return;
        }

        killCount++;
        if (killCount >= killsPerStage) AdvanceStage();
    }

    private void AdvanceStage()
    {
        killCount = 0;

        if (currentStage >= finalStage)
        {
            Debug.Log("모든 스테이지 클리어");
            OnAllStagesCleared?.Invoke();
            return;
        }

        currentStage++;
        Debug.Log($"스테이지 {currentStage} 시작 (보스 스테이지: {IsBossStage})");
        OnStageChanged?.Invoke(currentStage);
    }
    #endregion
}
