using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RewardsManager : MonoBehaviour
{
    private const string baseDialoguePath = "Robot/";
    private const string raceRewardDialoguePath = "SpaceRaceRewards/";
    private const string loseRaceDialoguePath = "Lose/";
    private const string winRaceDialoguePath = "Win/";

    private void Start()
    {
        CheckForRaceRewards();
    }

    public void CheckForRaceRewards()
    {
        bool wasRaceCompleted = DataManager.Instance.RaceStats.RaceCompleted;

        string dialoguePath = string.Empty;
        dialoguePath = baseDialoguePath + raceRewardDialoguePath;

        if (wasRaceCompleted)
        {
            bool wasRaceWon = DataManager.Instance.RaceStats.RaceWon;
            int difficulty = DataManager.Instance.RaceStats.SelectedDifficulty;

            if (wasRaceWon)
            {
                float rewardAmount = DataManager.Instance.RaceStats.RewardCurrency;

                // add reward amount to player currency
                DataManager.Instance.AddCurrency(rewardAmount);

                if (difficulty == 2)
                {
                    // dialogue for winning on insane
                    Debug.Log("Race won on insane difficulty.");
                }
                else
                {
                    // dialogue for winning with reward amount
                    Debug.Log($"Race won, rewarding {rewardAmount} currency.");
                }
            }
            else
            {
                dialoguePath += loseRaceDialoguePath;

                if (difficulty == 0)
                {
                    if (DataManager.Instance.RaceStats.AreUpgradesMaxed)
                    {
                        // dialogue for losing with max upgrades on easiest difficulty
                        Debug.Log("Woooow, lost on the easiest difficulty AND with max upgrades?");

                        dialoguePath += "easy_max_upgrades.txt";
                        List<string> dialogue = DialogueManager.Instance.GetDialogue(dialoguePath);
                    }
                    else
                    {
                        // dialogue suggesting upgrading the ship
                        Debug.Log("probably should upgrade the ship");
                    }
                }
                else if (difficulty == 2)
                {
                    // dialogue for losing on insane
                    Debug.Log("lost on insane difficulty.");
                }
                else
                {
                    // normal loss dialogue
                }
            }

            DataManager.Instance.ResetRaceRewards();
        }
    }
}
