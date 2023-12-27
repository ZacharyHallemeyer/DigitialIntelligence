using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using TMPro;
using UnityEditor;
using System.Linq;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;



public class MainMenu : MonoBehaviour
{
    // Prefabs
    public GameObject levelButtonPrefab;

    // Data
    public List<PuzzleContainer> puzzleDataList;

    // UI
    public GameObject startButton;
    public GameObject aboutButton;
    public GameObject settingsButton;
    public GameObject quitButton;
    public GameObject levelScrollView;
    public GameObject levelScrollContent;
    public ScrollRect levelScrollRect;

    // UI containers
    public GameObject startContainer;
    public GameObject aboutContainer;
    public GameObject settingsContainer;
    public GameObject sliderContainer;
    public GameObject colorContainer;
    public List<GameObject> levelMenuUI;


    // Start is called before the first frame update
    void Start()
    {
        //CreateResourcesPathJson();
        DownloadExternalResources();

        SetPlayerPrefs();
        startContainer.SetActive(true);
        aboutContainer.SetActive(false);
        settingsContainer.SetActive(false);

        // Add UI elements to level menu list
        levelMenuUI.Add(levelScrollView);

        // Create level menu
        CreateLevelMenu();

        // Hide level menu
        levelScrollView.SetActive(false);

    }

    // ============================== Buttons ============================== //

    // Go to Level screen
    public void StartButtonClick()
    {
        MoveToLevelScreen();
        AudioManager.instance.PlayButtonClickSoundEffect();
    }

    public void SandboxButtonClick()
    {
        // Move to sandbox scene
        SceneManager.LoadScene("Sandbox");
        SceneManager.UnloadSceneAsync("MainMenu");
        AudioManager.instance.PlayButtonClickSoundEffect();
    }

