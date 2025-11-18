using UnityEngine;
using System.Collections;

public class PlayerDamagedState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();
        ani.Play("Damaged");
        sm.StartCoroutine(DamageFinished());
    }

    private IEnumerator DamageFinished()
    {
        yield return null; // Wait for the next frame to ensure the animator has transitioned
        yield return new WaitForSeconds(ani.GetCurrentAnimatorStateInfo(0).length);

        if (IsGround())
        {
            sm.EnterState<PlayerIdleState>();
        }
        else
        {
            sm.EnterState<PlayerAirState>();
        }
    }
}
