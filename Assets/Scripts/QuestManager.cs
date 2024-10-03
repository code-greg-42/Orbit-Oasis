using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private int questProgress;
    private int activeQuestIndex;

    private Coroutine completeQuestCoroutine;
    private Coroutine startNewQuestCoroutine;

    [SerializeField] private GameObject treePrefab;
    [SerializeField] private Renderer groundRenderer;

    // grass color change variables
    private Material changeableGrassMaterial;
    private Color deadGrassColor = new(0f, 9f/255f, 1f, 1f);
    private Color aliveGrassColor = new(1f, 1f, 1f, 1f);
    private const float changeGrassFadeDuration = 5f;
    private Coroutine changeGrassColorCoroutine;

    public bool QuestLogActive => MainUIManager.Instance.QuestPanelActive;

    public enum IntroQuest
    {
        RemoveDeadTrees,
        SellDeadTrees,
        PlantNewTrees,
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
            new Quest("Remove Dead Trees", IntroQuest.RemoveDeadTrees, 10, RewardForRemoveDeadTrees),
            new Quest("Sell Dead Trees", IntroQuest.SellDeadTrees, 10, RewardForSellDeadTrees),
            new Quest("Plant New Trees", IntroQuest.PlantNewTrees, 5, RewardForPlantNewTrees, PlantNewTreesIntroAction),
            new Quest("Place Rocks", IntroQuest.PlaceRocks, 3, RewardForPlaceRocks),
            new Quest("Farm Trees", IntroQuest.FarmTrees, 3, RewardForFarmTrees),
            new Quest("Mine Rocks", IntroQuest.MineRocks, 2, RewardForMineRocks),
            new Quest("Build Objects", IntroQuest.BuildObjects, 5, RewardForBuildObjects),
            new Quest("Complete The Space Race", IntroQuest.SpaceRace, 1, RewardForSpaceRace)
        };
    }

    private void Start()
    {
        // create clone of grass material
        changeableGrassMaterial = Instantiate(groundRenderer.material);

        // if player has already reached grass growing quest, set color to alive color instantly
        if (activeQuestIndex >= 3)
        {
            changeableGrassMaterial.color = aliveGrassColor;
        }

        // set renderer's material to the changeableMaterial, which will either start as alive or be eligible to be changed later
        groundRenderer.material = changeableGrassMaterial;
    }

    public IntroQuest GetCurrentQuest()
    {
        return introQuests[activeQuestIndex].QuestType;
    }

    private IEnumerator StartNewQuestCoroutine()
    {
        // get current quest using the index
        Quest currentQuest = introQuests[activeQuestIndex];

        // update questlog with new quest details
        MainUIManager.Instance.UpdateQuestLogWithNewQuest(currentQuest.QuestTitle, currentQuest.TotalNeeded);

        // get intro dialogue for new quest
        List<string> introDialogue = DialogueManager.Instance.GetDialogue(currentQuest.IntroDialoguePath);

        // display dialogue and wait for the user to either press enter or for the timer to elapse
        yield return DialogueManager.Instance.ShowDialogue(introDialogue, true);

        // display UI QuestLog with new quest details
        MainUIManager.Instance.ActivateQuestLog();

        // execute intro action if available -- if null nothing occurs
        currentQuest.InitIntroAction();
    }

    public void UpdateCurrentQuest(int changeAmount = 1)
    {
        questProgress += changeAmount;
        MainUIManager.Instance.UpdateQuestProgress(questProgress, introQuests[activeQuestIndex].TotalNeeded, changeAmount);

        if (questProgress >= introQuests[activeQuestIndex].TotalNeeded)
        {
            CompleteQuest();
        }
    }

    private void StartNewQuest()
    {
        if (startNewQuestCoroutine != null)
        {
            StopCoroutine(startNewQuestCoroutine);
            startNewQuestCoroutine = null;
        }
        startNewQuestCoroutine = StartCoroutine(StartNewQuestCoroutine());
    }

    private void CompleteQuest()
    {
        if (completeQuestCoroutine != null)
        {
            StopCoroutine(completeQuestCoroutine);
            completeQuestCoroutine = null;
        }
        completeQuestCoroutine = StartCoroutine(CompleteQuestCoroutine());
    }

    private IEnumerator CompleteQuestCoroutine()
    {
        // get current quest
        Quest completedQuest = introQuests[activeQuestIndex];

        // update UI with a successful quest completion
        MainUIManager.Instance.ShowQuestSuccess();

        // give quest rewards to player and enact any necessary post-quest-completion logic
        completedQuest.CompleteQuest();

        // get completion dialogue from dialogue manager (fetches .txt file)
        List<string> completionDialogue = DialogueManager.Instance.GetDialogue(completedQuest.CompletionDialoguePath);

        // display dialogue and wait for the user to either press enter or the timer to elapse
        yield return DialogueManager.Instance.ShowDialogue(completionDialogue, true);

        // reset quest progress and change the quest index to the next intro quest
        questProgress = 0;
        activeQuestIndex++;

        StartNewQuest();
    }

    private IEnumerator ChangeGrassColorCoroutine()
    {
        // warning and exit if the material is null
        if (changeableGrassMaterial == null)
        {
            Debug.LogWarning("Cannot execute ChangeGrassColor as changeableGrassMaterial is null.");
            yield break;
        }

        float elapsedTime = 0f;
        Color initialColor = changeableGrassMaterial.color;

        while (elapsedTime < changeGrassFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // smoothly transition color towards target alive color
            changeableGrassMaterial.color = Color.Lerp(initialColor, aliveGrassColor, elapsedTime / changeGrassFadeDuration);
            yield return null;
        }

        changeableGrassMaterial.color = aliveGrassColor;
    }

    // QUEST REWARDS

    // Define the reward methods
    private void RewardForRemoveDeadTrees()
    {
        // Implement reward logic for RemoveDeadTrees
    }

    private void RewardForSellDeadTrees()
    {
        // toggle off inventory menu if active
        if (InventoryManager.Instance.IsMenuActive)
        {
            InventoryManager.Instance.ToggleInventoryMenu();
        }

        // set amount of trees to reward -- must be higher than required planting amount for the next 'plant trees' intro quest
        int treesToReward = 8;

        // reward trees to plant
        for (int i = 0; i < treesToReward; i++)
        {
            // instantiate new tree object
            GameObject newTree = Instantiate(treePrefab);
            
            // get item component and pickup item into player inventory
            if (newTree.TryGetComponent(out PlaceableItem placeableTree))
            {
                placeableTree.PickupItem();
            }
        }
    }

    private void RewardForPlantNewTrees()
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

    // INTRO ACTIONS

    private void PlantNewTreesIntroAction()
    {
        changeGrassColorCoroutine ??= StartCoroutine(ChangeGrassColorCoroutine());
    }
}
