using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaceableItemData
{
    public Vector3 placementPosition;
    public Quaternion placementRotation;
    public int prefabIndex;

    public PlaceableItemData(Vector3 pos, Quaternion rot, int index)
    {
        placementPosition = pos;
        placementRotation = rot;
        prefabIndex = index;
    }
}
