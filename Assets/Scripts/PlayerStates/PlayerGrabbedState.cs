using UnityEngine;

public class PlayerGrabbedState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        // Disable physics to prevent weird interactions
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;

        // Attach player to the grabber's hand
        if (sm.grabberHand != null)
        {
            player.SetParent(sm.grabberHand);
            player.localPosition = Vector3.zero; // Adjust as needed
            player.localRotation = Quaternion.identity;
        }

        // Play grabbed animation
        ani.Play("Grabbed");
    }

    public override void Exit()
    {
        base.Exit();

        // Re-enable physics and detach from hand
        rb.isKinematic = false;
        player.SetParent(null);
        sm.grabberHand = null;
    }
}
