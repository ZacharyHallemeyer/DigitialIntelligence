using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineInfo : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    public RectTransform scrollViewBounds;

    public Emulator emulator;
    public int lineNumber;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

            Debug.Log("AH");
            if(IsInsideBounds(eventData.position))
            {
                Debug.Log("Double AH");
                localPoint.x = localPoint.x - 20; 
                emulator.MoveCaretWithMouse(localPoint, lineNumber);
            }
        }
    }

    // Returns true if the line button is pressed in the scroll view
    // Returns false if the line button is pressed outside the scroll view
    private bool IsInsideBounds(Vector2 position)
    {
        // Convert position to local space of ScrollView
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollViewBounds, position, null, out localPoint);

        // Check if the local position is within the bounds of the ScrollView
        return scrollViewBounds.rect.Contains(localPoint);
    }
}
