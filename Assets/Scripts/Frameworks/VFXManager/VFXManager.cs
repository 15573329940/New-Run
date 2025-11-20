using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 定义特效类型
public enum VFXCategory
{
    None,
    JetSpray,       // 喷气
    SwordSlash,     // 刀光
    HitImpact,      // 金属火花
    BloodSpray      // [新增] 喷血特效
}
// 2. 配置类：将枚举和Prefab绑定
[System.Serializable]
public class VFXGroup
{
    public VFXCategory category;
    public GameObject prefab;     // 这里直接对应具体的粒子Prefab
    public int initialCount = 5;  // 这种特效预热多少个
}

public class VFXManager : SingletonBase<VFXManager>
{
    [Header("特效配置")]
    [SerializeField] private List<VFXGroup> vfxGroups = new List<VFXGroup>();

    // 3. 核心容器
    // Key: 特效类型, Value: 该类型对应的 Prefab (用于新生成)
    private Dictionary<VFXCategory, GameObject> prefabDict;
    
    // Key: 特效类型, Value: 该类型的对象池队列
    private Dictionary<VFXCategory, Queue<GameObject>> poolDict;

    // 父节点容器，保持Hierarchy整洁
    private Transform poolRoot;

    protected override void Awake()
    {
        base.Awake();
        
        poolRoot = new GameObject("VFX_Pool_Root").transform;
        poolRoot.SetParent(transform);

        prefabDict = new Dictionary<VFXCategory, GameObject>();
        poolDict = new Dictionary<VFXCategory, Queue<GameObject>>();

        // 初始化
        foreach (var group in vfxGroups)
        {
            if (group.category == VFXCategory.None || group.prefab == null) continue;

            // 记录 Prefab
            if (!prefabDict.ContainsKey(group.category))
            {
                prefabDict.Add(group.category, group.prefab);
            }

            // 创建对应的池子队列
            if (!poolDict.ContainsKey(group.category))
            {
                poolDict.Add(group.category, new Queue<GameObject>());
            }

            // 预热（可选）
            for (int i = 0; i < group.initialCount; i++)
            {
                CreateNewInstance(group.category);
            }
        }
    }

    // 创建新实例并放入池子（但不激活）
    private GameObject CreateNewInstance(VFXCategory category)
    {
        if (!prefabDict.ContainsKey(category)) return null;

        GameObject prefab = prefabDict[category];
        GameObject newObj = Instantiate(prefab, poolRoot);
        
        // 挂载自动回收脚本（如果没有的话）
        var recycler = newObj.GetComponent<AutoRecycleVFX>();
        if (recycler == null) recycler = newObj.AddComponent<AutoRecycleVFX>();
        recycler.Init(category, this); // 注入身份信息

        newObj.SetActive(false);
        poolDict[category].Enqueue(newObj); // 刚创建的直接入队
        return newObj;
    }

    // 从池中获取
    private GameObject GetFromPool(VFXCategory category)
    {
        if (!poolDict.ContainsKey(category))
        {
            Debug.LogWarning($"VFXManager: 没有配置类型为 {category} 的特效！");
            return null;
        }

        Queue<GameObject> queue = poolDict[category];
        GameObject obj = null;

        // 尝试出队，直到找到一个没被销毁的物体
        while (queue.Count > 0)
        {
            obj = queue.Dequeue();
            if (obj != null) break;
        }

        // 如果池子空了，扩容
        if (obj == null)
        {
            obj = CreateNewInstance(category);
            // CreateNewInstance 会把新对象入队，所以这里要把它拿出来用，还得再Dequeue一次
            if(queue.Count > 0) obj = queue.Dequeue();
        }

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 回收特效（由 AutoRecycleVFX 调用）
    /// </summary>
    public void Recycle(VFXCategory category, GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(poolRoot); // 归位
        obj.transform.localScale = prefabDict[category].transform.localScale; // 重置缩放（防止被父物体影响）
        
        if (poolDict.ContainsKey(category))
        {
            poolDict[category].Enqueue(obj);
        }
    }

    // =================================================
    //                  功能接口
    // =================================================

    /// <summary>
    /// 播放一次性特效（如：刀光、爆炸）
    /// </summary>
    public void PlayBurst(VFXCategory category, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetFromPool(category);
        if (obj == null) return;

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        // 获取粒子系统并重置播放
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            ps.Play();
            // 启动自动回收，时间是粒子的持续时间
            obj.GetComponent<AutoRecycleVFX>().StartAutoRecycle(ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }

    /// <summary>
    /// 播放跟随特效（如：喷气背包），返回对象由调用者控制
    /// </summary>
    public GameObject PlayLooping(VFXCategory category, Transform parent)
    {
        GameObject obj = GetFromPool(category);
        if (obj == null) return null;

        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        return obj; // 返回给玩家脚本，让玩家决定什么时候停止
    }
    
    /// <summary>
    /// 停止循环特效并回收
    /// </summary>
    public void StopLooping(GameObject vfxObj)
    {
        if (vfxObj == null) return;
        
        // 可以在这里加一个停止发射的缓冲，或者直接回收
        // 这里演示直接回收
        var recycler = vfxObj.GetComponent<AutoRecycleVFX>();
        if (recycler != null)
        {
            recycler.RecycleNow();
        }
        else
        {
            Destroy(vfxObj); // 安全兜底
        }
    }
}