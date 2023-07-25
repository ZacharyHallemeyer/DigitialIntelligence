using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileData : MonoBehaviour
{
    public string fileName;
    public string path;
    public string question;
    public string unlockKeyword;
    public bool unlocked;

    public FileData()
    {
        this.fileName = "";
        this.path = "";
        this.question = "";
        this.unlockKeyword = "";
        this.unlocked = true;
    }

    public FileData(string fileName, string path, string question, string unlockKeyword, bool unlocked)
    {
        this.fileName = fileName;
        this.path = path;
        this.question = question;
        this.unlockKeyword = unlockKeyword;
        this.unlocked = unlocked;
    }
}
