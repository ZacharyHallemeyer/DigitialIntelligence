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

    public TMP_Text storyText;
    public TMP_Text notesText;

    public void Initialize(string levelName)
    {
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
            storyText.text = "<color=#FF0000>Hub info was not found</color>";
            return;
        }

        // Set story text
        storyText.text = File.ReadAllText(hubInfo.storyPath);

        // Set notes text
        notesText.text = File.ReadAllText(hubInfo.notesPath);

        // Show story text and hide notes text
        ShowStoryText();
    }

    public void ShowNotesText()
    {
        // Hide story
        storyText.gameObject.SetActive(false);
        // Show notes
        notesText.gameObject.SetActive(true);
    }

    public void ShowStoryText()
    {
        // Hide notes
        notesText.gameObject.SetActive(false);
        // Show story
        storyText.gameObject.SetActive(true);
    }


    public void SettingsButtonClick()
    {

    }

    public void StoryButtonClick()
    {
        ShowStoryText();
    }

    public void NotesButtonClick()
    {
        ShowNotesText();
    }

    public void ExitButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Level");
    }

    public void TerminalButtonClick()
    {
        GameManager.terminalObject.SetActive(true);
        GameManager.instance.hubObject.SetActive(false);
    }

}
