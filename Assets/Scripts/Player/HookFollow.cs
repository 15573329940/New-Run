using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookFollow : MonoBehaviour
{
    void OnTriggerEnter2(Collider other)
    {
        if (transform.parent==null&&other.gameObject.layer == LayerMask.NameToLayer("TitanBodybox"))
        {

            transform.SetParent(other.transform);
            Debug.Log(transform.parent.name+transform.parent.position+"localPosition:"+transform.localPosition);
        }
    }
    void OnTriggerExit2(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TitanBodybox"))
        {
            transform.SetParent(null);
        }
    }
}
