using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RewardsManager : MonoBehaviour
{
    private const string baseDialoguePath = "Robot/";
    private const string raceRewardsDialoguePath = "SpaceRaceRewards/";
    private const string loseRaceDialoguePath = "Lose/";
    private const string winRaceDialoguePath = "Win/";

    private void Start()
    {
        CheckForRaceRewards();
    }

    public void CheckForRaceRewards()
    {
        bool wasRaceCompleted = DataManager.Instance.RaceStats.RaceCompleted;

        if (wasRaceCompleted)
        {
            // set dialogue path to rewards path
            string dialoguePath = baseDialoguePath + raceRewardsDialoguePath;

            // get race outcome variables from data manager
            bool wasRaceWon = DataManager.Instance.RaceStats.RaceWon;
            int difficulty = DataManager.Instance.RaceStats.SelectedDifficulty;
            float rewardAmount = DataManager.Instance.RaceStats.RewardCurrency;

            if (wasRaceWon)
            {
                // ---- WON RACE ---- //

                // add reward amount to player currency
                DataManager.Instance.AddCurrency(rewardAmount);

                // set correct dialogue path based on different outcomes
                dialoguePath += winRaceDialoguePath;
                if (difficulty == 2)
                {
                    dialoguePath += "space_race_win_insane";
                }
                else
                {
                    dialoguePath += "space_race_win";
                }

                // show dialogue
                ShowRewardsDialogue(dialoguePath, rewardAmount);
            }
            else
            {
                // ---- LOST RACE ---- //

                // set correct dialogue path for different outcomes
                dialoguePath += loseRaceDialoguePath;
                var compositeKey = (DataManager.Instance.RaceStats.AreUpgradesMaxed, difficulty);
                switch (compositeKey)
                {
                    // maxed, insane
                    case (true, 2):
                        dialoguePath += "insane_max_upgrades";
                        break;

                    // maxed, medium
                    case (true, 1):
                        dialoguePath += "max_upgrades";
                        break;

                    // maxed, easy
                    case (true, 0):
                        dialoguePath += "easy_max_upgrades";
                        break;

                    // not maxed, insane
                    case (false, 2):
                        dialoguePath += "insane_not_max_upgrades";
                        break;

                    // not maxed, other difficulties
                    case (false, _):
                        dialoguePath += "not_max_upgrades";
                        break;
                }

                // show dialogue
                ShowRewardsDialogue(dialoguePath);
            }

            // clear reward variables after use
            DataManager.Instance.ResetRaceRewards();
        }
    }

    private void ShowRewardsDialogue(string dialoguePath, float rewardAmount = 0)
    {
        Dictionary<DialogueManager.PlaceholderType, string> replacements = new();

        if (rewardAmount != 0)
        {
            replacements.Add(DialogueManager.PlaceholderType.Money, rewardAmount.ToString());
        }

        // get dialogue from file system/dialogue manager (including any placeholder replacements)
        List<string> dialogue = DialogueManager.Instance.GetDialogue(dialoguePath, replacements);

        // show dialogue in window
        DialogueManager.Instance.ShowDialogue(dialogue);
    }
}
