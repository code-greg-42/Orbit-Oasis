using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Quest
{
    public readonly string QuestTitle;
    public readonly QuestManager.IntroQuest QuestType;
    public readonly int TotalNeeded;
    public readonly string IntroDialogue;
    public readonly string CompletionDialogue;
    public readonly Action RewardAction;  // Delegate for the reward logic

    public Quest(string title, QuestManager.IntroQuest type, int total, string intro, string completion, Action reward)
    {
        QuestTitle = title;
        QuestType = type;
        TotalNeeded = total;
        IntroDialogue = intro;
        CompletionDialogue = completion;
        RewardAction = reward;
    }

    // Method to call when the quest is completed
    public void CompleteQuest()
    {
        RewardAction?.Invoke();
    }
}

