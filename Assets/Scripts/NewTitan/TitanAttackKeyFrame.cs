using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TitanAttackKeyFrame
{
    [Tooltip("此关键帧在动画中的时间点 (秒)")]
    public float Time;

    [Tooltip("攻击判定框(Hitbox)相对于巨人根节点的【局部位置】")]
    public Vector3 LocalPosition;

    [Tooltip("攻击判定框(Hitbox)的半径")]
    public float Radius;
    public bool CheckCollision(Transform attackerTransform, Vector3 targetCurrentPos,
                               Vector3 targetVelocity, float attackSpeed, float attackerSize)
    {
        // 1. 计算真正发生碰撞的时间 (动画速度越快, 时间越短)
        float timeToCollision = this.Time / attackSpeed;

        // 2. 预测玩家的未来位置 (未来位置 = 当前位置 + 速度 * 时间)
        Vector3 predictedTargetPos = targetCurrentPos + (targetVelocity * timeToCollision);

        // 3. 计算攻击 Hitbox 的未来世界位置
        Vector3 scaledLocalPos = this.LocalPosition * attackerSize;
        Vector3 worldHitboxPos = attackerTransform.TransformPoint(scaledLocalPos);

        // 4. 缩放 Hitbox 的半径
        float scaledRadius = this.Radius * attackerSize;
        // 5. 执行碰撞检查 (使用 SqrMagnitude 避免开根号)
        float distanceSqr = (predictedTargetPos - worldHitboxPos).sqrMagnitude;
        
        // (假设玩家半径为1，你可以按需修改)
        float playerRadius = 9f; 
        float radiiSqr = (scaledRadius + playerRadius) * (scaledRadius + playerRadius);

        return distanceSqr < radiiSqr;
    }
}
