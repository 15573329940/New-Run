using UnityEngine;
using UnityEngine.AI;

// 4. 巨人动画控制器（统一处理动画播放）
public class TitanAnimationController : MonoBehaviour
{
    private Animator _animator;
    public float coolDownTime = 3.5f;
    float coolDownTimer = 0f;
    public NavMeshAgent agent;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        // 找到所有部位的检测器，注册事件
        RegisterAllBodyParts();
    }
    private void Update()
    {
        if (coolDownTimer >= 0)
        {
            coolDownTimer -= Time.deltaTime;
        }
        if (coolDownTimer >= coolDownTime * 0.5f)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }
    // 注册所有部位的检测事件
    private void RegisterAllBodyParts()
    {
        // 获取巨人身上所有的BodyPartDetector组件（包括子物体）
        BodyPartDetector[] detectors = GetComponentsInChildren<BodyPartDetector>();
        foreach (var detector in detectors)
        {
            // 每个检测器触发时，调用OnBodyPartDetect方法
            detector.OnPlayerDetected += OnBodyPartDetect;
        }
    }

    // 当某个部位检测到玩家时执行
    private void OnBodyPartDetect(GiantBodyPart part)
    {
        // 从配置中获取对应动画名称
        string animName = AnimationConfig.GetAnimationName(part);
        if (!string.IsNullOrEmpty(animName))
        {
            // 播放动画（假设用Animator参数触发，或直接Play）
            //_animator.SetTrigger(animName);
            if (coolDownTimer <= 0)
            {
                coolDownTimer = coolDownTime;
                _animator.Play(animName);
            }
            
        }
    }

    // 清理事件监听，避免内存泄漏
    private void OnDestroy()
    {
        BodyPartDetector[] detectors = GetComponentsInChildren<BodyPartDetector>();
        foreach (var detector in detectors)
        {
            detector.OnPlayerDetected -= OnBodyPartDetect;
        }
    }
}