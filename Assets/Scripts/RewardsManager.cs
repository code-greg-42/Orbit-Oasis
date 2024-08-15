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

        if (wasRaceCompleted)
        {
            string dialoguePath;
            dialoguePath = baseDialoguePath + raceRewardDialoguePath;

            bool wasRaceWon = DataManager.Instance.RaceStats.RaceWon;
            int difficulty = DataManager.Instance.RaceStats.SelectedDifficulty;

            if (wasRaceWon)
            {
                float rewardAmount = DataManager.Instance.RaceStats.RewardCurrency;
                dialoguePath += winRaceDialoguePath;

                // add reward amount to player currency
                DataManager.Instance.AddCurrency(rewardAmount);

                if (difficulty == 2)
                {
                    // set correct dialogue path
                    dialoguePath += "space_race_win_insane";

                    ShowWinDialogue(dialoguePath, rewardAmount);
                }
                else
                {
                    // set correct dialogue path
                    dialoguePath += "space_race_win";

                    ShowWinDialogue(dialoguePath, rewardAmount);
                }
            }
            else
            {
                dialoguePath += loseRaceDialoguePath;

                if (difficulty == 0)
                {
                    if (DataManager.Instance.RaceStats.AreUpgradesMaxed)
                    {
                        dialoguePath += "easy_max_upgrades";
                    }
                    else
                    {
                        dialoguePath += "not_max_upgrades";
                    }
                }
                else if (difficulty == 2)
                {
                    dialoguePath += "insane";
                }
                else
                {
                    if (DataManager.Instance.RaceStats.AreUpgradesMaxed)
                    {
                        dialoguePath += "max_upgrades";
                    }
                    else
                    {
                        dialoguePath += "not_max_upgrades";
                    }
                }
                // get and show dialogue
                List<string> dialogue = DialogueManager.Instance.GetDialogue(dialoguePath);
                DialogueManager.Instance.ShowDialogue(dialogue);
            }

            // clear reward variables after use
            DataManager.Instance.ResetRaceRewards();
        }
    }

    private void ShowWinDialogue(string dialoguePath, float rewardAmount)
    {
        // get dialogue from file system/dialogue manager
        List<string> dialogue = DialogueManager.Instance.GetDialogue(dialoguePath);

        // create dictionary to use for placeholder replacement
        Dictionary<DialogueManager.PlaceholderType, string> replacements = new()
                    {
                        { DialogueManager.PlaceholderType.Money, rewardAmount.ToString() }
                    };

        // replace placeholders
        dialogue = DialogueManager.Instance.ReplacePlaceholders(dialogue, replacements);

        // show dialogue
        DialogueManager.Instance.ShowDialogue(dialogue);
    }
}
