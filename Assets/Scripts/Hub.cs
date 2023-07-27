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
        story = "\n" + File.ReadAllText(hubInfo.storyPath);
        infoText.text = story;

        // Set notes text
        notes = "\n" + File.ReadAllText(hubInfo.notesPath);

        // Show story text and hide notes text
        ShowStoryText();
    }

    public void ShowNotesText()
    {
        infoText.text = notes;
    }

    public void ShowStoryText()
    {
        infoText.text = story;
    }


    public void SettingsButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        settingsContainer.SetActive(true);
    }

    public void StoryButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        ShowStoryText();
    }

    public void NotesButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        ShowNotesText();
    }

    public void ExitButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Level");
    }

    public void BackButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        settingsContainer.SetActive(false);
    }

    public void TerminalButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        GameManager.terminalObject.SetActive(true);
        GameManager.instance.hubObject.SetActive(false);
    }

}
