using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a terminal in the game
/// This class provides methods to handle player input in a terminal including text captures and handling commands
/// </summary>
public class Terminal : MonoBehaviour
{
    public RectTransform textRect;
    public RectTransform viewport;
    public TMP_Text coloredTerminalDisplay;
    public List<string> terminalInput;
    public List<string> terminalColoredText;
    public List<string> oldCommands;
    public int terminalLineIndex;
    public string caret = "<color=#FF0000>_</color>";
    public int scrollPadding = 50;

    public int oldCommandIndex = 0;

    // Text colors
    public string commandColor = "#6495ED";
    public string puzzleColor = "#FFD700";
    public string errorColor = "#DC143C";
    public string successColor = "#50C878";
    public string plainTextColor = "#FFFFFF";
    public string directoryColor = "#9400D3";

    public readonly List<string> commands = new List<string>
    {
        "help",
        "exit",
        "cat",
        "unlock",
        "clear",
        "cat",
        "ls",
        "cd",
        "solve"
    };


    // REMOVE AFTER GAMEMANAGER IS ADDED
    public void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes the terminal by doing the following
    /// 1. Initialize variables
    /// 2. Add a buffer line
    /// 3. Displays current text
    /// </summary>
    public void Initialize()
    {
        terminalInput = new List<string>();
        terminalColoredText = new List<string>();
        terminalLineIndex = 0;

        terminalInput.Add("");
        terminalColoredText.Add("");
        HandleReturn(false);
        terminalInput[terminalLineIndex] = "help";
        ColorizeCurrentLine(true);
        DisplayText();
    }

