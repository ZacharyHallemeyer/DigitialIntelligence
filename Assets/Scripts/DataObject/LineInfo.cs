using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineInfo : MonoBehaviour, IPointerClickHandler
{
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

            // Here, we call the function responsible for moving the caret, passing in the necessary information.
            //MoveCaretWithMouse(localPoint, /* You need to pass the appropriate LineInfo here */);
            localPoint.x = localPoint.x - 20; 
               
            Debug.Log(localPoint);
            emulator.MoveCaretWithMouse(localPoint, lineNumber);
        }
    }
}
