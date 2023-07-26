using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundEffectsSlider;
    private AudioManager audioManager;


    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        SetSliders();
    }

    // Sets the sliders to player preferences
    public void SetSliders()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", .75f);
        soundEffectsSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume", .75f);
    }

    // Sets music volume and preferences to value in music slider
    public virtual void SetMusicVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        audioManager.SetMusicVolume();
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetSoundEffectsVolume()
    {
        PlayerPrefs.SetFloat("SoundEffectsVolume", soundEffectsSlider.value);
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        audioManager.SetSoundEffectVolume();
    }
}
