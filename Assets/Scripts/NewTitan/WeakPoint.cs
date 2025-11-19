using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [Tooltip("引用控制器")]
    public TitanController brain;
    [Tooltip("部位类型")]
    public BodyPart partType;
    public GameObject hurtVFX; //受伤特效
    void Awake()
    {
        if (brain == null)
            brain = GetComponentInParent<TitanController>();
    }
    /// <summary>
    /// 这个方法由玩家的武器调用 (例如: OnCollisionEnter)
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (brain == null) return;
        
        if (hurtVFX != null)
        {
            // 生成受伤特效
        }
            

        // 通知大脑
        brain.OnTakeDamage(partType, amount);
    }
}
