using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int prefabIndex;
    public int quantity;

    public ItemData(string name, int index, int initialQuantity)
    {
        itemName = name;
        prefabIndex = index;
        quantity = initialQuantity;
    }
}
