using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************
 * 파일: Spawner.cs
 * 역할: 스테이지 진행에 따라 몬스터/보스 스폰
 * 에디터 세팅:
 *   - monsterSpawnPointRoot / bossSpawnPointRoot: 자식 트랜스폼이 실제 스폰 지점
 *   - monsterKeys: PoolManager에 등록된 key 문자열과 동일하게 입력
 *   - bossPrefabs: 보스 단계 인덱스에 맞춰 배열로 등록 (5스테이지→[0], 10→[1], 15→[2])
 * 동작:
 *   - 일반: monsterKeys → PoolManager.Get(key) → Spawn()
 *   - 보스: 보스 스테이지 진입 시 bossPrefabs[StageManager.BossIndex] Instantiate
 * 주의:
 *   - 진행 기준은 플레이어 레벨이 아니라 StageManager.CurrentStage
 *   - 플레이어 exp/level은 여기서 쓰지 않음 (추후 스킬 시스템용)
 *****************************************************************************************/

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform monsterSpawnPointRoot;
    [SerializeField] private Transform bossSpawnPointRoot;
    [SerializeField] private float spawnInterval = 1.0f; //소환 간격 기준 시간(1레벨)

    [SerializeField] private string[] monsterKeys;
    [SerializeField] private GameObject[] bossPrefabs;
    
    private Transform[] bossSpawnPoint;
    private Transform[] spawnPoint;

    private GameObject SpawnBossObject;
    private int stage = 1;
    private float spawnTimer;
    private bool bossSpawnedThisStage = false;

    private void Awake()
    {
        spawnPoint = monsterSpawnPointRoot.GetComponentsInChildren<Transform>();
        bossSpawnPoint = bossSpawnPointRoot.GetComponentsInChildren<Transform>();
    }

    private void OnEnable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
    }

    private void Start()
    {
        // StageManager가 Spawner보다 늦게 Awake 될 수 있으므로 여기서 한 번 더 시도
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
            StageManager.Instance.OnStageChanged += HandleStageChanged;
        }
    }

    private void HandleStageChanged(int newStage)
    {
        // 스테이지가 바뀌면 다음 보스를 다시 소환할 수 있도록 초기화
        bossSpawnedThisStage = false;
        SpawnBossObject = null;
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        var stageManager = StageManager.Instance;
        if (stageManager == null)
        {
            Debug.LogWarning("StageManager is Null - 씬에 StageManager를 추가하세요.");
            return;
        }
        stage = stageManager.CurrentStage;

        if (stageManager.IsBossStage)
        {
            spawnInterval = 5.0f;

            if (!bossSpawnedThisStage)
            {
                SpawnBossMonster(stageManager.BossIndex);
                bossSpawnedThisStage = true;
            }
        }
        else
        {
            spawnInterval = 1.0f;
        }

        //스테이지가 오를수록 스폰 간격 감소(보스 스테이지 주기마다 초기화)
        float interval = spawnInterval - 0.1f * ((stage - 1) % 5);
        if (spawnTimer > interval)
        {
            spawnTimer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        string randKey = monsterKeys[Random.Range(0, monsterKeys.Length)];
        var pool = PoolManager.Instance.Get(randKey);
        var monsterObject = pool.Spawn(transform.position,Quaternion.identity,randKey).GetComponent<MonsterCharacter>();
        if (monsterObject == null) return;
        monsterObject.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        PowerUp(monsterObject);
    }

    // bossIndex: 보스 스테이지 순번 (5스테이지→0, 10→1, 15→2)
    void SpawnBossMonster(int bossIndex)
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("bossPrefabs가 비어있어 보스를 소환할 수 없습니다.");
            return;
        }

        // 스테이지 2/3 보스가 아직 없으므로, 등록된 마지막 보스를 재사용한다.
        int prefabIndex = Mathf.Clamp(bossIndex, 0, bossPrefabs.Length - 1);
        if (prefabIndex != bossIndex)
            Debug.LogWarning($"bossPrefabs[{bossIndex}]가 없어 [{prefabIndex}]로 대체합니다.");

        SpawnBossObject = Instantiate(bossPrefabs[prefabIndex]);
        if (SpawnBossObject == null) return;

        // bossSpawnPoint[0]은 루트 자신이므로 자식은 1부터 시작
        int pointIndex = Mathf.Clamp(bossIndex + 1, 1, bossSpawnPoint.Length - 1);
        SpawnBossObject.transform.position = bossSpawnPoint[pointIndex].position;
        Debug.Log($"보스 소환중... (스테이지 {stage}, bossPrefabs[{prefabIndex}])");

        PowerUp(SpawnBossObject.GetComponent<MonsterCharacter>(),true);
    }
    void PowerUp(MonsterCharacter monsterObject, bool isBoss = false)
    {
        //스테이지 기준으로 몬스터 강화 (보스 주기 1회당 1단계)
        int powerUpCount = stage / 5;

        //보스는 두 번째 보스 스테이지부터 강화
        if (isBoss) powerUpCount--;

        if (monsterObject == null) return;
        monsterObject.PowerUp(powerUpCount);
    }
}