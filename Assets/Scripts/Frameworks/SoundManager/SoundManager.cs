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
    Hit
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
    /// 
    /// </summary>
    public void PlayRandom(SoundCategory category, Vector3 position, float volume = 1f)
    {
        if (category == SoundCategory.None || !soundDictionary.ContainsKey(category))
        {
            return; 
        }

        // 1. 
        AudioClip[] clips = soundDictionary[category];
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];

        // 2. 
        AudioSource source = TakeFromPool();
        
        // 3. 
        source.transform.position = position;
        source.clip = randomClip;
        source.volume = volume;
        source.Play();
        
        // 4. 
        source.GetComponent<AutoRecycleAudio>()?.StartAutoRecycle(randomClip.length);
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