using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceData
{
    public int SelectedDifficulty;
    public int BoostUpgradeLevel;
    public int MaxBoostLevel = 3;
    public int RocketUpgradeLevel;
    public int MaxRocketLevel = 3;
    public float[] BestTimes = { 0f, 0f, 0f };

    // reward variables
    public float RewardCurrency;
    public bool RaceCompleted;
    public bool RaceWon;

    public bool AreUpgradesMaxed => BoostUpgradeLevel == MaxBoostLevel && RocketUpgradeLevel == MaxRocketLevel;
}
