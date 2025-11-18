using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
public class PlayerShiftState : PlayerStateBase
{
    bool forward;
    public override void Enter()
    {
        base.Enter();
        if (hasInput)
        {
            forward = true;
            rb.AddForce((rb.velocity).normalized * pd.shiftSpeed, ForceMode.Impulse);
        }
        else
        {
            forward = false;
            rb.AddForce(-player.forward * pd.shiftSpeed, ForceMode.Impulse);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (forward)
        {
            //rb.AddForce((rb.velocity).normalized * pd.shiftSpeed, ForceMode.Force);
        }
        else
        {
            //rb.AddForce(-player.forward * pd.shiftSpeed, ForceMode.Force);
        }
    }
    public override void Update()
    {
        base.Update();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
