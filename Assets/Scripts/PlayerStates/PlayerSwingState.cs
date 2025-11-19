using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwingState : PlayerStateBase
{
    public List<Vector3> targetPoints;
    public List<Vector3> hookFlyingPos;
    public List<bool> isShinking;
    public override void Init()
    {
        base.Init();
        targetPoints = new List<Vector3>{
            Vector3.zero,
            Vector3.zero
        };
        hookFlyingPos = new List<Vector3>{
            Vector3.zero,
            Vector3.zero
        };
        isShinking = new List<bool>{
            false,
            false
        };
    }
    override public void Enter()
    {
        base.Enter();
        ani.Play("Swing");
        rb.drag = pd.swingDrag;
        pd.StartCoroutine(DelayedSetShrinking(index, pd.hookDelay));
        hookActive[index] = true;
        targetPoints[index] = pd.swingPredictionBalls[index].position;//关节位置为带预测的hit点
        hookFlyingPos[index] = pd.gunTips[index].position;
        pd.lrs[index].positionCount = 2;
    }
    override public void Exit()
    {
        
        base.Exit();


    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        //Move();
        for (int i = 0; i < 2; i++)
        {
            if (!hookActive[i]) continue;
            if (isShinking[i])
            {
                ShinkJoint(i);
            }
        }
    }
    override public void Update()
    {
        base.Update();
        CheckKeyUp();
        CheckKeyDown();
        if (Input.GetKeyDown(shiftKey))
        {
            Spray();
        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
        DrawRoap();
    }
    void Move()
    {
        rb.AddForce((orientation.forward*horizontalInput+orientation.right*verticalInput)
                    .normalized*pd.swingMoveSpeed);        
    }

    void ShinkJoint(int i)
    {
        Vector3 directionToTargetPoint = (targetPoints[i] - player.transform.position).normalized;
        rb.AddForce(directionToTargetPoint * pd.shinkForce);
        
    }
    void DrawRoap()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!hookActive[i]) continue;
            hookFlyingPos[i] = Vector3.Lerp(hookFlyingPos[i], targetPoints[i], pd.hookDelay);
            pd.lrs[i].SetPosition(0, pd.gunTips[i].position);
            pd.lrs[i].SetPosition(1, hookFlyingPos[i]);
        }
    }
    void CheckKeyUp()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!hookActive[i]) continue;
            if (Input.GetKeyUp(swingKeys[i]))
            {
                isShinking[i] = false;
                pd.lrs[i].positionCount = 0;
                hookActive[i] = false;
                if (!hookActive[0] && !hookActive[1])
                {
                    sm.EnterState<PlayerAirState>();
                    ani.Play("Fall");
                }

            }
        }
    }
    void CheckKeyDown()
    {
        for (int i = 0; i < 2; i++)
        {
            if (hookActive[i]) continue;
            if (Input.GetKeyDown(swingKeys[i]))
            {
                pd.StartCoroutine(DelayedSetShrinking(i, pd.hookDelay));
                hookActive[i] = true;
                targetPoints[i] = pd.swingPredictionBalls[i].position;//关节位置为带预测的hit点
                hookFlyingPos[i] = pd.gunTips[i].position;
                pd.lrs[i].positionCount = 2;

            }
        }
    }
    public IEnumerator DelayedSetShrinking(int i, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isShinking[i] = true;
    }

    void Spray()
    {
        if (Time.time - pd.lastSprayTime > pd.sprayCooldown)
        {
            rb.AddForce(cam.transform.forward * pd.sprayForce, ForceMode.Impulse);
            pd.lastSprayTime = Time.time;
        }
    }
    void Attack()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(cam.transform.forward * pd.attackForce, ForceMode.Impulse);
    }
}