    // Go to About Screen
    public void AboutButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        startContainer.SetActive(false);
        aboutContainer.SetActive(true);
    }

    // Go to Settings Screen
    public void SettingsButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        startContainer.SetActive(false);
        settingsContainer.SetActive(true);
    }

    // Close application
    public void QuitButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        Application.Quit();
    }

    // Move to start container
    public void BackButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        startContainer.SetActive(true);
        aboutContainer.SetActive(false);
        settingsContainer.SetActive(false);
    }

    /// <summary>
    /// Sets color container to active and slider container to inactive
    /// </summary>
    public void ColorButtonClick()
    {
        sliderContainer.SetActive(false);
        colorContainer.SetActive(true);
    }

    /// <summary>
    /// Sets color container to inactive and slider container to active
    /// </summary>
    public void ColorBackButtonClick()
    {
        colorContainer.SetActive(false);
        sliderContainer.SetActive(true);
    }

    public void StartLevel(int levelIndex, string levelName)
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        // Set game manager level index
        GameManager.levelIndex = levelIndex;
        GameManager.levelName = levelName;
        // move to level scene
        SceneManager.LoadScene("Level");
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    // NOT USED...KEEP IN CASE IT IS NEEDED LATER
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "SampleScene")
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.StartGame();
            }
        }
    }

    // ============================== NAVIGATION ============================== //

    /// <summary>
    /// Hides main menu UI and shows level section
    /// </summary>
    public void MoveToLevelScreen()
    {
        // Hide Main Menu screen
        startContainer.SetActive(false);

        // Show button elements for each level
        levelScrollView.SetActive(true);
    }

    // ============================== DATA ============================== //

    /// <summary>
    /// Creates a section for levels
    /// A button is created for every level in the puzzle.json file
    /// THe button contains both puzzle index and puzzle name
    /// </summary>
    private void CreateLevelMenu()
    {
        List<LevelInfo> levelInfoList = new List<LevelInfo>();
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);
        // Check if persistent data file already exists
        if ( !File.Exists(persistentDataPath) )
        {
            // If not, create it for further use
            CreatePersistentPuzzleFile();
        }
        else
        {
            // Otherwise, get the data stored in persistent data
            levelInfoList = GetLevelInfoListFromPersistentDataFile();
        }


        // CREATE LEVEL BUTTONS
        GridLayoutGroup gridLayoutGroup = levelScrollRect.content.GetComponent<GridLayoutGroup>();
        float originalCellSizeY = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;
        string baseOutputDirectory = "ResourceContainer/Levels";
        string[] levels = Directory.GetDirectories(baseOutputDirectory);

        // Reorder default levels to prespecified order
        List<string> orderedLevels = new List<string>{ "ResourceContainer/Levels\\Tutorial", 
                                                        "ResourceContainer/Levels\\Variables", 
                                                        "ResourceContainer/Levels\\Conditionals",
                                                         "ResourceContainer/Levels\\Loops",
                                                         "ResourceContainer/Levels\\Lists",
                                                         "ResourceContainer/Levels\\Strings",
                                                         "ResourceContainer/Levels\\Functions",
                                                         "ResourceContainer/Levels\\Greek Myths" };
        
        foreach(string level in levels)
        {
            if( !orderedLevels.Contains(level))
            {

                orderedLevels.Add(level);
            }
        }

        int count = 0;
        foreach(string level in orderedLevels)
        {
            // Create Button
            GameObject button = Instantiate(levelButtonPrefab, levelScrollRect.content);
            int indexOfFilename = level.IndexOf("\\");
            string levelName = level.Substring(indexOfFilename+1);


            // Get LevelButton component and assign puzzle container index
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.puzzleContainerName = level;

            // Set button text to puzzle container name
            levelButton.SetButtonText(levelName);

            // Adjust scroll view
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelScrollRect.content);

            // Update currentYPosition
            float currentYPosition = (originalCellSizeY + spacingY) * (count);

            // Adjust position
            RectTransform rectTransform = levelButton.GetComponent<RectTransform>();
            Vector2 newPosition = rectTransform.anchoredPosition;
            newPosition.y = -currentYPosition;
            rectTransform.anchoredPosition = newPosition;

            // Add event listener
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                StartLevel(levelButton.puzzleContainerIndex, levelButton.puzzleContainerName);
            });

            count++;

            if(levelInfoList != null)
            {
                // Check if level is completed
                foreach(LevelInfo levelInfo in levelInfoList)
                {
                    if( levelInfo.levelName == level && levelInfo.completed)
                    {
                        Button buttonComponent = button.GetComponent<Button>();
                        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

                        // Change background color to white
                        var buttonColors = buttonComponent.colors;
                        buttonColors.normalColor = new Color(1f, 1f, 1f);
                        buttonComponent.colors = buttonColors;
                        //buttonComponent.image.color = new Color(1f, 1f, 1f);
                        buttonText.color = new Color(0f, 0f, 0f);
                    }
                }
            }
        }

    }

    private void ReadPuzzleData()
    {
        // Get puzzle data from Json
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/puzzles");
        string data = jsonData.text;
        puzzleDataList = JsonConvert.DeserializeObject<List<PuzzleContainer>>(data);
    }

    private List<string> GetLevels()
    {
        string levelsPath = "Levels";

        return null;
    }

    private void SetPlayerPrefs()
    {
        if(!PlayerPrefs.HasKey(PlayerPrefNames.MUSIC_VOLUME)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.MUSIC_VOLUME, .75f);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.SOUND_EFFECT_VOLUME)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.SOUND_EFFECT_VOLUME, .75f);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_FONT_SIZE)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.TERMINAL_FONT_SIZE, 15);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.HUB_FONT_SIZE)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.HUB_FONT_SIZE, 15);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.DIRECTIONS_FONT_SIZE)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.DIRECTIONS_FONT_SIZE, 15);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CONSOLE_FONT_SIZE)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.CONSOLE_FONT_SIZE, 15);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_FONT_SIZE)) {
            PlayerPrefs.SetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_PLAIN_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_PLAIN_COLOR, "#FFFFFF");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_COMMAND_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_COMMAND_COLOR, "#6495ED");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_CARET_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_CARET_COLOR, "#FF0000");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR, "#FFFFFF");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR, "#8B0000");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR, "#9400D3");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR, "#770737");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_PLAIN_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_PLAIN_COLOR, "#FFFFFF");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_KEYWORD_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_KEYWORD_COLOR, "#DC143C");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_FUNCTION_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_FUNCTION_COLOR, "#9400D3");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_STRING_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_STRING_COLOR, "#008000");
        }
        if(!PlayerPrefs.HasKey(PlayerPrefNames.CODE_CARET_COLOR)) {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_CARET_COLOR, "#0000FF");
        }
    }


    // ========================================= Data Manipulation =========================================
    private void CreatePersistentPuzzleFile()
    {
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        // Write JSON to a file at the specified path
        File.WriteAllText(persistentDataPath, "");
    }

    private List<LevelInfo> GetLevelInfoListFromPersistentDataFile()
    {
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        string levelInfoString = File.ReadAllText(persistentDataPath);

        List<LevelInfo> puzzleDataList = JsonConvert.DeserializeObject<List<LevelInfo>>(levelInfoString);

        return puzzleDataList;
    }

    private List<LevelInfo> ConvertLevelInfoStringToList(string levelInfoString)
    {
        List<LevelInfo> puzzleDataList = JsonConvert.DeserializeObject<List<LevelInfo>>(levelInfoString);

        return puzzleDataList;
    }

    public void ResetPlayerProgress()
    {
        CreatePersistentPuzzleFile();
    }

    // ========================= Resources ========================= //
    /*
    private void CreateResourcesPathJson()
    {
        string resourcePath = "Assets/Resources";
        string outputPath = "Assets/Resources/ResourcePaths.json";

        List<string> resourcePaths = new List<string>();

        // Recursively scan the Resources folder and collect relative paths
        string[] allAssets = AssetDatabase.FindAssets("", new[] { resourcePath });
        foreach (string assetGUID in allAssets)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            string relativePath = assetPath.Replace(resourcePath + "/", "");
            resourcePaths.Add(relativePath);
        }

        // Serialize the list of paths to JSON
        string json = JsonConvert.SerializeObject(resourcePaths, Formatting.Indented);

        // Write the JSON data to the output file
        File.WriteAllText(outputPath, json);

        Debug.Log("Resource paths exported to " + outputPath);
    }
    */

    private void DownloadExternalResources()
    {
        string resourcesPath = "ResourcePaths";
        string baseOutputDirectory = "ResourceContainer";
        
        if( Directory.Exists(baseOutputDirectory) )
        {
            Debug.Log("Resources already copied...Returning now");
            return;
        }

        Directory.CreateDirectory($"{baseOutputDirectory}");

        string resourcesUnparsed = Resources.Load<TextAsset>(resourcesPath).text;
        List<string> resources = JsonConvert.DeserializeObject<List<string>>(resourcesUnparsed);

        foreach(string resource in resources)
        {
            if(resource.EndsWith(".txt") || resource.EndsWith(".json"))
            {
                int indexOfDot = resource.IndexOf('.');
                string resourceNoExtension = resource.Substring(0, indexOfDot);
                string text = Resources.Load<TextAsset>(resourceNoExtension).text;

                using (StreamWriter writer = new StreamWriter($"{baseOutputDirectory}/{resource}"))
                {
                    writer.Write(text);
                }

                string filePath = Path.Combine(baseOutputDirectory, resource);
                SetFilePermissions(filePath);
            }
            else
            {
                Directory.CreateDirectory($"{baseOutputDirectory}/{resource}");
            }


        }
    }

    private static void SetFilePermissions(string filePath)
    {
        try
        {
            // Get the file's existing security settings
            FileSecurity fileSecurity = File.GetAccessControl(filePath);

            // Define a new access rule for read and write permissions
            FileSystemAccessRule accessRule = new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                FileSystemRights.ReadAndExecute | FileSystemRights.Write,
                InheritanceFlags.None,
                PropagationFlags.None,
                AccessControlType.Allow
            );

            // Add the access rule to the file's security settings
            fileSecurity.AddAccessRule(accessRule);

            // Apply the modified security settings to the file
            File.SetAccessControl(filePath, fileSecurity);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting file permissions: {e.Message}");
        }
    }
}
