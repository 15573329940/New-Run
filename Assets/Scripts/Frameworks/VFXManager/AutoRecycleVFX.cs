using UnityEngine;
using System.Collections;

public class AutoRecycleVFX : MonoBehaviour
{
    private VFXCategory myCategory;
    private VFXManager manager;
    private Coroutine recycleCoroutine;

    public void Init(VFXCategory category, VFXManager mgr)
    {
        myCategory = category;
        manager = mgr;
    }

    // 针对一次性特效：倒计时回收
    public void StartAutoRecycle(float duration)
    {
        if (recycleCoroutine != null) StopCoroutine(recycleCoroutine);
        recycleCoroutine = StartCoroutine(RecycleDelay(duration));
    }

    // 针对循环特效：外部强制回收
    public void RecycleNow()
    {
        if (recycleCoroutine != null) StopCoroutine(recycleCoroutine);
        manager.Recycle(myCategory, this.gameObject);
    }

    private IEnumerator RecycleDelay(float time)
    {
        yield return new WaitForSeconds(time);
        manager.Recycle(myCategory, this.gameObject);
    }
}