using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    #region [Singleton]
    public static PoolManager Instance { get; private set; }
    private void SingleTon()
    {
        Instance = this;
    }
    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

    [System.Serializable]
    public class PoolEntry
    {
        public string key;
        public Component prefab;
        public int size = 20;
    }
    [SerializeField] private List<PoolEntry> poolEntryList = new List<PoolEntry>();
    private Dictionary<string, object> pools = new();

    private void Awake()
    {
        SingleTon();

        foreach(var entry in poolEntryList)
        {
            var type = entry.prefab.GetType();
            var method = typeof(PoolManager).GetMethod(nameof(RegisterGeneric), System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?.MakeGenericMethod(type);
            method?.Invoke(this, new object[] { entry.key, entry.prefab, entry.size });

            //var entryPool = new GenericPool<Component>(entry.prefab,entry.size, transform);
            //PoolManager.Instance.Register(entry.key, entryPool);
        }
        
    }
    private void RegisterGeneric<T>(string key, Component prefab, int size) where T : Component
    {
        var pool = new GenericPool<T>((T)prefab, size, transform);
        Register(key, pool);
    }
    //신규 오브젝트 풀 등록
    public void Register<T>(string key, GenericPool<T> pool) where T : Component
    {
        pools[key] = pool;
    }

    //오브젝트 꺼내오기
    public GenericPool<T> Get<T>(string key) where T : Component
    {
        if (pools.Count <= 0) return null;
        return pools[key] as GenericPool<T>;
    }

}
