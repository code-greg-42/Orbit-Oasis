using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Item SlotItem { get; private set; }

    // item data
    //private string itemName;
    //private string itemDescription;
    //private Sprite itemSprite;
    //private int itemQuantity;
    //private GameObject itemPrefab;

    // max allowed in slot
    //private readonly int maxQuantity = 5;

    [Header("References")]
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySlotImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;
    
    private bool isFull;
    private bool isPopulated;
    private bool isSelected;

    //public void AddItem(string name, Sprite sprite, string description, GameObject prefab)
    //{
    //    // set name, description, image, and prefab
    //    itemName = name;
    //    itemDescription = description;
    //    itemSprite = sprite;
    //    itemPrefab = prefab;

    //    // set item slot as populated
    //    isPopulated = true;

    //    // increase item quantity
    //    itemQuantity++;

    //    // set quantity text and image and enable quantity text
    //    quantityText.text = itemQuantity.ToString();
    //    quantityText.enabled = true;
    //    itemImage.sprite = itemSprite;
    //}

    public void AddItem(Item itemToAdd)
    {
        // set name, description, image, and prefab
        SlotItem = itemToAdd;

        // set item slot as populated
        isPopulated = true;

        UpdateSlotUI();
    }

    public void AddAdditionalItem()
    {
        SlotItem.AddQuantity(1);
        quantityText.text = SlotItem.Quantity.ToString();

        // set slot to full if max quantity is reached
        if (SlotItem.Quantity >= SlotItem.MaxStackQuantity)
        {
            isFull = true;
        }
    }

    //public string GetItemName()
    //{
    //    return itemName;
    //}

    //public GameObject GetItemPrefab()
    //{
    //    return itemPrefab;
    //}

    public bool IsSlotFull()
    {
        return isFull;
    }

    public bool IsSlotPopulated()
    {
        return isPopulated;
    }

    public bool IsSlotSelected()
    {
        return isSelected;
    }

    private void SelectSlot()
    {
        selectedBackground.SetActive(true);
        isSelected = true;

        descriptionText.text = SlotItem.ItemName + ": " + SlotItem.Description;
    }

    public void DeselectSlot()
    {
        selectedBackground.SetActive(false);
        isSelected = false;

        descriptionText.text = string.Empty;
    }

    // ????? UPDATE THIS LATER WITH A SEPARATE CLASS FOR ITEM DATA ?????
    //private void SwapItems(InventorySlot originalSlot)
    //{
    //    Debug.Log($"Swapping items: {itemName} with {originalSlot.itemName}");

    //    // swap item data
    //    (itemName, originalSlot.itemName) = (originalSlot.itemName, itemName);
    //    (itemDescription, originalSlot.itemDescription) = (originalSlot.itemDescription, itemDescription);
    //    (itemSprite, originalSlot.itemSprite) = (originalSlot.itemSprite, itemSprite);
    //    (itemQuantity, originalSlot.itemQuantity) = (originalSlot.itemQuantity, itemQuantity);
    //    (itemPrefab, originalSlot.itemPrefab) = (originalSlot.itemPrefab, itemPrefab);

    //    // update ui image
    //    itemImage.sprite = itemSprite != null ? itemSprite : emptySlotImage;
    //    originalSlot.itemImage.sprite = originalSlot.itemSprite != null ? originalSlot.itemSprite : emptySlotImage;

    //    // update quantity text
    //    quantityText.text = itemQuantity.ToString();
    //    quantityText.enabled = itemQuantity > 0; // enable if quantity > 0
    //    originalSlot.quantityText.text = originalSlot.itemQuantity.ToString();
    //    originalSlot.quantityText.enabled = originalSlot.itemQuantity > 0; // enable if quantity > 0

    //    // update bools
    //    isPopulated = itemQuantity > 0;
    //    isFull = itemQuantity >= maxQuantity;
    //    originalSlot.isPopulated = originalSlot.itemQuantity > 0;
    //    originalSlot.isFull = originalSlot.itemQuantity >= originalSlot.maxQuantity;

    //    // deselect original slot if selected and select new slot
    //    if (originalSlot.IsSlotSelected())
    //    {
    //        originalSlot.DeselectSlot();
    //        SelectSlot();
    //    }
    //}

    private void SwapItems(InventorySlot originalSlot)
    {
        (this.SlotItem, originalSlot.SlotItem) = (originalSlot.SlotItem, this.SlotItem);

        originalSlot.UpdateSlotUI();
        this.UpdateSlotUI();

        // Update bools for both slots
        isPopulated = SlotItem != null && SlotItem.Quantity > 0;
        isFull = SlotItem != null && SlotItem.Quantity >= SlotItem.MaxStackQuantity;

        originalSlot.isPopulated = originalSlot.SlotItem != null && originalSlot.SlotItem.Quantity > 0;
        originalSlot.isFull = originalSlot.SlotItem != null && originalSlot.SlotItem.Quantity >= originalSlot.SlotItem.MaxStackQuantity;

        // deselect original slot if selected and select new slot
        if (originalSlot.IsSlotSelected())
        {
            originalSlot.DeselectSlot();
            SelectSlot();
        }
    }

    private void UpdateSlotUI()
    {
        if (SlotItem != null && SlotItem.Quantity > 0)
        {
            quantityText.text = SlotItem.Quantity.ToString();
            quantityText.enabled = true;
            itemImage.sprite = SlotItem.Image;
        }
        else
        {
            ClearSlotUI();
        }
    }

    private void ClearSlotUI()
    {
        SlotItem = null;
        quantityText.text = string.Empty;
        quantityText.enabled = false;
        itemImage.sprite = emptySlotImage;
    }

    // ------- required interface methods --------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // unselect current selection
            InventoryManager.Instance.RemoveSlotSelection();

            // select slot if slot has an item
            if (isPopulated)
            {
                SelectSlot();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPopulated)
        {
            InventoryManager.Instance.SetDragImage(SlotItem.Image, Input.mousePosition);
            InventoryManager.Instance.SetDragSlot(this);
            InventoryManager.Instance.SetIsDragging();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.GetIsDragging())
        {
            InventoryManager.Instance.SetDragPosition(Input.mousePosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.GetIsDragging())
        {
            InventoryManager.Instance.SetIsDragging(false);
            InventoryManager.Instance.DeactivateDragImage();
            InventoryManager.Instance.ResetDragSlot();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance.GetDragSlot() != null)
        {
            SwapItems(InventoryManager.Instance.GetDragSlot());
        }
    }
}
