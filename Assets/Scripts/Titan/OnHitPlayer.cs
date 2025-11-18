using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitPlayer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerStateMachine>().Die();
        }
    }
}
