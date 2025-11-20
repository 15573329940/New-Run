using UnityEngine;

public class SpearHitSensor : MonoBehaviour
{
    [Header("设置")]
    public LayerMask targetLayer; // 设置为 TitanBodybox
    public Transform playerRoot;  // 玩家根物体（用于确定射线的来源方向）

    // 防止一刀触发多次伤害/特效的计时器
    private float lastHitTime;
    private float hitCooldown = 0.1f; // 0.1秒内只算一次
    public bool hited=false;
    void OnTriggerEnter(Collider other)
    {
        if(hited) return;
        hited=true;
        // 1. 检查层级和冷却
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        // 2. 计算击中点 (Contact Point)
        // ClosestPoint 会返回 other 碰撞体表面距离 刀锋(transform.position) 最近的点
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // 3. 计算法线 (Normal) - 这是难点
        // 因为 OnTriggerEnter 不给法线，我们需要手动发射一条短射线来探测
        Vector3 normal = CalculateNormal(hitPoint, other);

        // 4. 播放喷血特效
        // Quaternion.LookRotation(normal) 会让特效的 Z 轴朝向法线方向
        SoundManager.Instance.PlayRandom(SoundCategory.Hit, hitPoint, 0.15f);
        VFXManager.Instance.PlayBurst(VFXCategory.BloodSpray, hitPoint, Quaternion.LookRotation(normal));
        SoundManager.Instance.PlayRandom(SoundCategory.Blood, hitPoint, 0.15f);

        // 5. 更新时间，防止连击
        lastHitTime = Time.time;

        // 6. 可以在这里通知 Player 或 Titan 扣血
        // Player.OnHitTitan(...)
    }

    /// <summary>
    /// 通过回溯射线计算法线
    /// </summary>
    Vector3 CalculateNormal(Vector3 hitPoint, Collider targetCollider)
    {
        // 方法 A：精准射线法
        // 从“玩家位置”或者“刀稍微往回一点的位置”向“击中点”发射射线
        Vector3 rayOrigin = transform.position; 
        
        // 如果有玩家引用，用玩家位置更稳，或者是刀柄位置
        if(playerRoot != null) rayOrigin = playerRoot.position; 

        Vector3 dir = (hitPoint - rayOrigin).normalized;
        
        // 稍微把起点往回拉一点，防止起点就在碰撞体内部
        // 同时也稍微把终点往里推一点，确保射线能穿过表面
        RaycastHit hitInfo;
        if (targetCollider.Raycast(new Ray(rayOrigin, dir), out hitInfo, 10f))
        {
            return hitInfo.normal;
        }

        // 方法 B：备用近似法 (如果射线没打中)
        // 假设巨人的身体部位近似圆柱体，从骨骼中心指向击中点的方向就是法线
        // 这对于胶囊体(Capsule)和球体(Sphere)非常准确
        return (hitPoint - targetCollider.transform.position).normalized;
    }
    
}