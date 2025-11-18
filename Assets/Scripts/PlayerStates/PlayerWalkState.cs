using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerStateBase
{
    
    RaycastHit slopeHit;
    public override void Enter()
    {
        base.Enter();
        rb.useGravity = true;
        rb.drag = pd.groundDrag;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Vector3 moveDirection = cam.transform.forward * verticalInput + cam.transform.right * horizontalInput;
        
        
        player.forward = Vector3.Lerp(player.forward,new Vector3(moveDirection.x,0,moveDirection.z), 7.0f);
        if (OnSlope())
        {

            rb.AddForce(GetSlopeMoveDirection(moveDirection) * pd.walkSpeed*ani.GetFloat("moveSpeed"), ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                //rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        else
        {
            rb.AddForce(moveDirection.normalized * pd.walkSpeed * ani.GetFloat("moveSpeed"), ForceMode.Force);
        }
        if (rb.velocity.magnitude > pd.walkSpeed)
        {
            rb.velocity = rb.velocity.normalized * pd.walkSpeed;
        }
        

    }
    public override void Update()
    {
        base.Update();
        if(hasInput==false)
        {
            sm.EnterState<PlayerIdleState>();
            return;
        }
        else
        {
            ani.SetFloat("moveSpeed", 1.9f, 0.7f, Time.deltaTime);
        }
        
        if (!IsGround())
        {
            ani.SetBool("isAir", true);
            sm.EnterState<PlayerAirState>();
        }
        if (Input.GetKeyDown(jumpKey))
        {
            Jump();
        }
        if (Input.GetKeyDown(shiftKey))
        {
            ani.SetTrigger("shift");
            sm.EnterState<PlayerShiftState>();
        }
    }
    void Jump()
    {
        rb.AddForce(Vector3.up * pd.jumpForce, ForceMode.Impulse);
        sm.SetStateCoolDown<PlayerWalkState>(0.3f);
        ani.SetBool("isAir", true);
        sm.EnterState<PlayerAirState>();
    }
    public bool OnSlope()
    {
        
        if (Physics.Raycast(player.transform.position, Vector3.down, out slopeHit, sm.playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < 45f && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
