using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MenuItemSlot
{
    public bool IsRecentlyClicked { get; private set; }
    private const float doubleClickWindow = 0.42f;

    public int AddAdditionalItem(int additional)
    {
        int total = SlotItem.Quantity + additional;

        // calculate the remainder for if the new quantity exceeds MaxStackQuantity
        int remainder = total - SlotItem.MaxStackQuantity;

        // calculate new quantity to set to this slot, not to exceed max stack quantity
        int newQuantity = Mathf.Min(total, SlotItem.MaxStackQuantity);

        // add to DataManager before setting the item's quantity so it finds the correct slot to add to
        DataManager.Instance.ChangeItemQuantity(SlotItem, newQuantity);

        // set quantity to SlotItem
        SlotItem.SetQuantity(newQuantity);

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

    private void ResetRecentlyClicked()
    {
        IsRecentlyClicked = false;
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

            if (SlotItem is PlaceableItem placeableItem)
            {
                if (IsRecentlyClicked)
                {
                    ItemPlacementManager.Instance.ActivateItemPlacement(placeableItem);
                    ClearSlot();
                }
                else
                {
                    IsRecentlyClicked = true;
                    Invoke(nameof(ResetRecentlyClicked), doubleClickWindow);
                }
            }
            else if (SlotItem is Animal animal)
            {
                if (IsRecentlyClicked)
                {
                    Transform playerTransform = GameObject.Find("PlayerModel").transform;

                    if (playerTransform != null)
                    {
                        // calculate drop position based on player
                        Vector3 dropPos = playerTransform.position + playerTransform.forward * 2;

                        // drop item out of inventory and into scene
                        animal.DropItem(dropPos);

                        // deselect and clear slot
                        DeselectSlot();
                        ClearSlot();
                    }
                }
                else
                {
                    IsRecentlyClicked = true;
                    Invoke(nameof(ResetRecentlyClicked), doubleClickWindow);
                }
            }
            else if (SlotItem is ToggleItem toggleItem)
            {
                if (IsRecentlyClicked)
                {
                    // toggle items ability and show alert message, without getting rid of the item or clearing the slot
                    toggleItem.ToggleAbility();
                }
                else
                {
                    IsRecentlyClicked = true;
                    Invoke(nameof(ResetRecentlyClicked), doubleClickWindow);
                }
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
