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

    [Header("Quest 2")]
    [SerializeField] private GameObject[] treePrefabs;
    
    [Header("Quest 3")]
    [SerializeField] private Renderer groundRenderer;
    [SerializeField] private GameObject[] rockPrefabs;

    // grass color change variables
    private Material changeableGrassMaterial;
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
        FarmTree,
        CollectWood,
        CollectMoreWood,
        CollectStones,
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
            new Quest("Plant New Trees", IntroQuest.PlantNewTrees, 6, RewardForPlantNewTrees, PlantNewTreesIntroAction),
            new Quest("Place Rocks", IntroQuest.PlaceRocks, 2, RewardForPlaceRocks),
            new Quest("Farm Tree", IntroQuest.FarmTree, 1, RewardForFarmTree),
            new Quest("Collect Wood", IntroQuest.CollectWood, 1, RewardForCollectWood),
            new Quest("Collect More Wood", IntroQuest.CollectMoreWood, 10, RewardForCollectMoreWood),
            new Quest("Collect Stones", IntroQuest.CollectStones, 5, RewardForCollectStones),
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

        // update UI with overall tutorial progress
        MainUIManager.Instance.UpdateTutorialProgressBar(activeQuestIndex + 1, introQuests.Length);

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

        // get amount from the quest object, this way if it's changed it only needs to be changed in one place
        int treesToReward = GetTotalNeeded(IntroQuest.PlantNewTrees);

        RewardPlaceableItems(treePrefabs, treesToReward);
    }

    private void RewardForPlantNewTrees()
    {
        // get amount from quest object for next quest
        int rocksToReward = GetTotalNeeded(IntroQuest.PlaceRocks);

        RewardPlaceableItems(rockPrefabs, rocksToReward);
    }

    private void RewardForPlaceRocks()
    {
        // Implement reward logic for PlantRocks
    }

    private void RewardForFarmTree()
    {
        // Implement reward logic for FarmTree
    }

    private void RewardForCollectWood()
    {

    }

    private void RewardForCollectMoreWood()
    {

    }

    private void RewardForCollectStones()
    {

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

    // HELPER METHODS
    private int GetTotalNeeded(IntroQuest questType)
    {
        foreach (Quest quest in introQuests)
        {
            if (quest.QuestType == questType)
            {
                return quest.TotalNeeded;
            }
        }

        return -1;
    }

    private void RewardPlaceableItems(GameObject[] prefabArray, int amountToReward)
    {
        for (int i = 0; i < amountToReward; i++)
        {
            int prefabIndex;

            if (prefabArray.Length > 1)
            {
                // random roll for which item to reward
                prefabIndex = UnityEngine.Random.Range(0, prefabArray.Length);
            }
            else
            {
                prefabIndex = 0;
            }

            // instantiate new item
            GameObject newItem = Instantiate(prefabArray[prefabIndex]);

            // get item component and pickup item into player inventory
            if (newItem.TryGetComponent(out PlaceableItem placeableItem))
            {
                placeableItem.PickupItem();
            }
        }
    }
}
