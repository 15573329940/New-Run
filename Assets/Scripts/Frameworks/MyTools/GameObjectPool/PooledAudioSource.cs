using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PooledAudioSource : MonoBehaviour, IPool
{
    [Header("必须与对象池配置的 Assets Name 一致")]
    [SerializeField]
    private string poolName = "AudioSource"; // 
    
    private AudioSource audioSource;
    private Coroutine autoRecycleCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 
    }

    /// <summary>
    /// 
    /// </summary>
    public void Play(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
        {
            // 
            GameObjectPoolSystem.Instance.RecycleGameObject(gameObject, poolName);
            return;
        }
        
        transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        
        // 
        autoRecycleCoroutine = StartCoroutine(AutoRecycle(clip.length));
    }

    private IEnumerator AutoRecycle(float clipLength)
    {
        // 
        yield return new WaitForSeconds(clipLength + 0.1f);
        
        // 
        GameObjectPoolSystem.Instance.RecycleGameObject(gameObject, poolName);
    }

    // --- 
    // 

    public void RecycleObject()
    {
        // 
        if (autoRecycleCoroutine != null)
        {
            StopCoroutine(autoRecycleCoroutine);
            autoRecycleCoroutine = null;
        }
        
        // 
        audioSource.Stop();
        audioSource.clip = null;
    }

    // 
    public void SpawnObject() { }
    public void SpawnObject(Transform user) { }
}