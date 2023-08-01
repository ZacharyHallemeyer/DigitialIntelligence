using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundEffectsSlider;
    public Slider terminalFSSlider;
    public Slider hubFSSlider;
    public Slider consoleFSSlider;
    public Slider directionFSSlider;
    public Slider codeFSSlider;

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
        terminalFSSlider.value = PlayerPrefs.GetFloat("TerminalFontSize", 15);
        hubFSSlider.value = PlayerPrefs.GetFloat("HubFontSize", 15);
        consoleFSSlider.value = PlayerPrefs.GetFloat("PuzzleConsoleFontSize", 15);
        directionFSSlider.value = PlayerPrefs.GetFloat("PuzzleDirectionsFontSize", 15);
        codeFSSlider.value = PlayerPrefs.GetFloat("PuzzleCodeFontSize", 15);
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

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetHubFontSize()
    {
        PlayerPrefs.SetFloat("HubFontSize", hubFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetTerminalFontSize()
    {
        PlayerPrefs.SetFloat("TerminalFontSize", terminalFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetConsoleFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleConsoleFontSize", consoleFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetDirectionsFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleDirectionsFontSize", directionFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetCodeFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleCodeFontSize", codeFSSlider.value);
    }
}
