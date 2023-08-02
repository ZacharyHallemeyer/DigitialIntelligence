using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager class responsible for controlling the game flow.
/// Handles the creation and interactions of puzzles, terminal and timer.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static int levelIndex;
    public static string folderName;
    public static string levelName;
    public static string startingDirectoryPath;
    public static int numRemainingLockedFiles;

    // Puzzles
    public List<PuzzleContainer> puzzleDataList;
    public static int puzzleIndex = 0;
    public static List<GameObject> puzzles; // List of instantiated puzzle prefab
    public GameObject puzzlePrefab;

    // Terminal
    public static GameObject terminalObject;
    public static Terminal terminal;
    public static GameObject terminalUI;
    public GameObject terminalObjectPrefab;

    // Timer
    public static TMP_Text timerText;
    public static int timeLimitMinutes = 29;
    private static int currentTimeMinutes;
    private static int currentTimeSeconds;


    // File system
    public static DirectoryData directoryRoot;
    public static DirectoryData currentDirectory;

    // Hub
    public GameObject hubPrefab;
    public GameObject hubObject;
    public Hub hub;



    public static readonly List<KeyValuePair<string, int>> pythonKeyWords = new List<KeyValuePair<string, int>>()
    {
        // Python keywords, color: Crimson Red
        new KeyValuePair<string, int>("and", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("as", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("assert", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("break", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("class", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("continue", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("def", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("del", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("elif", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("else", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("except", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("finally", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("for", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("from", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("global", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("if", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("import", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("in", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("is", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("lambda", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("nonlocal", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("not", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("or", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("pass", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("raise", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("return", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("try", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("while", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("with", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("yield", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("True", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("False", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("False", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),
        new KeyValuePair<string, int>("None", (int)PlayerPrefNames.PYTHON_COLORS.KEYWORD),

        // Python built-in functions, color: Violet Purple
        new KeyValuePair<string, int>("abs", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("all", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("any", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("ascii", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("bin", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("bool", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("bytearray", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("bytes", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("callable", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("chr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("classmethod", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("compile", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("complex", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("delattr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("dict", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("dir", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("divmod", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("enumerate", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("eval", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("exec", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("filter", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("float", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("format", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("frozenset", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("getattr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("globals", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("hasattr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("hash", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("help", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("hex", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("id", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("input", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("int", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("isinstance", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("issubclass", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("iter", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("len", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("list", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("locals", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("map", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("max", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("memoryview", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("min", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("next", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("object", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("oct", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("open", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("ord", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("pow", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("print", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("property", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("range", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("repr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("reversed", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("round", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("set", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("setattr", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("slice", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("sorted", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("staticmethod", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("str", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("sum", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("super", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("tuple", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("type", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("vars", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("zip", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION),
        new KeyValuePair<string, int>("__import__", (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION)
    };

    private void Awake()
    {
        
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        StartGame();
    }

    /// <summary>
    /// Sets up the initial game state.
    /// </summary>    
    public void StartGame()
    {
        puzzleIndex = 0;
        numRemainingLockedFiles = 0;
        currentTimeSeconds = 61;
        currentTimeMinutes = timeLimitMinutes;
        CreatePuzzles();

        CreateFileSystem();

        CreateTerminal();

        CreateHub();
        Camera.main.backgroundColor = Color.red;
    }

    // ========================= TERMINAL ========================= //

    /// <summary>
    /// Creates a terminal object in the game world and initializes its state.
    /// </summary>
    private void CreateTerminal()
    {
        // Create terminal object
        terminalObject = Instantiate(terminalObjectPrefab);
        terminal = terminalObject.GetComponent<Terminal>();
        terminalUI = GameObject.Find("TerminalUI");
        terminal.Initialize();
        terminalObject.SetActive(false);
    }

    // ========================= Data Manipulation ========================= //

    /// <summary>
    /// Create file system and assigns current directory
    /// </summary>
    private void CreateFileSystem()
    {
        // Get file system structure from json file
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/" + levelName);
        string data = jsonData.text;

        // Convert json to DirectoryData object
        DirectoryData rootDir = JsonConvert.DeserializeObject<DirectoryData>(data);
        // Assign parent directories
        AssignParentDirs(rootDir, null);
        // Assign current directory to root and set hasParent to false as it does not have a parent
        currentDirectory = rootDir;
        currentDirectory.hasParent = false;
    }

    /// <summary>
    /// Assigns directory's parent to its parent if it has one
    /// </summary>
    /// <param name="dir">current directory</param>
    /// <param name="parentDir">parent directory</param>
    void AssignParentDirs(DirectoryData dir, DirectoryData parentDir)
    {
        dir.parentDir = parentDir;
        dir.hasParent = true;
        foreach (DirectoryData subdir in dir.directories)
        {
            AssignParentDirs(subdir, dir);
        }
    }

    /// <summary>
    /// Used for debugging, prints directory and files
    /// </summary>
    /// <param name="dirData">current directory</param>
    /// <param name="indent">use ""</param>
    private void PrintDirectories(DirectoryData dirData, string indent)
    {
        string output = "";
        output += "\n" + indent + dirData.dirName;

        foreach(FileData fileData in dirData.files)
        {
            output += "\n\t" + indent + fileData.fileName;
        }

        Debug.Log(output);

        foreach(DirectoryData dirDataInner in dirData.directories)
        {
            PrintDirectories(dirDataInner, indent + "\t");
        }

    }

    /// <summary>
    /// Reads puzzle data from a Json file and creates corresponding puzzle objects in the game world.
    /// </summary>
    void CreatePuzzles()
    {
        // Get puzzle data from Json
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/puzzles");
        string data = jsonData.text;
        List<PuzzleContainer> puzzleDataList = JsonConvert.DeserializeObject<List<PuzzleContainer>>(data);

        puzzles = new List<GameObject>();


        foreach(Puzzle puzzleData in puzzleDataList[levelIndex].puzzles)
        {
            // Create the Puzzle classes from the data 
            GameObject puzzleObject = Instantiate(puzzlePrefab);
            Puzzle puzzleComponent = puzzleObject.GetComponent<Puzzle>();

            // Add the class data to the puzzlePrefab
            try
            {
                puzzleComponent.Initialize(puzzleData);
            }
            catch
            {
                // LEAVE EMPTY FOR NOW
            }

            // Store the puzzle object in the puzzles list
            puzzles.Add(puzzleObject);
            puzzleObject.SetActive(false);
        }
    }

    // ========================= Puzzles ========================= //

    /// <summary>
    /// Called from Puzzle.RunTests if all tests pass
    /// Loops through directories and unlocked the directory with the same name as the puzzle
    /// </summary>
    /// <param name="puzzleName">puzzle name that was solved</param>
    public void PuzzleSolved(string puzzleName)
    {
        // Find puzzle in directories
        foreach(DirectoryData dirData in currentDirectory.directories)
        {
            if(dirData.dirName == puzzleName)
            {
                dirData.unlocked = true;
                terminal.PrintLineToTerminal($"<color={terminal.successColor}>Directory {puzzleName} successfully unlocked</color>", false);
            }
        }

        puzzleIndex++;
    }

    // ========================= HUB ========================= //

    /// <summary>
    /// Instantiates the hub from a prefab and sets the hubObject and hub variables
    /// </summary>
    private void CreateHub()
    {
        hubObject = Instantiate(hubPrefab);
        hub = hubObject.GetComponent<Hub>();
        hub.Initialize(levelName);
    }

    // ========================= Game Management ========================= //

    public static void GameWon()
    {
        // Loop through puzzle UI and remove
        foreach(GameObject puzzle in puzzles)
        {
            Destroy(puzzle);
        }
        // Remove terminal
        Destroy(terminalObject);

        Debug.Log("You won!!!!!");
    }

    private static void GameFailed()
    {
        // TODO
        Debug.Log("Game Lost :(");
    }

    /// <summary>
    /// Creates a timer and displays it on the game screen.
    /// </summary>
    private void CreateTimer()
    {
        timerText = GameObject.Find("TimeText").GetComponent<TMP_Text>();
        StartCoroutine(Timer());
    }

    /// <summary>
    /// Coroutine that handles the countdown of the timer.
    /// </summary>
    private static IEnumerator Timer()
    {
        while(true)
        {
            // Decrease time seconds
            currentTimeSeconds--;

            // Check if a minute has passed
            if(currentTimeSeconds < 0)
            {
                // Check if there are any minutes left
                if(currentTimeMinutes > 0)
                {
                    // Decrease minute count and reset time seconds to 59
                    currentTimeMinutes--;
                    currentTimeSeconds = 59;
                }
            }

            // Set Display
            string minutes = currentTimeMinutes < 10 ? "0" + currentTimeMinutes : currentTimeMinutes.ToString();
            string seconds = currentTimeSeconds < 10 ? "0" + currentTimeSeconds : currentTimeSeconds.ToString();

            timerText.text = minutes + ":" + seconds;

            // Check if time limit has been reached
            if (currentTimeSeconds <= 0 && currentTimeMinutes <= 0)
            {
                // Call GameFailed and return out of function
                GameFailed();
                yield break;
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
