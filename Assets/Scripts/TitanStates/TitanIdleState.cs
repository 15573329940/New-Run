using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanIdleState : TitanStateBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (distanceToPlayer <= sm.walkDistance)
        {
            sm.EnterState<TitanWalkState>();
        }
    }
}
