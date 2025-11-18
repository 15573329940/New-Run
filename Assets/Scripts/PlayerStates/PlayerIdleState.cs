using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    override public void Enter()
    {
        base.Enter();
    }
    override public void Update()
    {
        base.Update();
        ani.SetFloat("moveSpeed", 0f, 0.7f, Time.deltaTime);
        if(horizontalInput!=0 || verticalInput!=0)
        {
            sm.EnterState<PlayerWalkState>();
        }
        else if (Input.GetKeyDown(shiftKey))
        {
            ani.SetTrigger("shift");
            sm.EnterState<PlayerShiftState>();
        }
        else if (!IsGround())
        {
            ani.SetBool("isAir", true);
            sm.EnterState<PlayerAirState>();
        }
        if(Input.GetKeyDown(jumpKey))
        {
            Jump();
        }
    }
    void Jump()
    {
        rb.AddForce(Vector3.up * pd.jumpForce, ForceMode.Impulse);
        sm.SetStateCoolDown<PlayerWalkState>(0.3f);
        sm.SetStateCoolDown<PlayerIdleState>(0.3f);
        ani.SetBool("isAir", true);
        sm.EnterState<PlayerAirState>();
    }
}
