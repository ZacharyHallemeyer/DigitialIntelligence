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
using UnityEngine.SceneManagement;


public class Emulator : MonoBehaviour
{
    public ScrollRect coloredScrollRect;
    public ScrollRect fileScrollRect;

    public RectTransform lineNumberRect;
    public RectTransform coloredCodeRect;
    public RectTransform codeScrollRect;

    // Input fields
    public TMP_InputField fileNameInput;

    // Text Components
    public TMP_Text coloredCodeDisplay; // This text component displays the colored text
    public TMP_Text lineNumDisplay;
    public TMP_Text emuConsole;
    public TMP_Text widthDisplay;

    // Emulator variables
    public bool isChangingCode = false;
    public Vector2 lastInputPosition;
    public bool recolorAllText = false;
    public int lineMarginY = -145;
    public List<string> pythonFunctions = new List<string>();
    public Thread pythonThread;
    public ManualResetEvent pythonFinishedEvent = new ManualResetEvent(false);

    public List<string> inputText;
    public List<string> coloredText;

    public int caretPosX = 0;
    public int caretPosY = 0;
    public int numOfLines = 0;

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

    // Colors
    public string caret = "<color=#0000FF>|</color>";
    public string keywordColor;
    public string functionColor = "#00FFFF";
    public string stringColor;

    public float verticalBuffer = 230;
    public int lineHeightScaler = 1;

    /// <summary>
    /// OnScroll is called when the input field is scrolled.
    /// It updates the position of the line numbers to match the scroll position.
    /// </summary>
    /// <param name="scrollPosition">The new scroll position.</param>
    public void OnScroll(Vector2 scrollPosition)
    {
        lineNumberRect.anchoredPosition = new Vector2(lineNumberRect.anchoredPosition.x, scrollPosition.y - 150);
    }

    // ========================= Navigation ========================= //

    /// <summary>
    /// Handles the Up Arrow key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    public IEnumerator HandleUpArrowWithDelay()
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
    public void HandleUpArrow()
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
    public IEnumerator HandleDownArrowWithDelay()
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

