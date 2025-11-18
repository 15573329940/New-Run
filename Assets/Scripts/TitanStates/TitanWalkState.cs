using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class TitanWalkState : TitanStateBase
{
    public override void Init()
    {
        base.Init();
    }//初始化
    public override void Enter()
    {
        base.Enter();
        
        ani.SetTrigger("Amarture_VER2|run_walk");
    }
    public override void Update()
    {
        base.Update();

        if (distanceToPlayer > sm.walkDistance)
        {
            sm.EnterState<TitanIdleState>();
        }
        else if (distanceToPlayer <= sm.attackDistance)
        {
            //sm.EnterState<TitanAttackState>();
        }
        sm.agent.SetDestination(player.position);//设置导航网格代理目标位置
        //rb.velocity = dirctionToPlayer * sm.walkSpeed;
        //sm.transform.forward = dirctionToPlayer;
    }
}

