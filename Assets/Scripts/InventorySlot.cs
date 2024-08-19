using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MenuItemSlot
{
    public int AddAdditionalItem(int quantityToAdd)
    {
        // calculate the remainder for if the new quantity exceeds MaxStackQuantity
        int remainder = SlotItem.Quantity + quantityToAdd - SlotItem.MaxStackQuantity;

        // add to DataManager first so it finds the correct slot to add to
        DataManager.Instance.ChangeItemQuantity(SlotItem, SlotItem.Quantity + quantityToAdd);

        // add quantity to SlotItem
        SlotItem.SetQuantity(SlotItem.Quantity + quantityToAdd);

        // update quantity UI
        quantityText.text = SlotItem.Quantity.ToString();

        // return remainder if applicable
        return Mathf.Max(remainder, 0);
    }

    public void SwapItems(InventorySlot originalSlot)
    {
        (this.SlotItem, originalSlot.SlotItem) = (originalSlot.SlotItem, this.SlotItem);

        originalSlot.UpdateSlotUI();
        this.UpdateSlotUI();

        // deselect original slot if selected and select new slot
        if (originalSlot.IsSelected)
        {
            originalSlot.DeselectSlot();
            SelectSlot();
        }
    }

    // ------- interface methods -------- //
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // unselect current selection
            InventoryManager.Instance.RemoveSlotSelection();

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
            InventoryManager.Instance.RemoveSlotSelection();
            SelectSlot();
            InventoryManager.Instance.StartDrag(this, Input.mousePosition);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.UpdateDragPosition(Input.mousePosition);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.EndDrag();
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.DragSlot != null)
        {
            SwapItems(InventoryManager.Instance.DragSlot);
        }
    }
}
