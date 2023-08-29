using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleContainer : MonoBehaviour
{
    public string puzzleName;
    public int index;
    public bool completed;
    public List<Puzzle> puzzles;


    public PuzzleContainer()
    {

    }

    public PuzzleContainer(string puzzleName, int index, bool completed, List<Puzzle> puzzles)
    {
        this.puzzleName = puzzleName;
        this.index = index;
        this.completed = completed;
        this.puzzles = puzzles;
    }

}
