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
    [SerializeField] private float pricePerUnit;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Image => image;
    public GameObject Prefab => prefab;
    public int MaxStackQuantity => maxStackQuantity;
    public float PricePerUnit => pricePerUnit;

    public int Quantity
    {
        get => quantity;
        private set => quantity = value;
    }

    public bool IsFullStack => Quantity >= MaxStackQuantity;

    public void SetQuantity(int amount)
    {
        Quantity = Mathf.Min(amount, MaxStackQuantity);
    }

    public void PickupItem()
    {
        // add item to inventory
        bool addedToNewSlot = InventoryManager.Instance.AddItem(this);

        // deactivate if a new stack was made, otherwise destroy game object
        if (addedToNewSlot)
        {
            // set object as child of player inventory and deactivate object
            transform.SetParent(InventoryManager.Instance.PlayerInventory.transform);
            gameObject.SetActive(false);
        }
        else
        {
            // destroy object as it's not needed
            DeleteItem();
        }
    }

    public void DropItem(Vector3 dropPosition)
    {
        // remove object as child of player inventory
        transform.SetParent(null);

        // set position to drop position
        transform.position = dropPosition;

        // activate object in hierarchy
        gameObject.SetActive(true);
    }

    public void DeleteItem()
    {
        Destroy(gameObject);
    }
}
