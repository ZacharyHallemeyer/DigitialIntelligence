using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using IronPython.Hosting;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using TMPro;
using Microsoft.Scripting.Hosting;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class PointerDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"Hit {results.Count} objects.");
            foreach (var hit in results)
            {
                Debug.Log($"Hit: {hit.gameObject.name}");
            }
        }
    }
}
