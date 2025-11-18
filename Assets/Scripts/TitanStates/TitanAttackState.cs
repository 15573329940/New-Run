using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanAttackState : TitanStateBase
{
    public Hand hand;
    public bool isAttacking = false;
    public bool isShinkHand = true;
    public Collider handCollider;
    Vector3 targetPos;
    Vector3 oriHandPos;
    public TitanAnimationController tac;
    public override void Init()
    {
        base.Init();
        tac=sm.tac;
    }
    public override void Enter()
    {   
        base.Enter();
        if (hand == null)
        {
            hand = sm.hand;
        }
        
        if (handCollider == null)
        {
            handCollider = sm.handCollider;
        }
        hand.OnTouchPlayerEvent += OnTouchPlayer;
        handCollider.enabled = true;
        oriHandPos = hand.transform.position;
    }
    public override void Update()
    {
        base.Update();
        if (distanceToPlayer >= sm.attackDistance)
        {
            sm.EnterState<TitanWalkState>();
        }
        hand.transform.LookAt(player);

        if ((hand.transform.position - oriHandPos).sqrMagnitude < 1.0f)
        {
            isAttacking = true;
            targetPos = player.position;
            oriHandPos = hand.transform.position;
            isShinkHand = false;
        }
        if(isAttacking==true)
        {
            hand.transform.position = Vector3.MoveTowards(hand.transform.position, targetPos, 0.1f);
        }
        if (handCollider.bounds.Contains(targetPos))
        {
            isAttacking = false;
            isShinkHand = true;

            
        }
        if (isShinkHand == true)
        {
            hand.transform.position = Vector3.MoveTowards(hand.transform.position, oriHandPos, 0.1f);
        }
        if ((hand.transform.position-player.position).sqrMagnitude < 100.0f)
        {
            OnTouchPlayer();
        }
    }
    public override void Exit()
    {
        base.Exit();
        hand.OnTouchPlayerEvent -= OnTouchPlayer;
        handCollider.enabled = false;
    }
    void OnTouchPlayer()
    {
        isAttacking = false;
        Grab();
    }
    void Grab()
    {
        player.GetComponentInChildren<Rigidbody>().useGravity = false;
        player.GetComponentInChildren<Collider>().enabled = false;
        player.SetParent(hand.transform);
        handCollider.enabled = false;
        isShinkHand = true;
    }
}
