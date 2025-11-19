// AttackHitbox.cs

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackHitbox : MonoBehaviour
{
    [Tooltip("引用巨人的大脑")]
    public TitanController brain; // [你代码中叫 TitanController]

    // ⬇️ 1. 删除这一行 (或注释掉)
    // public TitanAttackType attackType; 

    private Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
        _col.enabled = false; 

        brain = transform.root.GetComponent<TitanController>();
    }

    // EnableHitbox 和 DisableHitbox 保持不变...
    public void EnableHitbox()
    {
        _col.enabled = true;
    }
    public void DisableHitbox()
    {
        _col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (brain != null && other.CompareTag("Player"))
        {
            // [你代码中叫 PlayerStateMachine]
            PlayerStateMachine player = other.GetComponent<PlayerStateMachine>(); 
            if (player != null)
            {
                // ⬇️ 2. 修改这一行：
                // 不再传递 attackType，只通知“打到了”
                brain.OnAttackHitPlayer(player);

                DisableHitbox();
            }
        }
    }
}