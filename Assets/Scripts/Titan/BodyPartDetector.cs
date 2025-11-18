using UnityEngine;
using System;
using System.Collections;
// 3. 部位碰撞检测组件（挂载到每个碰撞部位的GameObject上）
public class BodyPartDetector : MonoBehaviour
{
    [Header("配置当前部位类型")]
    public GiantBodyPart bodyPart; // 在Inspector中手动选择当前部位（如LeftHand）
    public Collider[] colliders;
    // 定义带“部位类型”参数的事件（供外部监听）
    public event Action<GiantBodyPart> OnPlayerDetected;
    public HitBoxerController hitBoxerController;
    void Start()
    {
        hitBoxerController = GetComponentInParent<HitBoxerController>();
    }
    private void OnTriggerEnter(Collider other)
    {

        // 检测到玩家（假设玩家标签为"Player"）
        if (other.CompareTag("Player"))
        {
            hitBoxerController.setAllConlliderActive(false);
            SetColliderActive(true);
            StartCoroutine(DelayCall(1.5f));
            Debug.Log($"{bodyPart} 检测到玩家！");
            // 触发事件，传递当前部位类型
            OnPlayerDetected?.Invoke(bodyPart);
        }
    }
    public void SetColliderActive(bool active = false)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = active;
        }
    }
    private IEnumerator DelayCall(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetColliderActive(false); // 直接调用，确保执行
    }
}