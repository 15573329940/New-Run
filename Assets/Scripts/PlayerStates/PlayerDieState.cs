using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : PlayerStateBase
{
    private ParticleSystem dieParticles;
    float timer = 0f;
    public override void Enter()
    {
        base.Enter();
        dieParticles = pd.dieParticles;
        dieParticles.Play();
    }
    public override void Update()
    {
        base.Update();
        timer += Time.deltaTime;
        if (timer >= dieParticles.main.duration)
        {
            sm.EnterState<PlayerAirState>();
        }
    }
}
