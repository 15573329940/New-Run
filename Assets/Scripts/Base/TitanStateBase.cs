using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class TitanStateBase
{
    public TitanStateMachine sm;//状态机
    public Animator ani;
    public Transform player;
    public Transform orientation;
    public Rigidbody rb;
    public float distanceToPlayer;
    public Vector3 dirctionToPlayer;
    

    public virtual void Init()
    {
        player = sm.player;//获取玩家
        rb = sm.rb;//获取刚体
        ani = sm.ani;//获取动画控制器
    }//初始化
    public virtual void Enter() { }//进入状态
    public virtual void Exit() { }//退出状态
    public virtual void Destory() { }//销毁状态

    public virtual void Update()
    {
        distanceToPlayer = new Vector3(player.position.x - sm.transform.position.x, 0, player.position.z - sm.transform.position.z).magnitude;
        dirctionToPlayer = new Vector3(player.position.x - sm.transform.position.x, 0, player.position.z - sm.transform.position.z).normalized;
//        Debug.Log(distanceToPlayer);
    }//没有继承MonoBehaviour，不会每帧执行，由StateMachine统一管理
    
    public virtual void FixedUpdate() { }//没有继承MonoBehaviour，不会每帧执行，由MonoManager统一管理
    public virtual void LateUpdate() { }//没有继承MonoBehaviour，不会每帧执行，由MonoManager统一管理

    public void TryPlayAnimation(string targetAnimationName, int layer = 0)
    {
        AnimatorStateInfo currentState = ani.GetCurrentAnimatorStateInfo(layer);
        bool isInTargetState = currentState.IsName(targetAnimationName);

        if (!isInTargetState)
        {
            ani.Play(targetAnimationName);
        }
        // 若需要退出动画，可在其他逻辑中设置参数为false
    }
    public bool CheckAnimation(string targetAnimationName, int layer = 0)
    {
        AnimatorStateInfo currentState = ani.GetCurrentAnimatorStateInfo(layer);
        bool isInTargetState = currentState.IsName(targetAnimationName);
        return isInTargetState;
        // 若需要退出动画，可在其他逻辑中设置参数为false
    }
}
