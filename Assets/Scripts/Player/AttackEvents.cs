using UnityEngine;
using UnityEngine.Animations;

public class AlwaysExecuteOnExit : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<PlayerDatas>().knifeCollider.enabled = true;
        SoundManager.Instance.PlayRandom(SoundCategory.Attack, animator.transform.position,0.15f);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<PlayerDatas>().knifeCollider.enabled = false;
        animator.GetComponentInParent<PlayerDatas>().knifeCollider.GetComponent<SpearHitSensor>().hited=true;
    }

    
}