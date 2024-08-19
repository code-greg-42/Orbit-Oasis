using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class MenuItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Item SlotItem { get; protected set; }
    public bool IsSelected { get; protected set; }

    [Header("References")]
    [SerializeField] protected TMP_Text quantityText;
    [SerializeField] protected Image itemImage;
    [SerializeField] protected Sprite emptySlotImage;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private TMP_Text descriptionText;

    public void AddItem(Item itemToAdd)
    {
        SlotItem = itemToAdd;
        UpdateSlotUI();
    }

    protected void SelectSlot()
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
        // clear slot
        SlotItem = null;

        // clear slot UI
        quantityText.text = string.Empty;
        quantityText.enabled = false;
        itemImage.sprite = emptySlotImage;
    }

    // REQUIRED INTERFACE METHODS --- IMPLEMENT IN CHILD CLASSES
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {

    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        
    }
}
