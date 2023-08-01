using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;

public class Hub : MonoBehaviour
{

    public TMP_Text infoText;

    public GameObject settingsContainer;

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
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/HubInfoList");
        string data = jsonData.text;
        List<JsonHubObject> jsonHubInfoList = JsonConvert.DeserializeObject<List<JsonHubObject>>(data);

        // Get level information from list
        foreach(JsonHubObject currentHubInfo in jsonHubInfoList)
        {
            if(currentHubInfo.name == levelName)
            {
                hubInfo = currentHubInfo;
            }
        }

        // Check if hub info was found
        if(hubInfo == null)
        {
            infoText.text = "<color=#FF0000>Hub info was not found</color>";
            return;
        }

        // Set story text
        story = "\n" + Resources.Load<TextAsset>(hubInfo.storyPath).text;
        infoText.text = story;

        // Set notes text
        notes = "\n" + Resources.Load<TextAsset>(hubInfo.notesPath).text;

        // Show story text and hide notes text
        SetFontSize();
        ShowStoryText();
    }

    /// <summary>
    /// Moves to notes section
    /// </summary>
    public void ShowNotesText()
    {
        infoText.text = notes;
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
    /// Moves to notes section
    /// </summary>
    public void NotesButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        ShowNotesText();
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

    public void ShowHub()
    {
        SetFontSize();   
        gameObject.SetActive(true);
    }

    public void HideHub()
    {
        gameObject.SetActive(false);
    }

    private void SetFontSize()
    {
        infoText.fontSize = PlayerPrefs.GetFloat("HubFontSize", 15);
    }
}
