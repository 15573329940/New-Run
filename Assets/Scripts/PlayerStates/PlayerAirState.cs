using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerStateBase
{
    // Start is called before the first frame update
    bool canLand;
    public override void Enter()
    {   
        canLand = false;
        rb.useGravity = true;
        rb.drag = pd.airDrag;

        TimerPool.Instance.GetTimer(0.3f, () => { canLand = true; });
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Vector3 moveDirection = cam.transform.forward * verticalInput + cam.transform.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * pd.airSpeed, ForceMode.Force);
        float hSpeed=new Vector3(moveDirection.x,0f,moveDirection.z).magnitude;
        rb.velocity+=Vector3.down*pd.extraGravity*Time.deltaTime;
    }
    public override void Update()
    {
        base.Update();
        if (IsGround() && canLand)
        {
            ani.SetBool("isAir", false);
            sm.EnterState<PlayerWalkState>();
        }
        if(!Physics.Raycast(pd.footPos.position,Vector3.down,pd.minJumpHeight,whatIsGround))
        {
            FindClosestWall(out _);
        }
        
    }
    private bool FindClosestWall(out Vector3 targetNormal)
    {
        Vector3 checkPoint = pd.footPos.position; // 
        
        // 1. 
        Collider[] colliders = Physics.OverlapSphere(checkPoint, pd.wallCheckDistance, whatIsWall);

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
            ani.SetBool("isWallRun", true);
            ani.SetBool("isAir", false);
            sm.EnterState<PlayerWallRunningState>();
            targetNormal = (checkPoint - closestWallPoint).normalized;
            return true;
        }
        
        // 
        targetNormal = Vector3.zero;
        return false;
    }
    
}
