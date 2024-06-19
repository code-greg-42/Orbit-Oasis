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

    // inventory slot
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;

    private bool isFull;
    private bool isSelected;

    public void AddItem(string name, Sprite sprite, string description)
    {
        itemName = name;
        itemDescription = description;
        itemSprite = sprite;

        isFull = true;

        quantityText.text = "1";
        quantityText.enabled = true;

        itemImage.sprite = itemSprite;
    }

    private void OnLeftClick()
    {
        InventoryManager.Instance.RemoveSlotSelection();

        if (isFull)
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
