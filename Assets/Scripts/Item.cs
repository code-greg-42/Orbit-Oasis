using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite image;
    [SerializeField] private int maxStackQuantity;
    [SerializeField] private int quantity = 1;
    [SerializeField] private float sellPricePerUnit;
    [SerializeField] private float buyPricePerUnit;
    [SerializeField] private float buildMaterialPerUnit;
    [SerializeField] private int prefabIndex;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Image => image;
    public int MaxStackQuantity => maxStackQuantity;
    public float SellPricePerUnit => sellPricePerUnit;
    public float BuyPricePerUnit => buyPricePerUnit;
    public float BuildMaterialPerUnit => buildMaterialPerUnit;
    public int PrefabIndex => prefabIndex;

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
        InventoryManager.InventoryAddStatus status = InventoryManager.Instance.AddItem(this);

        switch (status)
        {
            case InventoryManager.InventoryAddStatus.NewItemAdded:
                transform.SetParent(InventoryManager.Instance.PlayerInventory.transform);
                gameObject.SetActive(false);
                break;
            case InventoryManager.InventoryAddStatus.ItemAddedToStack:
                DeleteItem();
                break;
            case InventoryManager.InventoryAddStatus.InventoryFull:
                DropItem(InventoryManager.Instance.PlayerInventory.transform.position);
                break;
        }
    }

    public void DropItem(Vector3 dropPosition)
    {
        // if item is a build material, subtract material from data manager
        if (BuildMaterialPerUnit > 0)
        {
            DataManager.Instance.SubtractBuildMaterial(Quantity * BuildMaterialPerUnit);
        }

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
