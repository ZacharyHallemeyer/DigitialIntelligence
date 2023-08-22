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



public class Sandbox : Emulator
{
    // Prefabs
    public GameObject filePrefab;

    // Pop Up
    public GameObject popUpContainer;
    public TMP_Text popUpTitle;
    public TMP_Text popUpText;
    private bool deletePopUpActive = false;
    private bool savePopUpActive = false;
    private bool fileSaved = true;
    private bool isOpeningFile = false;
    private bool isCreatingFile = false;
    private bool isExiting = false;
    private string fileToOpen = "";


    // Files
    public string fileDirectory = "SandboxFiles/";
    public string currentFileName;
    private List<GameObject> fileButtonObjects;


    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        pythonFunctions = new List<string>();
        pythonFinishedEvent = new ManualResetEvent(false);

        inputText = new List<string>();
        coloredText = new List<string>();
        inputText.Add("");
        coloredText.Add("");
        caretPosX = 0;
        caretPosY = 0;
        numOfLines = 1;

        SetColorSize();
        SetLineNumbers();

        codeFieldWidth = codeScrollRect.rect.width;
        codeFieldHeight = codeScrollRect.rect.height - verticalBuffer;
        verticalViewTop = 0;
        verticalViewBottom = codeFieldHeight;

        fileDirectory = Path.Combine(Application.dataPath, fileDirectory);

        // Create file directory if it does not exist
        if( !Directory.Exists(fileDirectory) )
        {
            Directory.CreateDirectory(fileDirectory);
        }

        fileButtonObjects = new List<GameObject>();
        DisplaySavedFiles();

