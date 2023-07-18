using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleObject : MonoBehaviour
{
    public int associatedPuzzleIndex;

    public void Initialize(int puzzleIndex)
    {
        associatedPuzzleIndex = puzzleIndex;
    }
}
