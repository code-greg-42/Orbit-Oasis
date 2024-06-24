using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private InventorySlot[] inventorySlots;

    private bool menuActivated;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleInventoryMenu()
    {
        inventoryMenu.SetActive(!menuActivated);
        menuActivated = !menuActivated;
    }

    public void AddItem(string name, Sprite itemSprite, string description)
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
            inventorySlots[emptySlotIndex].AddItem(name, itemSprite, description);
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
}
