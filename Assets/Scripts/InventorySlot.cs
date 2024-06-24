using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    // item data
    private string itemName;
    private string itemDescription;
    private Sprite itemSprite;
    private int itemQuantity;

    // max allowed in slot
    private readonly int maxQuantity = 5;

    [Header("References")]
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySlotImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject buttonMenu;
    [SerializeField] private Button dropOneButton;
    [SerializeField] private Button dropAllButton;
    
    private bool isFull;
    private bool isPopulated;
    private bool isSelected;
    private bool isMenuActivated;

    private void Start()
    {
        dropOneButton.onClick.AddListener(DropOneItem);
        dropAllButton.onClick.AddListener(DropAllItems);
    }

    private void ActivateSlotMenu()
    {
        buttonMenu.SetActive(true);
        isMenuActivated = true;
    }

    public void AddItem(string name, Sprite sprite, string description)
    {
        // set name, description, and image
        itemName = name;
        itemDescription = description;
        itemSprite = sprite;

        // set item slot as populated
        isPopulated = true;

        // increase item quantity
        itemQuantity++;

        // set quantity text and image and enable quantity text
        quantityText.text = itemQuantity.ToString();
        quantityText.enabled = true;
        itemImage.sprite = itemSprite;
    }

    public void AddAdditionalItem()
    {
        itemQuantity++;
        quantityText.text = itemQuantity.ToString();

        // set slot to full if max quantity is reached
        if (itemQuantity >= maxQuantity)
        {
            isFull = true;
        }
    }

    public void DeactivateSlotMenu()
    {
        buttonMenu.SetActive(false);
        isMenuActivated = false;
    }

    private void DropOneItem()
    {
        Debug.Log("One item dropped!");
    }

    private void DropAllItems()
    {
        Debug.Log("All items from the slot dropped!");
    }

    public string GetItemName()
    {
        return itemName;
    }

    private void OnLeftClick()
    {
        // unselect current selection
        InventoryManager.Instance.RemoveSlotSelection();

        // select slot if slot has an item
        if (isPopulated)
        {
            SelectSlot();
        }
    }

    private void OnRightClick()
    {
        // unselect current selection
        InventoryManager.Instance.RemoveSlotSelection();

        // bring up button menu and select slot if slot has an item
        if (isPopulated)
        {
            ActivateSlotMenu();
            SelectSlot();
        }
    }

    public bool IsSlotMenuActivated()
    {
        return isMenuActivated;
    }

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

        descriptionText.text = itemName + ": " + itemDescription;
    }

    public void DeselectSlot()
    {
        selectedBackground.SetActive(false);
        isSelected = false;

        descriptionText.text = string.Empty;
    }

    private void SwapItems(InventorySlot originalSlot)
    {
        Debug.Log($"Swapping items: {itemName} with {originalSlot.itemName}");

        // swap item data
        (itemName, originalSlot.itemName) = (originalSlot.itemName, itemName);
        (itemDescription, originalSlot.itemDescription) = (originalSlot.itemDescription, itemDescription);
        (itemSprite, originalSlot.itemSprite) = (originalSlot.itemSprite, itemSprite);
        (itemQuantity, originalSlot.itemQuantity) = (originalSlot.itemQuantity, itemQuantity);

        // update ui
        itemImage.sprite = itemSprite != null ? itemSprite : emptySlotImage;
        originalSlot.itemImage.sprite = originalSlot.itemSprite != null ? originalSlot.itemSprite : emptySlotImage;

        quantityText.text = itemQuantity.ToString();
        quantityText.enabled = itemQuantity > 0; // enable if quantity > 0

        originalSlot.quantityText.text = originalSlot.itemQuantity.ToString();
        originalSlot.quantityText.enabled = originalSlot.itemQuantity > 0; // enable if quantity > 0

        // update bools
        isPopulated = itemQuantity > 0;
        isFull = itemQuantity >= maxQuantity;
        originalSlot.isPopulated = originalSlot.itemQuantity > 0;
        originalSlot.isFull = originalSlot.itemQuantity >= originalSlot.maxQuantity;
    }

    // ------- required interface methods --------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPopulated)
        {
            InventoryManager.Instance.SetDragImage(itemSprite, Input.mousePosition);
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
