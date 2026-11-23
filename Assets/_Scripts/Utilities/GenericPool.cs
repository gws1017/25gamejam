using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private Transform container;
    private T prefab;
    private readonly int maxSize;   // 동시에 존재할 수 있는 최대 개수. 0이면 제한 없음
    private int createdCount;       // 지금까지 만든 개수 (= 풀이 관리하는 전체 개수)

    //풀 생성
    /// <param name="initialSize">미리 만들어 둘 개수 (최대 개수가 아님)</param>
    /// <param name="maxSize">최대 개수. 0이면 제한 없음. initialSize보다 작으면 initialSize로 맞춘다</param>
    public GenericPool(T prefab, int initialSize, Transform parent = null, int maxSize = 0)
    {
        //프리팹 및 풀 컨테이너 등록
        this.prefab = prefab;
        this.maxSize = (maxSize > 0) ? Mathf.Max(maxSize, initialSize) : 0;
        container = new GameObject($"{prefab.name}_Pool").transform;
        if (parent != null)
            container.SetParent(parent);

        //객체 미리 생성
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab, container);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            createdCount++;
        }
    }

    /// <returns>꺼낸 객체. 최대 개수까지 전부 사용 중이면 null (호출부에서 스폰을 건너뛰어야 한다)</returns>
    public T Spawn(Vector3 position, Quaternion rotation,string poolKey = null)
    {
        T obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            if (maxSize > 0 && createdCount >= maxSize) return null;   // 최대 개수 도달 → 스폰 안 함
            obj = GameObject.Instantiate(prefab, container);
            createdCount++;
        }

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
