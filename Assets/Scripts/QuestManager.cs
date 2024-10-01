using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private int questProgress;
    private int activeQuestIndex;

    public bool QuestLogActive => MainUIManager.Instance.QuestPanelActive;

    public enum IntroQuest
    {
        RemoveDeadTrees,
        SellDeadTrees,
        PlantTrees,
        PlaceRocks,
        FarmTrees,
        MineRocks,
        BuildObjects,
        SpaceRace,
    }

    private Quest[] introQuests;
    
    private void Awake()
    {
        Instance = this;

        introQuests = new Quest[]
        {
            new Quest("Remove Dead Trees", IntroQuest.RemoveDeadTrees, 10, "Robot/IntroQuests/remove_dead_trees_intro", "Robot/IntroQuests/remove_dead_trees_completion", RewardForRemoveDeadTrees),
            new Quest("Sell Dead Trees", IntroQuest.SellDeadTrees, 2, "Robot/IntroQuests/sell_dead_trees_intro", "Robot/IntroQuests/sell_dead_trees_completion", RewardForSellDeadTrees),
            new Quest("Plant New Trees", IntroQuest.PlantTrees, 5, "", "", RewardForPlantTrees),
            new Quest("Place Rocks", IntroQuest.PlaceRocks, 3, "", "", RewardForPlaceRocks),
            new Quest("Farm Trees", IntroQuest.FarmTrees, 3, "", "", RewardForFarmTrees),
            new Quest("Mine Rocks", IntroQuest.MineRocks, 2, "", "", RewardForMineRocks),
            new Quest("Build Objects", IntroQuest.BuildObjects, 5, "", "", RewardForBuildObjects),
            new Quest("Complete the Space Race", IntroQuest.SpaceRace, 1, "", "", RewardForSpaceRace)
        };
    }

    public IntroQuest GetCurrentQuest()
    {
        return introQuests[activeQuestIndex].QuestType;
    }

    public void StartNewQuest()
    {
        Quest currentQuest = introQuests[activeQuestIndex];

        // update UI
        MainUIManager.Instance.UpdateQuestLogWithNewQuest(currentQuest.QuestTitle, currentQuest.TotalNeeded);

        MainUIManager.Instance.ActivateQuestLog();
    }

    public void UpdateCurrentQuest()
    {
        questProgress++;
        MainUIManager.Instance.UpdateQuestProgress(questProgress, introQuests[activeQuestIndex].TotalNeeded);

        if (questProgress >= introQuests[activeQuestIndex].TotalNeeded)
        {
            StartCoroutine(QuestCompletionCoroutine());
        }
    }

    private IEnumerator QuestCompletionCoroutine()
    {
        yield return StartCoroutine(MainUIManager.Instance.ShowQuestSuccess());
        List<string> dialogues = DialogueManager.Instance.GetDialogue(introQuests[activeQuestIndex].CompletionDialoguePath);
        DialogueManager.Instance.ShowDialogue(dialogues);
        questProgress = 0;
        activeQuestIndex++;
        StartNewQuest();
    }

    // QUEST REWARDS

    // Define the reward methods
    private void RewardForRemoveDeadTrees()
    {
        // Implement reward logic for RemoveDeadTrees
    }

    private void RewardForSellDeadTrees()
    {
        // Implement reward logic for SellDeadTrees
    }

    private void RewardForPlantTrees()
    {
        // Implement reward logic for PlantTrees
    }

    private void RewardForPlaceRocks()
    {
        // Implement reward logic for PlantRocks
    }

    private void RewardForFarmTrees()
    {
        // Implement reward logic for FarmTree
    }

    private void RewardForMineRocks()
    {
        // Implement reward logic for FarmRock
    }

    private void RewardForBuildObjects()
    {
        // Implement reward logic for BuildObjects
    }

    private void RewardForSpaceRace()
    {
        // Implement reward logic for SpaceRace
    }
}
