using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InteractableType is an enum that provides a way to classify different types of Interactables.
/// </summary>
public enum InteractableType : int
{
    PUZZLE = 0,
    TERMINAL = 1,
    PICK_UP = 2
}

public class Interactable : MonoBehaviour
{
    public InteractableType interactableType;

    // Default constructor sets InteractableType to 'Puzzle' when an instance of Interactable is created with no arguments.
    public Interactable()
    {
        interactableType = InteractableType.PUZZLE;
    }

    // Constructor that allows setting the InteractableType when an instance of Interactable is created.
    public Interactable(InteractableType interactableType)
    {
        this.interactableType = interactableType;
    }
}
