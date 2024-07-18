using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public readonly string ItemName;
    public readonly int PrefabIndex;
    private int quantity;

    public int Quantity
    {
        get => quantity;
        set => quantity = value;
    }

    public ItemData(string itemName, int prefabIndex, int initialQuantity)
    {
        ItemName = itemName;
        PrefabIndex = prefabIndex;
        Quantity = initialQuantity;
    }
}
