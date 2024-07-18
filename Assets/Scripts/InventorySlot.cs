using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Item SlotItem { get; private set; }
    public bool IsSelected { get; private set; }

    [Header("References")]
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySlotImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;

    public void AddItem(Item itemToAdd)
    {
        SlotItem = itemToAdd;
        UpdateSlotUI();
    }

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

    private void SelectSlot()
    {
        selectedBackground.SetActive(true);
        IsSelected = true;

        descriptionText.text = SlotItem.ItemName + ": " + SlotItem.Description;
    }

    public void DeselectSlot()
    {
        selectedBackground.SetActive(false);
        IsSelected = false;

        descriptionText.text = string.Empty;
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

    public void UpdateSlotUI()
    {
        if (SlotItem != null && SlotItem.Quantity > 0)
        {
            // update quantity text and image
            quantityText.text = SlotItem.Quantity.ToString();
            quantityText.enabled = true;
            itemImage.sprite = SlotItem.Image;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        // remove item from data manager
        DataManager.Instance.RemoveItem(SlotItem);

        // clear slot
        SlotItem = null;

        // clear slot UI
        quantityText.text = string.Empty;
        quantityText.enabled = false;
        itemImage.sprite = emptySlotImage;
    }

    // ------- required interface methods -------- //
    public void OnPointerClick(PointerEventData eventData)
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SlotItem != null)
        {
            InventoryManager.Instance.RemoveSlotSelection();
            SelectSlot();
            InventoryManager.Instance.StartDrag(this, Input.mousePosition);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.UpdateDragPosition(Input.mousePosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.EndDrag();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.DragSlot != null)
        {
            SwapItems(InventoryManager.Instance.DragSlot);
        }
    }
}
