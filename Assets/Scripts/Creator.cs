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
using System.Collections.Generic;

public class Creator : MonoBehaviour
{
    private string storedLevelsDirectory = "Creator";

    private string selectedLevel = "";
    private string selectedDirectory = "";
    private string selectedFile = "";
    private string selectedPuzzle = "";

    // ------------------- LEVEL SELECT VARIABLES START -------------------
    public ScrollRect levelScrollRect;
    public GameObject levelButtonPrefab;
    public TMP_InputField levelNameInput;

    private List<GameObject> levelSelectButtonList;
    // ------------------- LEVEL SELECT VARIABLES END -------------------

    // ------------------- LEVEL EDITOR VARIABLES START -------------------
    public TMP_Text storyTextDisplay;
    // ------------------- LEVEL EDITOR VARIABLES END -------------------

    // ------------------- PUZZLE EDITOR VARIABLES START -------------------
    // ------------------- PUZZLE EDITOR VARIABLES END -------------------

    // ------------------- STRUCTURE EDITOR VARIABLES START -------------------
    // ------------------- STRUCTURE EDITOR VARIABLES END -------------------


    // Start is called before the first frame update
    void Start()
    {
        // Create base directory if it does not exist
        if(!System.IO.Directory.Exists(storedLevelsDirectory))
        {
            System.IO.Directory.CreateDirectory(storedLevelsDirectory);
        }

        levelSelectButtonList = new List<GameObject>();

        List<string> levels = GetStoredLevels();

        // Display levels to screen
        DisplayedStoredLevels(levels);
    }

