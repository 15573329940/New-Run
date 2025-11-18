using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class KyokoAnimationEvents : MonoBehaviour
{
    public PlayerStateMachine sm;
    void Start()
    {
        sm =GetComponentInParent<PlayerStateMachine>();
    }
    public void PlayFootSound()
    {
        if (string.IsNullOrEmpty("Footsteps")) return;

        // 
        SoundManager.Instance.PlayRandom("Footsteps", transform.position);
    }
    public void EndShift()
    {
        sm.EnterState<PlayerAirState>();
        sm.SetStateCoolDown<PlayerShiftState>(0.5f);
    }
}
