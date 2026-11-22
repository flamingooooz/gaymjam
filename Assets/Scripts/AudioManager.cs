using NUnit.Framework.Internal;
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
}
[RequireComponent(typeof(AudioSource))]
public class Audiomanager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundlist;
    [SerializeField] private AudioClip[] musiclist;
    private static Audiomanager instance;
    private AudioSource sfxSource;
    private AudioSource loopingSource;
    private AudioSource musicSource;




    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource = gameObject.AddComponent<AudioSource>();
        loopingSource = gameObject.AddComponent<AudioSource>();

        loopingSource.loop = true;
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
   
    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        int index = (int)sound;
        if (index >= 0 && index < instance.soundlist.Length)
        {
            instance.sfxSource.PlayOneShot(instance.soundlist[index], volume);
        }
    }
    public static void StartContinuousSound(SoundType sound, float volume = 1f)
    {
        int index = (int)sound;
        if (index >= 0 && index < instance.soundlist.Length)
        {
            AudioClip clipToPlay = instance.soundlist[index];
            if (instance.loopingSource.isPlaying && instance.loopingSource.clip == clipToPlay)
            {
                return;
            }
            instance.loopingSource.clip = instance.soundlist[index];
            instance.loopingSource.volume = volume;
            instance.loopingSource.Play();
        }
    }
    public static void StopContinuousSound()
    {
        if (instance != null && instance.loopingSource.isPlaying)
        {
            instance.loopingSource.Stop();
            instance.loopingSource.clip = null;
        }
    }
    public static void PlayMusic(MusicType music, float volume = 1f, bool loop = true)
    {
        if (instance == null) return;

        int index = (int)music;
        if (index >= 0 && index < instance.musiclist.Length)
        {
            AudioClip clipToPlay = instance.musiclist[index];

            // Prevent restarting the track if it is already playing
            if (instance.musicSource.isPlaying && instance.musicSource.clip == clipToPlay)
            {
                instance.musicSource.volume = volume;
                return;
            }

            instance.musicSource.clip = clipToPlay;
            instance.musicSource.volume = volume;
            instance.musicSource.loop = loop;
            instance.musicSource.Play();
        }
    }

    public static void StopMusic()
    {
        if (instance != null && instance.musicSource.isPlaying)
        {
            instance.musicSource.Stop();
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
