using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotesButton : MonoBehaviour
{
    public string notesName;
    public TMP_Text buttonText;

    public void SetText()
    {
        buttonText.text = notesName;
    }
}
