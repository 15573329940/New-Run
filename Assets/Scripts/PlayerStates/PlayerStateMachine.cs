using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Callbacks;
using System.Linq;

/// <summary>
/// 角色状态机
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerCam cam;
    public Transform player;
    public Transform orientation;
    public Rigidbody rb;
    public PlayerDatas pd;
    public float playerHeight = 2f;
    [SerializeField]
    public String curStateType;//当前状态类型
    private PlayerStateBase curState;//当前状态
    public int index = -1;//当前状态索引
    private Dictionary<Type, PlayerStateBase> stateDic = new Dictionary<Type, PlayerStateBase>();//状态字典
    private Dictionary<Type, float> stateCoolTimer = new Dictionary<Type, float>();//状态冷却时间字典
    /// <summary>
    /// 进入状态
    /// </summary>
    /// <typeparam name="T">状态子类</typeparam>

    void Start()
    {
        rb = GetComponent<Rigidbody>();//获取刚体
        EnterState<PlayerIdleState>();//默认进入待机状态
        
    }
    void FixedUpdate()
    {
        curState?.FixedUpdate();//有状态就更新当前状态
    }
    void Update()
    {
        curStateType = curState?.GetType().ToString();//更新当前状态类型
        UpdateStateCoolTime();
        curState?.Update();//有状态就更新当前状态
    }
    void LateUpdate()
    {
        curState?.LateUpdate();//有状态就更新当前状态
    }
    public void EnterState<T>(int index=-1) where T : PlayerStateBase, new()//进入状态
    {

        if (curState?.GetType() == typeof(T) || stateCoolTimer.ContainsKey(typeof(T)))
        { return; }//防止重复进入同一状态
//        Debug.Log("EnterState:" + typeof(T));
        curState?.Exit();//有状态就退出当前状态
        curState = LoadState<T>();
        if(index>=0)
        {
            curState.index = index;
        }
        curState.Enter();
    }
    /// <summary>
    /// 尝试从字典取出状态
    /// </summary>
    /// <typeparam name="T">状态类</typeparam>
    /// <returns>状态实例</returns>
    private PlayerStateBase LoadState<T>() where T : PlayerStateBase, new()//加载状态
    {
        Type stateType = typeof(T);
        if (!stateDic.TryGetValue(stateType, out PlayerStateBase state))
        {
            state = new T();
            state.sm = this;//设置状态机
            state.Init();

            stateDic.Add(stateType, state);//添加新状态到字典，下次不用再初始化            
        }
        return state;
    }
    /// <summary>
    /// 停止状态机
    /// </summary>
    public void Stop()
    {
        curState?.Exit();//有状态就退出当前状态
        foreach (var state in stateDic.Values)
        {
            state.Destory();
        }
        stateDic.Clear();
    }
    /// <summary>
    /// 设置状态冷却时间
    /// </summary>
    /// <typeparam name="T">状态类</typeparam>
    /// <param name="coolTime">冷却时间</param>
    public void SetStateCoolDown<T>(float coolTime) where T : PlayerStateBase//设置状态冷却时间
    {
        Type stateType = typeof(T);
        if (stateCoolTimer.ContainsKey(stateType))
        {
            stateCoolTimer[stateType] = coolTime;
        }
        else
        {
            stateCoolTimer.Add(stateType, coolTime);
        }
    }
    void UpdateStateCoolTime()//更新状态冷却时间
    {
        foreach (var state in stateCoolTimer.Keys.ToList())
        {
            if (stateCoolTimer[state] >= 0)
            {
                stateCoolTimer[state] -= Time.deltaTime;
                if (stateCoolTimer[state] <= 0)
                {
                    stateCoolTimer.Remove(state); // 安全，因为遍历的是副本
                }
            }
        }
    }
    
    public void Die()//死亡
    {
        
        EnterState<PlayerDieState>();
    }
}
