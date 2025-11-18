using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public event System.Action OnTouchPlayerEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void StrengthHand(Vector3 targetPos)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.1f);
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnTouchPlayer();
        }
    }
    void OnTouchPlayer()
    {
        Debug.Log("Touch Player");
        OnTouchPlayerEvent?.Invoke();
    }
}
