using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float PlayerCurrency;
    public float PlayerFood;
    public float CameraX;
    public float CameraY;
    public Vector3 PlayerPosition = Vector3.zero;
    public Quaternion PlayerRotation = Quaternion.identity;
    public int QuestIndex;
    public int QuestProgress;
    public float MasterVolume = 1.0f;
    public float LookSensitivity = 1.0f;
}
