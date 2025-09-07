using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.8f; //소환 간격
    private Transform[] spawnPoint;

    private float spawnTimer;

    private void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if(spawnTimer > spawnInterval)
        {
            spawnTimer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        var pool = GameManager.Instance.pool;
        GameObject monster = pool.GetObject(Random.Range(0, pool.prefabs.Length));
        monster.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
    }
}
