using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private Image dragImage; // image used for drag and drop functionality

    private bool menuActivated;
    private bool isDragging;
    private InventorySlot dragSlot;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleInventoryMenu()
    {
        inventoryMenu.SetActive(!menuActivated);
        menuActivated = !menuActivated;
    }

    public void AddItem(string name, Sprite itemSprite, string description, GameObject itemPrefab)
    {
        // index for first open slot
        int emptySlotIndex = -1;

        // loop through all inventory slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // check if slot already contains the item and is not full
            if (inventorySlots[i].IsSlotPopulated() && !inventorySlots[i].IsSlotFull() && inventorySlots[i].GetItemName() == name)
            {
                inventorySlots[i].AddAdditionalItem();
                return;
            }

            // track the first empty slot
            if (!inventorySlots[i].IsSlotPopulated() && emptySlotIndex == -1)
            {
                emptySlotIndex = i;
            }
        }

        // if no existing stack was found, add the item to the first available empty slot
        if (emptySlotIndex != -1)
        {
            inventorySlots[emptySlotIndex].AddItem(name, itemSprite, description, itemPrefab);
        }
        else
        {
            // add full inventory logic here later
            Debug.Log("No inventory slots available");
        }
    }

    public bool IsMenuActive()
    {
        return menuActivated;
    }

    public void RemoveSlotSelection()
    {
        // loop through slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // if a slot is selected or has an active menu, deselect it or deactivate the menu, or both
            if (inventorySlots[i].IsSlotSelected() || inventorySlots[i].IsSlotMenuActivated())
            {
                if (inventorySlots[i].IsSlotSelected())
                {
                    inventorySlots[i].DeselectSlot();
                }
                
                if (inventorySlots[i].IsSlotMenuActivated())
                {
                    inventorySlots[i].DeactivateSlotMenu();
                }
            }
        }
    }

    public void SetDragImage(Sprite imageSprite, Vector3 mousePosition)
    {
        if (!isDragging)
        {
            dragImage.sprite = imageSprite;

            Color seeThrough = Color.white;
            seeThrough.a = 0.5f;
            dragImage.color = seeThrough;

            dragImage.transform.position = mousePosition;
            dragImage.gameObject.SetActive(true);
        }
    }

    public void SetDragPosition(Vector3 mousePosition)
    {
        if (isDragging)
        {
            dragImage.transform.position = mousePosition;
        }
    }

    public void SetIsDragging(bool dragging = true)
    {
        isDragging = dragging;
    }

    public bool GetIsDragging()
    {
        return isDragging;
    }

    public void SetDragSlot(InventorySlot slot)
    {
        Debug.Log("setting drag slot");
        if (!isDragging)
        {
            dragSlot = slot;
        }
    }

    public void ResetDragSlot()
    {
        if (dragSlot != null)
        {
            dragSlot = null;
        }
    }

    public InventorySlot GetDragSlot()
    {
        return dragSlot;
    }

    public void DeactivateDragImage()
    {
        dragImage.gameObject.SetActive(false);
    }
}
