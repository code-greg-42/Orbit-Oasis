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
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].IsSlotFull())
            {
                inventorySlots[i].AddItem(name, itemSprite, description);
                return;
            }
        }
    }

    public bool IsMenuActive()
    {
        return menuActivated;
    }

    public void RemoveSlotSelection()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsSlotSelected())
            {
                inventorySlots[i].DeselectSlot();
            }
        }
    }
}
