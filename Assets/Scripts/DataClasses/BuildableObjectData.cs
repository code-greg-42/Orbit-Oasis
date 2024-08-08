using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildableObjectData
{
    public Vector3 placementPosition;
    public Quaternion placementRotation;
    public int buildPrefabIndex;

    public BuildableObjectData(Vector3 position, Quaternion rotation, int prefabIndex)
    {
        placementPosition = position;
        placementRotation = rotation;
        buildPrefabIndex = prefabIndex;
    }
}
