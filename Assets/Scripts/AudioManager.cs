using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles audio
/// </summary>
public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume;
        [Range(.1f, 3f)]
        public float pitch;

        public bool looping;

        [HideInInspector]
        public AudioSource source;
    }

    public Sound[] soundsMusic;
    public Sound[] soundsSoundEffects;
    public static AudioManager instance;

    /// <summary>
    /// Sets sounds and volume
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Set volumes pref if there is none
        if (PlayerPrefs.GetFloat("SoundEffectsVolume", 100) == 100)
            PlayerPrefs.SetFloat("SoundEffectsVolume", .75f);
        if (PlayerPrefs.GetFloat("MusicVolume", 100) == 100)
            PlayerPrefs.SetFloat("MusicVolume", .75f);

        // Set each source
        foreach (Sound s in soundsMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = PlayerPrefs.GetFloat("MusicVolume");

            s.source.pitch = s.pitch;
            s.source.loop = s.looping;
        }        
        foreach (Sound s in soundsSoundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = PlayerPrefs.GetFloat("SoundEffectsVolume");

            s.source.pitch = s.pitch;
            s.source.loop = s.looping;
        }
    }

    /// <summary>
    /// Starts music
    /// </summary>
    private void Start()
    {
        StartCoroutine(PlayMusicOnLoop());
    }

    /// <summary>
    /// Plays music on loop. Plays a random song from music list 
    /// </summary>
    private IEnumerator PlayMusicOnLoop()
    {
        // Get random song
        System.Random rand = new System.Random();
        int randIndex = rand.Next(0, soundsMusic.Length);
        Sound song = soundsMusic[randIndex];

        // Play random song
        PlayMusic(song.name);

        // Wait for song to end (plus 5 seconds)
        yield return new WaitForSeconds(song.source.clip.length);

        // Call itself 
        StartCoroutine(PlayMusicOnLoop());
    }

    /// <summary>
    /// Play audio clip with cooresponding name
    /// </summary>
    public void PlayMusic(string name)
    {
        Sound s = Array.Find(soundsMusic, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Play();
    }

    /// <summary>
    /// Stop audio clip with cooresponding name
    /// </summary>
    public void StopMusic(string name)
    {
        Sound s = Array.Find(soundsMusic, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Stop();
    }

    /// <summary>
    /// Plays success sound
    /// </summary>
    public void PlaySuccessSoundEffect()
    {
        Sound s = Array.Find(soundsSoundEffects, sound => sound.name == "SuccessSoundEffect");
        if (s == null)
            return;
        s.source.Play();
    }


    /// <summary>
    /// Plays error sound
    /// </summary>
    public void PlayErrorSoundEffect()
    {
        Sound s = Array.Find(soundsSoundEffects, sound => sound.name == "ErrorSoundEffect");
        if (s == null)
            return;
        s.source.Play();
    }

    /// <summary>
    /// Plays button click sound
    /// </summary>
    public void PlayButtonClickSoundEffect()
    {
        Sound s = Array.Find(soundsSoundEffects, sound => sound.name == "ButtonClickSoundEffect");
        if (s == null)
            return;
        s.source.Play();
    }

    /// <summary>
    /// Set music audio clip's volume to player preference
    /// </summary>
    public void SetMusicVolume()
    {
        foreach (Sound s in soundsMusic)
        {
            if (s.source != null)
            {
                s.source.volume = PlayerPrefs.GetFloat("MusicVolume", .75f);
            }
        }
    }

    /// <summary>
    /// Set sounds (other than music) audio clip's volume to player preference
    /// </summary>
    public void SetSoundEffectVolume()
    {
        foreach (Sound s in soundsSoundEffects)
        {
            if (s.source != null)
            {
                s.source.volume = PlayerPrefs.GetFloat("SoundEffectsVolume", .75f);
            }
        }
    }

}
