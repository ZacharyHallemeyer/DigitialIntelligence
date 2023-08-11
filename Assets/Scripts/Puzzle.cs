using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Text;
using IronPython.Hosting;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using TMPro;
using Microsoft.Scripting.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp;


// The PuzzleDataList class represents a serializable collection of Puzzle objects.
[Serializable]
public class PuzzleDataList
{
    public List<Puzzle> puzzles;
}

// The PuzzleData class holds data for a specific puzzle.
[Serializable]
public class PuzzleData
{
    public List<TestCase> testCases;
    public List<TestCase> hiddenTestCases;
    public string clueImagePath;
    public string startingCode;
    public string directions;
    public string puzzleType;
    public string unlockKeyword;
    public int puzzleIndex;
}

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
public class Puzzle : MonoBehaviour
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

    public int puzzleIndex;

    // UI Components
        // Containers
    public GameObject directionsContainer;
    public RectTransform viewport;
    public ScrollRect coloredScrollRect;

    // Transforms
    public RectTransform lineNumberRect;
    public RectTransform coloredCodeRect;
    public RectTransform codeScrollRect;
        // Text Components
    public TMP_Text coloredCodeDisplay; // This text component displays the colored text
    public TMP_Text lineNumDisplay;
    public TMP_Text emuConsole;
    public TMP_Text directionsDisplay;
    public TMP_Text widthDisplay;

        // Buttons
    public Button clearConsoleButton;
    public Button runTestsButton;
    public Image runButtonImage;
    public Button runButton;
    public Image runTestsButtonImage;
    public Image clearConsoleButtonImage;

    public Image directionsButtonImage;

    // Emulator variables
    public bool isChangingCode = false;
    public Vector2 lastInputPosition;
    public bool recolorAllText = false;
    public int lineMarginY = -145;
    public List<string> pythonFunctions = new List<string>();
    private Thread pythonThread;
    private ManualResetEvent pythonFinishedEvent = new ManualResetEvent(false);

    public List<string> inputText;
    public List<string> coloredText;

    public int caretPosX = 0;
    public int caretPosY = 0;

    public float codeFieldWidth;
    public float codeFieldHeight;
    public float verticalViewTop;
    public float verticalViewBottom;

    public bool processingInput = false;
    public float initialProcessingDelay = .05f;
    public float lineHeight;
    public int scrollPadding;
    public float processingDelay = .01f;

    // Puzzle variables
    public object pythonResult;
    public string pythonOutput;
    public string unsolvedImagePath = "PuzzleImages/Unsolved";

    // Colors
    public string caret = "<color=#0000FF>|</color>";
    public string keywordColor;
    public string functionColor = "#00FFFF";
    public string stringColor;

    public float verticalBuffer = 230;
    public int lineHeightScaler = 1;

    public Puzzle(List<TestCase> testCases, List<TestCase> hiddenTestCases, string clueImagePath, string startingCode, string directions, string puzzleType, string unlockKeyword, string puzzleName, int puzzleIndex)
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

        // Set text
        SetPuzzleDisplay();
        SetFontSize();
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
                StartCoroutine(HandleTabWithDelay());
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
    public float normalizedPos = 0;

    /// <summary>
    /// OnScroll is called when the input field is scrolled.
    /// It updates the position of the line numbers to match the scroll position.
    /// </summary>
    /// <param name="scrollPosition">The new scroll position.</param>
    private void OnScroll(Vector2 scrollPosition)
    {
        lineNumberRect.anchoredPosition = new Vector2(lineNumberRect.anchoredPosition.x, scrollPosition.y - 150);
    }

    // ========================= Python Emulator ========================= //

    /// <summary>
    /// Handles the Up Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleUpArrowWithDelay()
    {
        processingInput = true;

        HandleUpArrow();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.UpArrow))
        {
            HandleUpArrow();
            yield return new WaitForSeconds(processingDelay);
        }

        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Up Arrow key is pressed to navigate to the previous line.
    /// </summary>
    private void HandleUpArrow()
    {
        if (caretPosY - 1 >= 0)
        {
            // Remove caret from line
            RemoveCaretFromLine(caretPosY);
            caretPosY--;

            if (caretPosX > inputText[caretPosY].Length)
            {
                if (inputText[caretPosY].Length == 0)
                    caretPosX = 0;
                else
                    caretPosX = inputText[caretPosY].Length;
            }
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Down Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleDownArrowWithDelay()
    {
        processingInput = true;

        HandleDownArrow();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.DownArrow))
        {
            HandleDownArrow();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    private void HandleDownArrow()
    {
        // Check if there is a line below current line
        if (caretPosY + 1 < inputText.Count)
        {
            // Remove caret from line
            RemoveCaretFromLine(caretPosY);
            // Move to line down
            caretPosY++;

            // Check if current x position for caret is more than the length of new line
            if (caretPosX > inputText[caretPosY].Length)
            {
                // Move to index 0 if there is no text on new line
                if (inputText[caretPosY].Length == 0)
                    caretPosX = 0;
                // Move to end of current line
                else
                    caretPosX = inputText[caretPosY].Length;
            }
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Left Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleLeftArrowWithDelay()
    {
        processingInput = true;

        HandleLeftArrow();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.LeftArrow))
        {
            HandleLeftArrow();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Left Arrow key is pressed to navigate to the left character position within a line.
    /// If at the beginning of a line, it moves to the end of the previous line (if available).
    /// </summary>
    private void HandleLeftArrow()
    {
        // Check if moving left will be within bounds
        if (caretPosX - 1 >= 0)
        {
            caretPosX--;
        }
        // Otherwise
        else  if (caretPosY - 1 >= 0)
        {
            // Move to the end up the current line if the current line is not the first line
            RemoveCaretFromLine(caretPosY);
            caretPosY--;
            caretPosX = inputText[caretPosY].Length - 1;
            if (caretPosX < 0)
                caretPosX = 0;
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the CRTL+Left Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleCtrlLeftArrowWithDelay()
    {
        processingInput = true;

        HandleCtrlLeftArrow();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.LeftArrow))
        {
            HandleCtrlLeftArrow();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Ctrl + Left Arrow keys are pressed to navigate to the previous word position within a line.
    /// If at the beginning of a word, it moves to the end of the previous word (if available).
    /// If at the beginning of a line, it moves to the end of the previous line (if available).
    /// </summary>
    private void HandleCtrlLeftArrow()
    {
        // Check if moving left will be out of bounds
        if (caretPosX - 1 < 0)
        {
            HandleLeftArrow();
            return;
        }

        // Find next space to the left and move to it
        int oldPos = caretPosX;
        caretPosX = inputText[caretPosY].LastIndexOf(' ', caretPosX - 1);

        // If there is no space found to the left
        if (caretPosX == -1)
        {
            // move to next tab on left 
            caretPosX = inputText[caretPosY].LastIndexOf('\t', oldPos - 1);

            // If there is no tab found to the left, move to start of line
            if (caretPosX == -1)
            {
                caretPosX = 0;
            }
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Right Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleRightArrowWithDelay()
    {
        processingInput = true;

        HandleRightArrow();

        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.RightArrow))
        {
            HandleRightArrow();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Right Arrow key is pressed to move the caret one position to the right within the current line.
    /// If at the end of a line, it moves to the beginning of the next line (if available).
    /// </summary>
    private void HandleRightArrow()
    {
        // Check if moving right will be within range
        if (caretPosX + 1 <= inputText[caretPosY].Length)
        {
            // Move right
            caretPosX++;
        }
        /*
        // Fixes weird but where the last line has one less of a length than it should have
        else if (caretPosY == inputText.Count - 1)
        {
            Debug.Log("Right, but bottom line");
            caretPosX = inputText[caretPosY].Length;
        }
        */
        // Otherwise, Move to line down if there is a line to move to
        else if (caretPosY + 1 <  inputText.Count)
        {
            // Move to the next line and set caret to the start of the line
            RemoveCaretFromLine(caretPosY);
            caretPosY++;
            caretPosX = 0;
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the CRTL+Right Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleCtrlRightArrowWithDelay()
    {
        processingInput = true;

        HandleCtrlRightArrow();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.RightArrow))
        {
            HandleCtrlRightArrow();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Ctrl + Right Arrow keys are pressed to move the caret one word to the right within the current line.
    /// If at the end of a line, it moves to the beginning of the next line (if available).
    /// </summary>
    private void HandleCtrlRightArrow()
    {
        // Check if moving right will be out of range
        if (caretPosX + 1 >= inputText[caretPosY].Length)
        {
            // Move to next line
            HandleRightArrow();
            return;
        }

        // Find next space
        int oldPos = caretPosX;
        caretPosX = inputText[caretPosY].IndexOf(' ', caretPosX+1);

        // If there is no space found, move to the end of line
        if (caretPosX == -1)
        {
            // move to next tab on right 
            caretPosX = inputText[caretPosY].IndexOf('\t', oldPos + 1);

            // If there is no tab found to the right, move to end of line
            if (caretPosX == -1)
            {
                caretPosX = inputText[caretPosY].Length - 1;
            }

        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Return key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleReturnWithDelay()
    {
        processingInput = true;

        HandleReturn();

        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.Return))
        {
            HandleReturn();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Return key is pressed to create a new line.
    /// Inserts new lines in the input text and updates the caret position accordingly.
    /// </summary>
    private void HandleReturn()
    {
        // Get indent of current line
        string indent = GetIndent(inputText[caretPosY]);

        // Add new lines
        inputText.Insert(caretPosY + 1, indent);
        coloredText.Insert(caretPosY + 1, indent);

        ColorizeCurrentLine(false);
        DisplayText();

        // Check if any text will be carried to the next line
        if (inputText[caretPosY].Length > caretPosX)
        {
            // Move the text to the next line
            inputText[caretPosY + 1] = inputText[caretPosY].Substring(caretPosX);
            inputText[caretPosY] = inputText[caretPosY].Substring(0, caretPosX);
            ColorizeCurrentLine(false);
            DisplayText();
        }

        // Set caret position
        caretPosY++;
        caretPosX = 0 + indent.Length;

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the CRTL+Backspace key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleCtrlBackspaceWithDelay()
    {
        processingInput = true;

        HandleCtrlBackspace();

        yield return new WaitForSeconds(initialProcessingDelay);
        while ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Backspace))
        {
            HandleCtrlBackspace();
            yield return new WaitForSeconds(processingDelay);
        }

        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Ctrl + Backspace key combination is pressed.
    /// Deletes the word to the left of the caret position or performs a backspace if at the beginning of the line.
    /// </summary>
    private void HandleCtrlBackspace()
    {
        // Check if backspace is at the start of the line
        if (caretPosX == 0)
        {
            HandleBackspace();
            return;
        }

        // Find next space 
        int deleteIndex = inputText[caretPosY].LastIndexOf(' ', caretPosX - 1 >= 0 ? caretPosX - 1 : 0);

        // If there is no space found, set deleteIndex to the start of the line
        if (deleteIndex == -1)
            deleteIndex = 0;

        // Remove from caret position to delete index (either index of a space or the start of a line)
        inputText[caretPosY] = inputText[caretPosY].Remove(deleteIndex, caretPosX - deleteIndex);
        caretPosX = deleteIndex;


        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Backspace key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleBackspaceWithDelay()
    {
        processingInput = true;

        HandleBackspace();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.Backspace))
        {
            HandleBackspace();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Backspace key is pressed.
    /// Deletes the character to the left of the caret position or merges the line with the previous line if at the beginning of a line (except for the first line).
    /// </summary>
    private void HandleBackspace()
    {
        caretPosX--;

        // Check if this backspace is at the beginning of a line that is not the first line 
        if (caretPosX < 0)
        {
            // Set caret x position to 0
            caretPosX = 0;
            // Check if current line is not the first line
            if (caretPosY > 0)
            {
                // Add the text of the current line to the last line
                inputText[caretPosY - 1] += inputText[caretPosY];
                coloredText[caretPosY - 1] += coloredText[caretPosY];
                // Remove current line and decrement caret Y position
                inputText.RemoveAt(caretPosY);
                coloredText.RemoveAt(caretPosY);
                caretPosY--;

                // Set cursor X to the end of the current line
                caretPosX = inputText[caretPosY].Length;

                ColorizeCurrentLine(true);
                DisplayText();

                return;
            }

            return;
        }

        // Remove the character at the caret
        inputText[caretPosY] = inputText[caretPosY].Remove(caretPosX, 1);
        coloredText[caretPosY] = inputText[caretPosY];

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the Tab key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleTabWithDelay()
    {
        processingInput = true;
        HandleTab();
        yield return new WaitForSeconds(initialProcessingDelay);
        while (Input.GetKey(KeyCode.Tab))
        {
            HandleTab();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the behavior when the Tab key is pressed.
    /// Inserts a tab character ('\t') at the caret position.
    /// </summary>
    private void HandleTab()
    {
        HandleTextInput('\t');
    }

    /// <summary>
    /// Handles the CRTL+D (duplicate line) input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator HandleDuplicateWithDelay()
    {
        processingInput = true;

        HandleDuplicate();

        yield return new WaitForSeconds(initialProcessingDelay);
        while ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.D))
        {
            HandleDuplicate();
            yield return new WaitForSeconds(processingDelay);
        }
        processingInput = false;
    }

    /// <summary>
    /// Handles the duplication of the current line.
    /// Inserts a new line with the same content as the current line below the current line,
    /// and moves the caret to the duplicated line.
    /// </summary>
    private void HandleDuplicate()
    {
        // Add new lines with the same text as the current line
        inputText.Insert(caretPosY + 1, inputText[caretPosY]);
        coloredText.Insert(caretPosY + 1, inputText[caretPosY]);

        // Remove caret from line
        RemoveCaretFromLine(caretPosY);
        DisplayText();

        caretPosY++;
        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the ALT+Up Arrow (Swap line above) key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator SwapLineAboveWithDelay()
    {
        processingInput = true;

        SwapLineAbove();

        yield return new WaitForSeconds(initialProcessingDelay);
        while ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKey(KeyCode.UpArrow))
        {
            SwapLineAbove();
            yield return new WaitForSeconds(processingDelay);
        }

        processingInput = false;
    }

    /// <summary>
    /// Swaps the current line with the line above it.
    /// If the current line is not the first line, it exchanges the content of the current line with the line above it,
    /// adjusts the caret position and updates the display.
    /// </summary>
    private void SwapLineAbove()
    {
        // Check if there is a line above current line 
        if (caretPosY <= 0)
        {
            processingInput = false;
            return;
        }

        // Swap line between current line and the line above
        string tempString = inputText[caretPosY - 1];
        inputText[caretPosY - 1] = inputText[caretPosY];
        inputText[caretPosY] = tempString;

        ColorizeCurrentLine(false);
        RemoveCaretFromLine(caretPosY);
        caretPosY--;

        // Move caret to the end of the line if the current position is more than the length of the swapped line
        if (caretPosX >= inputText[caretPosY].Length)
        {
            caretPosX = inputText[caretPosY].Length - 1;
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the ALT+Down Arrow (Swap line below) key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    private IEnumerator SwapLineBelowWithDelay()
    {
        processingInput = true;

        SwapLineBelow();

        yield return new WaitForSeconds(initialProcessingDelay);
        while ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKey(KeyCode.DownArrow))
        {
            SwapLineBelow();
            yield return new WaitForSeconds(processingDelay);
        }

        processingInput = false;
    }

    /// <summary>
    /// Swaps the current line with the line below it.
    /// If the current line is not the last line, it exchanges the content of the current line with the line below it,
    /// adjusts the caret position and updates the display.
    /// </summary>
    private void SwapLineBelow()
    {
        // Check if there is a line below current line
        if (caretPosY >= inputText.Count - 1)
        {
            processingInput = false;
            return;
        }

        // Swap current line and line below
        string tempString = inputText[caretPosY + 1];
        inputText[caretPosY + 1] = inputText[caretPosY];
        inputText[caretPosY] = tempString;

        ColorizeCurrentLine(false);
        RemoveCaretFromLine(caretPosY);
        caretPosY++;

        // Move caret to the end of the line if the current position is more than the length of the swapped line
        if (caretPosX >= inputText[caretPosY].Length)
        {
            caretPosX = inputText[caretPosY].Length - 1;
        }

        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the input of a character and inserts it at the current caret position in the current line.
    /// It updates the input text, caret position, colors the current line, and updates the display.
    /// </summary>
    /// <param name="character">The character to be inserted.</param>
    private void HandleTextInput(char character)
    {
        // Insert character at caret and increment caret X position
        inputText[caretPosY] = inputText[caretPosY].Insert(caretPosX, character.ToString());

        caretPosX++;
        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Colorizes the current line of the input text based on syntax highlighting rules.
    /// It adds colors and formatting to different elements such as comments, strings, keywords, and defined functions.
    /// The caret position is optionally highlighted.
    /// </summary>
    /// <param name="addCaret">True to highlight the caret position, false otherwise.</param>
    private void ColorizeCurrentLine(bool addCaret)
    {
        // Set colored text to input text
        coloredText[caretPosY] = inputText[caretPosY];

        // Add caret if necessary
        if (addCaret)
        {
            if (caretPosX == coloredText[caretPosY].Length)
                coloredText[caretPosY] += caret;
            else
                coloredText[caretPosY] = coloredText[caretPosY].Insert(caretPosX, caret);
        }

        string coloredTextString = "";
        // Split current line by spaces into an array
        string[] words = coloredText[caretPosY].Split(' ');

        // Define colors for comments and strings
        string commentColor = "#808080"; // Grey

        bool wordAdded = false;
        bool isComment = false;
        bool isString = false;

        // Find defined functions in line if any
        FindDefinedFunctions(coloredText[caretPosY]);

        // Loop through words
        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            // Ger current word and remove caret if there is a caret
            string word = words[wordIndex];
            string wordNoCaret = word.Replace(caret, "");
            wordAdded = false;

            // Get index of caret 
            int caretIndex = word.IndexOf(caret);

            // Check if line is not a comment and if the word contains a complete string
            if (!isComment && word.Count(character => character == '\'') == 2 || word.Count(character => character == '\"') == 2)
            {
                int stringStartIndex = word.IndexOf('\'') != -1 ? word.IndexOf('\'') : word.IndexOf('\"');
                int stringEndIndex = word.LastIndexOf('\'') != -1 ? word.LastIndexOf('\'') : word.LastIndexOf('\"');

                // Insert color tags wrapping the string
                word = word.Insert(stringStartIndex, $"<color={stringColor}>");
                word = word.Insert(stringEndIndex + $"<color={stringColor}>".Length + 1, "</color>");

                coloredTextString += word;
                wordAdded = true;
            }
            // Check if line is not a comment or a string and contains quotation marks
            else if (!isComment && !isString && (word.Contains("'") || word.Contains("\"")))
            {
                // Set isString to true
                isString = true;
                // Insert starting color tag at quotation marks
                int startStringIndex = word.IndexOf('\'') != -1 ? word.IndexOf('\'') : word.IndexOf('\"');
                coloredTextString += $"{word.Substring(0, startStringIndex)}<color={stringColor}>{word.Substring(startStringIndex)} ";
                wordAdded = true;
            }
            // Check if line is not a comment, is a string, and contains a quotation mark
            else if (!isComment && isString && (word.Contains("'") || word.Contains("\"")))
            {
                // Set isString to false
                isString = false;
                // Close string color tag
                int endStringIndex = word.IndexOf('\'') != -1 ? word.IndexOf('\'') : word.IndexOf('\"');
                coloredTextString += $"{word.Substring(0, endStringIndex + 1)}</color>{word.Substring(endStringIndex + 1)} ";
                wordAdded = true;
            }
            // Check if line is a comment
            else if (!isComment && wordNoCaret.IndexOf("#") != -1)
            {
                // This is a comment, so color it grey
                isComment = true;
                int startCommentIndex = wordNoCaret.IndexOf("#");
                wordNoCaret = $"{wordNoCaret.Substring(0, startCommentIndex)}<color={commentColor}>{wordNoCaret.Substring(startCommentIndex)} ";

                // Check if caret out of word on the left
                if (caretIndex != -1 && caretIndex < startCommentIndex)
                {
                    wordNoCaret = wordNoCaret.Insert(caretIndex, caret);
                }
                else if (caretIndex != -1)
                {
                    wordNoCaret = wordNoCaret.Insert(caretIndex + "<color=#000000>".Length, caret);
                }

                coloredTextString += wordNoCaret;
                wordAdded = true;
            }
            // Check if the word is not a comment or a string
            else if (!isComment && !isString)
            {
                // Colorize defined functions 
                // Loop through known python functions
                for(int functionIndex = 0; !wordAdded && functionIndex < pythonFunctions.Count; functionIndex++)
                {
                    string functionWord = pythonFunctions[functionIndex];

                    // Check if caret is in word and check if the word is a function
                    if(caretIndex != -1 && wordNoCaret.Contains(functionWord)) {
                        wordAdded = true;
                        coloredTextString += AddColoredKeywordWithCaret(wordNoCaret, functionWord, functionColor, caretIndex);
                    }
                    // Check if word is a keyword
                    else if(wordNoCaret.Contains(functionWord))
                    {
                        wordAdded = true;
                        coloredTextString += AddColoredKeyword(wordNoCaret, functionWord, functionColor);
                    }
                }

                // Colorize Python Keywords
                // Loop through python keywords
                for (int keywordIndex = 0; !wordAdded && keywordIndex < GameManager.pythonKeyWords.Count; keywordIndex++)
                {
                    KeyValuePair<string, int> keyValue = GameManager.pythonKeyWords[keywordIndex];

                    string color = "";

                    if(keyValue.Value == (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION)
                    {
                        color = functionColor;
                    }
                    else
                    {
                        color = keywordColor;
                    }

                    // Check if caret is in word
                    if (caretIndex != -1)
                    {
                        // Check if word is a keyword
                        if (Regex.IsMatch(wordNoCaret, @"\b" + keyValue.Key + @"\b"))
                        {
                            wordAdded = true;
                            coloredTextString += AddColoredKeywordWithCaret(wordNoCaret, keyValue.Key, color, caretIndex);
                        }
                    }
                    // Check if word is a keyword
                    else if (Regex.IsMatch(wordNoCaret, @"\b" + keyValue.Key + @"\b"))
                    {
                        wordAdded = true;
                        coloredTextString += AddColoredKeyword(wordNoCaret, keyValue.Key, color);
                    }
                }
            }

            // If the word was not added, add the word without color tags
            if (!wordAdded)
            {
                coloredTextString += word + " ";
            }
        }

        // If the line is a string or comment, add an closing color tag
        if (isString || isComment)
            coloredTextString += "</color>";

        // Add a space at the start of each line
        if(caretPosY == 0)
            // Add an extra line on top if this line is the first line
            coloredText[caretPosY] = "\n " + coloredTextString;
        else 
            coloredText[caretPosY] = " " + coloredTextString;
    }

    /// <summary>
    /// Adds color formatting to the specified keyword within the given text string, including highlighting the caret position.
    /// The keyword is enclosed in a color tag with the specified hexadecimal color value.
    /// The caret position is optionally highlighted within the keyword.
    /// </summary>
    /// <param name="text">The text string to modify.</param>
    /// <param name="keyword">The keyword to format and highlight.</param>
    /// <param name="hexColor">The hexadecimal color value for the keyword.</param>
    /// <param name="caretIndex">The index of the caret position within the text.</param>
    /// <returns>The modified text string with the keyword formatted and the caret highlighted.</returns>
    private string AddColoredKeywordWithCaret(string text, string keyword, string hexColor, int caretIndex)
    {
        // Get start and end indexes of keyword
        int keywordStart = text.IndexOf(keyword);
        int keywordEnd = keywordStart + keyword.Length - 1;
        // set color tags around the keyword
        text = $"{ text.Substring(0, keywordStart) }<color={hexColor}>{text.Substring(keywordStart, keyword.Length)}</color>{text.Substring(keywordEnd + 1)} ";

        // Check if caret in word
        if (keywordStart <= caretIndex && keywordEnd >= caretIndex)
        {
            text = text.Insert(caretIndex + "<color=#000000>".Length, caret);
        }
        // Check if caret out of word on the right 
        else if (caretIndex >= keywordEnd)
        {
            text = text.Insert(caretIndex + "<color=#000000></color>".Length, caret);
        }
        // Check if caret out of word on the left
        else
        {
            text = text.Insert(caretIndex, caret);
        }

        return text;
    }

    /// <summary>
    /// Adds color formatting to the specified keyword within the given text string.
    /// The keyword is enclosed in a color tag with the specified hexadecimal color value.
    /// </summary>
    /// <param name="text">The text string to modify.</param>
    /// <param name="keyword">The keyword to format.</param>
    /// <param name="hexColor">The hexadecimal color value for the keyword.</param>
    /// <returns>The modified text string with the keyword formatted in the specified color.</returns>
    private string AddColoredKeyword(string text, string keyword, string hexColor)
    {
        // Get start and end of keyword
        int keywordStart = text.IndexOf(keyword);
        int keywordEnd = keywordStart + keyword.Length - 1;
        // Set color tags around keyword
        text = $"{ text.Substring(0, keywordStart) }<color={hexColor}>{text.Substring(keywordStart, keyword.Length)}</color>{text.Substring(keywordEnd + 1)} ";
        
        return text;
    }

    /// <summary>
    /// Removes the caret symbol from the specified line of colored text.
    /// </summary>
    /// <param name="lineIndex">The index of the line to remove the caret from.</param>
    private void RemoveCaretFromLine(int lineIndex)
    {
        // Remove caret from line   
        coloredText[lineIndex] = coloredText[lineIndex].Replace(caret, "");
    }

    /// <summary>
    /// Displays the colored text in the code display and adjusts the viewport.
    /// </summary>
    private void DisplayText()
    {
        // Set text component to text in coloredText
        coloredCodeDisplay.text = string.Join("\n", coloredText);
        FollowCaret();
    }

    /// <summary>
    /// Finds and stores the names of defined functions in the given text.
    /// </summary>
    /// <param name="text">The text to search for defined functions.</param>
    private void FindDefinedFunctions(string text)
    {
        // Remove caret
        text = text.Replace(caret, "");

        string pattern = @"def\s+(\w+)\s*\((.*?)\)\s*:";

        MatchCollection matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            if(!pythonFunctions.Contains(match.Groups[1].Value))
            {
                pythonFunctions.Add(match.Groups[1].Value);
            }
        }

    }

    /// <summary>
    /// Clears the emulator console by writing an empty string.
    /// </summary>
    public void ClearConsole()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        WriteToEmuConsole("");
    }

    /// <summary>
    /// Gets the Python code from the input field, calls the `RunPythonCodeWithTimeout` method,
    /// and writes the output to the emulator console once the Python code has finished or been aborted.
    /// </summary>
    public async void RunPython()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        // Get python code from input text
        string pythonCode = string.Join("\n", inputText);
        // Execute python 
        await RunPythonCodeWithTimeout(@"" + pythonCode);
        // Write python output to console
        WriteOutputToEmuConsole();
    }

    /// <summary>
    /// Calls the `PythonTime` method as a thread and waits for 5 seconds for it to finish.
    /// If the `PythonTime` method does not finish within 5 seconds, the thread is aborted.
    /// </summary>
    /// <param name="pythonCode">The Python code to execute.</param>
    private async Task RunPythonCodeWithTimeout(string pythonCode)
    {
        // Run python code
        await Task.Run(() =>
        {
            // Start python execution thread and timeout
            pythonFinishedEvent.Reset();
            pythonThread = new Thread(() => PythonTime(pythonCode));
            pythonThread.Start();

            // Wait for 5 seconds for python thread to finish
            if (!pythonFinishedEvent.WaitOne(5000))  // Wait for up to 5 seconds
            {
                pythonThread.Abort();  // Abort the thread if it's still running
                pythonOutput = "Python code execution was cancelled due to timeout.";
            }
        });
    }

    /// <summary>
    /// Executes the Python code and sets `pythonResult` and `pythonOutput` to the result and output of the Python execution.
    /// </summary>
    /// <param name="pythonCode">The Python code to execute.</param>
    private void PythonTime(string pythonCode)
    {
        // Create iron python variables
        var engine = Python.CreateEngine();
        dynamic scope = engine.CreateScope();
        var streamOutput = new MemoryStream();
        engine.Runtime.IO.SetOutput(streamOutput, Encoding.Default);

        try
        {
            // Execute code and store result and output
            pythonResult = engine.Execute(pythonCode, scope);
            pythonOutput = Encoding.Default.GetString(streamOutput.ToArray());
        }
        catch (Exception error)
        {
            // Set python output to error message if there was an error in execution
            pythonOutput = "<color=#FF0000>" + engine.GetService<ExceptionOperations>().FormatException(error) + "</color>";
            // Don't re-throw the exception because we're in a separate thread
        }
        finally
        {
            pythonFinishedEvent.Set();  // Signal that the Python code execution has finished (either successfully or due to an error)
        }
    }

    /// <summary>
    /// Writes the contents of `pythonOutput` to the emulator console.
    /// </summary>
    private void WriteOutputToEmuConsole()
    {
        WriteToEmuConsole(pythonOutput);
    }

    /// <summary>
    /// Writes the specified string to the emulator console.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    private void WriteToEmuConsole(string text)
    {
        emuConsole.text = "\n" + text;
        emuConsole.ForceMeshUpdate(true);
    }

    // ========================= Puzzle Code ========================= //

    /// <summary>
    /// Sets up the puzzle display with the initial code, input field text, directions, and unsolved image.
    /// </summary>
    public void SetPuzzleDisplay()
    {
        // Set starting code
        // Set input field text
        inputText = Resources.Load<TextAsset>(startingCode).text.Split('\n').ToList();
        coloredText = new List<string>();

        caretPosY = 0;
        caretPosX = 0;

        for(int index = 0; index < inputText.Count; index++)
        {
            string line = inputText[index].Replace("\n", "");
            line = line.Replace("\r", "");
            Debug.Log("|" + line + "|");
            inputText[index] = line;
            coloredText.Add(line);
            ColorizeCurrentLine(false);
            caretPosY++;
        }

        //caretPosY--;
        caretPosY = 0;
        ColorizeCurrentLine(true);
        DisplayText();

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

            Debug.Log("input: " + input + ", ans: " + ans);

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
            //ShowClue();
            testOutput += $"<color=#00AB66>directory {puzzleName} is now unlocked. Exit to terminal and use command `cd {puzzleName}` to move into the directory</color>";
            AudioManager.instance.PlaySuccessSoundEffect();
            GameManager.instance.PuzzleSolved(this.puzzleName);
        }
        else
        {
            testOutput += $"<color=#CC0000>directory {puzzleName} is still locked. Edit your code and try again!";
            AudioManager.instance.PlayErrorSoundEffect();
        }

        WriteToEmuConsole(testOutput);
    }

    /// <summary>
    /// Resets the puzzle by calling the SetPuzzleDisplay method.
    /// </summary>
    public void ResetPuzzle()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        SetPuzzleDisplay();
    }

    /// <summary>
    /// Exits the puzzle UI by seting the puzzle's state to inactive
    /// and sets the terminal UI to active
    /// </summary>
    public void ExitPuzzleUI()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        gameObject.SetActive(false);
        GameManager.terminal.gameObject.SetActive(true);
    }

    // ========================= Helper Functions ========================= //

    public void HidePuzzle()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        gameObject.SetActive(false);
        GameManager.terminal.ShowTerminal();
    }

    public void ShowPuzzle()
    {
        SetFontSize();
        gameObject.SetActive(true);
    }

    private void SetFontSize()
    {
        directionsDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.DIRECTIONS_FONT_SIZE, 15);
        widthDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        emuConsole.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CONSOLE_FONT_SIZE, 15);
        coloredCodeDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        lineNumDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        SetColorSize();
    }

    private void SetColorSize()
    {
        caret = $"<color={PlayerPrefs.GetString(PlayerPrefNames.CODE_CARET_COLOR)}>|</color>";
        keywordColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_KEYWORD_COLOR);
        functionColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_FUNCTION_COLOR);
        stringColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_STRING_COLOR);
        ColorizeAllLines();
    }

    private void ColorizeAllLines()
    {
        int oldCaretY = caretPosY;

        for(int rowIndex = 0; rowIndex < coloredText.Count; rowIndex++)
        {
            if(rowIndex != oldCaretY)
            {
                caretPosY = rowIndex;
                ColorizeCurrentLine(false);
            }
        }

        caretPosY = oldCaretY;
        ColorizeCurrentLine(true);
        DisplayText();
    }

    private string GetIndent(string line)
    {
        string indent = "";
        int index = 0;
        bool indentEnded = false;

        while( index < line.Length && !indentEnded)
        {
            // Check if current char is tab
            if (line[index] == '\t' )
            {
                indent += '\t';
            }
            else
            {
                indentEnded = true;
            }

            index++;
        }

        return indent;
    }

    /// <summary>
    /// Adjusts the scroll view to follow the caret when caret is out of viewport
    /// </summary>
    private void FollowCaret()
    {
        coloredCodeDisplay.ForceMeshUpdate();

        string[] lines = coloredCodeDisplay.text.Split('\n');
        string line = lines[caretPosY + 1];
        TMP_TextInfo textInfo = coloredCodeDisplay.textInfo;
        TMP_LineInfo lineInfo = textInfo.lineInfo[caretPosY];

        int index = lineInfo.firstCharacterIndex;

        // Handle follow horizontally

        // Normalized caret x position for a given line
        float characterWidth = lineInfo.width / inputText[caretPosY].Length;
        float normalizedX = Normalize(characterWidth * caretPosX, 0, lineInfo.width);

        // Normalized caret x position for entire text box
        float caretXPos = GetCaretX();
        if(caretXPos >= 840)
        {
            normalizedX = Normalize(GetCaretX(), 0, lineInfo.width);

            if( normalizedX < .25)
            {
                normalizedX = 0;
            }
            coloredScrollRect.horizontalNormalizedPosition = normalizedX;
        }
        else
        {
            coloredScrollRect.horizontalNormalizedPosition = 0;
        }


        // Handle follow vertically
        lineHeight = lineInfo.lineHeight;
        float totalHeight = coloredCodeDisplay.textInfo.lineCount * lineHeight;
        float caretHeight = caretPosY * lineHeight;

        if(caretHeight < verticalViewTop)
        {
            verticalViewTop -= lineHeight;
            verticalViewBottom -= lineHeight;
            coloredScrollRect.verticalNormalizedPosition = 1 - Normalize(caretHeight, 0, totalHeight + lineHeight);
        }
        else if (caretHeight > verticalViewBottom)
        {
            verticalViewTop += lineHeight;
            verticalViewBottom += lineHeight;
            coloredScrollRect.verticalNormalizedPosition = 1 - Normalize(caretHeight, 0, totalHeight + lineHeight);
        }
    }

    public float Normalize(float value, float min, float max)
    {
        if (max == min || value == max)
        {
            // Avoid division by zero.
            // Return 0 (or another default value) or handle this scenario differently depending on the context.
            return 0;
        }

        if(((value - min) / (max - min)) > .95)
        {
            return 1;
        }

        return (value - min) / (max - min);
    }

    private float GetCaretX()
    {
        widthDisplay.text = inputText[caretPosY].Substring(0, caretPosX);

        return widthDisplay.preferredWidth;
    }

    float GetLongestLineWidth(TMP_Text textComponent)
    {
        // Ensure the text info is updated
        textComponent.ForceMeshUpdate();

        float longestWidth = 0;
        TMP_TextInfo textInfo = coloredCodeDisplay.textInfo;

        for(int index = 0; index < textInfo.lineCount; index++)
        {
            TMP_LineInfo lineInfo = textInfo.lineInfo[index];

            if (lineInfo.width > longestWidth)
                longestWidth = lineInfo.width;
        }

        return longestWidth;
    }
}
