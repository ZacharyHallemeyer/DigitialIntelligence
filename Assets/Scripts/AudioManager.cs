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
    }

    private void Start()
    {
        StartCoroutine(PlayMusicOnLoop());
    }
        
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

}
