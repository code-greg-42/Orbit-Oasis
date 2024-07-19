using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float playerCurrency;
    public float playerFood;
    public List<BuildableObjectData> buildList;
    public List<ItemData> inventoryItems;

    // player build material not needed as it will always start from 0 and be calculated as items are added to inventory

    public GameData(float currency, float food, List<BuildableObjectData> builds, List<ItemData> items)
    {
        playerCurrency = currency;
        playerFood = food;
        buildList = builds;
        inventoryItems = items;
    }
}
