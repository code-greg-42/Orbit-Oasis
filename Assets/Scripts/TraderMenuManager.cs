using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TMP_Text buyPriceText;
    [SerializeField] private GameObject buySlotHighlightPanel;
    [SerializeField] private GameObject[] tradeItemPrefabs; // used for knowing which items are available for purchase
    [SerializeField] private TMP_Text refreshTimerText;
    [SerializeField] private CinemachineControls camControls;

    private readonly int[] weightsNumberOfItems = { 50, 40, 9, 1 }; // for 5, 6, 7, 8
    private readonly int[] numberOfItemsArray = { 5, 6, 7, 8 };
    private readonly int[] weightsTraderItems = { 2, 23, 30, 5, 13, 5, 10, 3, 4, 2, 3 }; // weights for tradeItemPrefabs array
    private readonly int[] quantitiesTraderItems = { 20, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    // timer variables
    private const float refreshInterval = 300.0f;
    private const float refreshTimerSaveInterval = 30.0f;
    private float refreshTimer;
    private Coroutine refreshTimerCoroutine;

    public bool IsMenuActive { get; private set; }
    public bool IsDragging { get; private set; }
    public TraderSlot DragSlot { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (DataManager.Instance.TraderStats.TraderItems.ItemList.Count > 0)
        {
            // load existing items and resume timer at last saved point
            LoadTraderInventory();
            StartRefreshTimer(DataManager.Instance.TraderStats.RefreshTimer);
        }
        else
        {
            // no items exist in data manager, start new inventory/timer
            RefreshTraderInventory();
            StartRefreshTimer();
        }
    }

    private void LoadTraderInventory()
    {
        List<ItemData> itemDataList = new(DataManager.Instance.TraderStats.TraderItems.ItemList);

        for (int i = 0; i < itemDataList.Count; i++)
        {
            GameObject tradeItem = Instantiate(InventoryManager.Instance.ItemPrefabs[itemDataList[i].prefabIndex]);
            tradeItem.SetActive(false);

            if (tradeItem.TryGetComponent(out Item item))
            {
                // set quantity from item data
                item.SetQuantity(itemDataList[i].quantity);

                // attach gameobject to trader inventory and add to slot
                tradeItem.transform.SetParent(traderInventory.transform);
                traderSlots[i].AddItem(item);
            }
            else
            {
                Debug.LogError("Could not get item component of new gameobject: " + tradeItem.name);
            }
        }
    }

    private void RefreshTraderInventory()
    {
        // clear any current drag/drop and any selected item
        EndDrag();
        RemoveSlotSelection();

        // clear all current items
        ClearAllItems();

        // roll fresh set of new random item indices (to use with item prefab array)
        List<int> itemIndices = GenerateItemIndices();

        // list for passing items to DataManager
        List<Item> itemsToAdd = new();

        if (itemIndices.Count > 0 && itemIndices.Count <= traderSlots.Length)
        {
            for (int i = 0; i < itemIndices.Count; i++)
            {
                GameObject tradeItem = Instantiate(tradeItemPrefabs[itemIndices[i]]);
                tradeItem.SetActive(false);

                if (tradeItem.TryGetComponent(out Item item))
                {
                    // get pre-assigned quantity from array and set to new item
                    int traderQuantity = quantitiesTraderItems[itemIndices[i]];
                    item.SetQuantity(traderQuantity);

                    // add item to list for DataManager
                    itemsToAdd.Add(item);

                    // attach deactivated gameobject to trader inventory and add item to the item slot
                    tradeItem.transform.SetParent(traderInventory.transform);
                    traderSlots[i].AddItem(item);
                }
                else
                {
                    Debug.LogWarning("Could not get item component of new gameobject: " + tradeItem.name);
                }
            }

            // pass list along to DataManager
            DataManager.Instance.AddTraderItems(itemsToAdd, refreshTimer);
        }
        else
        {
            Debug.LogError("Issue with generating prefabs for trader inventory. List of indices is not within bounds.");
        }
    }

    public void BuyDraggedItem()
    {
        if (DragSlot != null && DragSlot.BuyPrice < DataManager.Instance.PlayerStats.PlayerCurrency)
        {
            // update data manager with purchase
            DataManager.Instance.SubtractCurrency(DragSlot.BuyPrice);

            // remove item from trader inventory in DataManager
            DataManager.Instance.RemoveSingleTraderItem(DragSlot.SlotItem, refreshTimer);

            // add item to player inventory --- .PickupItem() will automatically add to inventory in data manager as well
            DragSlot.SlotItem.PickupItem();

            // clear slot selection
            if (DragSlot.IsSelected)
            {
                RemoveSlotSelection();
            }
            DragSlot.ClearSlot();
        }
    }

    private void ClearAllItems()
    {
        foreach (TraderSlot slot in traderSlots)
        {
            if (slot.SlotItem != null)
            {
                // no need to clear item from data manager as it will be done in RefreshTraderInventory

                // delete the item in the slot (includes the prefab from trader inventory)
                slot.SlotItem.DeleteItem();

                // reset/clear the slot
                slot.ClearSlot();
            }
        }
    }

    private List<int> GenerateItemIndices()
    {
        // weighted roll for how many items to generate
        int numberOfItemsIndex = WeightedRandom.GetWeightedRandomIndex(weightsNumberOfItems);
        int numberOfItems = numberOfItemsArray[numberOfItemsIndex];

        List<int> indices = new();

        for (int i = 0; i < numberOfItems; i++)
        {
            // weighted roll for which prefab to put in trader inventory
            int prefabIndex = WeightedRandom.GetWeightedRandomIndex(weightsTraderItems);
            indices.Add(prefabIndex);
        }

        return indices;
    }

    public void ToggleTraderMenu()
    {
        traderMenu.SetActive(!IsMenuActive);
        IsMenuActive = !IsMenuActive;

        EndDrag();
        RemoveSlotSelection();

        // disables camera mouse movement when menu is active, enable when menu inactive
        camControls.ToggleMouseMovement(IsMenuActive);
    }

    public void RemoveSlotSelection()
    {
        // clear buy price text
        buyPriceText.text = "BUY";

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

    // called from both SelectSlot (in TraderSlot script) and StartDrag
    public void UpdateBuyPrice(TraderSlot slot)
    {
        // update UI with price of the slots buyprice
        buyPriceText.text = $"BUY (${slot.BuyPrice})";
    }

    public void StartDrag(TraderSlot slot, Vector3 mousePos)
    {
        if (!IsDragging && slot.SlotItem != null)
        {
            SetDragImage(slot.SlotItem.Image, mousePos);
            DragSlot = slot;
            IsDragging = true;
            
            if (slot.BuyPrice < DataManager.Instance.PlayerStats.PlayerCurrency)
            {
                buySlotHighlightPanel.SetActive(true);
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
            buySlotHighlightPanel.SetActive(false);
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

    private void StartRefreshTimer(float remainingTime = refreshInterval)
    {
        if (refreshTimerCoroutine != null)
        {
            StopCoroutine(refreshTimerCoroutine);
            refreshTimerCoroutine = null;
        }

        // set refreshtimer and start new coroutine
        refreshTimer = remainingTime;
        refreshTimerCoroutine = StartCoroutine(RefreshTimerCoroutine());
    }

    private IEnumerator RefreshTimerCoroutine()
    {
        while (refreshTimer >= 0)
        {
            // update UI
            UpdateTimerDisplay(refreshTimer);

            yield return new WaitForSeconds(1.0f);

            // decrease remaining time
            refreshTimer -= 1.0f;

            if (refreshTimer % refreshTimerSaveInterval == 0)
            {
                // update in data manager
                DataManager.Instance.SetTraderRefreshTimer(refreshTimer);
            }
        }

        // reset refresh timer to the full amount
        refreshTimer = refreshInterval;

        // clear all slots and add new items to the shop
        RefreshTraderInventory();

        // restart timer coroutine
        refreshTimerCoroutine = StartCoroutine(RefreshTimerCoroutine());
    }

    private void UpdateTimerDisplay(float time)
    {
        string timerText = FormatTime(time);
        refreshTimerText.text = "Item Refresh - " + timerText;
    }

    private string FormatTime(float timeInSeconds)
    {
        // calc min and seconds
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        // format as "MM:SS"
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private float CalculateBuyPrice(Item item)
    {
        return item.Quantity * item.BuyPricePerUnit;
    }
}
