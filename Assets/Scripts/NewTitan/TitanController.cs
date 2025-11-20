using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TitanController : MonoBehaviour
{
    #region --- 配置参数 ---
    [Header("核心引用")]
    public Transform player;
    public Transform headBone; // 必须赋值
    public Transform neckBone; // 必须赋值
    
    [Header("AI 决策与范围")]
    public List<AttackTriggerZone> attackDecisionZones;
    public AttackHitbox[] allAttackHitboxes;
    public List<TitanAttackSO> attackInfos;

    [Header("AI 基础参数")]
    public float Size = 1.0f;
    public float AttackSpeed = 1.0f;
    public float hurtTime = 4.0f;    // 受伤硬直时间
    [Min(0.1f)] public float decisionInterval = 0.25f; // 决策频率
    public bool useSmartAttack = true;

    [Header("生物感参数 (AoTTG Style)")]
    [Tooltip("头部追踪最大水平/垂直角度")]
    public float maxHeadAngleX = 60f;
    public float maxHeadAngleY = 40f;
    [Tooltip("重新寻路的时间间隔 (模拟思考)")]
    public Vector2 chaseRepathInterval = new Vector2(2.0f, 5.0f);
    [Tooltip("攻击前的蓄力/确认时间")]
    public float attackWaitTime = 0.5f;
    [Tooltip("追逐时预判玩家走位的权重 (0=纯随机, 1=强预判)")]
    [Range(0f, 1f)] public float interceptFactor = 0.6f;

    [Header("动画状态名")]
    public string hurtAnimationName = "Hurt";
    public string deathAnimationName = "Death";
    public string runAnimationName = "Walk";
    #endregion

    #region --- 内部状态变量 ---
    public enum TitanState { Idle, Chasing, WaitAttack, Attacking, Turning, Hurt, Dead }
    [Header("当前状态 (Debug)")]
    public TitanState currentState;

    // 组件缓存
    private NavMeshAgent _agent;
    private Animator _animator;
    private Dictionary<string, AttackHitbox> _hitboxCache;

    // 逻辑计时器与缓存
    private float _decisionTimer;
    private float _stateTimeLeft;
    private float _currentRandomMoveAngle;
    private Quaternion _targetTurnRotation;
    private float _turnSpeed;
    private TitanAttackType _currentAttackType = TitanAttackType.None;

    // 物理与预测
    private Queue<Vector3> _playerPosHistory = new Queue<Vector3>(3);
    private Vector3 _calculatedVelocity;     // 统一使用这个速度
    private Vector3 _calculatedAcceleration; // 统一使用这个加速度
    
    // 头部追踪缓存
    private Quaternion _oldHeadRotation;

    // 协程引用
    private Coroutine _activeStateCoroutine = null;
    private Coroutine _hitboxCoroutine = null; // 如果你有Hitbox自动协程的话
    #endregion

    #region --- Unity 生命周期 ---

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        // 防止 NavMesh 滑步，逻辑与位移分离
        _agent.updatePosition = false;
        _agent.updateRotation = false;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化 Hitbox 缓存
        _hitboxCache = new Dictionary<string, AttackHitbox>();
        foreach (var hitbox in allAttackHitboxes)
        {
            if (!_hitboxCache.ContainsKey(hitbox.gameObject.name))
                _hitboxCache.Add(hitbox.gameObject.name, hitbox);
        }
    }

    void Start()
    {
        currentState = TitanState.Idle;
        
        // 自动参数初始化
        if (Size == 1.0f) Size = transform.localScale.x;
        if (AttackSpeed <= 0) AttackSpeed = 1.0f;
        if (headBone) _oldHeadRotation = headBone.rotation;

        ResetChaseLogic(); // 初始决策
    }

    void FixedUpdate()
    {
        if (player == null || currentState == TitanState.Dead) return;
        
        // 1. 物理计算必须在 FixedUpdate (保证 dt 恒定)
        RecordPlayerPhysics();

        // 2. 同步 Agent 位置
        _agent.nextPosition = transform.position;
    }

    void Update()
    {
        if (player == null || currentState == TitanState.Dead)
        {
            if(_agent.enabled) _agent.isStopped = true;
            return;
        }

        // 状态机逻辑
        switch (currentState)
        {
            case TitanState.Idle:
                LookForPlayer();
                break;

            case TitanState.Chasing:
                HandleChasing();
                break;

            case TitanState.WaitAttack:
                HandleWaitAttack();
                break;

            case TitanState.Turning:
                HandleTurning();
                break;

            case TitanState.Attacking:
            case TitanState.Hurt:
                _agent.isStopped = true;
                break;
        }
    }

    void LateUpdate()
    {
        if (currentState == TitanState.Dead || headBone == null || player == null) return;

        // 允许头部追踪的状态
        bool canTrack = currentState == TitanState.Idle || 
                        currentState == TitanState.Chasing || 
                        currentState == TitanState.WaitAttack || 
                        currentState == TitanState.Turning;

        if (canTrack)
        {
            LateUpdateHead(player.position);
        }
    }

    #endregion

    #region --- AI 核心逻辑 ---

    public void InitializeTitan(float sizeScale, bool isAbnormal)
    {
        this.Size = sizeScale;
        transform.localScale = Vector3.one * sizeScale;

        // 根据体型调整攻速：体型越大越慢，奇行种更快
        float baseSpeed = 1.0f;
        float sizeFactor = Mathf.Clamp(15.0f / sizeScale, 0.5f, 2.0f);
        if (isAbnormal) baseSpeed *= 1.5f;

        this.AttackSpeed = baseSpeed * sizeFactor;
        _animator.speed = this.AttackSpeed;
    }

    void LookForPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < _agent.stoppingDistance + 100f)
        {
            currentState = TitanState.Chasing;
            ResetChaseLogic();
        }
    }

    // 【改进】带预判倾向的重置逻辑
    void ResetChaseLogic()
    {
        _stateTimeLeft = Random.Range(chaseRepathInterval.x, chaseRepathInterval.y);

        // --- 预判逻辑 ---
        // 1. 计算玩家相对于泰坦的方位
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        // 2. 获取玩家的移动方向 (归一化)
        Vector3 playerDir = _calculatedVelocity.normalized;

        float biasAngle = 0f;

        // 只有当玩家在移动时才预判
        if (_calculatedVelocity.sqrMagnitude > 4f) // 速度大于2m/s才算跑
        {
            // 计算玩家移动方向相对于泰坦视线的夹角
            // 结果 > 0 说明玩家在往泰坦的右侧跑，< 0 说明在往左侧跑
            float relativeRunAngle = Vector3.SignedAngle(dirToPlayer, playerDir, Vector3.up);

            // 如果玩家往右跑，泰坦也往右偏 (拦截)
            biasAngle = Mathf.Clamp(relativeRunAngle * interceptFactor, -45f, 45f);
        }

        // 混合：预判偏差 + 少量随机扰动
        _currentRandomMoveAngle = Mathf.Clamp(biasAngle + Random.Range(-20f, 20f), -60f, 60f);
    }

    void HandleChasing()
    {
        _agent.isStopped = false;
        _animator.SetBool("isChasing", true);
        _stateTimeLeft -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        // 尝试进入攻击前摇
        if (dist < 30f && CanAttackAny())
        {
            currentState = TitanState.WaitAttack;
            _stateTimeLeft = attackWaitTime;
            _agent.isStopped = true;
            _animator.SetBool("isChasing", false);
            return;
        }

        // 定时重置寻路
        if (_stateTimeLeft <= 0 || !_agent.hasPath)
        {
            _agent.SetDestination(player.position);
            ResetChaseLogic();
        }

        // 移动控制
        if (_agent.hasPath)
        {
            Vector3 desiredDir = (_agent.steeringTarget - transform.position);
            desiredDir.y = 0;

            // 施加预判/随机偏移
            Vector3 finalDir = Quaternion.Euler(0, _currentRandomMoveAngle, 0) * desiredDir.normalized;

            // 转身检测
            if (Vector3.Angle(transform.forward, finalDir) > 45f)
            {
                TryEnterTurnState();
            }
            else if (finalDir != Vector3.zero)
            {
                // 平滑转向
                Quaternion targetRot = Quaternion.LookRotation(finalDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 120f * Time.deltaTime);
            }

            // 移动位置
            transform.position += transform.forward * _agent.speed * Time.deltaTime;
        }
    }

    bool CanAttackAny() { return true; } // 简化的检查

    void HandleWaitAttack()
    {
        _stateTimeLeft -= Time.deltaTime;
        
        // 锁定玩家朝向
        //Vector3 dirToPlayer = (player.position - transform.position).normalized;
        //dirToPlayer.y = 0;
        //if(dirToPlayer != Vector3.zero)
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToPlayer), 200f * Time.deltaTime);

        if (_stateTimeLeft <= 0)
        {
            DecideAttack();
        }
    }

    void TryEnterTurnState()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        dirToPlayer.y = 0;
        
        float angle = Vector3.SignedAngle(transform.forward, dirToPlayer, Vector3.up);

        currentState = TitanState.Turning;
        _targetTurnRotation = Quaternion.LookRotation(dirToPlayer);

        string turnAnim = angle > 0 ? "Turn90R" : "Turn90L";
        _animator.CrossFade(turnAnim, 0.2f);

        // 动态计算旋转速度：假设转身动画基准为1.5s，转90度
        float animLength = 1.5f; 
        _turnSpeed = 90f / animLength;
    }

    void HandleTurning()
    {
        // 持续转向
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetTurnRotation, _turnSpeed * Time.deltaTime);

        // 检查角度差
        if (Quaternion.Angle(transform.rotation, _targetTurnRotation) < 5f)
        {
            currentState = TitanState.Chasing;
            _animator.CrossFade(runAnimationName, 0.25f);
        }
    }

    void DecideAttack()
    {
        // 1. 物理筛选
        var potentialAttacks = new List<TitanAttackType>();
        var validAttacks = new List<TitanAttackType>();
        foreach (var zone in attackDecisionZones)
        {
            //if (zone.IsPlayerInside()) 
            potentialAttacks.Add(zone.attackType);
        }
        if (potentialAttacks.Count == 0) 
        {
            ReturnToChase(); // 没有可用攻击，回追逐
            return;
        }

        // 2. 智能筛选
        validAttacks.Clear();
        if (useSmartAttack)
        {
            foreach (var type in potentialAttacks)
            {
                var info = attackInfos.FirstOrDefault(a => a.attackType == type);
                if (info != null && info.CheckSmartAttack(
                    transform, player.position, 
                    _calculatedVelocity, _calculatedAcceleration*0, // 使用FixedUpdate计算出的物理量
                    AttackSpeed, Size))
                {
                    validAttacks.Add(type);
                }
            }
        }
        else
        {
            validAttacks.AddRange(potentialAttacks);
        }

        // 3. 执行
        if (validAttacks.Count > 0)
        {
            StartAttack(validAttacks[Random.Range(0, validAttacks.Count)]);
        }
        else
        {
            ReturnToChase();
        }
    }
    
    void ReturnToChase()
    {
        currentState = TitanState.Chasing;
        ResetChaseLogic();
    }

    void StartAttack(TitanAttackType attackType)
    {
        if (_activeStateCoroutine != null) StopCoroutine(_activeStateCoroutine);

        currentState = TitanState.Attacking;
        _currentAttackType = attackType;

        // 攻击瞬间再次校准朝向
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        _animator.speed = this.AttackSpeed;
        _animator.Play(attackType.ToString());

        var info = attackInfos.FirstOrDefault(a => a.attackType == attackType);
        float duration = (info != null && info.animationLength > 0) ? info.animationLength : 2.0f;
        
        // 启动状态恢复协程
        _activeStateCoroutine = StartCoroutine(StateFinishedRoutine(duration / AttackSpeed, TitanState.Attacking, TitanState.Chasing));
    }

    #endregion

    #region --- 物理计算 (FixedUpdate) ---

    void RecordPlayerPhysics()
    {
        _playerPosHistory.Enqueue(player.position);
        if (_playerPosHistory.Count > 3) _playerPosHistory.Dequeue();

        if (_playerPosHistory.Count == 3)
        {
            Vector3[] p = _playerPosHistory.ToArray();
            float dt = Time.fixedDeltaTime;

            Vector3 v1 = (p[1] - p[0]) / dt;
            Vector3 v2 = (p[2] - p[1]) / dt;

            _calculatedVelocity = v2; // 更新全局速度
            Vector3 rawAccel = (v2 - v1) / dt;

            // 过滤爆发力
            _calculatedAcceleration = (rawAccel.magnitude > 30f) ? Physics.gravity : rawAccel;
        }
        else
        {
            _calculatedVelocity = Vector3.zero;
            _calculatedAcceleration = Physics.gravity;
        }
    }
    #endregion

    #region --- 公共接口 & 辅助 ---

    public void OnTakeDamage(BodyPart part, int damage)
    {
        if (currentState == TitanState.Dead) return;

        if (_activeStateCoroutine != null)
        {
            StopCoroutine(_activeStateCoroutine);
            _activeStateCoroutine = null;
        }
        
        _currentAttackType = TitanAttackType.None;
        _animator.speed = 1.0f; // 重置动画速度
        DisableAllHitboxes();   // 关闭残留判定

        if (part == BodyPart.Nape)
        {
            currentState = TitanState.Dead;
            _agent.enabled = false;
            _animator.Play(deathAnimationName);
        }
        else if (IsNonLethalPart(part))
        {
            currentState = TitanState.Hurt;
            _animator.Play(hurtAnimationName);
            _activeStateCoroutine = StartCoroutine(StateFinishedRoutine(hurtTime, TitanState.Hurt, TitanState.Chasing));
        }
    }
    
    private bool IsNonLethalPart(BodyPart part)
    {
        return part == BodyPart.Hand_L || part == BodyPart.Foot_L || part == BodyPart.Eye; // 示例
    }

    public void OnAttackHitPlayer(PlayerStateMachine player)
    {
        var info = attackInfos.FirstOrDefault(a => a.attackType == _currentAttackType);
        if (info == null) return;
        // 处理伤害逻辑...
    }

    private IEnumerator StateFinishedRoutine(float duration, TitanState stateToExit, TitanState nextState)
    {
        float waitTime = Mathf.Max(0, duration - 0.05f);
        yield return new WaitForSeconds(waitTime);

        if (currentState == stateToExit)
        {
            currentState = nextState;
            _currentAttackType = TitanAttackType.None;
        }
        _activeStateCoroutine = null;
    }

    // 头部追踪逻辑封装
    void LateUpdateHead(Vector3 targetPos)
    {
        Vector3 dir = targetPos - headBone.position;
        Quaternion targetLook = Quaternion.LookRotation(dir);
        Quaternion localTarget = Quaternion.Inverse(transform.rotation) * targetLook;
        
        Vector3 euler = localTarget.eulerAngles;
        float x = ClampAngle(euler.x, -maxHeadAngleY, maxHeadAngleY);
        float y = ClampAngle(euler.y, -maxHeadAngleX, maxHeadAngleX);

        Quaternion clampedLocal = Quaternion.Euler(x, y, 0);
        headBone.rotation = Quaternion.Lerp(_oldHeadRotation, transform.rotation * clampedLocal, Time.deltaTime * 5f);
        _oldHeadRotation = headBone.rotation;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    
    private void DisableAllHitboxes()
    {
        foreach(var kvp in _hitboxCache) kvp.Value.DisableHitbox();
    }

    #endregion

    #region --- 动画事件 ---
    public void AnimationEvent_EnableHitbox(string hitboxName)
    {
        if (_hitboxCache.TryGetValue(hitboxName, out var hitbox))
            hitbox.EnableHitbox();
    }

    public void AnimationEvent_DisableHitbox(string hitboxName)
    {
        if (_hitboxCache.TryGetValue(hitboxName, out var hitbox))
            hitbox.DisableHitbox();
    }
    #endregion
}