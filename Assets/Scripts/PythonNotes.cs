using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Linq;

public class PythonNotes : MonoBehaviour
{
    private string pythonNotesDirectory = "TextFiles/Notes";
    private string contentsFileName = "PythonNotesContentList";

    public TMP_Text contentText;
    public ScrollRect contentScrollRect;
    public GridLayoutGroup contentGridLayoutGroup;
    public GameObject contentButtonPrefab;

    public void Initialize()
    {
        // Get python notes

        // Fill in table of contents
        FillTableOfContents();

        // Set the notes section to first content

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

        // Create a button for each content item
    }

    private void SetNotesSection(string infoPath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(infoPath);

        if (textAsset != null)
        {
            string pythonNotesContent = textAsset.text;
            contentText.text = pythonNotesContent;
        }
        else
        {
            Debug.LogError("Failed to load content at path: " + infoPath);
        }
    }


}
