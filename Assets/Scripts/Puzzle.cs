using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using System.Threading;

// The TestCase class represents a test case for a puzzle.
[Serializable]
public class TestCase
{
    public string problem;
    public string answer;

    public TestCase()
    {
        // LEAVE EMPTY
    }

    public TestCase(string problem, string answer)
    {
        this.problem    = problem;
        this.answer     = answer;
    }

    public TestCase(TestCase testCase)
    {
        this.problem    = testCase.problem;
        this.answer     = testCase.answer;
    }
}

/// <summary>
/// Represents a puzzle in the game.
/// Each puzzle contains information about the puzzle's code, test cases, and clues.
/// The class provides methods to handle user input, run tests, display the puzzle, and show clues.
/// </summary>
[System.Serializable]
public class Puzzle : Emulator
{
    // Puzzle Data
    [JsonProperty("testCases")]
    public List<TestCase> testCases;
    [JsonProperty("hiddenTestCases")]
    public List<TestCase> hiddenTestCases;
    [JsonProperty("startingCode")]
    public string startingCode;
    [JsonProperty("directions")]
    public string directions;
    [JsonProperty("puzzleName")]
    public string puzzleName;
    [JsonProperty("puzzleIndex")]
    public int puzzleIndex;

    private string oldCode;

    // UI Components
    public TMP_Text directionsDisplay;

    public Puzzle(List<TestCase> testCases, List<TestCase> hiddenTestCases, string startingCode, string directions, string puzzleName, int puzzleIndex)
    {
        this.testCases          = new List<TestCase>(testCases);
        this.hiddenTestCases    = new List<TestCase>(hiddenTestCases);
        this.startingCode       = startingCode;
        this.directions         = directions;
        this.puzzleName         = puzzleName;
        this.puzzleIndex        = puzzleIndex;
    }

    public void Initialize(Puzzle puzzleData)
    {
        this.testCases          = new List<TestCase>(puzzleData.testCases);
        this.hiddenTestCases    = new List<TestCase>(puzzleData.hiddenTestCases);
        this.startingCode       = puzzleData.startingCode;
        this.directions         = puzzleData.directions;
        this.puzzleIndex        = puzzleData.puzzleIndex;
        this.puzzleName         = puzzleData.puzzleName;

        pythonFunctions = new List<string>();
        pythonFinishedEvent = new ManualResetEvent(false);

        inputText.Add("");

        codeFieldWidth = codeScrollRect.rect.width;
        codeFieldHeight = codeScrollRect.rect.height - verticalBuffer;
        verticalViewTop = 0;
        verticalViewBottom = codeFieldHeight;

        // Mouse input set up
        SetFontSize(false);
        widthDisplay.text = "a";
        widthDisplay.ForceMeshUpdate();
        charWidth = widthDisplay.preferredWidth;
        charHeight = widthDisplay.textInfo.lineInfo[0].lineHeight;

        // Get old code ("" if no old code avaliable)
        oldCode = GetOldCode();

        // Set text
        SetPuzzleDisplay();
        SetFontSize();

        // Create Notes
        pythonNotes.Initialize();
    }


