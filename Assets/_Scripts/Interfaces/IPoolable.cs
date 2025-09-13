using UnityEngine;

public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();

    string poolKey { get; set; }
}
