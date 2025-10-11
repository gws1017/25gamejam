using System.Collections.Generic;
using UnityEngine;

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
    private int level;
    private float spawnTimer;
    private bool bossSpawnedThisCycle = false;

    private void Awake()
    {
        spawnPoint = monsterSpawnPointRoot.GetComponentsInChildren<Transform>();
        bossSpawnPoint = bossSpawnPointRoot.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        var player = PlayerCharacter.Instance;
        if (player == null)
        {
            Debug.Log("Player is Null");
            return;
        }
        level = player.Level;

        if (level % 10 == 0)
        {
            spawnInterval = 5.0f;

            if (!bossSpawnedThisCycle)
            {
                SpawnBossMonster();
                bossSpawnedThisCycle = true;
            }
        }
        else
        {
            spawnInterval = 1.0f;

            // 다음 보스 사이클 대비 초기화
            if (SpawnBossObject != null && !SpawnBossObject.activeSelf)
            {
                SpawnBossObject = null;
            }

            bossSpawnedThisCycle = false; // 보스 스폰 플래그 초기화
        }

        //레벨이 오를수록 스폰 간격 감소(10레벨에서 초기화)
        float interval = spawnInterval - 0.1f * ((level - 1) % 10);
        if (spawnTimer > interval)
        {
            spawnTimer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        string randKey = monsterKeys[Random.Range(0, monsterKeys.Length)];
        var pool = PoolManager.Instance.Get<MonsterCharacter>(randKey);
        var monsterObject = pool.Spawn(transform.position,Quaternion.identity,randKey);
        if (monsterObject == null) return;
        monsterObject.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        PowerUp(monsterObject);
    }

    //레벨 파라미터는 보스 추가될 경우 사용 , 플레이어레벨 / 10으로 나눈값을 넣어야함
    void SpawnBossMonster(int lv = 0)
    {
        SpawnBossObject = Instantiate(bossPrefabs[lv]);
        if (SpawnBossObject == null) return;
        SpawnBossObject.transform.position = bossSpawnPoint[lv+1].position;
        Debug.Log("보스 소환중...");

        PowerUp(SpawnBossObject.GetComponent<MonsterCharacter>(),true);
    }
    void PowerUp(MonsterCharacter monsterObject, bool isBoss = false)
    {
        //플레이어 레벨 기준으로 몬스터 강화
        int powerUpCount = level / 10;
        
        //보스는 20레벨 부터 강화
        if (isBoss) powerUpCount--;

        if (monsterObject == null) return;
        monsterObject.PowerUp(powerUpCount);
    }
}