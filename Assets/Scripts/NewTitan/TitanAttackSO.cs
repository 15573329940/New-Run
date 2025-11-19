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
                                 Vector3 targetVelocity, float attackSpeed, float attackerSize)
    {
        if (!HasKeyframes) 
            return false; // 没有数据，无法预测

        foreach (var keyframe in Keyframes)
        {
            if (keyframe.CheckCollision(attackerTransform, targetCurrentPos,
                                       targetVelocity, attackSpeed, attackerSize))
            {
                // 只要有任何一个关键帧能"预测"到碰撞，就返回 true
                return true;
            }
        }
        return false;
    }
}
