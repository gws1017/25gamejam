using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private Transform container;
    private T prefab;

    //풀 생성
    public GenericPool(T prefab, int initialSize, Transform parent = null)
    {
        //프리팹 및 풀 컨테이너 등록
        this.prefab = prefab;
        container = new GameObject($"{prefab.name}_Pool").transform;
        if (parent != null)
            container.SetParent(parent);

        //객체 미리 생성
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab, container);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Spawn(Vector3 position, Quaternion rotation,string poolKey = null)
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : GameObject.Instantiate(prefab, container);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);

        if(obj is IPoolable poolableObj)
        {
            if(poolKey != null)
                poolableObj.poolKey = poolKey;
            poolableObj.OnSpawn();
        }
        return obj;
    }

    public void Despawn(T obj)
    {
        if (obj is IPoolable poolableObj)
            poolableObj.OnDespawn();

        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}

