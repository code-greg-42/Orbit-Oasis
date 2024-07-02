using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    [Header("References")]
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private Image dragImage; // image used for drag and drop functionality
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerInventory;
    [SerializeField] private GameObject sellSlotHighlightPanel;
    [SerializeField] private TMP_Text sellSlotMoneyDisplay;

    public GameObject PlayerInventory => playerInventory;
    public bool IsMenuActive { get; private set; }
    public bool IsDragging { get; private set; }
    public InventorySlot DragSlot { get; private set; }

    private float dragSlotSellPrice;

    public enum InventoryAddStatus
    {
        ItemAddedToStack,
        NewItemAdded,
        InventoryFull
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleInventoryMenu()
    {
        inventoryMenu.SetActive(!IsMenuActive);
        IsMenuActive = !IsMenuActive;

        RemoveSlotSelection();
    }

    public InventoryAddStatus AddItem(Item item)
    {
        // index for first open slot
        int emptySlotIndex = -1;
        int stackSlotIndex = -1;

        // loop through all inventory slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // check if slot already contains the item and is not full
            if (inventorySlots[i].SlotItem != null && !inventorySlots[i].SlotItem.IsFullStack && inventorySlots[i].SlotItem.ItemName == item.ItemName)
            {
                stackSlotIndex = i;

                // add additional to stack and get remainder
                int remainder = inventorySlots[i].AddAdditionalItem(item.Quantity);

                // if remainder, set item quantity to remainder to add to new slot
                if (remainder > 0)
                {
                    item.SetQuantity(remainder);
                }
                else
                {
                    // if no remainder, return ItemAddedToStack as no additional gameobject is needed
                    return InventoryAddStatus.ItemAddedToStack;
                }
            }

            // track the first empty slot
            if (inventorySlots[i].SlotItem == null && emptySlotIndex == -1)
            {
                emptySlotIndex = i;
            }
        }

        // if no existing stack was found or a remainder was found, add the item to the first available empty slot
        if (emptySlotIndex != -1)
        {
            inventorySlots[emptySlotIndex].AddItem(item);

            // swap slots to keep the highest stack on the left
            if (emptySlotIndex < stackSlotIndex)
            {
                inventorySlots[emptySlotIndex].SwapItems(inventorySlots[stackSlotIndex]);
            }
            return InventoryAddStatus.NewItemAdded;
        }
        else
        {
            Debug.Log("No inventory slots available");
            return InventoryAddStatus.InventoryFull;
        }
    }

    public void RemoveSlotSelection()
    {
        // loop through slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsSelected)
            {
                inventorySlots[i].DeselectSlot();
                return;
            }
        }
    }

    public void StartDrag(InventorySlot slot, Vector3 mousePos)
    {
        if (!IsDragging && slot.SlotItem != null)
        {
            SetDragImage(slot.SlotItem.Image, mousePos);
            DragSlot = slot;
            IsDragging = true;
            CalculateSellPrice();
            sellSlotHighlightPanel.SetActive(true);
            sellSlotMoneyDisplay.text = "SELL\n($" + dragSlotSellPrice + ")";
        }
    }

    public void EndDrag()
    {
        if (IsDragging)
        {
            IsDragging = false;
            DragSlot = null;
            dragImage.gameObject.SetActive(false);
            sellSlotHighlightPanel.SetActive(false);
            UpdateCurrencyDisplay();
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

    public void UpdateCurrencyDisplay()
    {
        sellSlotMoneyDisplay.text = "$" + DataManager.Instance.PlayerCurrency;
    }

    public void DropDraggedItem()
    {
        Debug.Log("dropping item!");
        if (DragSlot != null)
        {
            // instantiate item in game world near player
            Vector3 dropPos = playerTransform.position + playerTransform.forward * 2;
            DragSlot.SlotItem.DropItem(dropPos);

            // clear slot selection
            if (DragSlot.IsSelected)
            {
                RemoveSlotSelection();
            }

            // clear slot from inventory
            DragSlot.ClearSlot();
        }
    }

    public void SellDraggedItem()
    {
        Debug.Log("selling item!");
        if (DragSlot != null)
        {
            // update Data Manager
            DataManager.Instance.AddCurrency(dragSlotSellPrice);

            // delete game object from player inventory
            DragSlot.SlotItem.DeleteItem();

            // clear slot selection
            if (DragSlot.IsSelected)
            {
                RemoveSlotSelection();
            }
            DragSlot.ClearSlot();
        }
    }

    private void CalculateSellPrice()
    {
        if (DragSlot != null)
        {
            if (DragSlot.SlotItem != null)
            {
                dragSlotSellPrice = DragSlot.SlotItem.Quantity * DragSlot.SlotItem.PricePerUnit;
            }
            else
            {
                Debug.LogWarning("SlotItem is null. Cannot calculate sell price.");
            }
        }
        else
        {
            Debug.LogWarning("DragSlot is null. Cannot calculate sell price.");
        }
    }
}
