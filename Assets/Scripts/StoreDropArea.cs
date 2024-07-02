using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoreDropArea : DropArea
{
    public override void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            Debug.Log("storing item!");
        }
    }
}
