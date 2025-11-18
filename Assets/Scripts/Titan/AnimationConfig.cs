using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 枚举定义所有碰撞部位（可根据需求扩展）
public enum GiantBodyPart
{
    HighL, HighR, MiddleL, MiddleR, LowL, LowR,
    FarGround,Chest,Belly,LKick,RSlap,

}

// 2. 动画配置管理器（关联部位与动画名称，可扩展为ScriptableObject配置）
public static class AnimationConfig
{
    // 字典：部位 → 对应的攻击动画名称
    private static readonly Dictionary<GiantBodyPart, string> _partToAnimation = new Dictionary<GiantBodyPart, string>()
    {
        { GiantBodyPart.HighL, "Amarture_VER2|attack_anti_AE_high_l" },
        { GiantBodyPart.HighR, "Amarture_VER2|attack_anti_AE_high_r" },
        { GiantBodyPart.MiddleL, "Amarture_VER2|attack_anti_AE_l" },
        { GiantBodyPart.MiddleR, "Amarture_VER2|attack_anti_AE_r" },
        { GiantBodyPart.LowL, "Amarture_VER2|attack_anti_AE_low_l" },
        { GiantBodyPart.LowR, "Amarture_VER2|attack_anti_AE_low_r" },
        { GiantBodyPart.FarGround, "Amarture_VER2|attack_comboSlam" },
        { GiantBodyPart.Chest, "Amarture_VER2|attack_jumper_1" },
        { GiantBodyPart.LKick, "Amarture_VER2|attack_kick" },
        { GiantBodyPart.RSlap, "Amarture_VER2|attack_swing_l" },
    };
    // 获取部位对应的动画名称
    public static string GetAnimationName(GiantBodyPart part)
    {
        if (_partToAnimation.TryGetValue(part, out string animName))
        {
            return animName;
        }
        Debug.LogError($"未配置部位 {part} 对应的动画！");
        return null;
    }
}