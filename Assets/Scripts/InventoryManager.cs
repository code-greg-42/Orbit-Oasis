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
    [SerializeField] private GameObject storeSlotHighlightPanel;
    [SerializeField] private TMP_Text storeSlotAmountDisplay;

    [Header("Item List")]
    [SerializeField] private GameObject[] itemPrefabs; // used for instantiating saved items

    public GameObject PlayerInventory => playerInventory;
    public bool IsMenuActive { get; private set; }
    public bool IsDragging { get; private set; }
    public InventorySlot DragSlot { get; private set; }

    private float dragSlotSellPrice;
    private float dragSlotStoreAmount;

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

    private void Start()
    {
        // load any inventory items from data manager
        LoadInventory();

        // update currency from data manager
        UpdateCurrencyDisplay();
    }

    private void LoadInventory()
    {
        if (DataManager.Instance.InventoryItems.ItemList.Count > 0)
        {
            // copy of item list to iterate through
            List<ItemData> itemDataList = new(DataManager.Instance.InventoryItems.ItemList);

            // clear build materials and item list as they will be re-added in .PickupItem()
            DataManager.Instance.ClearInventoryItems();
            DataManager.Instance.ClearPlayerBuildMaterial();

            // loop through each item
            foreach (ItemData itemData in itemDataList)
            {
                // instantiate new item from prefab
                GameObject itemObject = Instantiate(itemPrefabs[itemData.prefabIndex]);

                if (itemObject.TryGetComponent(out Item newItem))
                {
                    // set quantity equal to that in data manager
                    newItem.SetQuantity(itemData.quantity);

                    // "pickup" item to ensure it goes through all necessary steps for adding to inventory
                    newItem.PickupItem();
                }
                else
                {
                    Debug.LogError("Attempted to add item to inventory, but prefab does not contain an Item component.");
                }
            }
        }

        // check for any caught fish
        if (DataManager.Instance.CaughtFishIndex.Count > 0)
        {
            // copy of list of caught fish
            List<int> fishIndices = new(DataManager.Instance.CaughtFishIndex);

            // loop through each caught fish index
            foreach(int fishIndex in fishIndices)
            {
                // instantiate new fish object from item prefab array
                GameObject fishObject = Instantiate(itemPrefabs[fishIndex]);

                if (fishObject.TryGetComponent(out Item newFishItem))
                {
                    // pickup new item
                    newFishItem.PickupItem();
                }
                else
                {
                    Debug.LogError("Attempted to add fish item to inventory, but prefab does not contain an Item component.");
                }
            }

            // clear caught fish list
            DataManager.Instance.ClearCaughtFish();
        }
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

                // if item is a build material, add to data manager (minus the remainder which will be added below for consistency)
                if (item.BuildMaterialPerUnit > 0)
                {
                    float amountToAdd = (item.Quantity - remainder) * item.BuildMaterialPerUnit;
                    DataManager.Instance.AddBuildMaterial(amountToAdd);
                }

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

            // add item to data manager
            DataManager.Instance.AddItem(item);

            // if item is a build material, add to data manager
            if (item.BuildMaterialPerUnit > 0)
            {
                DataManager.Instance.AddBuildMaterial(item.BuildMaterialPerUnit * item.Quantity);
            }

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
            CalculateSellPrices();
            sellSlotHighlightPanel.SetActive(true);
            sellSlotMoneyDisplay.text = "SELL\n($" + dragSlotSellPrice.ToString("N0") + ")";

            if (DragSlot.SlotItem is Animal)
            {
                storeSlotHighlightPanel.SetActive(true);
                storeSlotAmountDisplay.text = "STORE FOOD\n(" + dragSlotStoreAmount + " days)";
            }
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
            storeSlotHighlightPanel.SetActive(false);
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
        sellSlotMoneyDisplay.text = "$" + DataManager.Instance.PlayerStats.PlayerCurrency.ToString("N0");
        storeSlotAmountDisplay.text = DataManager.Instance.PlayerStats.PlayerFood + "\nDays of Food";
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

            // remove item from data manager
            DataManager.Instance.RemoveItem(DragSlot.SlotItem);

            // clear slot from inventory
            DragSlot.ClearSlot();
        }
    }

    public void SellDraggedItem()
    {
        if (DragSlot != null)
        {
            // if build material, subtract amount from data manager
            if (DragSlot.SlotItem.BuildMaterialPerUnit > 0)
            {
                DataManager.Instance.SubtractBuildMaterial(DragSlot.SlotItem.BuildMaterialPerUnit * DragSlot.SlotItem.Quantity);
            }

            // update Data Manager with new currency
            DataManager.Instance.AddCurrency(dragSlotSellPrice);

            // update Data Manager by removing item
            DataManager.Instance.RemoveItem(DragSlot.SlotItem);

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

    public void StoreDraggedItem()
    {
        if (DragSlot != null && DragSlot.SlotItem is Animal)
        {
            Debug.Log("storing item as food!");
            // update Data Manager
            DataManager.Instance.AddFood(dragSlotStoreAmount);

            DataManager.Instance.RemoveItem(DragSlot.SlotItem);

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

    public void UseItem(string itemName, float quantity, bool isBuildingMaterial = false)
    {
        List<InventorySlot> matchingSlots = new List<InventorySlot>();

        // find all slots containing the item
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.SlotItem != null && slot.SlotItem.ItemName == itemName)
            {
                matchingSlots.Add(slot);
            }
        }

        // sort the slots by item quantity in ascending order
        matchingSlots.Sort((a, b) => a.SlotItem.Quantity.CompareTo(b.SlotItem.Quantity));

        // if building material, update data manager
        if (isBuildingMaterial)
        {
            DataManager.Instance.SubtractBuildMaterial(quantity);
        }

        // use the item
        foreach(InventorySlot slot in matchingSlots)
        {
            if (quantity <= 0) break;

            if (quantity >= slot.SlotItem.Quantity)
            {
                // update data manager
                DataManager.Instance.RemoveItem(slot.SlotItem);

                // decrement quantity and clear slot
                quantity -= slot.SlotItem.Quantity;

                // delete the game object from player inventory
                slot.SlotItem.DeleteItem();

                // reset/clear the slot
                slot.ClearSlot();
            }
            else
            {
                int newQuantity = slot.SlotItem.Quantity - (int)quantity;

                // update data manager first so it finds correct slot
                DataManager.Instance.ChangeItemQuantity(slot.SlotItem, newQuantity);

                // set new quantity, don't clear slot
                slot.SlotItem.SetQuantity(newQuantity);

                // update UI
                slot.UpdateSlotUI();

                // reset variable
                quantity = 0;
            }
        }
    }

    private void CalculateSellPrices()
    {
        if (DragSlot != null)
        {
            if (DragSlot.SlotItem != null)
            {
                dragSlotSellPrice = DragSlot.SlotItem.Quantity * DragSlot.SlotItem.SellPricePerUnit;

                if (DragSlot.SlotItem is Animal animal)
                {
                    dragSlotStoreAmount = animal.Quantity * animal.FoodPerUnit;
                }
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
