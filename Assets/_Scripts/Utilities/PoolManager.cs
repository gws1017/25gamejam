using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오브젝트 풀 총괄 매니저.
/// - 에디터에서 <see cref="poolEntryList"/>에 풀 항목을 등록하면 Awake() 때 자동으로 풀 생성/등록.
/// - 스폰 시 <see cref="Spawn{T}(Vector3, Quaternion, string)"/> 으로 key 기반 꺼내쓰기.
/// - key 이름은 Spawner의 monsterKeys 등 외부 참조와 반드시 동일해야 함.
/// </summary>
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
        [Tooltip("이 풀을 식별하는 Key. Spawner.monsterKeys 등 외부에서 사용할 키와 '완전히 동일'해야 합니다.")]
        public string key;

        [Tooltip("풀링할 프리팹. 프리팹에는 반드시 IPoolable을 구현한 컴포넌트가 1개 이상 붙어 있어야 합니다.")]
        public GameObject prefab;

        [Tooltip("초기 생성 개수 예약(preSize). 런타임 중 부족하면 자동 생성될 수 있습니다.")]
        public int size = 20;
    }

    [Header("Editor Setup")]
    [Tooltip("에디터에서 등록할 풀 목록입니다. key/프리팹/초기수량을 채우면 Awake() 시 자동 등록됩니다.")]
    [SerializeField] private List<PoolEntry> poolEntryList = new List<PoolEntry>();
    private Dictionary<string, GenericPool<MonoBehaviour>> pools = new Dictionary<string, GenericPool<MonoBehaviour>>();

    /// <summary>
    /// key에 해당하는 풀을 등록합니다.
    /// </summary>
    /// <param name="key">식별용 키(Spawner 등 외부 참조와 동일해야 함)</param>
    /// <param name="prefab">IPoolable을 구현한 컴포넌트를 가진 프리팹의 '컴포넌트'</param>
    /// <param name="size">초기 생성 수량</param>
    public void Register(string key, MonoBehaviour prefab, int size)
    {
        var pool = new GenericPool<MonoBehaviour>(prefab, size, transform);
        Register(key, pool);
    }
    public GenericPool<MonoBehaviour> Get(string key)
    {
        if (!pools.TryGetValue(key, out var pool)) return null;
        return pool;
    }

    /// <summary>
    /// key에 해당하는 풀에서 객체를 스폰합니다.
    /// </summary>
    /// <typeparam name="T">요청하는 컴포넌트 타입(예: Bullet, MonsterCharacter)</typeparam>
    /// <param name="position">스폰 위치</param>
    /// <param name="rotation">스폰 회전</param>
    /// <param name="poolKey">풀 key (Spawner/Robot/총알 등에서 사용하는 문자열)</param>
    /// <returns>성공 시 해당 컴포넌트, 실패 시 null</returns>
    public T Spawn<T>(Vector3 position, Quaternion rotation, string poolKey) where T : Component
    {
        var pool = Get(poolKey);
        if(pool != null)
        {
            var obj = pool.Spawn(position, rotation, poolKey);
            return obj as T;
        }
        Debug.LogError($"[{poolKey}] 잘못된 Key 값입니다..");
        return null;
    }

    #region Private

    private void Awake()
    {
        SingleTon();

        foreach (var entry in poolEntryList)
        {
            // 프리팹에서 실제로 풀링할 '컴포넌트' 하나를 고른다.
            // 규칙: IPoolable을 구현한 컴포넌트를 풀링 대상으로 삼자 (없으면 에러)
            var comp = entry.prefab.GetComponent<IPoolable>() as MonoBehaviour;
            if (comp == null)
            {
                Debug.LogError($"[{entry.key}] 프리팹에 IPoolable 컴포넌트가 없습니다.");
                continue;
            }

            var pool = new GenericPool<MonoBehaviour>(comp, entry.size, transform);
            Register(entry.key, pool);
        }

    }

    //신규 오브젝트 풀 등록
    private void Register(string key, GenericPool<MonoBehaviour> pool)
    {
        pools[key] = pool;
    }
    #endregion
}