    public void HandleDownArrow()
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
    public IEnumerator HandleLeftArrowWithDelay()
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
    public void HandleLeftArrow()
    {
        // Check if moving left will be within bounds
        if (caretPosX - 1 >= 0)
        {
            caretPosX--;
        }
        // Otherwise
        else if (caretPosY - 1 >= 0)
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
    public IEnumerator HandleCtrlLeftArrowWithDelay()
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
    public void HandleCtrlLeftArrow()
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
    public IEnumerator HandleRightArrowWithDelay()
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
    public void HandleRightArrow()
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
        else if (caretPosY + 1 < inputText.Count)
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
    public IEnumerator HandleCtrlRightArrowWithDelay()
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
    public void HandleCtrlRightArrow()
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
        caretPosX = inputText[caretPosY].IndexOf(' ', caretPosX + 1);

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
    public IEnumerator HandleReturnWithDelay()
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
    public void HandleReturn()
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

        AddLineNumber();
        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Handles the CRTL+Backspace key input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    public IEnumerator HandleCtrlBackspaceWithDelay()
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
    public void HandleCtrlBackspace()
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
    public IEnumerator HandleBackspaceWithDelay()
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
    public void HandleBackspace()
    {
        caretPosX--;

        // Check if this backspace is at the beginning of a line that is not the first line 
        if (caretPosX < 0)
        {
            RemoveLineNumber();
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
    public IEnumerator HandleTabWithDelay()
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
    public void HandleTab()
    {
        HandleTextInput('\t');
    }

    /// <summary>
    /// Handles the CRTL+D (duplicate line) input with a delay between each action.
    /// </summary>
    /// <returns>An IEnumerator object.</returns>
    public IEnumerator HandleDuplicateWithDelay()
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
    public void HandleDuplicate()
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
    public IEnumerator SwapLineAboveWithDelay()
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
    public void SwapLineAbove()
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
    public IEnumerator SwapLineBelowWithDelay()
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
    public void SwapLineBelow()
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
    public void HandleTextInput(char character)
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
    public void ColorizeCurrentLine(bool addCaret)
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
                for (int functionIndex = 0; !wordAdded && functionIndex < pythonFunctions.Count; functionIndex++)
                {
                    string functionWord = pythonFunctions[functionIndex];

                    // Check if caret is in word and check if the word is a function
                    if (caretIndex != -1 && wordNoCaret.Contains(functionWord))
                    {
                        wordAdded = true;
                        coloredTextString += AddColoredKeywordWithCaret(wordNoCaret, functionWord, functionColor, caretIndex);
                    }
                    // Check if word is a keyword
                    else if (wordNoCaret.Contains(functionWord))
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

                    if (keyValue.Value == (int)PlayerPrefNames.PYTHON_COLORS.FUNCTION)
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
        if (caretPosY == 0)
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
    public string AddColoredKeywordWithCaret(string text, string keyword, string hexColor, int caretIndex)
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
    public string AddColoredKeyword(string text, string keyword, string hexColor)
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
    public void RemoveCaretFromLine(int lineIndex)
    {
        // Remove caret from line   
        coloredText[lineIndex] = coloredText[lineIndex].Replace(caret, "");
    }

    /// <summary>
    /// Displays the colored text in the code display and adjusts the viewport.
    /// </summary>
    public void DisplayText()
    {
        // Set text component to text in coloredText
        coloredCodeDisplay.text = string.Join("\n", coloredText);
        FollowCaret();
    }

    /// <summary>
    /// Finds and stores the names of defined functions in the given text.
    /// </summary>
    /// <param name="text">The text to search for defined functions.</param>
    public void FindDefinedFunctions(string text)
    {
        // Remove caret
        text = text.Replace(caret, "");

        string pattern = @"def\s+(\w+)\s*\((.*?)\)\s*:";

        MatchCollection matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            if (!pythonFunctions.Contains(match.Groups[1].Value))
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


    // ================================== Python ================================== 

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
    public async Task RunPythonCodeWithTimeout(string pythonCode)
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
    public void PythonTime(string pythonCode)
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
    public void WriteOutputToEmuConsole()
    {
        WriteToEmuConsole(pythonOutput);
    }

    /// <summary>
    /// Writes the specified string to the emulator console.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    public void WriteToEmuConsole(string text)
    {
        emuConsole.text = "\n" + text;
        emuConsole.ForceMeshUpdate(true);
    }


    public void SetFontSize()
    {
        //fileDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.DIRECTIONS_FONT_SIZE, 15);
        widthDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        emuConsole.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CONSOLE_FONT_SIZE, 15);
        coloredCodeDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        lineNumDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.CODE_FONT_SIZE, 15);
        SetColorSize();
    }

    public void SetColorSize()
    {
        caret = $"<color={PlayerPrefs.GetString(PlayerPrefNames.CODE_CARET_COLOR)}>|</color>";
        keywordColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_KEYWORD_COLOR);
        functionColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_FUNCTION_COLOR);
        stringColor = PlayerPrefs.GetString(PlayerPrefNames.CODE_STRING_COLOR);
        ColorizeAllLines();
    }

    public void ColorizeAllLines()
    {
        int oldCaretY = caretPosY;

        for (int rowIndex = 0; rowIndex < coloredText.Count; rowIndex++)
        {
            if (rowIndex != oldCaretY)
            {
                caretPosY = rowIndex;
                ColorizeCurrentLine(false);
            }
        }

        caretPosY = oldCaretY;
        ColorizeCurrentLine(true);
        DisplayText();
    }

    public string GetIndent(string line)
    {
        string indent = "";
        int index = 0;
        bool indentEnded = false;

        while (index < line.Length && !indentEnded)
        {
            // Check if current char is tab
            if (line[index] == '\t')
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
    public void FollowCaret()
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
        if (caretXPos >= 840)
        {
            normalizedX = Normalize(GetCaretX(), 0, lineInfo.width);

            if (normalizedX < .25)
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

        if (caretHeight < verticalViewTop)
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

        if (((value - min) / (max - min)) > .95)
        {
            return 1;
        }

        return (value - min) / (max - min);
    }

    public float GetCaretX()
    {
        widthDisplay.text = inputText[caretPosY].Substring(0, caretPosX);

        return widthDisplay.preferredWidth;
    }

    public float GetLongestLineWidth(TMP_Text textComponent)
    {
        // Ensure the text info is updated
        textComponent.ForceMeshUpdate();

        float longestWidth = 0;
        TMP_TextInfo textInfo = coloredCodeDisplay.textInfo;

        for (int index = 0; index < textInfo.lineCount; index++)
        {
            TMP_LineInfo lineInfo = textInfo.lineInfo[index];

            if (lineInfo.width > longestWidth)
                longestWidth = lineInfo.width;
        }

        return longestWidth;
    }

    public void AddLineNumber()
    {
        numOfLines++;
        SetLineNumbers();
    }

    public void RemoveLineNumber()
    {
        numOfLines--;
        if (numOfLines < 1)
        {
            numOfLines = 1;
        }

        SetLineNumbers();
    }

    public void SetLineNumbers()
    {
        string lineNumberString = "\n";

        for (int count = 1; count <= numOfLines; count++)
        {
            lineNumberString += $"{count}\n";
        }

        lineNumDisplay.text = lineNumberString;
    }

    public void ClearCodeEditor()
    {
        // Set code field to default
        inputText = new List<string>();
        coloredText = new List<string>();

        inputText.Add("");
        coloredText.Add("");

        caretPosX = 0;
        caretPosY = 0;
        numOfLines = 1;

        ColorizeCurrentLine(true);
        DisplayText();
        SetLineNumbers();

        fileNameInput.text = "";
    }

    public void DisableInput(bool addOpenCreateFileMessage = false)
    {
        enabled = false;
        if (addOpenCreateFileMessage)
            coloredCodeDisplay.text = "\n  <color=#FF0000>Create or Open a file to start coding!</color>";
        fileNameInput.enabled = false;
    }

    public void EnableInput()
    {
        enabled = true;
        fileNameInput.enabled = true;
    }
}
