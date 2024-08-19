using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraderMenuManager : MonoBehaviour
{
    public static TraderMenuManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject traderMenu;
    [SerializeField] private TraderSlot[] traderSlots;
    [SerializeField] private Image dragImage; // image used for drag and drop functionality
    [SerializeField] private GameObject traderInventory;
    [SerializeField] private GameObject buySlotHighlightPanel;

    public bool IsMenuActive { get; private set; }
    public bool IsDragging { get; private set; }
    public TraderSlot DragSlot { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleTraderMenu()
    {
        traderMenu.SetActive(!IsMenuActive);
        IsMenuActive = !IsMenuActive;

        RemoveSlotSelection();
    }

    public void RemoveSlotSelection()
    {
        // loop through slots
        for (int i = 0; i < traderSlots.Length; i++)
        {
            if (traderSlots[i].IsSelected)
            {
                traderSlots[i].DeselectSlot();
                return;
            }
        }
    }

    public void StartDrag(TraderSlot slot, Vector3 mousePos)
    {
        if (!IsDragging && slot.SlotItem != null)
        {
            SetDragImage(slot.SlotItem.Image, mousePos);
            DragSlot = slot;
            IsDragging = true;
        }
    }

    public void EndDrag()
    {
        if (IsDragging)
        {
            IsDragging = false;
            DragSlot = null;
            dragImage.gameObject.SetActive(false);
        }
    }

    private void SetDragImage(Sprite imageSprite, Vector3 mousePosition)
    {
        if (!IsDragging)
        {
            dragImage.sprite = imageSprite;

            Color seeThrough = Color.white;
            seeThrough.a = 0.5f;
            dragImage.color = seeThrough;

            dragImage.transform.position = mousePosition;
            dragImage.gameObject.SetActive(true);
        }
    }

    public void UpdateDragPosition(Vector3 mousePos)
    {
        if (IsDragging)
        {
            dragImage.transform.position = mousePos;
        }
    }
}
