using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AutoRecycleAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine recycleCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartAutoRecycle(float duration)
    {
        // 
        if (recycleCoroutine != null)
        {
            StopCoroutine(recycleCoroutine);
        }
        recycleCoroutine = StartCoroutine(RecycleAfter(duration));
    }

    private IEnumerator RecycleAfter(float duration)
    {
        // 
        yield return new WaitForSeconds(duration + 0.1f); 
        
        // 
        SoundManager.Instance.Recycle(audioSource);
    }
}