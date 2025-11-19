using UnityEngine;
using UnityEngine.Animations;

public class AlwaysExecuteOnExit : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<PlayerDatas>().knifeCollider.enabled = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<PlayerDatas>().knifeCollider.enabled = false;
    }

    
}