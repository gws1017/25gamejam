using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialSize = 20;

    private readonly Queue<Bullet> pool = new Queue<Bullet>();
    private Transform container;

    private void Awake()
    {
        Instance = this;
        container = new GameObject("BulletPool_Container").transform;
        container.SetParent(transform);
        Prewarm(initialSize);
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bullet = Instantiate(bulletPrefab, container);
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public Bullet Spawn(Vector3 pos, Quaternion rot)
    {
        Bullet bullet = pool.Count > 0 ? pool.Dequeue() : Instantiate(bulletPrefab, container);
        bullet.transform.SetPositionAndRotation(pos, rot);
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    public void Despawn(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}