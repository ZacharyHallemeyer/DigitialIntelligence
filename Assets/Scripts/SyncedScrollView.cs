using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SyncedScrollView : MonoBehaviour, IScrollHandler
{
    public ScrollRect targetScrollRect;

    private bool isSyncing = false;

    public void OnScroll(PointerEventData data)
    {
        if (isSyncing) return;

        isSyncing = true;

        if (targetScrollRect != null)
        {
            targetScrollRect.verticalNormalizedPosition = GetComponent<ScrollRect>().verticalNormalizedPosition;
        }

        isSyncing = false;
    }
}
