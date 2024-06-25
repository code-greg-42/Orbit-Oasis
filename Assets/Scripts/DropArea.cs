using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.GetIsDragging())
        {
            Debug.Log("Dropping item!");
        }
    }
}
