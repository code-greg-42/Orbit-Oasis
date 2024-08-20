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

    private readonly int[] weightsNumberOfItems = { 30, 40, 20, 10 }; // for 5, 6, 7, 8
    private readonly int[] numberOfItemsArray = { 5, 6, 7, 8 };
    private readonly int[] weightsTraderItems = { 10, 10, 5, 5, 5, 5, 30, 30 }; // weights for tradeItemPrefabs array
    private readonly int[] quantitiesTraderItems = { 20, 1, 1, 1, 1, 1, 10, 5 };

    // timer variables
    private const float refreshInterval = 1800.0f;
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
        RefreshTraderInventory();
        StartRefreshTimer();
    }

    private void RefreshTraderInventory()
    {
        // clear all current items
        ClearAllItems();

        // roll fresh set of new random item indices (to use with item prefab array)
        List<int> itemIndices = GenerateItemIndices();

        if (itemIndices.Count > 0 && itemIndices.Count <= traderSlots.Length)
        {
            for (int i = 0; i < itemIndices.Count; i++)
            {
                AddItem(itemIndices[i], i);
            }
        }
        else
        {
            Debug.LogError("Issue with generating prefabs for trader inventory. List of indices is not within bounds.");
        }
    }

    private void AddItem(int prefabIndex, int slotNumber)
    {
        GameObject tradeItem = Instantiate(tradeItemPrefabs[prefabIndex]);
        tradeItem.SetActive(false);

        if (tradeItem.TryGetComponent(out Item item))
        {
            // get pre-assigned quantity from array and set to new item
            int traderQuantity = quantitiesTraderItems[prefabIndex];
            item.SetQuantity(traderQuantity);

            // attach deactivated gameobject to trader inventory and add item to the item slot
            tradeItem.transform.SetParent(traderInventory.transform);
            traderSlots[slotNumber].AddItem(item);
        }
        else
        {
            Debug.LogWarning("Could not get item component of new gameobject: " + tradeItem.name);
        }
    }

    private void ClearAllItems()
    {
        foreach (TraderSlot slot in traderSlots)
        {
            if (slot.SlotItem != null)
            {
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

        RemoveSlotSelection();
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
    public void UpdateBuyPrice(Item item)
    {
        // calc buy price from given item
        float price = CalculateBuyPrice(item);

        // update UI with calculated price
        buyPriceText.text = $"BUY (${price})";
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

    private void StartRefreshTimer()
    {
        if (refreshTimerCoroutine != null)
        {
            StopCoroutine(refreshTimerCoroutine);
            refreshTimerCoroutine = null;
        }

        // start new coroutine
        refreshTimerCoroutine = StartCoroutine(RefreshTimerCoroutine());
    }

    private IEnumerator RefreshTimerCoroutine()
    {
        refreshTimer = refreshInterval;

        while (refreshTimer >= 0)
        {
            // update UI
            UpdateTimerDisplay(refreshTimer);

            yield return new WaitForSeconds(1.0f);

            // decrease remaining time
            refreshTimer -= 1.0f;
        }

        // refresh trader inventory when timer reaches zero
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
