using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 
public class PlayerWallRunningState : PlayerStateBase
{
    // --- 
    [Header("WallRun References")]
    // [SerializeField] private PlayerData pd; // 
    // [SerializeField] private CustomCamera cam; // 

    [Header("WallRun Rotation")]
    [SerializeField] private float rotationSpeed = 15f; 
    [SerializeField] private float wallTiltAngle ; // 

    // --- 
    private KeyCode wallJumpKey = KeyCode.Space;


 
    private Vector3 smoothedWallNormal;
    Vector3 moveDirection;
    private Vector3 moveDirectionDampVelocity; //
    private Vector3 wallNormalDampVelocity; //
public override void Enter()
    {
        base.Enter();
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        cam.DoFov(90f);
        wallTiltAngle = pd.wallTiltAngle;
        // --- 
        // 
        if (FindClosestWall(out Vector3 targetNormal))
        {
            smoothedWallNormal = targetNormal; // 
        }
        else
        {
            // 
            sm.EnterState<PlayerAirState>();
        }
    }

    // 
    // 
    // 
    public override void FixedUpdate()
    {
        // 
        HandleWallRunning();
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandleWallRunning()
    {
        // --- 1. 
        if (!FindClosestWall(out smoothedWallNormal))
        {
            sm.EnterState<PlayerAirState>(); // 
            return;
        }

        moveDirection = (Vector3.ProjectOnPlane(cam.transform.forward, smoothedWallNormal)*verticalInput
                         +Vector3.ProjectOnPlane(cam.transform.right, smoothedWallNormal)*horizontalInput).normalized;

        // --- 4. 
        rb.velocity = moveDirection * pd.wallRunSpeed;
        //rb.AddForce(-smoothedWallNormal * 20, ForceMode.Force); // 

        // --- 5. 
        if(moveDirection!=Vector3.zero)
        SetPlayerRotationForWallRun(moveDirection, smoothedWallNormal);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetPlayerRotationForWallRun(Vector3 moveDirection, Vector3 wallNormal)
    {


        Quaternion targetRotationStep1 = Quaternion.LookRotation(moveDirection, wallNormal);

        // --- 
        Vector3 wallHorizontal = Vector3.Cross(wallNormal, Vector3.up).normalized;

        // 
        
        Quaternion tiltRotationStep2 = Quaternion.AngleAxis(wallTiltAngle, wallHorizontal);

        // --- 
        Quaternion finalTargetRotation = tiltRotationStep2 * targetRotationStep1;

        // --- 
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, finalTargetRotation, rotationSpeed * Time.fixedDeltaTime);
    }


    // --- 
    // 
    // --- 

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(wallJumpKey))
        {
            WallJump();
        }
    }

    public override void Exit()
    {
        base.Exit();
        ani.SetBool("isWallRun", false);
        player.transform.rotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y, 0f);
        cam.DoFov(pd.startCamFov);
        rb.useGravity = true;
    }

private bool FindClosestWall(out Vector3 targetNormal)
    {
        Vector3 checkPoint = pd.footPos.position; // 
        
        // 1. 
        Collider[] colliders = Physics.OverlapSphere(checkPoint, pd.wallCheckDistance, pd.whatIsWall);

        float closestDistanceSqr = float.MaxValue;
        Vector3 closestWallPoint = Vector3.zero;
        bool foundWall = false;

        // 2. 
        foreach (Collider col in colliders)
        {
            // 
            Vector3 pointOnCollider = col.ClosestPoint(checkPoint);
            
            // 3. 
            float distanceSqr = (checkPoint - pointOnCollider).sqrMagnitude;

            // 4. 
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestWallPoint = pointOnCollider;
                foundWall = true;
            }
        }

        // 5. 
        if (foundWall)
        {
            // 
            // 
            targetNormal = (checkPoint - closestWallPoint).normalized;
            return true;
        }
        else
        {
            targetNormal = Vector3.zero;
            ani.SetBool("isWallRun", false);
            ani.SetBool("isAir", true);
            sm.EnterState<PlayerAirState>();
            return false;
        }
        
        // 
        
    }

    void WallJump()
    {
        // 
        Vector3 forceToApply = smoothedWallNormal * pd.wallJumpSideForce + player.transform.up * pd.wallJumpUpForce;
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
        sm.SetStateCoolDown<PlayerWallRunningState>(0.3f);
        ani.SetBool("isWallRun", false);
        ani.SetBool("isAir", true);
        sm.EnterState<PlayerAirState>();
    }
}