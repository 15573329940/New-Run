using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private Transform target;

    public override void Enter()
    {
        base.Enter();
        ani.Play("Attack");
        FindTarget();
        if (target != null)
        {
            Vector3 direction = (target.position - player.position).normalized;
            player.forward = new Vector3(direction.x, 0, direction.z);
            // In a real scenario, you'd trigger an animation event
            // to deal damage at the right time.
            // For now, we'll just assume the attack hits.
        }

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
    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = pd.detectRange;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                // Check if the enemy is in front of the player
                Vector3 toEnemy = (enemy.transform.position - player.position).normalized;
                if (Vector3.Dot(cam.transform.forward, toEnemy) > 0.5f) // 90 degree cone
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            target = closestEnemy.transform;
        }
    }

    private System.Collections.IEnumerator AttackFinished()
    {
        yield return null; // Wait one frame to ensure the animation state is updated
        yield return new WaitForSeconds(ani.GetCurrentAnimatorStateInfo(0).length);

        if (IsGround())
        {
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
            ani.CrossFadeInFixedTime("Fall", 0.2f);
            sm.EnterState<PlayerAirState>();
        }
    }
}
