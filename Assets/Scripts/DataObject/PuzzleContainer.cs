using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleContainer 
{
    public string levelName;
    public int index;
    public List<Puzzle> puzzles;


    public PuzzleContainer()
    {

    }

    public PuzzleContainer(string levelName, int index, List<Puzzle> puzzles)
    {
        this.levelName = levelName;
        this.index = index;
        this.puzzles = puzzles;
    }

}
