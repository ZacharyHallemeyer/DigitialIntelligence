using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;

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
        SetPlayerPrefs();
        startContainer.SetActive(true);
        aboutContainer.SetActive(false);
        settingsContainer.SetActive(false);

        // Add UI elements to level menu list
        levelMenuUI.Add(levelScrollView);

        // Create level menu
        ReadPuzzleData();
        CreateLevelMenu();

        // Hide level menu
        levelScrollView.SetActive(false);

        //
        //SceneManager.sceneLoaded += OnSceneLoaded;
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
        GridLayoutGroup gridLayoutGroup = levelScrollRect.content.GetComponent<GridLayoutGroup>();

        float originalCellSizeY = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;

        // Get persistent level data
        List<LevelInfo> levelData;
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);
        string persistentData;
        // Check if persistent data of puzzles exist
        if (File.Exists(persistentDataPath))
        {
            // Check if persistent data file is valid
            ValidatePersistentPuzzleFile();
        }
        // Otherwise, get puzzle data from resources folder
        else
        {
            CreatePersistentPuzzleFile(puzzleDataList);
        }

        persistentData = File.ReadAllText(persistentDataPath);
        levelData = ConvertLevelInfoStringToList(persistentData);


        // Loop through levels
        foreach (PuzzleContainer puzzleContainer in puzzleDataList)
        {
            // Create Button
            GameObject button = Instantiate(levelButtonPrefab, levelScrollRect.content);

            // Get LevelButton component and assign puzzle container index
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.puzzleContainerName = puzzleContainer.levelName;
            levelButton.puzzleContainerIndex = puzzleContainer.index;

            // Set button text to puzzle container name
            levelButton.SetButtonText(puzzleContainer.levelName);

            // Adjust scroll view
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelScrollRect.content);

            // Update currentYPosition
            float currentYPosition = (originalCellSizeY + spacingY) * (puzzleDataList.Count + puzzleContainer.index);

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


            // Check if level is completed
            if(levelData[puzzleContainer.index].completed)
            {

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



    // Data Manipulation

    private List<LevelInfo> CreatePersistentPuzzleFile(List<PuzzleContainer> puzzleDataList)
    {
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        List<LevelInfo> levelInfoList = new List<LevelInfo>();

        foreach (PuzzleContainer puzzleContainer in puzzleDataList)
        {
            LevelInfo levelInfo = new LevelInfo();
            levelInfo.levelName = puzzleContainer.levelName;
            levelInfo.index = puzzleContainer.index;
            levelInfo.completed = false;
            levelInfo.puzzles = new List<PuzzleInfo>();

            foreach (Puzzle puzzle in puzzleContainer.puzzles)
            {
                PuzzleInfo puzzleInfo = new PuzzleInfo();

                puzzleInfo.puzzleIndex = puzzle.puzzleIndex;
                puzzleInfo.oldCode = "";

                levelInfo.puzzles.Add(puzzleInfo);
            }

            levelInfoList.Add(levelInfo);
        }

        // Serialize levelInfoList to JSON
        string json = JsonConvert.SerializeObject(levelInfoList, Formatting.Indented);

        // Write JSON to a file at the specified path
        File.WriteAllText(persistentDataPath, json);

        return levelInfoList;
    }

    public void CreatePersistentPuzzleFile()
    {
        // Get Levels from level json
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/puzzles");
        string data = jsonData.text;

        List<PuzzleContainer> puzzleDataList = JsonConvert.DeserializeObject<List<PuzzleContainer>>(data);

        CreatePersistentPuzzleFile(puzzleDataList);
    }

    private void ValidatePersistentPuzzleFile()
    {
        bool deviationFound = false;
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        // Get LevelInfo list
        List<LevelInfo> levelInfoList = GetLevelInfoListFromPersistentDataFile();

        
        // Check if puzzleContainers is not the same amount as levelInfos
        if(levelInfoList.Count != puzzleDataList.Count)
        {
            // If so, deviation found
            deviationFound = true;
        }
        
        // Loop through persistent data and puzzle data unitl the two level names/indexes do not match
        int index = 0;
        while (index < puzzleDataList.Count && !deviationFound)
        {
            // Check if current persistent data level info cooresponds with puzzle data
            if(levelInfoList[index].levelName != puzzleDataList[index].levelName )
            {
                deviationFound = true;
            }

            index++;
        }

        if(deviationFound)
        {
            // Create a new persistent data file
            CreatePersistentPuzzleFile(puzzleDataList);

            // Get data from file
            List<LevelInfo> newLevelInfoList = GetLevelInfoListFromPersistentDataFile();
            // Add information from levelInfoList to new file

            // Loop through new level info list 
            for(index = 0; index < newLevelInfoList.Count; index++)
            {
                // Check if current level has information to add
                if(newLevelInfoList[index].levelName == levelInfoList[index].levelName)
                {
                    newLevelInfoList[index].completed = levelInfoList[index].completed;
                    

                    // Loop through puzzles and add old code
                    for(int puzzleIndex = 0; puzzleIndex < newLevelInfoList[index].puzzles.Count; puzzleIndex++)
                    {
                        PuzzleInfo newPuzzle = newLevelInfoList[index].puzzles[puzzleIndex];
                        PuzzleInfo oldPuzzle = levelInfoList[index].puzzles[puzzleIndex];


                        if (newPuzzle.puzzleIndex == oldPuzzle.puzzleIndex)
                        {
                            newPuzzle.oldCode = oldPuzzle.oldCode;
                        }
                    }

                }
            }


            // Write newLevelInfoList information to file
            string json = JsonConvert.SerializeObject(newLevelInfoList, Formatting.Indented);

            // Write JSON to a file at the specified path
            File.WriteAllText(persistentDataPath, json);

        }

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
}
