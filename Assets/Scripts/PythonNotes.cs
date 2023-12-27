using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Linq;

public class PythonNotes : MonoBehaviour
{
    private string pythonNotesDirectory = "Notes";
    private string contentsFileName = "PythonNotesContentList";

    public TMP_Text notesText;
    public ScrollRect contentScrollRect;
    public GridLayoutGroup contentGridLayoutGroup;
    public GameObject contentButtonPrefab;

    public void Initialize()
    {
        // Fill in table of contents
        FillTableOfContents();
    }

    private void FillTableOfContents()
    {
        string contentsFilePath = Path.Combine(pythonNotesDirectory, contentsFileName);
        // Get contents

        List<string> pythonNotesContent = Resources.Load<TextAsset>(contentsFilePath).text.Split('\n').ToList();

        foreach (string element in pythonNotesContent) 
        {
            string trimmedElement = element.Trim();
            // Create a button for each element
            GameObject newContentButton = Instantiate(contentButtonPrefab, contentScrollRect.content);
            NotesButton notesButtonComponent = newContentButton.GetComponent<NotesButton>();

            notesButtonComponent.notesName = trimmedElement;
            notesButtonComponent.SetText();


            // Add event listener
            newContentButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SetNotesSection( Path.Combine(pythonNotesDirectory, trimmedElement) );
            });
        }


        // Set notes text to first file if avaliable
        if( pythonNotesContent.Count > 0 )
        {
            SetNotesSection(Path.Combine(pythonNotesDirectory, pythonNotesContent[0].Trim()));
        }
    }

    private void SetNotesSection(string infoPath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(infoPath);

        if (textAsset != null)
        {
            string[] pythonNotesContent = textAsset.text.Split('\n');
            // Add extra space at the start of each line
            pythonNotesContent = pythonNotesContent.Select(str => " " + str).ToArray();
            notesText.text = "\n" + string.Join('\n', pythonNotesContent);
        }
        else
        {
            Debug.LogError("Failed to load content at path: " + infoPath);
        }
    }

    public void setFontSize(float fontSize)
    {
        notesText.fontSize = fontSize;
    }


}
