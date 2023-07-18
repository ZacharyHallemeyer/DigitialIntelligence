using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the player in the game
/// The class provides methods to handle user input, move player character and interact with interactable objects
/// </summary>
public class Player : MonoBehaviour
{
    public float inputMovementX;
    public float inputMovementY;

    public float speed;
    public float frictionCoefficient;

    public LayerMask interactableMask;
    public float interactableRadius = 1f;
    private Collider2D highlighedInteractable;
    private Rigidbody2D rb;
    public  bool movementLock = false;

    /// <summary>
    /// Sets the rb to player's rigidbody
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Captures player inputs
    /// </summary>
    void Update()
    {
        // Check if any interactables can be interacted with
        if(highlighedInteractable != null)
        {
            // Check if input is key E
            if(Input.GetKeyDown(KeyCode.E) && !movementLock)
            {
                Interact();
            }
            // Check if input is key escape
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                ReleaseInteract();
            }
        }

    }

    /// <summary>
    /// Handles the movement of the player and interacting with interactables.
    /// </summary>
    private void FixedUpdate()
    {
        // Get player movement variables
        inputMovementX = Input.GetAxis("Horizontal");
        inputMovementY = Input.GetAxis("Vertical");

        // If the player is trying to move and is not locked, then move the player
        if ( (inputMovementX != 0 || inputMovementY != 0) && !movementLock)
            Movement();
        // If the player is not trying to move, set the player's velocity to 0
        else if(rb.velocity.magnitude != 0)
            rb.velocity = Vector2.zero;

        // Handle interactables (check if any interactables are interactable)
        if(!movementLock)
            HandleInteractables();
    }

    /// <summary>
    /// Moves the player based on the input and speed.
    /// </summary>
    private void Movement()
    {
        rb.velocity = new Vector2(inputMovementX, inputMovementY).normalized * speed * Time.deltaTime;
    }

    /// <summary>
    /// Interacts with the highlighted interactable.
    /// </summary>
    private void Interact()
    {
        // Get interactable type 
        InteractableType interactableType = highlighedInteractable.gameObject.GetComponent<Interactable>().interactableType;

        if(interactableType == InteractableType.PUZZLE)
        {
            int puzzleIndex = highlighedInteractable.gameObject.GetComponent<PuzzleObject>().associatedPuzzleIndex;

            // Find associated Puzzle gameobject
            GameObject puzzle = GameManager.puzzles[puzzleIndex];
            puzzle.SetActive(true);
        }
        else if(interactableType == InteractableType.TERMINAL)
        {
            GameManager.terminal.ActivateTerminal();
        }
        else if(interactableType == InteractableType.PICK_UP)
        {
            // TODO 
        }

        movementLock = true;
    }

    /// <summary>
    /// Releases the interaction with the highlighted interactable.
    /// </summary>
    private void ReleaseInteract()
    {
        InteractableType interactableType = highlighedInteractable.gameObject.GetComponent<Interactable>().interactableType;

        if (interactableType == InteractableType.PUZZLE)
        {
            int puzzleIndex = highlighedInteractable.gameObject.GetComponent<PuzzleObject>().associatedPuzzleIndex;
            Debug.Log("Releasing Interaction with " + puzzleIndex);

            // Find associated Puzzle gameobject
            GameObject puzzle = GameManager.puzzles[puzzleIndex];
            puzzle.SetActive(false);
        }
        else if (interactableType == InteractableType.TERMINAL)
        {
            GameManager.terminal.DeactivateTerminal();
        }
        else if (interactableType == InteractableType.PICK_UP)
        {
            // TODO 
        }

        movementLock = false;
    }

    // KEEP FOR NOW
    private void ApplyFriction()
    {
        Vector2 currentVelocity = rb.velocity;
        Vector2 frictionForce = -currentVelocity.normalized * frictionCoefficient;
        rb.AddForce(frictionForce);
    }

    /// <summary>
    /// Handles the highlighting and dehighlighting of interactables within range.
    /// </summary>
    private void HandleInteractables()
    {
        Collider2D[] interactableColliders = Physics2D.OverlapCircleAll(transform.position, interactableRadius, interactableMask);

        // Remove highlight from interactables if it is no longer in interactableRadius
        if ( highlighedInteractable != null 
             && !System.Array.Exists(interactableColliders, currentCollider => currentCollider == highlighedInteractable) )
        {
            
            DehighlightInteractable(highlighedInteractable);
        }

        // If there is at least one interactable within range, highlight the closest interactable
        if(interactableColliders.Length != 0)
        {
            Collider2D collider = FindClosestInteractable(interactableColliders);

            if (highlighedInteractable != null && collider != highlighedInteractable)
                DehighlightInteractable(highlighedInteractable);

            HighlightInteractable(collider);
        }
    }

    /// <summary>
    /// Highlights the given interactable.
    /// </summary>
    private void HighlightInteractable(Collider2D interactable)
    {
        // Get sprite component and highlight 
        SpriteRenderer spriteComponent = interactable.gameObject.GetComponentInChildren<SpriteRenderer>();
        spriteComponent.color = new Color(1f, 0f, 0f, 1f);

        highlighedInteractable = interactable;
    }

    /// <summary>
    /// Dehighlights the given interactable.
    /// </summary>
    private void DehighlightInteractable(Collider2D interactable)
    {
        // Get sprite component and dehighlight
        SpriteRenderer spriteComponent = interactable.gameObject.GetComponentInChildren<SpriteRenderer>();
        spriteComponent.color = new Color(1f, 1f, 1f, 0f);

        highlighedInteractable = null;
    }

    /// <summary>
    /// Finds the closest interactable to the player and returns it.
    /// </summary>
    private Collider2D FindClosestInteractable(Collider2D[] colliders)
    {

        Collider2D closestCollider = null;
        float smallestDistance = Mathf.Infinity;
        float distance;
        
        for(int index = 0; index < colliders.Length; index++)
        {
            distance = Vector2.Distance(colliders[index].transform.position, transform.position);
            if (distance < smallestDistance)
            {
                closestCollider = colliders[index];
            }
        }

        if(highlighedInteractable != null)
        {
            distance = Vector2.Distance(highlighedInteractable.transform.position, transform.position);
            if (distance <= smallestDistance)
            {
                closestCollider = highlighedInteractable;
            }
        }

        return closestCollider;
    }
}
