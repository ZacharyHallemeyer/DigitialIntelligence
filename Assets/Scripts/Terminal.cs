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
    public string commandColor = "#6495ED"; // cornflower blue
    public string puzzleColor = "#FFD700"; // gold
    public string errorColor = "#DC143C"; // crimson
    public string successColor = "#50C878"; // emerald green
    public string plainTextColor = "#FFFFFF"; // white
    public string directoryColor = "#9400D3"; // Dark Orchid (purple)
    public string lockedFileColor = "#8B0000"; // Dark red
    public string lockedDirectoryColor = "#770737"; // Mulberry (Dark red and purple)

    public readonly List<string> commands = new List<string>
    {
        "help",
        "cat",
        "unlock",
        "clear",
        "cat",
        "ls",
        "cd",
        "solve",
        "hint"
    };


    // REMOVE AFTER GAMEMANAGER IS ADDED
    public void Start()
    {
        //Initialize();
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
        HandleReturn(true);
        SetFontSize();
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

            case "hint":
                HandleHintCommand(line);
                break;

            case "extract":
                HandleExtractCommand(line);
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
        PrintLineToTerminal("unlock: Unlocks a file when the correct keyword is provided", false);
        PrintLineToTerminal("   Example: unlock FILE_NAME KEYWORD ", false);
        PrintLineToTerminal("   Example: unlock Hello.txt fun", false);
        PrintLineToTerminal("solve: Opens a puzzle to unlock a directory", false);
        PrintLineToTerminal("   Example: solve DIRECTORY_NAME", false);
        PrintLineToTerminal("   Example: solve VariableOne", false);
        PrintLineToTerminal("clear: Clears all the content of the terminal", false);
        PrintLineToTerminal("ls: Shows files and child directories in current directory", false);
        PrintLineToTerminal("cd: Moves into the directory provided by argument", false);
        PrintLineToTerminal("   Example: cd DIRECTORY_NAME", false);
        PrintLineToTerminal("   Example: cd VariableOne", false);
        PrintLineToTerminal("   Example: cd ..", false);
        PrintLineToTerminal("       This commands move one directory up", false);
        PrintLineToTerminal("cat: Prints text from files to terminal when a file path is provided", false);
        PrintLineToTerminal("   Example: cat comments.txt", false);
        PrintLineToTerminal("extract: This command allows the player to complete the level if all files and directories are unlocked", false);
    }

    /// <summary>
    /// Prints the question associated with a locked file
    /// </summary>
    /// <param name="line"></param>
    private void HandleHintCommand(string line)
    {
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string argument = line.Split(' ')[1].Replace(".txt", "");
        bool fileFound = false;

        // Check if target exists
        // Check if file exists in current context
        foreach (FileData file in GameManager.currentDirectory.files)
        {
            // Check if file name matches argument
            if (file.fileName.Replace(".txt", "") == argument)
            {
                fileFound = true;
                // Check if file is locked
                if(!file.unlocked)
                    PrintLineToTerminal($"<color={successColor}>The file {argument} has the following hint: {file.question}</color>", false);      
                else 
                    PrintLineToTerminal($"<color={successColor}>The file {argument} is already unlocked!</color>", false);      

            }
        }

        // Otherwise, print that the argument does not exit
        if (!fileFound)
        {
            AudioManager.instance.PlayErrorSoundEffect();
            PrintLineToTerminal($"<color={errorColor}>The file {argument} does not exist</color>", false);
        }
    }

    /// <summary>
    /// Handles the unlock command. 
    /// Unlocks a puzzle if a puzzle keyword is entered
    /// </summary>
    /// <param name="line">command line</param>
    private void HandleUnlockCommand(string line)
    {
        if (!CheckCommandSyntax(line, 2))
        {
            return;
        }

        // Get argument after command
        string target = line.Split(' ')[1].Replace(".txt", "");
        string keyword = line.Split(' ')[2].ToLower();
        bool fileFound = false;

        // Check if target exists
        // Check if file exists in current context
        foreach (FileData file in GameManager.currentDirectory.files)
        {
            if (file.fileName.Replace(".txt", "") == target)
            {
                fileFound = true;
                // Check if correct unlock keyword 
                if (file.unlockKeyword.ToLower() == keyword)
                {
                    AudioManager.instance.PlaySuccessSoundEffect();
                    file.unlocked = true;
                    GameManager.numRemainingLockedFiles--;
                    PrintLineToTerminal($"<color={successColor}>{target} was successfully unlocked</color>", false);
                }
                // Otherwise, print error message
                else
                {
                    AudioManager.instance.PlayErrorSoundEffect();
                    PrintLineToTerminal($"<color={errorColor}>Incorrect keyword. {target} was not unlocked</color>", false);
                }
            }
        }

        // Otherwise, print that the argument does not exit
        if (!fileFound)
        {
            AudioManager.instance.PlayErrorSoundEffect();
            PrintLineToTerminal($"<color={errorColor}>The file {target} does not exist</color>", false);
        }
    }

    /// <summary>
    /// Ends level if all files and directories are unlocked
    /// </summary>
    /// <param name="line"></param>
    private void HandleExtractCommand(string line)
    {
        if (!CheckCommandSyntax(line, 0))
        {
            return;
        }

        // Check if all files have been unlocked and game is won
        if (GameManager.numRemainingLockedFiles <= 0)
        {
            AudioManager.instance.PlaySuccessSoundEffect();
            PrintLineToTerminal($"<color={successColor}>You have unlocked all files and directories!</color>", false);
            PrintLineToTerminal($"<color={successColor}>This is temporary</color>", false);
            //GameManager.GameWon();
        }
        else
        {
            AudioManager.instance.PlayErrorSoundEffect();
            PrintLineToTerminal($"<color={errorColor}>You have not unlocked all files and directories</color>", false);
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
        string argument = line.Split(' ')[1].Replace(".txt", "");
        bool fileFound = false;

        // Check if file exists in current context
        foreach(FileData file in GameManager.currentDirectory.files)
        {
            if(file.fileName.Replace(".txt", "") == argument)
            {
                fileFound = true;
                // Check if the file is unlocked 
                if(file.unlocked)
                {
                    string fileContent = Resources.Load<TextAsset>(file.path).text;
                    PrintLineToTerminal(fileContent, false);
                }
                // Otherwise, print that the file is locked
                else
                {
                    AudioManager.instance.PlayErrorSoundEffect();
                    PrintLineToTerminal($"<color={errorColor}>This file is locked. Use `unlock` command</color>", false);
                }
            }
        }

        // Otherwise, print that the argument does not exit
        if(!fileFound)
        {
            AudioManager.instance.PlayErrorSoundEffect();
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
            // Check if file is locked
            if(!fileData.unlocked)
            {
                PrintLineToTerminal($"<color={lockedFileColor}>{fileData.fileName}.txt</color>", false);
            }
            // Otherwise, the file is unlocked
            else
            {
                PrintLineToTerminal(fileData.fileName+".txt", false);
            }

        }

        // Print child directories of current directory
        foreach (DirectoryData dirData in GameManager.currentDirectory.directories)
        {
            string[] dirPath = dirData.dirName.Split('\\');
            string dirName = dirPath[dirPath.Length - 1];
            
            // Check if directory is locked
            if(!dirData.unlocked)
            {
                PrintLineToTerminal($"<color={lockedDirectoryColor}>{dirName}</color>", false);
            }
            // Otherwise, the directory is unlocked
            else
            {
                PrintLineToTerminal($"<color={directoryColor}>{dirName}</color>", false);
            }
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
                    AudioManager.instance.PlayErrorSoundEffect();
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
            AudioManager.instance.PlayErrorSoundEffect();
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
        bool puzzleFound = false;

        // Loop through level puzzles to see if any puzzles have the name held in `argument`
        foreach(GameObject puzzleObject in GameManager.puzzles)
        {
            Puzzle puzzle = puzzleObject.GetComponent<Puzzle>();
            Debug.Log(puzzle.puzzleName);
            
            if(puzzle.puzzleName == argument)
            {
                // Check if puzzle is in directory
                foreach( DirectoryData dirData in GameManager.currentDirectory.directories)
                {
                    if(dirData.dirName == argument)
                    {
                        puzzleFound = true;
                        // show puzzle and hide terminal
                        //puzzleObject.SetActive(true);
                        //gameObject.SetActive(false);
                        puzzle.ShowPuzzle();
                        HideTerminal();
                    }
                }
            }
        }

        if(!puzzleFound)
        {
            AudioManager.instance.PlayErrorSoundEffect();
            PrintLineToTerminal($"<color={errorColor}>Directory not found in current directory</color>", false);
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

        AudioManager.instance.PlayErrorSoundEffect();
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
    public void ShowTerminal()
    {
        SetFontSize();
        gameObject.SetActive(true);
        GameManager.terminalUI.SetActive(true);
    }

    /// <summary>
    /// Hides terminal UI and disables update loop
    /// </summary>
    public void HideTerminal()
    {
        this.enabled = false;
        GameManager.terminalUI.SetActive(false);
    }

    public void GoToHub()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        GameManager.instance.hubObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void SetFontSize()
    {
        coloredTerminalDisplay.fontSize = PlayerPrefs.GetFloat("TerminalFontSize", 15);
    }
}
