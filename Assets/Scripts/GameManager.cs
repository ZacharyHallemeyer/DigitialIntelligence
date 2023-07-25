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



    public static readonly List<KeyValuePair<string, string>> pythonKeyWords = new List<KeyValuePair<string, string>>()
    {
        // Python keywords, color: Crimson Red
        new KeyValuePair<string, string>("and", "#DC143C"),
        new KeyValuePair<string, string>("as", "#DC143C"),
        new KeyValuePair<string, string>("assert", "#DC143C"),
        new KeyValuePair<string, string>("break", "#DC143C"),
        new KeyValuePair<string, string>("class", "#DC143C"),
        new KeyValuePair<string, string>("continue", "#DC143C"),
        new KeyValuePair<string, string>("def", "#DC143C"),
        new KeyValuePair<string, string>("del", "#DC143C"),
        new KeyValuePair<string, string>("elif", "#DC143C"),
        new KeyValuePair<string, string>("else", "#DC143C"),
        new KeyValuePair<string, string>("except", "#DC143C"),
        new KeyValuePair<string, string>("finally", "#DC143C"),
        new KeyValuePair<string, string>("for", "#DC143C"),
        new KeyValuePair<string, string>("from", "#DC143C"),
        new KeyValuePair<string, string>("global", "#DC143C"),
        new KeyValuePair<string, string>("if", "#DC143C"),
        new KeyValuePair<string, string>("import", "#DC143C"),
        new KeyValuePair<string, string>("in", "#DC143C"),
        new KeyValuePair<string, string>("is", "#DC143C"),
        new KeyValuePair<string, string>("lambda", "#DC143C"),
        new KeyValuePair<string, string>("nonlocal", "#DC143C"),
        new KeyValuePair<string, string>("not", "#DC143C"),
        new KeyValuePair<string, string>("or", "#DC143C"),
        new KeyValuePair<string, string>("pass", "#DC143C"),
        new KeyValuePair<string, string>("raise", "#DC143C"),
        new KeyValuePair<string, string>("return", "#DC143C"),
        new KeyValuePair<string, string>("try", "#DC143C"),
        new KeyValuePair<string, string>("while", "#DC143C"),
        new KeyValuePair<string, string>("with", "#DC143C"),
        new KeyValuePair<string, string>("yield", "#DC143C"),
        new KeyValuePair<string, string>("True", "#DC143C"),
        new KeyValuePair<string, string>("False", "#DC143C"),
        new KeyValuePair<string, string>("False", "#DC143C"),
        new KeyValuePair<string, string>("None", "#DC143C"),

        // Python built-in functions, color: Violet Purple
        new KeyValuePair<string, string>("abs", "#9400D3"),
        new KeyValuePair<string, string>("all", "#9400D3"),
        new KeyValuePair<string, string>("any", "#9400D3"),
        new KeyValuePair<string, string>("ascii", "#9400D3"),
        new KeyValuePair<string, string>("bin", "#9400D3"),
        new KeyValuePair<string, string>("bool", "#9400D3"),
        new KeyValuePair<string, string>("bytearray", "#9400D3"),
        new KeyValuePair<string, string>("bytes", "#9400D3"),
        new KeyValuePair<string, string>("callable", "#9400D3"),
        new KeyValuePair<string, string>("chr", "#9400D3"),
        new KeyValuePair<string, string>("classmethod", "#9400D3"),
        new KeyValuePair<string, string>("compile", "#9400D3"),
        new KeyValuePair<string, string>("complex", "#9400D3"),
        new KeyValuePair<string, string>("delattr", "#9400D3"),
        new KeyValuePair<string, string>("dict", "#9400D3"),
        new KeyValuePair<string, string>("dir", "#9400D3"),
        new KeyValuePair<string, string>("divmod", "#9400D3"),
        new KeyValuePair<string, string>("enumerate", "#9400D3"),
        new KeyValuePair<string, string>("eval", "#9400D3"),
        new KeyValuePair<string, string>("exec", "#9400D3"),
        new KeyValuePair<string, string>("filter", "#9400D3"),
        new KeyValuePair<string, string>("float", "#9400D3"),
        new KeyValuePair<string, string>("format", "#9400D3"),
        new KeyValuePair<string, string>("frozenset", "#9400D3"),
        new KeyValuePair<string, string>("getattr", "#9400D3"),
        new KeyValuePair<string, string>("globals", "#9400D3"),
        new KeyValuePair<string, string>("hasattr", "#9400D3"),
        new KeyValuePair<string, string>("hash", "#9400D3"),
        new KeyValuePair<string, string>("help", "#9400D3"),
        new KeyValuePair<string, string>("hex", "#9400D3"),
        new KeyValuePair<string, string>("id", "#9400D3"),
        new KeyValuePair<string, string>("input", "#9400D3"),
        new KeyValuePair<string, string>("int", "#9400D3"),
        new KeyValuePair<string, string>("isinstance", "#9400D3"),
        new KeyValuePair<string, string>("issubclass", "#9400D3"),
        new KeyValuePair<string, string>("iter", "#9400D3"),
        new KeyValuePair<string, string>("len", "#9400D3"),
        new KeyValuePair<string, string>("list", "#9400D3"),
        new KeyValuePair<string, string>("locals", "#9400D3"),
        new KeyValuePair<string, string>("map", "#9400D3"),
        new KeyValuePair<string, string>("max", "#9400D3"),
        new KeyValuePair<string, string>("memoryview", "#9400D3"),
        new KeyValuePair<string, string>("min", "#9400D3"),
        new KeyValuePair<string, string>("next", "#9400D3"),
        new KeyValuePair<string, string>("object", "#9400D3"),
        new KeyValuePair<string, string>("oct", "#9400D3"),
        new KeyValuePair<string, string>("open", "#9400D3"),
        new KeyValuePair<string, string>("ord", "#9400D3"),
        new KeyValuePair<string, string>("pow", "#9400D3"),
        new KeyValuePair<string, string>("print", "#9400D3"),
        new KeyValuePair<string, string>("property", "#9400D3"),
        new KeyValuePair<string, string>("range", "#9400D3"),
        new KeyValuePair<string, string>("repr", "#9400D3"),
        new KeyValuePair<string, string>("reversed", "#9400D3"),
        new KeyValuePair<string, string>("round", "#9400D3"),
        new KeyValuePair<string, string>("set", "#9400D3"),
        new KeyValuePair<string, string>("setattr", "#9400D3"),
        new KeyValuePair<string, string>("slice", "#9400D3"),
        new KeyValuePair<string, string>("sorted", "#9400D3"),
        new KeyValuePair<string, string>("staticmethod", "#9400D3"),
        new KeyValuePair<string, string>("str", "#9400D3"),
        new KeyValuePair<string, string>("sum", "#9400D3"),
        new KeyValuePair<string, string>("super", "#9400D3"),
        new KeyValuePair<string, string>("tuple", "#9400D3"),
        new KeyValuePair<string, string>("type", "#9400D3"),
        new KeyValuePair<string, string>("vars", "#9400D3"),
        new KeyValuePair<string, string>("zip", "#9400D3"),
        new KeyValuePair<string, string>("__import__", "#9400D3")
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
        currentTimeSeconds = 61;
        currentTimeMinutes = timeLimitMinutes;
        CreatePuzzles();
        //SpawnPuzzleObjects(puzzleObjectTransforms);
        levelName = "Variables"; // REMOVE
        startingDirectoryPath = "Assets/Resources/Variables"; // REMOVE 
        CreateFileSystem();

        CreateTerminal();

        CreateHub();
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

    private void CreateFileSystem()
    {
        // Inititialize directories

        string currentDirectoryPath = startingDirectoryPath;

        directoryRoot = FillDirectories(currentDirectoryPath, 0, null);
        currentDirectory = directoryRoot;
        directoryRoot.hasParent = false;
    }

    private DirectoryData FillDirectories(string currentDirectoryPath, int directoryIndex, DirectoryData parentDir)
    {
        int newDirectoryIndex = directoryIndex + 1;
        DirectoryData dirData = new DirectoryData();
        string[] dirPathSplit = currentDirectoryPath.Split('/');

        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/DirectoryInfoList");
        string data = jsonData.text;
        List<JsonDirectoryObject> jsonDirInfo = JsonConvert.DeserializeObject<List<JsonDirectoryObject>>(data);

        dirData.path = dirPathSplit[dirPathSplit.Length - 1];
        string[] dirNames = dirData.path.Split('\\');
        dirData.dirName = dirNames[dirNames.Length - 1];
        dirData.parentDir = parentDir;

        // Add information if found in `DirectoryInfoList`
        foreach (JsonDirectoryObject dirObject in jsonDirInfo)
        {

            if (dirData.dirName == dirObject.name)
            {
                dirData.unlocked = false;
            }
        }

        // Fill directory with files if there are files
        if (Directory.GetFiles(currentDirectoryPath).Length > 0)
        {
            FillDirectoryWithFiles(dirData, currentDirectoryPath);
        }


        string[] dirsInCurrentDir = Directory.GetDirectories(currentDirectoryPath);
        foreach(string dirName in dirsInCurrentDir)
        {
            dirData.directories.Add(FillDirectories(dirName, newDirectoryIndex, dirData));
        }

        return dirData;
    }

    private void FillDirectoryWithFiles(DirectoryData dirData, string currentDirectoryPath)
    {
        string[] files = Directory.GetFiles(currentDirectoryPath);
        TextAsset jsonData = Resources.Load<TextAsset>("JsonData/FileInfoList");
        string data = jsonData.text;
        List<JsonFileObject> jsonFileInfo = JsonConvert.DeserializeObject<List<JsonFileObject>>(data);

        // Loop through files 
        foreach(string fileName in files)
        {
            // Check if not meta file
            if (!fileName.Contains(".meta"))
            {
                string[] fileNameSplit = fileName.Split('\\');
                //Debug.Log(fileName);
                FileData newFile = new FileData();
                newFile.fileName = fileNameSplit[fileNameSplit.Length - 1];
                newFile.path = fileName;

                // add current file to current directory
                dirData.files.Add(newFile);

                // Add information if found in `FileInfoList`
                foreach(JsonFileObject fileObject in jsonFileInfo)
                {
                    if(newFile.fileName == fileObject.name)
                    {
                        newFile.unlocked = false;
                        newFile.unlockKeyword = fileObject.unlockKeyword;
                        newFile.question = fileObject.question;
                    }
                }
            }
        }

    }

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
    /// Activates the puzzle associated with a given keyword, if one exists.
    /// </summary>
    public static bool UnlockPuzzle(string keyword)
    {
        // Activate any puzzles that has the parameter keyword as the unlockKeyword and return true
        foreach(GameObject puzzleObject in puzzles)
        {
            Puzzle puzzleComponent = puzzleObject.GetComponent<Puzzle>();

            if(keyword == puzzleComponent.unlockKeyword)
            {
                puzzleIndex++;
                if(puzzleIndex >= puzzles.Count)
                {
                    GameWon();
                }
                
                return true;
            }
        }

        // If no key word was found, return false
        return false;
    }

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

        // Check if game won
        if(puzzleIndex >= puzzles.Count)
        {
            GameWon();
        }
    }

    // ========================= HUB ========================= //

    private void CreateHub()
    {
        hubObject = Instantiate(hubPrefab);
        hub = hubObject.GetComponent<Hub>();
        hub.Initialize(levelName);
    }

    // ========================= Game Management ========================= //

    private static void GameWon()
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
