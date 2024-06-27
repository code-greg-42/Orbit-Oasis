using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite image;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int maxStackQuantity;
    [SerializeField] private int quantity = 1;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Image => image;
    public GameObject Prefab => prefab;
    public int MaxStackQuantity => maxStackQuantity;

    public int Quantity
    {
        get => quantity;
        private set => quantity = value;
    }

    public bool IsFullStack => Quantity >= MaxStackQuantity;

    public void AddQuantity(int amount)
    {
        Quantity += amount;
    }

    public void PickupItem()
    {
        // add item to inventory
        bool addedToNewSlot = InventoryManager.Instance.AddItem(this);

        // deactivate if a new stack was made, otherwise destroy game object
        if (addedToNewSlot)
        {
            transform.SetParent(InventoryManager.Instance.PlayerInventory.transform);
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
