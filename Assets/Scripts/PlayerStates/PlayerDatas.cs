using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatas : MonoBehaviour
{
    [Header("Stats")]
    public int health = 100;

    public Animator animator;
    public Transform footPos;
    public float checkGroundHeight = 1.2f;
    #region WalkState
    [Header("WalkState")]
    public float walkSpeed = 100.0f;
    public float groundDrag = 6f;
    public float jumpForce = 30f;
    #endregion
    #region ShiftState
    [Header("ShiftState")]
    public float shiftSpeed = 200f;
    public float shiftTime = 0.5f;
    #endregion
    #region AirState
    [Header("AirState")]
    public float airDrag = 1.0f;
    public float airSpeed = 100.0f;
    public float wallCheckDistance = 0.7f;
    public float minJumpHeight = 1.2f;
    public float extraGravity = 10f;
    #endregion
    #region WallRunState
    [Header("WallRunState")]
    public LayerMask whatIsWall;
    public float wallRunSpeed = 100.0f;
    public float wallJumpUpForce = 7f;
    public float wallJumpSideForce = 12f;
    public float startCamFov = 60f;
    public float Ztile = 5f;
    public float rayRadius = 0.25f;
    public float wallTiltAngle = 15f;
    #endregion
    #region SwingState
    [Header("SwingState")]
    public float swingDrag=6.0f;
    public float maxHookDistance = 50.0f;
    public float shinkForce = 2000.0f;
    public float swingMoveSpeed = 100.0f;
    public float maxShinkSpeed=300.0f;
    public float predictionSphereRadius = 1.0f;
    public List<Transform> gunTips;
    public float hookDelay = 0.2f;
    public List<Transform> swingPredictionBalls;
    public List<LineRenderer> lrs;
    public LayerMask whatIsGrappleable;
    public ParticleSystem dieParticles;
    [Header("Spray")]
    public float sprayForce = 500f;
    public float sprayCooldown = 2f;
    [HideInInspector]
    public float lastSprayTime = -100f;
    #endregion
    
}
