using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsManager : MonoBehaviour
{
    private void Start()
    {
        CheckForRaceRewards();
    }

    public void CheckForRaceRewards()
    {
        bool wasRaceCompleted = DataManager.Instance.RaceStats.RaceCompleted;

        if (wasRaceCompleted)
        {
            bool wasRaceWon = DataManager.Instance.RaceStats.RaceWon;

            if (wasRaceWon)
            {
                float rewardAmount = DataManager.Instance.RaceStats.RewardCurrency;

                // add reward amount to player currency
                DataManager.Instance.AddCurrency(rewardAmount);

                // dialogue for winning including reward amount
                Debug.Log($"Race won, rewarding {rewardAmount} currency.");
            }
            else
            {
                // dialogue for losing
                Debug.Log("Race lost, show dialogue for losing.");
            }

            DataManager.Instance.ResetRaceRewards();
        }
    }
}
