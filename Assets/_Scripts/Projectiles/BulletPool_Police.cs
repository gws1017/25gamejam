using System.Collections.Generic;
using UnityEngine;

public class BulletPool_Police : MonoBehaviour
{
    public static BulletPool_Police Instance { get; private set; }

    [SerializeField] private Bullet bulletPrefab;   // 풀에서 사용할 탄환 프리팹
    [SerializeField] private int initialSize = 20;  // 시작 시 미리 만들어 둘 개수(프리워밍)

    private readonly Queue<Bullet> pool = new Queue<Bullet>(); // 비활성 탄환 보관 큐
    private Transform container;                                // 풀 오브젝트 정리용 부모

    private void Awake()
    {
        Instance = this;
        container = new GameObject("playerBullet").transform; // 풀 게임오브젝트 정리용 빈 오브젝트
        container.SetParent(transform);
        Prewarm(initialSize); // 시작할 때 미리 생성(Instantiate는 여기서 한 번만)
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bullet = Instantiate(bulletPrefab, container);
            bullet.gameObject.SetActive(false); // 비활성화한 채로 큐에 보관
            pool.Enqueue(bullet);
        }
    }

    // 탄환 꺼내기(스폰): 위치/회전 세팅하고 활성화
    public Bullet Spawn(Vector3 pos, Quaternion rot)
    {
        Bullet bullet = pool.Count > 0 ? pool.Dequeue() : Instantiate(bulletPrefab, container);
        bullet.transform.SetPositionAndRotation(pos, rot);
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    // 탄환 되돌리기(디스폰): 비활성화 후 큐에 다시 보관
    public void Despawn(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}