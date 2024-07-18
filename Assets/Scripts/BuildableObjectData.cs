using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildableObjectData
{
    public readonly Vector3 PlacementPosition;
    public readonly Quaternion PlacementRotation;
    public readonly int BuildPrefabIndex;

    public BuildableObjectData(Vector3 position, Quaternion rotation, int prefabIndex)
    {
        PlacementPosition = position;
        PlacementRotation = rotation;
        BuildPrefabIndex = prefabIndex;
    }
}
