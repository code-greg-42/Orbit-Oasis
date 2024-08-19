using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraderSlot : MenuItemSlot
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // unselect current selection
            TraderMenuManager.Instance.RemoveSlotSelection();

            // select slot if slot has an item
            if (SlotItem != null)
            {
                SelectSlot();
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (SlotItem != null)
        {
            TraderMenuManager.Instance.RemoveSlotSelection();
            SelectSlot();
            TraderMenuManager.Instance.StartDrag(this, Input.mousePosition);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (TraderMenuManager.Instance.IsDragging)
        {
            TraderMenuManager.Instance.UpdateDragPosition(Input.mousePosition);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (TraderMenuManager.Instance.IsDragging)
        {
            TraderMenuManager.Instance.EndDrag();
        }
    }
}
