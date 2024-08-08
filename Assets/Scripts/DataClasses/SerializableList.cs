using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableList<T>
{
    public List<T> ItemList;

    public SerializableList(List<T> items)
    {
        ItemList = items;
    }
}
