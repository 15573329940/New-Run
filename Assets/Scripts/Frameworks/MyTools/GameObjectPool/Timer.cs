using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float _timeLeft;
    private Action _onComplete;
    private Action<Timer> _onRecycle; // 用于通知池子回收自己
    private bool _isRunning = false;

    /// <summary>
    /// 初始化并启动计时器
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="onComplete">完成时的回调</param>
    /// <param name="onRecycle">回收时的回调（通常是还给对象池）</param>
    public void StartTimer(float duration, Action onComplete, Action<Timer> onRecycle)
    {
        _timeLeft = duration;
        _onComplete = onComplete;
        _onRecycle = onRecycle;
        _isRunning = true;
        
        // 确保启用 Update
        enabled = true; 
    }

    private void Update()
    {
        if (!_isRunning) return;

        if (_timeLeft > 0)
        {
            // 这里使用 unscaledDeltaTime 还是 deltaTime 取决于你是否希望计时器受游戏暂停(TimeScale)影响
            // 通常战斗逻辑用 deltaTime，UI逻辑用 unscaledDeltaTime
            _timeLeft -= Time.deltaTime;
        }
        else
        {
            Finish();
        }
    }
    
    // 强制停止（如果需要手动打断）
    public void Stop()
    {
        if (_isRunning)
        {
            // 即使被打断，通常也需要回收
            _isRunning = false;
            _onRecycle?.Invoke(this);
        }
    }

    private void Finish()
    {
        _isRunning = false;
        
        // 1. 执行业务逻辑
        _onComplete?.Invoke();
        
        // 2. 自动回收（通知池子把自己收回去）
        _onRecycle?.Invoke(this);
    }
}