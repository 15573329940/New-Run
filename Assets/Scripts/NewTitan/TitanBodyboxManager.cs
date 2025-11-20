using UnityEngine;
using System.Collections.Generic;

public class TitanBodyboxManager : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("巨人身体碰撞器所在的图层")]
    public LayerMask titanBodyLayer;

    [Tooltip("玩家在这个距离内才会尝试启用碰撞器")]
    public float detectionDistance = 100f;
    public float detectionDistance2 = 15f;

    [Tooltip("摄像机朝向与巨人连线的最大夹角 (度)")]
    [Range(0, 180)]
    public float viewAngleThreshold = 25f;

    [Header("调试/状态")]
    [Tooltip("自动获取到的所有身体碰撞器")]
    public List<Collider> bodyColliders = new List<Collider>();

    [Tooltip("当前碰撞器是否处于激活状态")]
    [SerializeField] private bool _areActive = false;

    // 缓存引用
    private Transform _playerCamera;
    public Transform _titanCenter; // 用于计算角度的中心点

    void Awake()
    {
        // 1. 获取玩家摄像机引用 (假设主摄像机带有 MainCamera 标签)
        if (Camera.main != null)
        {
            _playerCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("TitanBodyManager: 找不到 MainCamera，请确保场景中有标签为 MainCamera 的摄像机！");
            enabled = false;
            return;
        }

        // 2. 确定巨人的“中心点” (默认用根节点，如果有胸部骨骼建议赋值给这里，计算角度更准)
        // 简单起见，我们这里使用 transform.position + 向上偏移一点，防止脚底坐标导致角度偏差
        // 你也可以手动在 Inspector 中指定一个 Transform


        // 3. 获取所有指定图层的子碰撞器并初始化
        FindAndInitColliders();
    }

    void Update()
    {
        if (_playerCamera == null) return;

        // --- 核心检测逻辑 ---

        // 1. 计算距离平方 (比 Vector3.Distance 更快)
        float sqrDist = (_titanCenter.position - _playerCamera.position).sqrMagnitude;
        bool isCloseEnough = sqrDist < (detectionDistance * detectionDistance);
        bool isCloseEnough2 = sqrDist < (detectionDistance2 * detectionDistance2);
        // 默认不看
        bool isLookingAt = false;
        if (isCloseEnough2)
        {
            isLookingAt = true;
        }
        else
        {
            if (isCloseEnough)
            {
                // 2. 计算角度
                // 向量：摄像机 -> 巨人
                Vector3 dirToTitan = (_titanCenter.position - _playerCamera.position).normalized;
                // 夹角：摄像机正前方 vs 指向巨人的方向
                float angle = Vector3.Angle(_playerCamera.forward, dirToTitan);

                if (angle < viewAngleThreshold)
                {
                    isLookingAt = true;
                }
            }
        }

        // 只有距离够近时，才去计算角度 (省性能)


        // --- 状态切换逻辑 ---
        // 只有满足：(够近) AND (正在看) -> 启用
        bool shouldBeActive = isCloseEnough && isLookingAt;

        // 只有当“目标状态”与“当前状态”不一致时，才执行循环 (性能优化关键)
        if (shouldBeActive != _areActive)
        {
            SetCollidersState(shouldBeActive);
        }
    }

    /// <summary>
    /// 初始化：查找并禁用
    /// </summary>
    void FindAndInitColliders()
    {
        bodyColliders.Clear();

        // 获取所有子物体的碰撞器 (包括未启用的)
        Collider[] allChildColliders = GetComponentsInChildren<Collider>(true);

        foreach (var col in allChildColliders)
        {
            // 使用位运算检查 Layer 是否匹配
            // (1 << col.gameObject.layer) 创建该物体图层的掩码
            if ((titanBodyLayer.value & (1 << col.gameObject.layer)) != 0)
            {
                bodyColliders.Add(col);
            }
        }

        // 初始化：强制禁用所有
        SetCollidersState(false);

        Debug.Log($"TitanBodyManager: 初始化完成，找到 {bodyColliders.Count} 个身体碰撞器。");
    }

    /// <summary>
    /// 批量设置碰撞器启用状态
    /// </summary>
    void SetCollidersState(bool state)
    {
        _areActive = state;

        for (int i = 0; i < bodyColliders.Count; i++)
        {
            if (bodyColliders[i] != null)
            {
                bodyColliders[i].enabled = state;
            }
        }
    }

    // 可视化调试范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}