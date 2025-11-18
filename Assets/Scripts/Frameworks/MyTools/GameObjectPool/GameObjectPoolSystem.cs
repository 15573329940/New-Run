using System.Collections.Generic;
using UnityEngine;
public class GameObjectPoolSystem : SingletonBase<GameObjectPoolSystem>
{
    [System.Serializable]
    private class GameObjectPool
    {
        public string objName; // 
        public int count;
        public GameObject[] prefabs;
    }

    [SerializeField, Header("预制体配置")] 
    private List<GameObjectPool> _poolsList = new List<GameObjectPool>();
    
    [SerializeField, Header("池对象根目录")] 
    private Transform poolObjectParent;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObjectPool> poolsDictionary = new Dictionary<string, GameObjectPool>();

    protected override void Awake()
    {
        InitPool();
    }

    private void InitPool()
    {
        if (poolObjectParent == null) poolObjectParent = transform; // 

        foreach (var pool in _poolsList)
        {
            // 1. 
            if (!poolsDictionary.ContainsKey(pool.objName))
            {
                poolsDictionary.Add(pool.objName, pool);
            }
            
            // 2. 
            if (!pools.ContainsKey(pool.objName))
            {
                pools.Add(pool.objName, new Queue<GameObject>());
                if (pool.prefabs.Length == 0) continue;

                for (int j = 0; j < pool.count; j++)
                {
                    GameObject newObj = CreateNewObject(pool);
                    pools[pool.objName].Enqueue(newObj);
                }
            }
        }
    }

    private GameObject CreateNewObject(GameObjectPool pool)
    {
        GameObject prefab = pool.prefabs[Random.Range(0, pool.prefabs.Length)];
        GameObject temp_Gameobject = Instantiate(prefab, poolObjectParent); // 
        temp_Gameobject.SetActive(false); // 
        return temp_Gameobject;
    }

    /// <summary>
    /// 
    /// </summary>
    public GameObject TakeGameObject(string objectName)
    {
        if (!pools.ContainsKey(objectName))
        {
            Debug.LogError($"对象池中不存在名为 {objectName} 的对象");
            return null;
        }

        GameObject dequeueObject;
        Queue<GameObject> queue = pools[objectName];

        if (queue.Count > 0)
        {
            // 
            dequeueObject = queue.Dequeue();
        }
        else
        {
            // 
            if (!poolsDictionary.ContainsKey(objectName)) return null; 
            GameObjectPool pool = poolsDictionary[objectName];
            dequeueObject = CreateNewObject(pool); // 
        }

        dequeueObject.SetActive(true);
        dequeueObject.transform.SetParent(null); // 
        return dequeueObject;
    }

    /// <summary>
    /// 
    /// </summary>
    public void RecycleGameObject(GameObject gameObject, string objectName)
    {
        if (gameObject == null) return;

        if (!pools.ContainsKey(objectName))
        {
            Debug.LogWarning($"试图回收未注册的池对象: {objectName}，已销毁。");
            Destroy(gameObject);
            return;
        }

        // 
        gameObject.GetComponent<IPool>()?.RecycleObject();

        // 
        gameObject.SetActive(false);
        gameObject.transform.SetParent(poolObjectParent); // 
        
        pools[objectName].Enqueue(gameObject); // 
    }
}