using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceData
{
    public int SelectedDifficulty;
    public int BoostUpgradeLevel;
    public int RocketUpgradeLevel;
    public float[] BestTimes = { 0f, 0f, 0f };
}
