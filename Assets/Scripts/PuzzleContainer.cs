using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleContainer : MonoBehaviour
{
    public string name;
    public int index;
    public List<Puzzle> puzzles;


    public PuzzleContainer()
    {

    }

    public PuzzleContainer(string name, int index, List<Puzzle> puzzles)
    {
        this.name = name;
        this.index = index;
        this.puzzles = puzzles;
    }

}
