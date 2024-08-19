using System;
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
    [SerializeField] private GameObject[] tradeItemPrefabs; // used for knowing which items are available for purchase

    private readonly int[] weightsNumberOfItems = { 30, 40, 20, 10 }; // for 5, 6, 7, 8
    private readonly int[] numberOfItemsArray = { 5, 6, 7, 8 };

    private readonly int[] weightsTraderItems = { 10, 10, 5, 5, 5, 5, 30, 30 }; // weights for tradeItemPrefabs array

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
