using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************
 * 파일: Spawner.cs
 * 역할: 스테이지 진행에 따라 몬스터/보스 스폰
 * 에디터 세팅:
 *   - monsterSpawnPointRoot / bossSpawnPointRoot: 자식 트랜스폼이 실제 스폰 지점
 *   - monsterZones: 스테이지 구간별 몬스터 세트. 구간이 겹치면 먼저 등록된 것이 우선
 *       예) 1~5  : BasicZombie, PoliceZombie
 *           6~10 : Spitter, Spitter, Burster   (같은 키를 여러 번 넣으면 등장 비율이 올라감)
 *       모든 스테이지가 어느 한 구간에는 포함되어야 한다 (빠지면 그 스테이지는 스폰 안 됨)
 *   - bossPrefabs: 보스 단계 인덱스에 맞춰 배열로 등록 (5스테이지→[0], 10→[1], 15→[2])
 * 동작:
 *   - 일반: 현재 스테이지에 맞는 세트에서 key 추출 → PoolManager.Get(key) → Spawn()
 *   - 보스: 보스 스테이지 진입 시 bossPrefabs[StageManager.BossIndex] Instantiate
 * 주의:
 *   - 진행 기준은 플레이어 레벨이 아니라 StageManager.CurrentStage
 *   - 플레이어 exp/level은 여기서 쓰지 않음 (추후 스킬 시스템용)
 *   - 스테이지 6부터 기존 좀비는 등장하지 않는다 (구역 교체). 세트 구성으로 보장할 것
 *****************************************************************************************/

public class Spawner : MonoBehaviour
{
    /// <summary>스테이지 구간별 몬스터 세트. 구역이 바뀌면 등장 몬스터가 통째로 교체된다.</summary>
    [System.Serializable]
    public class MonsterZone
    {
        [Tooltip("이 세트가 적용되는 시작 스테이지 (포함)")]
        public int fromStage = 1;
        [Tooltip("이 세트가 적용되는 종료 스테이지 (포함)")]
        public int toStage = 5;
        [Tooltip("PoolManager에 등록된 key. 같은 키 여러 번 == 중복뽑기, 확률증가")]
        public string[] monsterKeys;
    }

    [SerializeField] private Transform monsterSpawnPointRoot;
    [SerializeField] private Transform bossSpawnPointRoot;
    [SerializeField] private float spawnInterval = 1.0f; //소환 간격 기준 시간(1레벨)

    [Tooltip("스테이지 구간별 몬스터 세트. 모든 스테이지가 어느 한 구간에는 포함되어야 한다")]
    [SerializeField] private MonsterZone[] monsterZones;

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
        string[] keys = GetKeysForStage(stage);
        if (keys == null || keys.Length == 0)
        {
            Debug.LogWarning($"스테이지 {stage}에 해당하는 몬스터 세트가 없습니다. Spawner의 monsterZones를 확인하세요.");
            return;
        }

        string randKey = keys[Random.Range(0, keys.Length)];

        // key 오타나 PoolManager 미등록이면 null이 돌아온다
        var pool = PoolManager.Instance.Get(randKey);
        if (pool == null)
        {
            Debug.LogWarning($"PoolManager에 \"{randKey}\" key가 등록되어 있지 않습니다.");
            return;
        }

        var spawned = pool.Spawn(transform.position, Quaternion.identity, randKey);
        if (spawned == null) return;

        var monsterObject = spawned.GetComponent<MonsterCharacter>();
        if (monsterObject == null) return;

        monsterObject.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        PowerUp(monsterObject);
    }

    /// <summary>
    /// 현재 스테이지가 속한 구간의 몬스터 세트를 반환한다.
    /// 구간이 겹치면 먼저 등록된 것이 우선. 맞는 구간이 없으면 null을 반환하며,
    /// 호출부(Spawn)에서 경고를 띄우고 스폰을 건너뛴다.
    /// </summary>
    private string[] GetKeysForStage(int currentStage)
    {
        if (monsterZones == null) return null;

        for (int i = 0; i < monsterZones.Length; i++)
        {
            var zone = monsterZones[i];
            if (zone == null) continue;
            if (currentStage < zone.fromStage || currentStage > zone.toStage) continue;
            if (zone.monsterKeys == null || zone.monsterKeys.Length == 0) continue;

            return zone.monsterKeys;
        }

        return null;
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