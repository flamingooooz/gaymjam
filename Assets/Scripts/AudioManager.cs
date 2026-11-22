
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



/* 
   --- SFX COMMANDS ---
   One-shot SFX: 
   Audiomanager.PlaySound(SoundType.Button);

   Continuous SFX (e.g., walking, rain): 
   Audiomanager.StartContinuousSound(SoundType.Walk);
   Audiomanager.StopContinuousSound();

   --- MUSIC COMMANDS ---
   Play background music: 
   Audiomanager.PlayMusic(MusicType.Main_Theme);

   Stop or pause music: 
   Audiomanager.StopMusic();
   Audiomanager.PauseMusic();
   Audiomanager.ResumeMusic();
*/
// This is the list of sound types that can be called with their specific name
public enum SoundType
{
 Button,
 Cool_Speech,
 Death_Sound,
 Drink,
 Hallucination,
 Sanity,
 Creepy_Laugh,
 Odd_echo,
 Heartbeat,
 Near_Death,
 Wind,
 PickUp,
 Rain,
 Walk,
 Winded_Breath,
}
public enum MusicType
{
    Main_Menu,
    Low_Sanity,
    Victory,
}
[System.Serializable]
public class SoundEntry
{
    public SoundType soundType;
    public AudioClip clip;
    [Range(0f, 1f)] public float defaultVolume = 1f;
}

[System.Serializable]
public class MusicEntry
{
    public MusicType musicType;
    public AudioClip clip;
    [Range(0f, 1f)] public float defaultVolume = 0.5f;
}
[RequireComponent(typeof(AudioSource))]
public class Audiomanager : MonoBehaviour
{
    [SerializeField] private SoundEntry[] soundlist;
    [SerializeField] private MusicEntry[] musiclist;
    private static Audiomanager instance;
    private AudioSource sfxSource;
    private AudioSource loopingSource;
    private AudioSource musicSource;


    private Dictionary<SoundType, SoundEntry> soundDict;
    private Dictionary<MusicType, MusicEntry> musicDict;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource = GetComponent<AudioSource>();
        loopingSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        loopingSource.loop = true;
        musicSource.loop = true;

        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        soundDict = new Dictionary<SoundType, SoundEntry>();
        foreach (var entry in soundlist)
        {
            if (!soundDict.ContainsKey(entry.soundType))
                soundDict.Add(entry.soundType, entry);
        }

        musicDict = new Dictionary<MusicType, MusicEntry>();
        foreach (var entry in musiclist)
        {
            if (!musicDict.ContainsKey(entry.musicType))
                musicDict.Add(entry.musicType, entry);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
        StopContinuousSound();
        if (scene.name == "Main Menu")
        {
            PlayMusic(MusicType.Main_Menu);
        }
        else if (scene.name == "Forest")
        {
            StopMusic();
            StartContinuousSound(SoundType.Rain);
        }
        else
        {
            StopContinuousSound();
        }
    }

    public void Start()
    {
        sfxSource = GetComponent<AudioSource>();
        StartCoroutine(RandomWind());
    }
    private IEnumerator RandomWind()
    {
        while (true)
        {
            float randomWind = Random.Range(20f, 300f);
            yield return new WaitForSeconds(randomWind);
            Audiomanager.PlaySound(SoundType.Wind);
        }
    }
    public static void PlaySound(SoundType sound, float volumeScale = 1f)
    {
        if (instance == null || !instance.soundDict.TryGetValue(sound, out SoundEntry entry) || entry.clip == null)
            return;

        float finalVolume = entry.defaultVolume * volumeScale;
        instance.sfxSource.PlayOneShot(entry.clip, finalVolume);
    }

    public static void StartContinuousSound(SoundType sound, float volumeScale = 1f)
    {
        if (instance == null || !instance.soundDict.TryGetValue(sound, out SoundEntry entry) || entry.clip == null)
            return;

        float finalVolume = entry.defaultVolume * volumeScale;

        if (instance.loopingSource.isPlaying && instance.loopingSource.clip == entry.clip)
        {
            instance.loopingSource.volume = finalVolume;
            return;
        }

        instance.loopingSource.clip = entry.clip;
        instance.loopingSource.volume = finalVolume;
        instance.loopingSource.Play();
    }

    public static void StopContinuousSound()
    {
        if (instance != null && instance.loopingSource.isPlaying)
        {
            instance.loopingSource.Stop();
            instance.loopingSource.clip = null;
        }
    }

    public static void PlayMusic(MusicType music, float volumeScale = 1f, bool loop = true)
    {
        if (instance == null || !instance.musicDict.TryGetValue(music, out MusicEntry entry) || entry.clip == null)
            return;

        float finalVolume = entry.defaultVolume * volumeScale;

        if (instance.musicSource.isPlaying && instance.musicSource.clip == entry.clip)
        {
            instance.musicSource.volume = finalVolume;
            return;
        }

        instance.musicSource.clip = entry.clip;
        instance.musicSource.volume = finalVolume;
        instance.musicSource.loop = loop;
        instance.musicSource.Play();
    }

    public static void StopMusic()
    {
        if (instance != null && instance.musicSource.isPlaying)
        {
            instance.sfxSource.Stop();
            instance.loopingSource.Stop();
            instance.musicSource.Stop();
            instance.loopingSource.clip = null;
            instance.sfxSource.clip = null;
            instance.musicSource.clip = null;
        }
    }

    public static void PauseMusic()
    {
        if (instance != null && instance.musicSource.isPlaying)
        {
            instance.musicSource.Pause();
        }
    }

    public static void ResumeMusic()
    {
        if (instance != null && !instance.musicSource.isPlaying && instance.musicSource.clip != null)
        {
            instance.musicSource.UnPause();
        }
    }

    public static void SetMusicVolume(float volume)
    {
        if (instance != null)
        {
            instance.musicSource.volume = Mathf.Clamp01(volume);
        }
    }
}
