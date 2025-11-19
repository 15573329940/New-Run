using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 巨人的核心状态机
/// </summary>
public enum TitanState
{
    Idle,
    Chasing,
    Attacking,
    Hurt,
    Dead
}
/// <summary>
/// 巨人的可受伤部位
/// </summary>
public enum BodyPart 
{ 
    Body, 
    Hand_L, 
    Hand_R, 
    Foot_L, 
    Foot_R, 
    Eye, 
    Nape // 后颈
}
public enum TitanAttackType
{
    None,
    LeftFootStomp,
    RightFootStomp,
    LHighGrab,LMiddleGrab,LLowGrab,
    RHighGrab,RMiddleGrab,RLowGrab,
    ClapAttack,
    ThrowRock
    // ... 在这里添加你所有的攻击动画 ...
}