    /// <summary>
    /// Updates the input handling and executes corresponding actions based on the user's input.
    /// </summary>
    void Update()
    {
        // Check for change in input field scroll position
        Vector2 newInputPosition = coloredCodeRect.anchoredPosition;
        if (lastInputPosition != newInputPosition)
        {
            // Update last input position and trigger scroll event
            lastInputPosition = newInputPosition;
            OnScroll(newInputPosition);
        }

        if (Input.anyKey)
        {
            // Check if input is down arrow
            if (Input.GetKeyDown(KeyCode.DownArrow) && !processingInput)
            {
                // Check if swap lines (ALT + Down arrow keys)
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    StartCoroutine(SwapLineBelowWithDelay());
                else
                    StartCoroutine(HandleDownArrowWithDelay());
            }
            // Check if input is up arrow
            if (Input.GetKeyDown(KeyCode.UpArrow) && !processingInput)
            {
                // Check if swap lines (ALT + Up arrow keys)
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    StartCoroutine(SwapLineAboveWithDelay());
                else
                    StartCoroutine(HandleUpArrowWithDelay());
            }
            // Check if input is left arrow
            if (Input.GetKeyDown(KeyCode.LeftArrow) && !processingInput)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    StartCoroutine(HandleCtrlLeftArrowWithDelay());
                }
                else
                {
                    StartCoroutine(HandleLeftArrowWithDelay());
                }
            }
            // Check if input is right arroww
            if (Input.GetKeyDown(KeyCode.RightArrow) && !processingInput)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    StartCoroutine(HandleCtrlRightArrowWithDelay());
                }
                else
                {
                    StartCoroutine(HandleRightArrowWithDelay());
                }
            }
            // Check if input is return
            if (Input.GetKeyDown(KeyCode.Return) && !processingInput)
            {
                StartCoroutine(HandleReturnWithDelay());
            }
            // Check if input is backspace
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (!processingInput)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        StartCoroutine(HandleCtrlBackspaceWithDelay());
                    }
                    else
                    {
                        StartCoroutine(HandleBackspaceWithDelay());
                    }
                }
            }
            // Check if input is tab
            else if (Input.GetKeyDown(KeyCode.Tab) && !processingInput)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    StartCoroutine(HandleShiftTabWithDelay());
                }
                else
                {
                    StartCoroutine(HandleTabWithDelay());
                }
            }
            // Check if duplicate (CRTL + D)
            else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.D) && !processingInput)
            {
                StartCoroutine(HandleDuplicateWithDelay());
            }
            // If not a command and not processing input
            else if (!processingInput)
            {
                // Handle text input
                foreach (char inputChar in Input.inputString)
                {
                    HandleTextInput(inputChar);
                }
            }

            

        }
        //coloredScrollRect.verticalNormalizedPosition = normalizedPos;
    }

    // ========================= Puzzle Code ========================= //

    /// <summary>
    /// Sets up the puzzle display with the initial code, input field text, directions, and unsolved image.
    /// </summary>
    public void SetPuzzleDisplay()
    {
        List<string> startingCodeList = new List<string>();
        // Set starting code
        // Set input field text
        if(oldCode == "")
        {
            inputText = Resources.Load<TextAsset>(startingCode).text.Split('\n').ToList();
        }
        else
        {
            inputText = oldCode.Split('\n').ToList();
        }

        for(int index = 0; index < inputText.Count; index++)
        {
            CreateNewLineCover(index);
        }


        coloredText = new List<string>();

        caretPosY = 0;
        caretPosX = 0;
        numOfLines = 0;

        for(int index = 0; index < inputText.Count; index++)
        {
            string line = inputText[index].Replace("\n", "");
            line = line.Replace("\r", "");
            inputText[index] = line;
            coloredText.Add(line);
            ColorizeCurrentLine(false);
            caretPosY++;
            numOfLines++;
        }

        caretPosY = 0;
        ColorizeCurrentLine(true);
        DisplayText();
        SetLineNumbers();

        // Set directions text
        directionsDisplay.text = "\n" + Resources.Load<TextAsset>(directions).text;
    }

    /// <summary>
    /// Runs the tests for the puzzle, compares the output to the expected answers, and prints the results to the emulator console.
    /// If all tests pass, calls the ShowClue method to display the clue image.
    /// </summary>
    async public void RunTests()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        bool testFailed = false;
        string pythonCode = string.Join("\n", inputText);
        string testOutput = "";

        // Loop through test cases and print results
        for(int testIndex = 0; testIndex < testCases.Count; testIndex++)
        {
            string input = testCases[testIndex].problem;
            string ans = testCases[testIndex].answer;

            // Append test case to end of python file and convert the result to a string
            await RunPythonCodeWithTimeout(pythonCode + "\nstr(main("+input+"))");

            // Check the python code result to test case answer
            if(pythonResult.ToString() == ans)
            {
                if(input != "")
                    testOutput += "<color=#00AB66>Input: " + input + ", Correct Output: " + ans + ", Output: " + pythonResult + "</color>\n";
                else 
                    testOutput += "<color=#00AB66>Correct Output: " + ans + ", Output: " + pythonResult + "</color>\n";
            }
            else
            {
                if(input != "")
                    testOutput += "<color=#CC0000>Input: " + input + ", Correct Output: " + ans + ", Output: " + pythonResult + "</color>\n";
                else 
                    testOutput += "<color=#CC0000>Correct Output: " + ans + ", Output: " + pythonResult + "</color>\n";
                testFailed = true;
            }

            WriteToEmuConsole(testOutput);
        }

        // Loop through hidden test cases
        for(int testIndex = 0; testIndex < hiddenTestCases.Count; testIndex++)
        {
            string input = hiddenTestCases[testIndex].problem;
            string ans = hiddenTestCases[testIndex].answer;

            // Append test case to end of python file and convert the result to a string
            await RunPythonCodeWithTimeout(pythonCode + "\nstr(main(" + input + "))");

            // Check the python code result to test case answer
            if (pythonResult.ToString() == ans)
            {
                testOutput += "<color=#00AB66>Hidden Test Passed</color>\n";
            }
            else
            {
                testOutput += "<color=#CC0000>Hidden Test Failed</color>\n";
                testFailed = true;
            }

        }

        // Check if all tests passed
        if(!testFailed)
        {
            testOutput += $"<color=#00AB66>directory {puzzleName} is now unlocked. \n\nExit to terminal and use command `cd {puzzleName}` to move into the directory</color>";
            AudioManager.instance.PlaySuccessSoundEffect();
            GameManager.instance.PuzzleSolved(this.puzzleName);
        }
        else
        {
            testOutput += $"<color=#CC0000>directory {puzzleName} is still locked. \n\nEdit your code and try again!";
            AudioManager.instance.PlayErrorSoundEffect();
        }

        WriteToEmuConsole(testOutput);
    }

    /// <summary>
    /// Resets the puzzle by calling the SetPuzzleDisplay method.
    /// </summary>
    public void ResetPuzzle()
    {
        oldCode = "";
        AudioManager.instance.PlayButtonClickSoundEffect();
        SetPuzzleDisplay();
    }

    // ========================= Helper Functions ========================= //

    /// <summary>
    /// Exits the puzzle UI by seting the puzzle's state to inactive
    /// and sets the terminal UI to active
    /// </summary>
    public void HidePuzzle()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        gameObject.SetActive(false);
        GameManager.terminal.ShowTerminal();
        WriteToPuzzleJson();
    }

    public void ShowPuzzle()
    {
        SetFontSize();
        gameObject.SetActive(true);
    }

    public void SetFontSize(bool setColors = true)
    {
        emuConsole.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CONSOLE_FONT_SIZE, 15);
        directionsDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.DIRECTIONS_FONT_SIZE, 15);
        widthDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        coloredCodeDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        lineNumDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        pythonNotes.setFontSize(PlayerPrefs.GetFloat(PlayerPrefNames.DIRECTIONS_FONT_SIZE, 15));

        widthDisplay.text = "a";
        widthDisplay.ForceMeshUpdate();
        charWidth = widthDisplay.preferredWidth;
        charHeight = widthDisplay.textInfo.lineInfo[0].lineHeight;

        Vector3 parentContainerPos = lineScrollRect.GetComponent<RectTransform>().anchoredPosition;
        parentContainerPos.y = -157.96f - charHeight;
        lineScrollRect.GetComponent<RectTransform>().anchoredPosition = parentContainerPos;


        Vector2 cellSize = lineCoverGroup.cellSize;
        cellSize.y = charHeight;
        lineCoverGroup.cellSize = cellSize;

        if (setColors)
        {
            SetColorSize(setColors);
        }
    }

    private void WriteToPuzzleJson()
    {
        // Get puzzle data from Json
        string data = "";
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        // Check if persistent data of puzzles exist
        if (File.Exists(persistentDataPath))
        {
            data = File.ReadAllText(persistentDataPath);
        }
        // Otherwise, get puzzle data from resources folder
        else
        {
            // Quit to Main Menu
            GameManager.instance.MoveToMainMenu();
            return;
        }

        List<LevelInfo> levelDataList = JsonConvert.DeserializeObject<List<LevelInfo>>(data);


        levelDataList[GameManager.levelIndex].puzzles[puzzleIndex].oldCode = string.Join("\n", inputText);

        
        // Serialize the updated list to a JSON string
        string updatedData = JsonConvert.SerializeObject(levelDataList, Formatting.Indented);

        File.WriteAllText(persistentDataPath, updatedData);
    }

    private string GetOldCode()
    {
        string data = "";
        string persistentDataPath = Path.Combine(Application.persistentDataPath, GameManager.persistentPuzzleFile);

        // Check if persistent data of puzzles exist
        if (File.Exists(persistentDataPath))
        {
            data = File.ReadAllText(persistentDataPath);
        }
        // Otherwise, get puzzle data from resources folder
        else
        {
            // Quit to Main Menu
            GameManager.instance.MoveToMainMenu();
            return "";
        }

        List<LevelInfo> levelDataList = JsonConvert.DeserializeObject<List<LevelInfo>>(data);

        return levelDataList[GameManager.levelIndex].puzzles[puzzleIndex].oldCode;

    }

}
