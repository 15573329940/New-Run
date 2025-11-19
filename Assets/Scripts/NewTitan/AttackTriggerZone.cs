using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTriggerZone : MonoBehaviour
{
    [Tooltip("这个区域激活时，对应哪种攻击")]
    public TitanAttackType attackType;
    private bool isPlayerInside = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
    public bool IsPlayerInside()
    {
        return isPlayerInside;
    }
}
