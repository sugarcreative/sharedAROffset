using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    [SerializeField] private AudioSource audioSourcePrefab;

    [SerializeField] private int initialPoolSize = 5;

    [SerializeField] private Dictionary<string, AudioClip> audioClipsDict = new Dictionary<string, AudioClip>();

    [SerializeField] private AudioClip[] audioClips;

    [SerializeField] private List<AudioSource> audioSourcePool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
    }

    private void InitializeAudioSourcePool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            AddNewAudioSourceToPool();
        }
    }

    private AudioSource AddNewAudioSourceToPool()
    {
        AudioSource newAudioSource = Instantiate(audioSourcePrefab);
        audioSourcePool.Add(newAudioSource);
        return newAudioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }
        return AddNewAudioSourceToPool();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioClipsDict.Add("woodenShipBreak", audioClips[0]);
        audioClipsDict.Add("localCannonFire1", audioClips[1]);
        audioClipsDict.Add("localCannonFire2", audioClips[2]);
        audioClipsDict.Add("networkCannonFire", audioClips[3]);
    }

    public void PlaySound(string soundName)
    {
        AudioSource availableSource = GetAvailableAudioSource();
        availableSource.clip = audioClipsDict[soundName];
        //float pitch = Random.Range(-0.4f, 0.4f);
        //availableSource.pitch = pitch;
        availableSource.Play();
    }
}
