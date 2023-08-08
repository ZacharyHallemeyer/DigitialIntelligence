using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

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

        // Loop through levels
        foreach (PuzzleContainer puzzleContainer in puzzleDataList)
        {
            // Create Button
            GameObject button = Instantiate(levelButtonPrefab, levelScrollRect.content);

            // Get LevelButton component and assign puzzle container index
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.puzzleContainerName = puzzleContainer.puzzleName;
            levelButton.puzzleContainerIndex = puzzleContainer.index;

            // Set button text to puzzle container name
            levelButton.SetButtonText(puzzleContainer.puzzleName);

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
}
