using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public TMP_Text buttonText;

    public int puzzleContainerIndex;

    public LevelButton(int puzzleContainerIndex)
    {
        this.puzzleContainerIndex = puzzleContainerIndex;
    }

    public void SetButtonText(string newText)
    {
        buttonText.text = newText;
    }

    public string GetButtonText()
    {
        return buttonText.text;
    }

}
