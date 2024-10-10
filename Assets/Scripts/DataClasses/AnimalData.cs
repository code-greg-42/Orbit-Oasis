using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalData
{
    public Vector3 position;
    public Quaternion rotation;
    public int prefabIndex;

    public AnimalData(Vector3 pos, Quaternion rot, int index)
    {
        position = pos;
        rotation = rot;
        prefabIndex = index;
    }
}
