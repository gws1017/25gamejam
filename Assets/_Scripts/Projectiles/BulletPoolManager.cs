using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Bullet playerBulletPrefab;
    [SerializeField] private Bullet bossBulletPrefab;
    [SerializeField] private Bullet policeBulletPrefab;

    [Header("Sizes")]
    [SerializeField] private int playerBulletSize = 20;
    [SerializeField] private int bossBulletSize = 20;
    [SerializeField] private int policeBulletSize = 20;

    private GenericPool<Bullet> playerBulletPool;
    private GenericPool<Bullet> bossBulletPool;
    private GenericPool<Bullet> policeBulletPool;

    private void Awake()
    {
        Instance = this;

        playerBulletPool = new GenericPool<Bullet>(playerBulletPrefab, playerBulletSize, transform);
        bossBulletPool = new GenericPool<Bullet>(bossBulletPrefab, bossBulletSize, transform);
        policeBulletPool = new GenericPool<Bullet>(policeBulletPrefab, policeBulletSize, transform);
    }

    public Bullet Spawn(BulletType type, Vector3 pos, Quaternion rot)
    {
        return GetPool(type)?.Spawn(pos, rot);
    }

    public void Despawn(BulletType type, Bullet bullet)
    {
        GetPool(type)?.Despawn(bullet);
    }

    private GenericPool<Bullet> GetPool(BulletType type)
    {
        return type switch
        {
            BulletType.Player => playerBulletPool,
            BulletType.Boss => bossBulletPool,
            BulletType.Police => policeBulletPool,
            _ => null
        };
    }
}

public enum BulletType
{
    Player,
    Boss,
    Police
}
