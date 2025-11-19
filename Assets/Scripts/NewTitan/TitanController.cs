using UnityEngine;
using UnityEngine.AI;
using System.Collections; // 1. 导入协程所需的库
using System.Collections.Generic;
using System.Linq;

public class TitanController : MonoBehaviour
{
    [Header("核心组件")]
    public Transform player; // 拖入玩家对象
    private NavMeshAgent agent;
    private Animator animator;

    [Header("AI 决策 (拖入所有子对象)")]
    public List<AttackTriggerZone> attackDecisionZones; // 拖入所有 "决策区"
    public AttackHitbox[] allAttackHitboxes; // 拖入所有 "攻击判定" Hitbox

    [Header("AI 攻击数据 (拖入所有资产)")]
    public List<TitanAttackSO> attackInfos; // 拖入所有 "Attack Info" 资产

    [Header("AI 行为参数")]
    public bool useSmartAttack = true; // 是否启用智能预测
    [Min(0.1f)]
    public float decisionInterval = 0.25f; // AI 决策间隔 (性能优化)
    public float AttackSpeed = 1.0f; // 巨人攻击速度
    public float Size = 1.0f;        // 巨人尺寸
    public float hurtTime = 4.0f;    // 受伤动画时间

    [Header("Animation Clip Names")]
    [Tooltip("Animator中的受伤动画片段的 *确切* 名字")]
    public string hurtAnimationName = "Hurt"; // 确保这个名字和你的动画剪辑名字一致
    [Tooltip("Animator中的死亡动画片段的 *确切* 名字")]
    public string deathAnimationName = "Death"; // 确保这个名字和你的动画剪辑名字一致

    // --- 状态机 ---
    public TitanState currentState;
    private float decisionTimer;
    private Dictionary<string, AttackHitbox> hitboxCache; // 性能优化

    // --- 玩家速度追踪 ---
    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;

    // --- 临时列表 (性能优化) ---
    private List<TitanAttackType> potentialAttacks = new List<TitanAttackType>();
    private List<TitanAttackType> validAttacks = new List<TitanAttackType>();
    private TitanAttackType currentAttackType = TitanAttackType.None;

    // --- 自动化协程 ---
    // 2. 缓存 *所有* 动画剪辑的长度 (按名字)
    // 3. 引用 *当前* 正在运行的状态协程
    private Coroutine _activeStateCoroutine = null;


