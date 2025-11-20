using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TitanAttack", menuName = "Titan/Attack Info")]
[System.Serializable]
public class TitanAttackSO : ScriptableObject
{
    [Tooltip("攻击类型 (必须与 Animator Trigger 匹配)")]
    public TitanAttackType attackType;
    [Tooltip("此次攻击造成的伤害")]
    public int Damage = 10;
    [Tooltip("攻击动画的冷却时间")]
    public float Cooldown = 2.5f;
    // ⬇️ 新增：直接存储动画长度
    public float animationLength;
    [Header("智能攻击关键帧")]
    [Tooltip("“录制”的动画关键帧数据")]
    public List<TitanAttackKeyFrame> Keyframes = new List<TitanAttackKeyFrame>();
    public bool HasKeyframes => Keyframes.Count > 0;
    /// <summary>
    /// 预测此攻击是否击中
    /// </summary>
    public bool CheckSmartAttack(Transform attackerTransform, Vector3 targetCurrentPos,
                                 Vector3 targetVelocity, Vector3 targetAcceleration, // <--- 1. 这里也要加参数
                                 float attackSpeed, float attackerSize)
    {
        if (!HasKeyframes) return false;

        foreach (var keyframe in Keyframes)
        {
            // 2. 这里调用 KeyFrame 的新方法，把加速度传进去
            if (keyframe.CheckCollision(attackerTransform, targetCurrentPos,
                                       targetVelocity, targetAcceleration, // <--- 传入加速度
                                       attackSpeed, attackerSize))
            {
                return true;
            }
        }
        return false;
    }
}
