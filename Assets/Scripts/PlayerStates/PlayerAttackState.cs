using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{

    public override void Enter()
    {
        base.Enter();
        ani.Play("Attack");

        sm.StartCoroutine(AttackFinished());
    }
    public override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(shiftKey))
        {
            ani.SetTrigger("shift");
            sm.EnterState<PlayerShiftState>();
        }
    }

    private System.Collections.IEnumerator AttackFinished()
    {
        yield return null; // Wait one frame to ensure the animation state is updated
        yield return new WaitForSeconds(ani.GetCurrentAnimatorStateInfo(0).length);

        if (IsGround())
        {
            ani.SetBool("isAir",false);
            if(hasInput)
            {
                ani.CrossFadeInFixedTime("RunTree", 0.2f);
                sm.EnterState<PlayerWalkState>();
            }
            else
            {
                ani.CrossFadeInFixedTime("Idle", 0.2f);
                sm.EnterState<PlayerIdleState>();
            }
        }
        else 
        {
            ani.SetBool("isAir",true);
            ani.CrossFadeInFixedTime("Fall", 0.2f);
            sm.EnterState<PlayerAirState>();
        }
    }
}