    #region Unity 生命周期函数
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化 Hitbox 缓存
        hitboxCache = new Dictionary<string, AttackHitbox>();
        foreach (var hitbox in allAttackHitboxes)
        {
            hitboxCache.TryAdd(hitbox.gameObject.name, hitbox);
        }
    }

    void Start()
    {
        lastPlayerPos = player.position;
        currentState = TitanState.Idle;
    }

    void Update()
    {
        if (player == null || currentState == TitanState.Dead)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            return;
        }
        UpdatePlayerVelocity();
        // 核心状态机... (保持不变)
        switch (currentState)
        {
            case TitanState.Idle:
                LookForPlayer();
                break;

            case TitanState.Chasing:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("isChasing", true);

                decisionTimer -= Time.deltaTime;
                if (decisionTimer <= 0)
                {
                    DecideAttack();
                    decisionTimer = decisionInterval;
                }
                break;

            case TitanState.Attacking:
                agent.isStopped = true;
                animator.SetBool("isChasing", false);
                break;

            case TitanState.Hurt:
                agent.isStopped = true;
                animator.SetBool("isChasing", false);
                break;
        }
    }
    #endregion

    #region AI 决策逻辑

    void LookForPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < agent.stoppingDistance + 20f)
        {
            currentState = TitanState.Chasing;
        }
    }

    void DecideAttack()
    {
        foreach (var zone in attackDecisionZones)
        {
            if (zone.IsPlayerInside())
            {
                potentialAttacks.Add(zone.attackType);
            }
        }
        if (potentialAttacks.Count == 0) return; // 没有可用的攻击
        // 物理检测 (你已注释掉)
        
        // ... (你的 foreach (var zone...) 逻辑) ...
        
        // 智能检测 (你正在使用的逻辑)
        validAttacks.Clear();
        if (useSmartAttack)
        {
            foreach (var type in potentialAttacks)
            {
                TitanAttackSO info = attackInfos.FirstOrDefault(a => a.attackType == type);
                if (info != null && info.CheckSmartAttack(transform, player.position, playerVelocity, AttackSpeed, Size))
                {
                    validAttacks.Add(type);
                }
            }
        }
        else
        {
            validAttacks.AddRange(potentialAttacks); // 不用智能检测，全都有效
        }

        // 随机选择
        if (validAttacks.Count > 0)
        {
            TitanAttackType attackToUse = validAttacks[Random.Range(0, validAttacks.Count)];
            StartAttack(attackToUse);
        }
    }

    void StartAttack(TitanAttackType attackType)
    {
        // 5. (关键!) 停止任何正在运行的旧协程 (无论是攻击还是受伤)
        if (_activeStateCoroutine != null)
        {
            StopCoroutine(_activeStateCoroutine);
        }

        currentState = TitanState.Attacking;
        currentAttackType = attackType;
        
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        animator.Play(attackType.ToString());
        TitanAttackSO info = attackInfos.FirstOrDefault(a => a.attackType == attackType);
        // 6. 启动新的“攻击结束”协程
        if (info != null && info.animationLength > 0)
        {
            float scaledDuration = info.animationLength / AttackSpeed;
            // 启动协程
            _activeStateCoroutine = StartCoroutine(StateFinishedRoutine(scaledDuration, TitanState.Attacking, TitanState.Chasing));
        }
        else
        {
            // 容错处理
            Debug.LogWarning($"找不到 {attackType} 的 SO 或 AnimationLength 为 0，使用默认 2秒");
            _activeStateCoroutine = StartCoroutine(StateFinishedRoutine(2.0f, TitanState.Attacking, TitanState.Chasing));
        }
    }

    void UpdatePlayerVelocity()
    {
        if (Time.deltaTime > 0)
        {
            playerVelocity = (player.position - lastPlayerPos) / Time.deltaTime;
            lastPlayerPos = player.position;
        }
    }

    #endregion

    #region 公共接口 (由其他脚本调用)

    public void OnTakeDamage(BodyPart part, int damage)
    {
        if (currentState == TitanState.Dead) return;

        // 8. (关键!) 无论如何，先停止当前正在运行的协程 (它可能是攻击协程)
        if (_activeStateCoroutine != null)
        {
            StopCoroutine(_activeStateCoroutine);
            _activeStateCoroutine = null;
        }
        
        // 立即重置攻击类型 (因为攻击被打断了)
        currentAttackType = TitanAttackType.None;

        if (part == BodyPart.Nape)
        {
            currentState = TitanState.Dead;
            agent.enabled = false;
            animator.Play(deathAnimationName); // 播放死亡动画
            // 死亡是最终状态，不启动新协程
        }
        else if (part == BodyPart.Hand_L || part == BodyPart.Foot_L || part == BodyPart.Eye)
        {
            currentState = TitanState.Hurt;
            animator.Play(hurtAnimationName); // 播放受伤动画

            // 9. 启动新的“受伤结束”协程
            
                // 启动协程, 告诉它在 "Hurt" 状态结束后, 下一步进入 "Chasing"
            _activeStateCoroutine = StartCoroutine(StateFinishedRoutine(hurtTime, TitanState.Hurt, TitanState.Chasing));
            
        }
    }

    public void OnAttackHitPlayer(PlayerStateMachine player)
    {
        TitanAttackSO info = attackInfos.FirstOrDefault(a => a.attackType == currentAttackType);
        if (info == null) return;

        if (currentAttackType == TitanAttackType.LMiddleGrab) // 假设这是抓取
        {
            // ... (抓取逻辑)
        }
        else
        {
            // ... (伤害逻辑)
        }
    }
    #endregion

    #region 自动化协程

    // 7. 我们的自动化状态机协程
    /// <summary>
    /// 在指定时间后自动退出一个状态
    /// </summary>
    /// <param name="duration">动画时长</param>
    /// <param name="stateToExit">要检查的当前状态</param>
    /// <param name="nextState">要进入的下一个状态</param>
    private IEnumerator StateFinishedRoutine(float duration, TitanState stateToExit, TitanState nextState)
    {
        // 在动画结束前 0.05 秒检查
        float waitTime = duration - 0.05f;
        if (waitTime < 0) waitTime = 0;

        yield return new WaitForSeconds(waitTime);

        // (关键!) 检查我们是否仍处于我们期望的状态
        // 如果是，说明我们没有被另一个状态（如Hurt）打断
        if (currentState == stateToExit)
        {
            currentState = nextState;
            currentAttackType = TitanAttackType.None; // 只有在状态正常结束时才重置
        }

        _activeStateCoroutine = null; // 协程任务完成，清空引用
    }

    // 缓存所有动画剪辑的长度

    #endregion

    #region 动画事件 (现在只剩 Hitbox)

    // 10. (注意!) 我们现在删除了 AttackFinished 和 HurtFinished
    // 我们 *仍然需要* Hitbox 的事件

    public void AnimationEvent_EnableHitbox(string hitboxName)
    {
        if (hitboxCache.ContainsKey(hitboxName))
        {
            hitboxCache[hitboxName].EnableHitbox();
        }
    }

    public void AnimationEvent_DisableHitbox(string hitboxName)
    {
        if (hitboxCache.ContainsKey(hitboxName))
        {
            hitboxCache[hitboxName].DisableHitbox();
        }
    }
    
    // AnimationEvent_AttackFinished() <-- 已删除
    // AnimationEvent_HurtFinished() <-- 已删除

    #endregion
}