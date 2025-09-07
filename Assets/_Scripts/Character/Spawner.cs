using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.0f; //소환 간격 기준 시간(1레벨)
    private Transform[] spawnPoint;
    private int level;
    private float spawnTimer;
    private void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        level = GetComponentInParent<PlayerCharacter>().Level;

        float interval = spawnInterval - 0.1f * ((level - 1) % 10);
        if (spawnTimer > interval)
        {
            spawnTimer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        var pool = GameManager.Instance.pool;
        GameObject monster = pool.GetObject(Random.Range(0,pool.prefabs.Length));
        if (monster == null) return;
        monster.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
    }
}