using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxerController : MonoBehaviour
{
    public Collider handL, handR, body, footL, footR, chest, mouth;
    void Start()
    {
        setAllConlliderActive(false);
    }
    public void setAllConlliderActive(bool active)
    {
        handL.enabled = active;
        handR.enabled = active;
        body.enabled = active;
        footL.enabled = active;
        footR.enabled = active;
        chest.enabled = active;
        mouth.enabled = active;
    }
}