        // Turn off input until user creates/opens a file
        DisableInput(true);

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
                fileSaved = false;
                StartCoroutine(HandleReturnWithDelay());
            }
            // Check if input is backspace
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                fileSaved = false;
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
                fileSaved = false;
                StartCoroutine(HandleTabWithDelay());
            }
            // Check if duplicate (CRTL + D)
            else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.D) && !processingInput)
            {
                fileSaved = false;
                StartCoroutine(HandleDuplicateWithDelay());
            }
            // If not a command and not processing input
            else if (!processingInput)
            {
                // Handle text input
                foreach (char inputChar in Input.inputString)
                {
                    if (!char.IsControl(inputChar))
                    {
                        fileSaved = false;
                        HandleTextInput(inputChar);
                    }
                }
            }
        }
    }

    // ================================== Files ================================== 

    private void DisplaySavedFiles()
    {
        List<string> fileNames = GetSavedFileList();

        // Remove old buttons
        for( int index = 0; index < fileButtonObjects.Count; index++)
        {
            Destroy(fileButtonObjects[index]);
        }

        fileButtonObjects = new List<GameObject>();

        // Make a button for each file
        GridLayoutGroup gridLayoutGroup = fileScrollRect.content.GetComponent<GridLayoutGroup>();

        float originalCellSizeY = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;

        int count = 0;
        foreach( string fileName in fileNames )
        {
            GameObject newFileButton = Instantiate(filePrefab, fileScrollRect.content);

            FileButton fileButtonComponent = newFileButton.GetComponent<FileButton>();

            fileButtonComponent.fileName = fileName;
            fileButtonComponent.SetText();

            // Adjust scroll view
            LayoutRebuilder.ForceRebuildLayoutImmediate(fileScrollRect.content);

            // Update currentYPosition
            float currentYPosition = (originalCellSizeY + spacingY) * (fileNames.Count + count);

            // Adjust position
            RectTransform rectTransform = newFileButton.GetComponent<RectTransform>();
            Vector2 newPosition = rectTransform.anchoredPosition;
            newPosition.y = -currentYPosition;
            rectTransform.anchoredPosition = newPosition;

            // Add event listener
            newFileButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                OpenFile(fileName);
            });

            count++;
            fileButtonObjects.Add(newFileButton);
        }
    }

    private List<string> GetSavedFileList()
    {
        string[] filePathsBase = Directory.GetFiles(fileDirectory);
        List<string> fileNames = new List<string>();
        
        // Loop through files and get only the name of the file
        for( int fileIndex = 0; fileIndex < filePathsBase.Length; fileIndex++ )
        {
            string[] splitFilePath = filePathsBase[fileIndex].Split("/");
            string fileName = splitFilePath[splitFilePath.Length - 1];

            // Add file name to file list if it not a meta file
            if(!fileName.EndsWith(".meta"))
            {
                fileNames.Add(fileName);
            }
        }

        return fileNames;
    }

    public void CreateNewFile()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        if (!fileSaved)
        {
            isCreatingFile = true;
            ShowSavePopUp();
            return;
        }

        string fileName = "NewFile" + UnityEngine.Random.Range(0, 1000) + ".py";
        string path = Path.Combine(fileDirectory, fileName);

        // Check if any file has the same name

        File.WriteAllText(path, "");
        DisplaySavedFiles();

        // Set name
        ClearCodeEditor();

        currentFileName = fileName;
        fileNameInput.text = currentFileName;

        EnableInput();
        SaveFile();
    }

    public void RenameFile()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        string currentNamePath = Path.Combine(fileDirectory, currentFileName);
        string newNamePath = Path.Combine(fileDirectory, fileNameInput.text);
        currentFileName = fileNameInput.text;

        // Add .py to file name if not included already
        if (!newNamePath.EndsWith(".py"))
        {
            newNamePath += ".py";
            currentFileName +=  ".py";
        }

        // Return out of function if same name
        if (currentNamePath == newNamePath) return;


        File.Move(currentNamePath, newNamePath);


        DisplaySavedFiles();
    }

    public void SaveFile()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();
        if (currentFileName == "") return;

        Debug.Log("SAVE");
        fileSaved = true;

        string text = string.Join('\n', inputText);
        string path = Path.Combine(fileDirectory, currentFileName);

        File.WriteAllText(path, text);
    }

    private void OpenFile(string fileName)
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        if (!fileSaved)
        {
            fileToOpen = fileName;
            isOpeningFile = true;
            ShowSavePopUp();
            return;
        }

        EnableInput();

        string fileContent = File.ReadAllText(Path.Combine(fileDirectory, fileName));
        currentFileName = fileName;
        fileNameInput.text = currentFileName;


        inputText = fileContent.Split('\n').ToList();
        coloredText = new List<string>();

        caretPosX = 0;
        caretPosY = 0;
        numOfLines = 0;

        for (int index = 0; index < inputText.Count; index++)
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
    }
    
    private void DeleteFile()
    {
        string path = Path.Combine(fileDirectory, currentFileName);

        File.Delete(path);
        currentFileName = "";

        ClearCodeEditor();
        DisableInput(true);

        DisplaySavedFiles();
    }


    public void ShowDeletePopUp()
    {
        DisableInput();

        popUpTitle.text = "Delete";
        popUpText.text = $"Are you sure you want to delete {currentFileName}?";

        deletePopUpActive = true;
        popUpContainer.SetActive(true);
    }

    public void ShowSavePopUp()
    {
        DisableInput();

        popUpTitle.text = "Save";
        popUpText.text = $"{currentFileName} is not saved. Would you like to save?";

        savePopUpActive = true;
        popUpContainer.SetActive(true);
    }

    public void PopUpConfirm()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        // Check if delete
        if (deletePopUpActive)
        {
            DeleteFile();
        }
        // Check if save
        else if (savePopUpActive)
        {
            SaveFile();
            if (isCreatingFile)
            {
                isCreatingFile = false;
                CreateNewFile();
            }
            else if (isOpeningFile)
            {
                isOpeningFile = false;
                OpenFile(fileToOpen);
            }
            else if (isExiting)
            {
                isExiting = false;
                MoveToMainMenu();
            }
        }

        popUpContainer.SetActive(false);
        EnableInput();
    }

    public void PopUpDecline()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        // Check if save
        if (savePopUpActive)
        {
            fileSaved = true;
            if(isCreatingFile)
            {
                isCreatingFile = false;
                CreateNewFile();
            }
            else if (isOpeningFile)
            {
                isOpeningFile = false;
                OpenFile(fileToOpen);
            }
            else if (isExiting)
            {
                isExiting = false;
                MoveToMainMenu();
            }
        }

        popUpContainer.SetActive(false);
        EnableInput();
    }

    // Disable sandbox input while renaming
    public void RenameInputFieldSelected()
    {
        enabled = false;
    }

    // Enable sandbox input after renaming
    public void RenameInputFieldEnded()
    {
        enabled = true;
    }

    // ================================== Navigation ================================== 

    public void ExitButtonClick()
    {
        AudioManager.instance.PlayButtonClickSoundEffect();

        if (fileSaved)
        {
            MoveToMainMenu();
        }
        else
        {
            isExiting = true;
            ShowSavePopUp();
        }
    }

    private void MoveToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("Sandbox");
    }

    public void MoveToSettings()
    {

    }
}
