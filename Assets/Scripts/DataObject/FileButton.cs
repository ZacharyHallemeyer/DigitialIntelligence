using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FileButton : MonoBehaviour
{
    public string fileName;

    public TMP_Text buttonText;

    public void SetText()
    {
        buttonText.text = fileName;
    }
}
