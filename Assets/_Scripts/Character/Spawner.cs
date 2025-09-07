using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform monsterSpawnPointRoot;
    [SerializeField] private Transform bossSpawnPointRoot;
    [SerializeField] private float spawnInterval = 1.0f; //소환 간격 기준 시간(1레벨)

    [SerializeField] private GameObject[] bossPrefabs;
    
    private Transform[] bossSpawnPoint;
    private Transform[] spawnPoint;

    private GameObject SpawnBossObject;
    private int level;
    private float spawnTimer;

    private void Awake()
    {
        spawnPoint = monsterSpawnPointRoot.GetComponentsInChildren<Transform>();
        bossSpawnPoint = bossSpawnPointRoot.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        level = GetComponentInParent<PlayerCharacter>().Level;
        if (level % 10 == 0)
        {
            spawnInterval = 5.0f;

            Debug.Log("보스 소환중...");

            //보스 없으면 소환
            if (SpawnBossObject == null)
                SpawnBossMonster();
            else
            {
                //있으면 살아있는지 체크
                if(SpawnBossObject.gameObject.activeSelf == false)
                {
                    //죽어있으면 연결해제
                    SpawnBossObject = null;
                }
            }

        }
        else
            spawnInterval = 1.0f;
        float interval = spawnInterval - 0.1f * ((level - 1) % 10);
        if (spawnTimer > interval)
        {
            spawnTimer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        PoolManager pool = GameManager.Instance.pool;

        GameObject monsterObject = pool.GetObject(Random.Range(0,pool.prefabs.Length));
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

        PowerUp(SpawnBossObject,true);
    }
    void PowerUp(GameObject monsterObject, bool isBoss = false)
    {
        //플레이어 레벨 기준으로 몬스터 강화
        int powerUpCount = level / 10;
        
        //보스는 20레벨 부터 강화
        if (isBoss) powerUpCount--;

        var monster = monsterObject.GetComponent<MonsterCharacter>();
        if (monster == null) return;
        monster.PowerUp(powerUpCount);
    }
}