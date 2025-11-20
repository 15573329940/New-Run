using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwingState : PlayerStateBase
{
    public List<Vector3> targetPoints;
    private float lastAttackTime;
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
        ani.SetBool("isSwing",true);
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
        ani.SetBool("isSwing",false);
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
        if (hasInput)
        {
            Move();
        }
        
        if (Input.GetKeyDown(shiftKey))
        {
            Spray();
        }
        if(Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
        DrawRoap();
    }
    void Move()
    {
        rb.AddForce((cam.transform.forward*verticalInput+cam.transform.right*horizontalInput)
                    .normalized*pd.swingMoveSpeed);        
    }

    void ShinkJoint(int i)
    {
        float currentDistanceSqr = (targetPoints[i] - player.transform.position).sqrMagnitude;
        if (currentDistanceSqr < pd.minShinkDistance * pd.minShinkDistance)
        {
            return;
        }
        targetPoints[i] = pd.swingPredictionBalls[i].position;//关节位置为带预测的hit点
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
                pd.swingPredictionBalls[i].gameObject.SetActive(false);
                GameObject.Destroy(currentAnchors[i]?.gameObject);
                currentAnchors[i] = null;
                //pd.swingPredictionBalls[i].SetParent(null);
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
                
                pd.swingPredictionBalls[i].gameObject.SetActive(true);
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
        if(Time.time - lastAttackTime < pd.attackCooldown)
        {
            return;
        }
        lastAttackTime = Time.time;
        rb.velocity = Vector3.zero;
        
        ani.Play("Attack");
        Collider[] necks = Physics.OverlapSphere(pd.headPos.position, pd.detectRange, pd.neckLayer);
        if (necks.Length != 0)
        {
            Transform neck = necks[0].transform;
            Debug.Log("Neck position: " + neck.position + ", Player position: " + player.position);
            Vector3 direction = (neck.position - player.position).normalized;
            if(Vector3.Dot(direction, cam.transform.forward)>0.5f)
            {
                player.forward = new Vector3(direction.x, 0, direction.z);
                rb.AddForce(direction * pd.attackForce, ForceMode.Impulse);
            }
            
        }
        else
        {
            rb.AddForce(cam.transform.forward * pd.attackForce, ForceMode.Impulse);
        }
        
    }
}
