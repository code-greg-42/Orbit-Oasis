using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private int questCompletionAmount;
    private int questTotalNeeded = 10;
    private int activeQuestIndex;

    public enum IntroQuest
    {
        RemoveDeadTrees,
        SellDeadTrees,
        PlantTrees,
        PlantRocks,
        FarmTree,
        FarmRock,
        BuildObjects,
        SpaceRace,
    }

    private IntroQuest[] introQuests =
    {
        IntroQuest.RemoveDeadTrees,
        IntroQuest.SellDeadTrees,
        IntroQuest.PlantTrees,
        IntroQuest.PlantRocks,
        IntroQuest.FarmTree,
        IntroQuest.FarmRock,
        IntroQuest.BuildObjects,
        IntroQuest.SpaceRace
    };
    
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCurrentQuest()
    {
        questCompletionAmount++;
        Debug.Log($"Quest updated! Completion: {questCompletionAmount}/{questTotalNeeded}");
        MainUIManager.Instance.UpdateQuestProgress(questCompletionAmount, questTotalNeeded);
    }

    public void UpdateQuestRequirements(int requiredAmount)
    {
        questTotalNeeded = requiredAmount;
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

    private void RewardForPlantRocks()
    {
        // Implement reward logic for PlantRocks
    }

    private void RewardForFarmTree()
    {
        // Implement reward logic for FarmTree
    }

    private void RewardForFarmRock()
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
