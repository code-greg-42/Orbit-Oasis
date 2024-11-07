using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Quest
{
    public readonly string QuestTitle;
    public readonly QuestManager.IntroQuest QuestType;
    public readonly int TotalNeeded;
    public readonly string IntroDialoguePath;
    public readonly string CompletionDialoguePath;
    public readonly Action RewardAction; // holder for executing reward code
    public readonly Action IntroAction; // holder for optionally executing code after intro dialogue

    // dialogue path creation variables
    private readonly string baseDialoguePath = "Robot/IntroQuests/";
    private readonly string introDialogueFolder = "/Intro";
    private readonly string completionDialogueFolder = "/Completion";

    public Quest(string title, QuestManager.IntroQuest type, int total, Action reward, Action introAction = null)
    {
        QuestTitle = title;
        QuestType = type;
        TotalNeeded = total;
        RewardAction = reward;
        IntroAction = introAction;

        (IntroDialoguePath, CompletionDialoguePath) = GetDialoguePath(title);
    }

    // Method to call when the quest is completed
    public void CompleteQuest()
    {
        RewardAction?.Invoke();
    }

    public void InitIntroAction()
    {
        IntroAction?.Invoke();
    }

    private (string, string) GetDialoguePath(string title)
    {
        string questFolder = baseDialoguePath + title.Replace(" ", "");

        return (questFolder + introDialogueFolder, questFolder + completionDialogueFolder);
    }
}

