using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;    
// --- 
// 
// 
public enum SoundCategory
{
    None,
    Footsteps,
    Attack,
    Hit,
    Blood,
}

[System.Serializable]
public class SoundGroup
{
    public SoundCategory category;
    public AudioClip[] clips; // 
}
// --- 


// 
// 
// 
public class SoundManager : SingletonBase<SoundManager> // 
{
    [Header("音源 Prefab")]
    [SerializeField] private GameObject audioSourcePrefab; // 
    
    [Header("池设置")]
    [SerializeField] private int initialPoolSize = 10;
    
    [Header("声音类别配置")]
    [SerializeField] private List<SoundGroup> soundGroups = new List<SoundGroup>();

    // 
    private Dictionary<SoundCategory, AudioClip[]> soundDictionary;
    
    // 
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();

    protected override void Awake()
    {
        base.Awake();
        
        // 1. 
        soundDictionary = new Dictionary<SoundCategory, AudioClip[]>();
        foreach (var group in soundGroups)
        {
            if (!soundDictionary.ContainsKey(group.category) && group.clips.Length > 0)
            {
                soundDictionary.Add(group.category, group.clips);
            }
        }
        
        // 2. 
        InitPool();
    }

    private void InitPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            audioSourcePool.Enqueue(CreateNewAudioSource());
        }
    }

    // 
    private AudioSource CreateNewAudioSource()
    {
        GameObject newObj = Instantiate(audioSourcePrefab, transform);
        newObj.SetActive(false);
        return newObj.GetComponent<AudioSource>();
    }

    // 
    private AudioSource TakeFromPool()
    {
        AudioSource source;
        if (audioSourcePool.Count > 0)
        {
            source = audioSourcePool.Dequeue(); // 
        }
        else
        {
            // 
            source = CreateNewAudioSource(); 
        }
        
        source.gameObject.SetActive(true);
        return source;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Recycle(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        source.transform.SetParent(transform); // 
        
        audioSourcePool.Enqueue(source); // 
    }
    
    // --- 
    // 

    /// <summary>
    /// 播放随机音效
    /// </summary>
    /// <param name="category">音效类别</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">音量</param>
    /// <param name="startTime">起始播放时间（秒）</param>
    public void PlayRandom(SoundCategory category, Vector3 position,  float startTime = 0f,float volume = 1f)
    {
        if (category == SoundCategory.None || !soundDictionary.ContainsKey(category))
        {
            return; 
        }

        // 1. 获取随机片段
        AudioClip[] clips = soundDictionary[category];
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];

        // [安全检查] 如果起始时间超过了音频长度，重置为0或者报错
        if (startTime >= randomClip.length)
        {
            // Debug.LogWarning("起始时间超过音频长度，已重置为0");
            startTime = 0f; 
        }

        // 2. 从池中取对象
        AudioSource source = TakeFromPool();
        
        // 3. 设置属性
        source.transform.position = position;
        source.clip = randomClip;
        source.volume = volume;
        
        // --- 核心修改 ---
        source.time = startTime; // 设置起始时间 (注意：这必须在 Play() 之前或之后立即设置)
        // ----------------

        source.Play();
        
        // 4. 计算剩余时长进行回收
        // 实际播放时长 = 总长度 - 起始时间
        // 注意：如果你的 AudioSource Pitch (音调) 不是 1，这里还需要除以 Math.Abs(source.pitch)
        float remainingDuration = randomClip.length - startTime;
        
        // 加上一点点缓冲时间(0.1f)，防止正好卡在结束时回收导致听起来像被截断
        source.GetComponent<AutoRecycleAudio>()?.StartAutoRecycle(remainingDuration + 0.1f);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void PlayRandom(string categoryName, Vector3 position, float volume = 1f)
    {
        if (System.Enum.TryParse(categoryName, true, out SoundCategory category))
        {
            PlayRandom(category, position, volume);
        }
    }
}