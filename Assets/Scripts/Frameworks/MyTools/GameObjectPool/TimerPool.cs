using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerPool : MonoBehaviour
{
    // 单例模式，方便全局访问
    public static TimerPool Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("池子初始容量")]
    public int initialCapacity = 10;

    // 真正的池子容器
    private readonly Queue<Timer> _pool = new Queue<Timer>();
    
    // 存放所有计时器的父节点，保持Hierarchy整洁
    private Transform _poolContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 保证切换场景不销毁池子
            DontDestroyOnLoad(gameObject); 
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        _poolContainer = new GameObject("TimerContainer").transform;
        _poolContainer.SetParent(transform);

        for (int i = 0; i < initialCapacity; i++)
        {
            CreateNewTimer();
        }
    }

    private Timer CreateNewTimer()
    {
        // 创建一个新的空物体
        GameObject go = new GameObject("PooledTimer");
        go.transform.SetParent(_poolContainer);
        
        // 添加 Timer 组件
        Timer timer = go.AddComponent<Timer>();
        
        // 默认设为非激活状态
        go.SetActive(false);
        
        // 入队
        _pool.Enqueue(timer);
        return timer;
    }

    /// <summary>
    /// 从池中获取并启动一个计时器
    /// </summary>
    public Timer GetTimer(float duration, Action onComplete)
    {
        Timer timer;

        if (_pool.Count > 0)
        {
            timer = _pool.Dequeue();
        }
        else
        {
            // 池子空了，创建新的（自动扩容）
            timer = CreateNewTimer();
            // 刚创建的是入队了的，需要先出队
            _pool.Dequeue(); 
        }

        timer.gameObject.SetActive(true);
        
        // 启动计时器，并传入“回收自身”的方法
        timer.StartTimer(duration, onComplete, ReturnTimer);
        
        return timer;
    }

    /// <summary>
    /// 回收计时器（由 Timer 类自动调用）
    /// </summary>
    private void ReturnTimer(Timer timer)
    {
        timer.gameObject.SetActive(false);
        _pool.Enqueue(timer);
    }
}