﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using Newtonsoft.Json;
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

    public int caretPositionX = 0;

    // Text colors
    public string commandColor = "#6495ED"; // cornflower blue
    public string errorColor = "#DC143C"; // crimson
    public string successColor = "#50C878"; // emerald green
    public string plainTextColor = "#FFFFFF"; // white
    public string directoryColor = "#9400D3"; // Dark Orchid (purple)
    public string lockedFileColor = "#8B0000"; // Dark red
    public string lockedDirectoryColor = "#770737"; // Mulberry (Dark red and purple)
    public string unlockedFileColor = "#FFFFFF"; // Mulberry (Dark red and purple)

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

    // Pop Up
    public GameObject PopUpContainer;

    /// <summary>
    /// Initializes the terminal by doing the following
    /// 1. Initialize variables
    /// 2. Add a buffer line
    /// 3. Displays current text
    /// </summary>
    public void Initialize()
    {
        SetColors();
        terminalInput = new List<string>();
        terminalColoredText = new List<string>();
        terminalLineIndex = 0;
        caretPositionX = 0;

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
            // Check if input is left arrow
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                HandleLeftArrow();
            }            
            // Check if input is right arrow
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                HandleRightArrow();
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
                    if(caretPositionX >= terminalInput[terminalLineIndex].Length)
                    {
                        terminalInput[terminalLineIndex] += inputChar;
                    }
                    else
                    {
                        terminalInput[terminalLineIndex] = terminalInput[terminalLineIndex].Insert(caretPositionX, inputChar.ToString());
                    }
                    caretPositionX++;
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
        coloredTerminalDisplay.text = $"<color={plainTextColor}>";
        coloredTerminalDisplay.text += string.Join("\n", terminalColoredText);
        coloredTerminalDisplay.text += "</color>";
    }

    private void ColorizeAllLines()
    {
        int oldTerminalLineIndex = terminalLineIndex;

        for(int rowIndex = 0; rowIndex < terminalInput.Count; rowIndex++)
        {

            if(rowIndex != oldTerminalLineIndex)
            {
                terminalLineIndex = rowIndex;
                ColorizeCurrentLine(false);
            }
        }

        terminalLineIndex = oldTerminalLineIndex;
        ColorizeCurrentLine(false);
    }

    /// <summary>
    /// This function colorizes commands
    /// </summary>
    private void ColorizeCurrentLine(bool caretIncluded = true)
    {
        string line = terminalInput[terminalLineIndex];
        string[] words = Regex.Split(line, @"(?= )|(?<= )");
        int coloredCaretIndex = caretPositionX;

        if (words.Length <= 0)
        {
            words = new string[] { line };
        }

        string coloredLine = " ";

        if(caretIncluded)
        {
            coloredLine = $" {GameManager.currentDirectory.path}> ";
            coloredCaretIndex += coloredLine.Length;
        }

        foreach (string word in words)
        {
            string trimmedWord = word.Trim();
            if(commands.Contains(trimmedWord))
            {
                coloredLine += $"<color={commandColor}>{word}</color>";
                coloredCaretIndex += $"<color={commandColor}></color>".Length;
            }
            else
            {
                coloredLine += word;
            }
        }

        terminalColoredText[terminalLineIndex] = coloredLine;
        if(terminalColoredText[terminalLineIndex].Length <= coloredCaretIndex)
        {
            terminalColoredText[terminalLineIndex] += caret;
        }
        else
        {
            if(commands.Contains(GetWordAtIndex(line, caretPositionX)))
            {
                coloredCaretIndex -= "</color>".Length;
            }

            terminalColoredText[terminalLineIndex] = terminalColoredText[terminalLineIndex].Insert(coloredCaretIndex, caret);
        }
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
        terminalColoredText.Add($"{caret}");
        terminalLineIndex++;
        caretPositionX = 0;
        
        ColorizeCurrentLine();


        // Adjust the scroll view to show the bottom of the text
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
            caretPositionX = 0;
        }
        else if (caretPositionX - 1 >= 0)
        {
            terminalInput[terminalLineIndex] = terminalInput[terminalLineIndex].Remove(caretPositionX - 1, 1);
            caretPositionX--;
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

            case "devUnlock":
                HandleDevUnlock(line);
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
        else
        {
            terminalInput[terminalLineIndex] = $"unlock {argument} ";
            caretPositionX = terminalInput[terminalLineIndex].Length;
            ColorizeCurrentLine(true);
            DisplayText();
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
                    // Check if file is already unlocked
                    if(file.unlocked)
                    {
                        AudioManager.instance.PlaySuccessSoundEffect();
                        PrintLineToTerminal($"<color={successColor}>{target} has already been unlocked</color>", false);
                    }
                    else
                    {
                        AudioManager.instance.PlaySuccessSoundEffect();
                        file.unlocked = true;
                        GameManager.instance.FileUnlocked();
                        PrintLineToTerminal($"<color={successColor}>{target} was successfully unlocked</color>", false);

                        // Store that the file was unlocked
                        StoreFileUnlock(file.fileName);
                    }
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

    private void HandleLsCommand(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 0))
        {
            return;
        }
        
        if(GameManager.currentDirectory.files.Count > 0)
        {
            PrintLineToTerminal("Files", false);
            PrintLineToTerminal("====================================", false);
        }

        // Print files of current directory
        foreach (FileData fileData in GameManager.currentDirectory.files)
        {
            // Check if file is locked
            if(!fileData.unlocked)
            {
                PrintLineToTerminal($"<color={lockedFileColor}>{fileData.fileName}.txt: Locked(use hint {fileData.fileName} and unlock {fileData.fileName} commands)</color>", false);
            }
            // Otherwise, the file is unlocked
            else
            {
                PrintLineToTerminal($"<color={unlockedFileColor}>{fileData.fileName}.txt: Unlocked (use cat {fileData.fileName} command)</color>", false);
            }

        }

        if (GameManager.currentDirectory.files.Count > 0)
        {
            PrintLineToTerminal($"====================================\n\n", false);
        }

        if(GameManager.currentDirectory.directories.Count > 0)
        {
            PrintLineToTerminal($"Directories", false);
            PrintLineToTerminal($"====================================", false);
        }

        // Print child directories of current directory
        foreach (DirectoryData dirData in GameManager.currentDirectory.directories)
        {
            string[] dirPath = dirData.dirName.Split('\\');
            string dirName = dirPath[dirPath.Length - 1];
            
            // Check if directory is locked
            if(!dirData.unlocked)
            {
                PrintLineToTerminal($"<color={lockedDirectoryColor}>{dirName}: Locked (use solve {dirName} to unlock directory)</color>", false);
            }
            // Otherwise, the directory is unlocked
            else
            {
                PrintLineToTerminal($"<color={directoryColor}>{dirName}: Unlocked (use cd {dirName} to move into directory)</color>", false);
            }
        }

        if (GameManager.currentDirectory.directories.Count > 0)
        {
            PrintLineToTerminal($"====================================", false);
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
                    AudioManager.instance.PlaySuccessSoundEffect();
                    PrintLineToTerminal($"<color={successColor}>Successfully moved into directory {dirData.dirName}</color>", false);
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

            if(puzzle.puzzleName == argument)
            {
                // Check if puzzle is in directory
                foreach( DirectoryData dirData in GameManager.currentDirectory.directories)
                {
                    if(dirData.dirName == argument)
                    {
                        puzzleFound = true;
                        // show puzzle and hide terminal
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

    // Unlocks directories (for dev purposes)
    private void HandleDevUnlock(string line)
    {
        // Check if syntax is incorrect
        if (!CheckCommandSyntax(line, 1))
        {
            return;
        }

        // Get argument after command
        string argument = line.Split(' ')[1];

        GameManager.instance.PuzzleSolved(argument);
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
            caretPositionX = oldCommands[oldCommandIndex].Length;
            ColorizeCurrentLine();
            DisplayText();
        } 
        else if(oldCommands.Count > 0)
        {
            oldCommandIndex = 0;
            terminalInput[terminalLineIndex] = oldCommands[oldCommandIndex];
            caretPositionX = oldCommands[oldCommandIndex].Length;
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
            caretPositionX = oldCommands[oldCommandIndex].Length;
            ColorizeCurrentLine();
            DisplayText();
        }
    }

    private void HandleLeftArrow()
    {
        if(caretPositionX > 0)
        {
            caretPositionX--;
            ColorizeCurrentLine();
        }
    }

    private void HandleRightArrow()
    {
        if(caretPositionX < terminalColoredText[terminalLineIndex].Length)
        {
            caretPositionX++;
            ColorizeCurrentLine();
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
        SetColors();
    }

    /// <summary>
    /// Hides terminal UI and disables update loop
    /// </summary>
    public void HideTerminal()
    {
        gameObject.SetActive(false);
    }

    public void GoToHub()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        GameManager.instance.hubObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void SetFontSize()
    {
        coloredTerminalDisplay.fontSize = PlayerPrefs.GetFloat(PlayerPrefNames.TERMINAL_FONT_SIZE, 15);
    }

    private void SetColors()
    {
        commandColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_COMMAND_COLOR);
        plainTextColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_PLAIN_COLOR); 
        directoryColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR);
        unlockedFileColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR);
        lockedFileColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR);
        lockedDirectoryColor = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR);

        //ColorizeAllLines();
    }

    // ==================== Level Complete ==================== //

    public void ShowLevelCompletePopUp()
    {
        PopUpContainer.SetActive(true);
    }

    public void PopUpExitChosen()
    {
        GameManager.instance.MoveToMainMenu();
    }

    public void PopUpStayChosen()
    {
        PopUpContainer.SetActive(false);
    }


    // ==================== Helper Functions ==================== //
    
    private string GetWordAtIndex(string line, int index)
    {
        string[] words = Regex.Split(line, @"(?= )|(?<= )");
        int currentIndex = 0;

        foreach( string word in words )
        {
            currentIndex += word.Length;

            if(index <= currentIndex)
            {
                return word;
            }
        }



        return words[words.Length - 1];
    }

    // ==================== Data Storage ==================== //

    private void StoreFileUnlock(string targetFileName)
    {
        string structurePath = $"{GameManager.levelName}/Structure.json";
        string data = File.ReadAllText(structurePath);

        DirectoryData rootDir = JsonConvert.DeserializeObject<DirectoryData>(data);

        StoreFileUnlockHelper(rootDir, targetFileName);

        // store new directory data
        try
        {
            string json = JsonConvert.SerializeObject(rootDir, Formatting.Indented);
            File.WriteAllText(structurePath, json);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

    }

    private void StoreFileUnlockHelper(DirectoryData currentDir, string targetFileName)
    {
        foreach(FileData file in currentDir.files)
        {
            if(file.fileName == targetFileName)
            {
                file.unlocked = true;
                return;
            }
        }

        foreach (DirectoryData childDir in currentDir.directories)
        {
            StoreFileUnlockHelper(childDir, targetFileName);
        }
    }
}
