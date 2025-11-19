using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase
{
    public PlayerStateMachine sm;//状态机
    public PlayerCam cam;
    public Animator ani;
    public Transform player;
    public Transform orientation;
    public Rigidbody rb;
    public PlayerDatas pd;
    public float minRayDis = 2f;
    public float horizontalInput;
    public float verticalInput;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode shiftKey = KeyCode.LeftShift;
    public KeyCode attackKey = KeyCode.Mouse0;
    public List<KeyCode> swingKeys = new List<KeyCode>() { KeyCode.Q, KeyCode.E };
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;
    #region  swinging
    public List<RaycastHit> swingPredictionHits;
    public List<bool> hookActive;
    public int index = -1;//当前状态索引
    bool hitFound = false;
    private float lastGrappleHitTime;
    public float rayRadius;
    public bool hasInput;
    #endregion
    public virtual void Init()
    {
        pd = sm.pd;//获取玩家数据
        ani = pd.animator;//获取动画控制器
        cam = sm.cam;//获取相机
        player = sm.player;//获取玩家
        orientation = sm.orientation;//获取方向
        rb = sm.rb;//获取刚体
        hasInput = false;
        whatIsGround = LayerMask.GetMask("WhatIsGround");//获取地面层
        whatIsWall = LayerMask.GetMask("WhatIsWall");//获取墙壁层
        hookActive = new List<bool> { false, false };
        rayRadius = pd.rayRadius;
    }//初始化
    public virtual void Enter() { }//进入状态
    public virtual void Exit() { }//退出状态
    public virtual void Destory() { }//销毁状态

    public virtual void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if(horizontalInput!=0 || verticalInput!=0)
        {
            hasInput = true;
            ani.SetBool("hasInput", true); 
        }
        else
        {
            hasInput = false;
            ani.SetBool("hasInput", false);
        }
        CheckForSwingPoints();
        for (int i = 0; i < 2; i++)
        {
            if (Input.GetKeyDown(swingKeys[i]) && !hookActive[i])
            {
                if (hitFound)
                {

                    sm.EnterState<PlayerSwingState>(i);
                }
            }
        }

        if (Input.GetKeyDown(attackKey) && sm.curState.GetType() != typeof(PlayerWallRunningState))
        {
            sm.EnterState<PlayerAttackState>();
        }
    }//没有继承MonoBehaviour，不会每帧执行，由StateMachine统一管理

    public virtual void FixedUpdate() { }//没有继承MonoBehaviour，不会每帧执行，由MonoManager统一管理
    public virtual void LateUpdate() { }//没有继承MonoBehaviour，不会每帧执行，由MonoManager统一管理

    public bool IsGround()
    {
        return Physics.Raycast(player.transform.position, Vector3.down, pd.checkGroundHeight, whatIsGround);
    }
    void CheckForSwingPoints()
    {
        if (Camera.main == null) return;

        for (int i = 0; i < 2; i++)
        {
            if (hookActive[i])
                continue;

            // 获取 9 条射线：中心 + 周围 8 点
            List<Ray> rays = CursorAimer.GetCrosshairRays(
                Camera.main,
                screenRadius: 0.02f,
                sampleCount: 8,
                includeCenter: true
            );

            Vector3 targetPoint = Vector3.zero;
            
            hitFound = false;
            // 尝试所有射线，找第一个命中点
            foreach (Ray ray in rays)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, pd.maxHookDistance, pd.whatIsGrappleable))
                {
                    targetPoint = hit.point;
                    hitFound = true;
                    break; // 找到就停
                }
            }

            if (hitFound)
            {
                lastGrappleHitTime = Time.time;
            }

            // 如果在0.1秒内曾命中过，则保持准星为红色
            if (Time.time - lastGrappleHitTime < 0.1f)
            {
                CursorAimer.SetCursorColor(new Color(1, 0, 0, 0.5f));
                if (!hitFound) // 如果当前帧未命中，我们需要重新计算目标点用于显示
                {
                    Ray centerRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    targetPoint = centerRay.origin + centerRay.direction * pd.maxHookDistance;
                }
            }
            else
            {
                CursorAimer.SetCursorColor(new Color(1, 1, 1, 0.5f));
                Ray centerRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                targetPoint = centerRay.origin + centerRay.direction * pd.maxHookDistance;
            }

            // 更新预测球位置（即使不可见，也设位置便于调试）
            pd.swingPredictionBalls[i].position = targetPoint;

            // 始终隐藏预测球（根据你的要求 "不可见"）
            //pd.swingPredictionBalls[i].gameObject.SetActive(true);
        }
    }

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

    public virtual void HandleCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Titan"))
        {
            // For non-swinging states, inherit the Titan's velocity to stay attached.
            if (sm.curState.GetType() != typeof(PlayerSwingState))
            {
                Rigidbody titanRb = collision.gameObject.GetComponent<Rigidbody>();
                if (titanRb != null)
                {
                    rb.velocity += titanRb.velocity * Time.deltaTime;
                }
            }
        }
    }
}
