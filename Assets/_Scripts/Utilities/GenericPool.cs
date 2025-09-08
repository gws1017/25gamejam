using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private Transform container;
    private T prefab;

    public GenericPool(T prefab, int initialSize, Transform containerParent = null)
    {
        this.prefab = prefab;
        container = new GameObject($"{typeof(T).Name}_Pool").transform;
        if (containerParent != null)
            container.SetParent(containerParent);

        for (int i = 0; i < initialSize; i++)
        {
            var obj = GameObject.Instantiate(prefab, container);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Spawn(Vector3 position, Quaternion rotation)
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : GameObject.Instantiate(prefab, container);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Despawn(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}

