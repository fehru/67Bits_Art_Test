using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ObjectPool
{
    public static Dictionary<GameObject, Pool> Pools = new Dictionary<GameObject, Pool>();
    public static Pool CreatePool(this GameObject obj, int? starSize = null, int? maxSize = null)
    {
        if (!Pools.TryGetValue(obj, out Pool pool))
        {
            pool = new Pool(obj, new GameObject(obj.name.ToString() + "(Pool)").transform, starSize, maxSize);
            Pools.Add(obj, pool);
        }
        return pool;
    }
    public static GameObject InstantiateFromPool(this GameObject poolId,
        Vector3? position = null, Quaternion? rotation = null,
        bool disabledOnly = false, bool autoEnable = true, bool autoReturn = true)
    {
        GameObject instance;
        if (!Pools.TryGetValue(poolId, out Pool pool))
            pool = CreatePool(poolId);
        instance = pool.Get(disabledOnly);
        if (instance)
        {
            if (autoEnable) instance.SetActive(true);
            if (autoReturn) pool.Return(instance);
            instance.transform.SetPositionAndRotation(position ?? instance.transform.position, rotation ?? instance.transform.rotation);
        }
        return instance;
    }
}
public class Pool
{
    public GameObject Prefab;
    public Transform PoolParent;
    public int MaxSize;
    public int StartSize;
    public Queue<GameObject> Objects = new Queue<GameObject>();
    public Pool(GameObject prefab, Transform poolParent, int? minSize = null, int? maxSize = null, bool spawnSizeAtStart = false)
    {
        Prefab = prefab;
        StartSize = minSize ?? 5;
        MaxSize = maxSize ?? 1000;
        PoolParent = poolParent;
        if (spawnSizeAtStart) for (int i = 0; i < StartSize; i++) Spawn(false);
    }
    public GameObject Get(bool firstDisbled)
    {
        if (Objects.Count < StartSize) return Spawn();
        if (firstDisbled)
        {
            var instance = Objects.FirstOrDefault((obj) => !obj.activeInHierarchy);
            if (!instance && Objects.Count < MaxSize) instance = Spawn();
            return instance;
        }
        if (Objects.Count == 0 && Objects.Count < MaxSize) Spawn();
        var toReturn = Objects.Dequeue();
        toReturn.SetActive(false);
        return toReturn;
    }
    public void Return(GameObject toReturn) => Objects.Enqueue(toReturn);
    private GameObject Spawn(bool setActive = true)
    {
        var instance = MonoBehaviour.Instantiate(Prefab);
        instance.SetActive(setActive);
        Objects.Enqueue(instance);
        instance.transform.parent = PoolParent;
        return instance;
    }
}
