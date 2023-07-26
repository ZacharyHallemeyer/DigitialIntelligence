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
    public List<GameObject> levelMenuUI;


    // Start is called before the first frame update
    void Start()
    {
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
        startContainer.SetActive(true);
        aboutButton.SetActive(false);
        settingsContainer.SetActive(false);
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

    public void MoveToLevelScreen()
    {
        // Hide Main Menu screen
        startContainer.SetActive(false);

        // Show button elements for each level
        levelScrollView.SetActive(true);
    }

    // ============================== DATA ============================== //

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
}