    /// <summary>
    /// Captures and processes player input
    /// </summary>
    private void Update()
    {
        if(Input.anyKeyDown)
        {
            // Check if input is down arrow
            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                HandleDownArrow();
            }
            // Check if input is up arroww
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HandleUpArrow();
            }
            // Check if input is return
            if ( Input.GetKeyDown(KeyCode.Return))
            {
                HandleReturn(true);
            }
            // Check if input is backspace
            else if(Input.GetKeyDown(KeyCode.Backspace))
            {
                HandleBackspace();
            }
            else
            {
                foreach(char inputChar in Input.inputString)
                {
                    terminalInput[terminalLineIndex] += inputChar;
                    ColorizeCurrentLine();
                }
            }
            DisplayText();
        }
    }

    /// <summary>
    /// Sets the text ccmponent to the text in terminalColoredText list
    /// </summary>
    private void DisplayText()
    {
        coloredTerminalDisplay.text = string.Join("\n", terminalColoredText);
    }

    /// <summary>
    /// This function colorizes commands
    /// </summary>
    private void ColorizeCurrentLine(bool caretIncluded = true)
    {
        string line = terminalInput[terminalLineIndex];
        string[] words = Regex.Split(line, @"(?<= )");

        if (words.Length <= 0)
        {
            words = new string[] { line };
        }

        string coloredLine = " ";

        if(caretIncluded)
        {
            coloredLine = " " + GameManager.currentDirectory.path + " > ";
        }

        foreach(string word in words)
        {
            string trimmedWord = word.Trim();
            if(commands.Contains(trimmedWord))
            {
                coloredLine += $"<color={commandColor}>{word}</color>";
            }
            else
            {
                coloredLine += word;
            }
        }

        terminalColoredText[terminalLineIndex] = coloredLine + caret;
    }

    /// <summary>
    /// Adds a new line and handles the command if any
    /// </summary>
    /// <param name="handleCommand">A boolean indicating whether to handle the command.</param>
    private void HandleReturn(bool handleCommand)
    {
        // Get previous line
        string line = GetPreviousLine();

        // Remove caret from previous line
        terminalColoredText[terminalLineIndex] = terminalColoredText[terminalLineIndex].Replace(caret, "");

        // Add a new line to input and colored text lists 
        terminalInput.Add("");
        terminalColoredText.Add(" " + GameManager.currentDirectory.path + " > " + caret);
        terminalLineIndex++;

        // Adjust the scroll view to show the bottom of the text
        float contentHeight = coloredTerminalDisplay.rectTransform.rect.height;
        float viewportHeight = viewport.rect.height;

        // Check if the content height exceeds the viewport height
        if(coloredTerminalDisplay.preferredHeight >= viewportHeight - scrollPadding)
        {
            textRect.anchoredPosition = new Vector2(0, coloredTerminalDisplay.preferredHeight - viewportHeight + scrollPadding);
        }


        // Handle command
        if (handleCommand)
            HandleCommand(line);

        // Reset old command index to count
        oldCommandIndex = oldCommands.Count;
    }

    /// <summary>
    /// Removes the last character of the current line
    /// </summary>
    private void HandleBackspace()
    {
        string line = terminalInput[terminalLineIndex];
        if(line.Length < 1)
        {
            terminalInput[terminalLineIndex] = "";
        }
        else
        {
            terminalInput[terminalLineIndex] = line.Substring(0, line.Length - 1);
        }
        ColorizeCurrentLine();
    }

    /// <summary>
    /// Find command from line and processes the command
    /// </summary>
    /// <param name="line"></param>
    private void HandleCommand(string line)
    {
        // Get command (first word of line)
        string[] commandWords = line.Split(' ');
        string command;

        if (commandWords.Length > 0)
        {
            command = commandWords[0];
        }
        else
        {
            command = line;
        }

        switch (command.Trim())
        {
            case "help":
                HandleHelpCommand(line);
                break;

            case "exit":
                HandleExitCommand(line);
                break;

            case "unlock":
                HandleUnlockCommand(line);
                break;

            case "cat":
                HandleCatCommand(line);
                break;

            case "ls":
                HandleLsCommand(line);
                break;

            case "clear":
                HandleClearCommand(line);
                break;

            case "cd":
                HandleCdCommand(line);
                break;

            case "solve":
                HandleSolveCommand(line);
                break;

            case "":
                break;

            default:
                // Write error message
                PrintLineToTerminal("<color=#FF0000>"+command+" is not a command. Please type 'help' to see all commands avaliable</color>");
                break;
        }

    }

    /// <summary>
    /// Handles the help command. 
    /// Prints all avaliable commands to terminal
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleHelpCommand(string line)
    {
        if(!CheckCommandSyntax(line, 0))
        {
            return;
        }

        PrintLineToTerminal("Avaliable Commands", false);
        PrintLineToTerminal("==================", false);
        PrintLineToTerminal("help: Prints all the commands avaliable", false);
        PrintLineToTerminal("exit: Exits the application", false);
        PrintLineToTerminal("unlock: Unlocks the next puzzle when the correct keyword is provided", false);
        PrintLineToTerminal("   Example: unlock KEYWORD", false);
        PrintLineToTerminal("clear: Clears all the content of the terminal", false);
        PrintLineToTerminal("ls: Shows files in current directory", false);
        PrintLineToTerminal("cat: Prints text from files to terminal when a file path is provided", false);
        PrintLineToTerminal("   Example: cat comments.txt", false);
    }

    /// <summary>
    /// Handles the exit command. 
    /// Closes the game
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleExitCommand(string line)
    {
        if (!CheckCommandSyntax(line, 0))
        {
            return;
        }

        Application.Quit();
    }

    /// <summary>
    /// Handles the unlock command. 
    /// Unlocks a puzzle if a puzzle keyword is entered
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleUnlockCommand(string line)
    {
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string target = line.Split(' ')[1];
        string keyword = line.Split(' ')[2];
        bool fileFound = false;

        // Check if target exists
        // Check if file exists in current context
        foreach (FileData file in GameManager.currentDirectory.files)
        {
            if (file.fileName == target)
            {
                fileFound = true;
                // Check if correct unlock keyword 
                if (file.unlockKeyword == keyword)
                {
                    string fileContent = File.ReadAllText(file.path);
                    PrintLineToTerminal($"<color={successColor}>{target} was successfully unlocked</color>", false);
                }
                // Otherwise, print error message
                else
                {
                    PrintLineToTerminal($"<color={errorColor}>Incorrect keyword. {target} was not unlocked</color>", false);
                }
            }
        }

        // Otherwise, print that the argument does not exit
        if (!fileFound)
        {
            PrintLineToTerminal($"<color={errorColor}>The file {target} does not exist</color>", false);
        }
    }

    /// <summary>
    /// Handles the clear command. 
    /// Clears the terminal by writing an empty string
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleClearCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 0))
        {
            return;
        }

        terminalInput.Clear();
        terminalColoredText.Clear();
        terminalInput.Add("");
        terminalColoredText.Add("");
        terminalLineIndex = 0;
        HandleReturn(false);
    }

    /// <summary>
    /// Handles the cat command. 
    /// Check if the argument passed in exists pythonNotes and prints the cooresponding informtaion 
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleCatCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string argument = line.Split(' ')[1];
        bool fileFound = false;

        // Check if file exists in current context
        foreach(FileData file in GameManager.currentDirectory.files)
        {
            if(file.fileName == argument)
            {
                fileFound = true;
                // Check if the file is unlocked 
                if(file.unlocked)
                {
                    string fileContent = File.ReadAllText(file.path);
                    PrintLineToTerminal(fileContent, false);
                }
                // Otherwise, print that the file is locked
                else
                {
                    PrintLineToTerminal($"<color={errorColor}>This file is locked. Use `unlock` command</color>", false);
                }
            }
        }

        // Otherwise, print that the argument does not exit
        if(!fileFound)
        {
            PrintLineToTerminal($"<color={errorColor}>The file {argument} does not exist</color>", false);
        }
    }

    /// <summary>
    /// Handles the ls command. 
    /// Prints contents of pythonNotes
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleLsCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 0))
        {
            return;
        }

        // Print files of current directory
        foreach(FileData fileData in GameManager.currentDirectory.files)
        {
            PrintLineToTerminal(fileData.fileName, false);
        }

        // Print child directories of current directory
        foreach (DirectoryData dirData in GameManager.currentDirectory.directories)
        {
            string[] dirPath = dirData.dirName.Split('\\');
            string dirName = dirPath[dirPath.Length - 1];
            PrintLineToTerminal($"<color={directoryColor}>{dirName}</color>", false);
        }
    }

    private void HandleCdCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string argument = line.Split(' ')[1];
        bool dirFound = false;

        // Check if argument is a directory that exists
        foreach(DirectoryData dirData in GameManager.currentDirectory.directories)
        {

            if(argument == dirData.dirName)
            {
                dirFound = true;
                // Check if directory is unlocked
                if (dirData.unlocked)
                {
                    // Move to new directory
                    GameManager.currentDirectory = dirData;
                }
                else
                {
                    PrintLineToTerminal($"<color={errorColor}>Directory is locked. Use the command `solve {dirData.dirName}` to unlock it</color>", false);
                }

            }
            
        }

        // Check for back command
        if (argument == "..")
        {
            if (GameManager.currentDirectory.hasParent)
            {
                dirFound = true;
                // Move to parent directory
                GameManager.currentDirectory = GameManager.currentDirectory.parentDir;
            }
        }

        if(!dirFound)
        {
            PrintLineToTerminal($"<color={errorColor}>Directory {argument} was not found</color>", false);
        }

        terminalColoredText[terminalColoredText.Count - 1] = " " + GameManager.currentDirectory.path + " > " + caret;

    }

    private void HandleSolveCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string argument = line.Split(' ')[1];

        // Loop through level puzzles to see if any puzzles have the name held in `argument`
        foreach(GameObject puzzleObject in GameManager.puzzles)
        {
            Puzzle puzzle = puzzleObject.GetComponent<Puzzle>();
            
            if(puzzle.puzzleName == argument)
            {
                // show puzzle and hide terminal
                puzzleObject.SetActive(true);
                gameObject.SetActive(false);
            }

        }
    }

    /// <summary>
    /// Writes previous command to console
    /// </summary>
    private void HandleUpArrow()
    {
        // && oldCommandIndex - 1 < oldCommands.Count
        if (oldCommandIndex - 1 >= 0)
        {
            oldCommandIndex--;
            terminalInput[terminalLineIndex] = oldCommands[oldCommandIndex];
            ColorizeCurrentLine();
            DisplayText();
        } 
        else if(oldCommands.Count > 0)
        {
            oldCommandIndex = 0;
            terminalInput[terminalLineIndex] = oldCommands[oldCommandIndex];
            ColorizeCurrentLine();
            DisplayText();
        }
    }
    
    /// <summary>
    /// Writes future command to console
    /// </summary>
    private void HandleDownArrow()
    {
        if (oldCommandIndex + 1 < oldCommands.Count)
        {
            oldCommandIndex++;
            terminalInput[terminalLineIndex] = oldCommands[oldCommandIndex];
            ColorizeCurrentLine();
            DisplayText();
        }
    }

    /// <summary>
    /// Returns true if the correct number of arguments is in line. 
    /// Returns false otherwise
    /// </summary>
    /// <param name="line"> command line </param>
    /// <param name="numOfArguments"> number of arguments to check for </param>
    /// <returns></returns>
    private bool CheckCommandSyntax(string line, int numOfArguments)    
    {
        // Add command line to old commands list
        oldCommands.Add(line);

        string[] words = line.Split(' ');

        if (words.Length == numOfArguments + 1)
            return true;

        PrintLineToTerminal(words[0] + " needs " + numOfArguments + " argument(s)");

        return false;
    }

    /// <summary>
    /// Prints text to a new line in the terminal
    /// </summary>
    /// <param name="line">line to print</param>
    public void PrintLineToTerminal(string line, bool caretIncluded = true)
    {
        terminalInput[terminalLineIndex] = line;
        ColorizeCurrentLine(caretIncluded);
        DisplayText();
        HandleReturn(false);
    }

    /// <summary>
    /// Returns the previous line in terminal if any
    /// </summary>
    /// <returns>The previous line</returns>
    private string GetPreviousLine()
    {
        return terminalInput[terminalInput.Count - 1];
    }

    /// <summary>
    /// Displays terminal UI and called AllowInput
    /// </summary>
    public void ActivateTerminal()
    {
        GameManager.terminalUI.SetActive(true);
        StartCoroutine(AllowInput());
    }

    /// <summary>
    /// Allows player input after .1 second after terminal activation to prevent any unintended input
    /// </summary>
    /// <returns></returns>
    private IEnumerator AllowInput()
    {
        yield return new WaitForSecondsRealtime(.1f);
        this.enabled = true;
    }

    /// <summary>
    /// Hides terminal UI and disables update loop
    /// </summary>
    public void DeactivateTerminal()
    {
        this.enabled = false;
        GameManager.terminalUI.SetActive(false);
    }

    public void GoToHub()
    {
        GameManager.instance.hubObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
