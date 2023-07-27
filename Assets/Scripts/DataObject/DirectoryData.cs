using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectoryData : MonoBehaviour
{
    public string dirName;
    public string path;
    public List<FileData> files;
    public List<DirectoryData> directories;
    public DirectoryData parentDir;
    public bool unlocked;
    public bool hasParent;

    public DirectoryData()
    {
        this.dirName = "";
        this.path = "";
        this.unlocked = true;
        this.files = new List<FileData>();
        this.directories = new List<DirectoryData>();
        this.parentDir = null;
        this.hasParent = true;
    }

    public DirectoryData(string dirName, string path, string unlockKeyword, bool unlocked, List<FileData> files, List<DirectoryData> directories, DirectoryData parentDir)
    {
        this.dirName = dirName;
        this.path = path;
        this.unlocked = unlocked;
        this.files = new List<FileData>(files);
        this.directories = new List<DirectoryData>(directories);
        this.parentDir = parentDir;
    }

    public override string ToString()
    {
        return dirName;
    }
}
