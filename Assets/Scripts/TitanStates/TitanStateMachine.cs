using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AI;
public class TitanStateMachine : MonoBehaviour
{
    public NavMeshAgent agent;
    public TitanAnimationController tac; 
    public string currentState;
    public float walkDistance = 10f;
    public float walkSpeed = 10f;
    public float attackDistance = 2f;
    public Transform player;
    public Hand hand;
    public Collider handCollider;
    public Rigidbody rb;
    public Animator ani;
    [SerializeField]
    private TitanStateBase curState;//当前状态
    public int index = -1;//当前状态索引
    private Dictionary<Type, TitanStateBase> stateDic = new Dictionary<Type, TitanStateBase>();//状态字典
    private Dictionary<Type, float> stateCoolTimer = new Dictionary<Type, float>();//状态冷却时间字典
    /// <summary>
    /// 进入状态
    /// </summary>
    /// <typeparam name="T">状态子类</typeparam>
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();//获取导航网格代理
    }
    void Start()
    {
        ani = GetComponent<Animator>();//获取动画控制器
        rb = GetComponent<Rigidbody>();//获取刚体
        EnterState<TitanIdleState>();//默认进入空中状态
        tac = GetComponent<TitanAnimationController>();//获取动画控制器
    }
    void FixedUpdate()
    {
        curState?.FixedUpdate();//有状态就更新当前状态
        //agent.SetDestination(new Vector3(0,0.5f,0));//设置导航网格代理目标位置
    }
    void Update()
    {
        currentState = curState?.GetType().Name;
        UpdateStateCoolTime();
        curState?.Update();//有状态就更新当前状态
    }
    void LateUpdate()
    {
        curState?.LateUpdate();//有状态就更新当前状态
    }
    public void EnterState<T>(int index=-1) where T : TitanStateBase, new()//进入状态
    {

        if (curState?.GetType() == typeof(T) || stateCoolTimer.ContainsKey(typeof(T)))
        { return; }//防止重复进入同一状态
        Debug.Log("EnterState:" + typeof(T));
        curState?.Exit();//有状态就退出当前状态
        curState = LoadState<T>();
        curState.Enter();
    }
    /// <summary>
    /// 尝试从字典取出状态
    /// </summary>
    /// <typeparam name="T">状态类</typeparam>
    /// <returns>状态实例</returns>
    private TitanStateBase LoadState<T>() where T : TitanStateBase, new()//加载状态
    {
        Type stateType = typeof(T);
        if (!stateDic.TryGetValue(stateType, out TitanStateBase state))
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
    public void SetStateCoolDown<T>(float coolTime) where T : TitanStateBase//设置状态冷却时间
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
}
