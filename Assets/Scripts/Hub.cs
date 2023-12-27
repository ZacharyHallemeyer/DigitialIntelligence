using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.IO;

public class Hub : MonoBehaviour
{

    public TMP_Text infoText;

    public GameObject settingsContainer;
    public GameObject sliderContainer;
    public GameObject colorContainer;

    private string story;
    private string notes;

    /// <summary>
    /// Sets hub to default state and gets story and notes from json files
    /// </summary>
    /// <param name="levelName"></param>
    public void Initialize(string levelName)
    {
        settingsContainer.SetActive(false);
        JsonHubObject hubInfo = null;

        // Get hub information list
        story = File.ReadAllText($"{levelName}/Story.txt");

        // Check if hub info was found
        if(story == null)
        {
            infoText.text = "<color=#FF0000>Hub info was not found</color>";
            return;
        }

        // Set story text
        infoText.text = "/n" + story;

        // Show story text and hide notes text
        SetFontSize();
        ShowStoryText();
    }

    /// <summary>
    /// Moves to story section
    /// </summary>
    public void ShowStoryText()
    {
        infoText.text = story;
    }

    /// <summary>
    /// Moves to settings screen
    /// </summary>
    public void SettingsButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        settingsContainer.SetActive(true);
    }

    /// <summary>
    /// Moves to notes section
    /// </summary>
    public void StoryButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        ShowStoryText();
    }

    /// <summary>
    /// Moves to main menu
    /// </summary>
    public void ExitButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Level");
    }

    /// <summary>
    /// Moves to hub
    /// </summary>
    public void BackButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        settingsContainer.SetActive(false);
        ShowHub();
    }

    /// <summary>
    /// Sets color container to active and slider container to inactive 
    /// </summary>
    public void ColorButtonClick()
    {
        colorContainer.SetActive(true);
        sliderContainer.SetActive(false);
    }

    /// <summary>
    /// Sets slider container to active and color container to inactive 
    /// </summary>
    public void ColorBackButtonClick()
    {
        sliderContainer.SetActive(true);
        colorContainer.SetActive(false);
    }

    /// <summary>
    /// Moves to terminal
    /// </summary>
    public void TerminalButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        //GameManager.terminalObject.SetActive(true);
        //GameManager.instance.hubObject.SetActive(false);
        GameManager.terminal.ShowTerminal();
        HideHub();
    }

    /// <summary>
    /// Sets hub object to active
    /// </summary>
    public void ShowHub()
    {
        SetFontSize();   
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets hub object to inactive
    /// </summary>
    public void HideHub()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the font size for story/notes
    /// </summary>
    private void SetFontSize()
    {
        infoText.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.HUB_FONT_SIZE, 15);
    }
}