    public void MoveToMainMenu()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Creator");
    }

    private void DisplayedStoredLevels(List<string> levels)
    {
        // Delete old level buttons
        for(int index = 0; index < levelSelectButtonList.Count; index++)
        {
            Destroy( levelSelectButtonList[index] );
        }

        levelSelectButtonList = new List<GameObject>();

        // Make a button for each file
        GridLayoutGroup gridLayoutGroup = levelScrollRect.content.GetComponent<GridLayoutGroup>();

        float originalCellSizeY = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;

        int count = 0;
        foreach (string level in levels)
        {
            GameObject newLevelButton = Instantiate(levelButtonPrefab, levelScrollRect.content);

            FileButton levelButtonComponent = newLevelButton.GetComponent<FileButton>();

            levelButtonComponent.fileName = level;
            levelButtonComponent.SetText();

            // Adjust scroll view
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelScrollRect.content);

            // Update currentYPosition
            float currentYPosition = (originalCellSizeY + spacingY) * (levelSelectButtonList.Count + count);

            // Adjust position
            RectTransform rectTransform = newLevelButton.GetComponent<RectTransform>();
            Vector2 newPosition = rectTransform.anchoredPosition;
            newPosition.y = -currentYPosition;
            rectTransform.anchoredPosition = newPosition;

            // Add event listener
            newLevelButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectLevel(level);
            });

            count++;
            levelSelectButtonList.Add(newLevelButton);
        }
    }

    private List<string> GetStoredLevels()
    {
        string[] levels = Directory.GetDirectories(storedLevelsDirectory);
        List<string> levelNames = new List<string>();

        // Loop through levels and get the name of each level
        foreach(string level in levels)
        {
            string[] splitDirPath = level.Split("/");
            string localDirPath = splitDirPath[splitDirPath.Length - 1];
            string[] spliLocalDirPath = level.Split("\\");
            string levelName = spliLocalDirPath[spliLocalDirPath.Length - 1];

            levelNames.Add(levelName);
        }

        return levelNames;
    }

    // ============================== LEVEL SELECT ============================== //
    
    // Creates a new level to modified. The new level is given a default name
    public void CreateNewLevel(string levelName)
    {
        if(levelName == null || levelName == "")
        {
            // TODO: make random string
            levelName = "Test";
        }

        string containerPath = Path.Combine(storedLevelsDirectory, levelName);

        if(System.IO.Directory.Exists(containerPath))
        {
            Debug.Log("Level name already exists");
            return;
        }

        // Create container folder (levelName)
        System.IO.Directory.CreateDirectory(containerPath);

        // Create default files and folder

        // Create sub folder called PuzzleInfo
        System.IO.Directory.CreateDirectory(Path.Combine(containerPath, "PuzzleInfo"));

        // Create file called Puzzle.json
        string filePath = Path.Combine(containerPath, "Puzzle.json");
        File.Create(filePath).Close();
        // Create file called Structure.json
        filePath = Path.Combine(containerPath, "Structure.json");
        File.Create(filePath).Close();
        // Create file called Story.txt 
        filePath = Path.Combine(containerPath, "Story.txt");
        File.Create(filePath).Close();

        // Redisplay the levels
        List<string> levels = GetStoredLevels();
        DisplayedStoredLevels(levels);
    }

    // Opens the selected level
    public void OpenLevel() 
    {
        // Get level data

        // Write level data to level editor

        // Enable/Display level editor
    }

    // Deletes the selected level
    public void DeleteLevel()
    {
        if(selectedLevel == null || selectedLevel == "")
            return;

        string levelPath = Path.Combine(storedLevelsDirectory, selectedLevel);
        // Check if file exists
        if(System.IO.Directory.Exists(levelPath))
        {
            // Delete directory and all subfiles/subdirectories
            System.IO.Directory.Delete(levelPath, true);
        }

        selectedLevel = "";
        levelNameInput.text = "";

        // Redisplay the levels
        List<string> levels = GetStoredLevels();
        DisplayedStoredLevels(levels);
    }

    // Selects the level that is clicked on
    public void SelectLevel(string levelName)
    {
        selectedLevel = levelName;

        // Change level name input field
        levelNameInput.text = levelName;
    }

    // Renames the selected level to the string passed into function
    public void RenameLevel()
    {
        // Remove characters that can cause harm
        string newLevelName = "";
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        foreach(char levelChar in levelNameInput.text)
        {
            if( !invalidChars.Contains( levelChar ) )
            {
                newLevelName += levelChar;
            }
        }

        string oldNamePath = Path.Combine(storedLevelsDirectory, selectedLevel);
        string newNamePath = Path.Combine(storedLevelsDirectory, newLevelName);

        System.IO.Directory.Move(oldNamePath, newNamePath);

        // Redisplay the levels
        List<string> levels = GetStoredLevels();
        DisplayedStoredLevels(levels);
    }

    // Exports Level in proper format to be distributed
    public void ExportLevel()
    {
        // Combile data into a new folder to download 

        // Open file explorer

        // Download to inputted location
    }

    // Moves from this scene to Main Menu scene
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Creator");
    }

    // ============================== Level Editor ============================== //

    // Opens story text editor
    public void EditStoryText()
    {

    }

    // Saves new story text to file 
    public void SaveStoryText()
    {
        string storyPath = Path.Combine(storedLevelsDirectory, selectedLevel);
        storyPath = Path.Combine(storyPath, "Story.txt");

        // Get story data: TODO
        string storyText = "";


        File.WriteAllText(storyPath, storyText);
    }


    public void OpenFileStructureMenu()
    {
        // Get level structure data

        // Write structure data to level editor

        // Enable/Display File Structure Editor
    }


    // ============================== Puzzle ============================== //

    // Creates a new puzzle
    public void CreatePuzzle(string puzzleName)
    {
        // Create starting code file

        // Create directions file
    }

    // Deletes the selected puzzle
    public void DeletePuzzle()
    {

        selectedPuzzle = "";
    }

    // Opens the selected puzzle to be edited
    public void OpenSelectedPuzzle()
    {
        
    }

    // Selects the puzzle clicked on by user
    public void SelectPuzzle(string puzzleName)
    {
        selectedPuzzle = puzzleName;
    }

    // Renames the selected puzzle to string passed into function
    public void RenamesSelectedPuzzle(string newPuzzleName)
    {

    }

    // Saves puzzle data to file
    public void SavePuzzle()
    {

    }

    // Opens the editing starting code screen
    public void OpenStartingCode()
    {

    }

    // Closes and saves the editing starting code screen
    public void SaveStartingCode()
    {

    }

    // Opens the editing directions screen
    public void OpenDirections()
    {

    }

    // Closes and saves the editing directions screen
    public void SaveDirections()
    {

    }

    // Opens the editing test cases screen
    public void OpenTestCases()
    {

    }
    
    // Closes and saves the editing test cases screen
    public void CloseTestCases()
    {

    }

    // ============================== File Structure ============================== //


    // Creates a new directory under the current directory
    // (a default root directory is created for each puzzle which can not be deleted)
    // The root directory has the same name as the level
    public void CreateSubDirectory()
    {

    }

    // Deletes the selected directory
    public void DeleteDirectory()
    {
        selectedDirectory = "";
    }

    // Renames the selected directory to the string passed into function
    public void RenameDirectory(string newDirName)
    {

    }

    // Selects the directory clicked by user
    public void SelectDirectory(string dirName)
    {
        selectedDirectory = dirName;
    }

    // Creates a new file to be edited by user
    public void CreateFile()
    {

    }

    // Opens editing menu for selected file
    public void EditFile()
    {

    }

    // Selects the file that has been clicked by user
    public void SelectFile()
    {

    }

    // Deletes the selected file
    public void DeleteFile()
    {
        selectedFile = "";
    }

    // Saves file contents as well as unlocked status and unlock question/answer
    public void SaveFile()
    {

    }

    // Renames the selected file
    public void RenameFile(string newFileName)
    {

    }
}
