using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    // item data
    private string itemName;
    private string itemDescription;
    private Sprite itemSprite;
    private int itemQuantity;

    // max allowed in slot
    private readonly int maxQuantity = 5;

    // inventory slot
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;

    private bool isFull;
    private bool isPopulated;
    private bool isSelected;

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

    public string GetItemName()
    {
        return itemName;
    }

    private void OnLeftClick()
    {
        InventoryManager.Instance.RemoveSlotSelection();

        // select slot if slot has an item
        if (isPopulated)
        {
            SelectSlot();
        }
    }

    private void OnRightClick()
    {

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

    // required for IPointerClick
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnRightClick();
        }
    }
}
