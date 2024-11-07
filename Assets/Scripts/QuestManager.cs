using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private int questProgress;
    private int activeQuestIndex;

    private Coroutine completeQuestCoroutine;
    private Coroutine startNewQuestCoroutine;

    [Header("Sell Dead Trees Quest Rewards")]
    [SerializeField] private GameObject[] treePrefabs;
    
    [Header("Plant New Trees Quest Rewards")]
    [SerializeField] private Renderer groundRenderer;
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Undo Last Build Quest Rewards")]
    [SerializeField] private GameObject[] buildMaterialPrefabs;

    // gemstone collection quest
    private readonly string[] gemstoneNames = { "Blue Gemstone", "Green Gemstone", "Red Gemstone", "Gold Gemstone" };

    [Header("Tutorial Completion Rewards")]
    [SerializeField] private GameObject[] tutorialRewardPrefabs;

    // grass color change variables
    private Material changeableGrassMaterial;
    private Color aliveGrassColor = new(1f, 1f, 1f, 1f);
    private const float changeGrassFadeDuration = 5f;
    private Coroutine changeGrassColorCoroutine;

    // quest indices and corresponding flags for disallowing certain actions before the quest has introduced it
    private int inventoryQuestIndex;
    private int farmingQuestIndex;
    private int buildingQuestIndex;
    private int spaceRaceQuestIndex;
    public bool InventoryQuestReached => activeQuestIndex >= inventoryQuestIndex;
    public bool FarmingQuestReached => activeQuestIndex >= farmingQuestIndex;
    public bool BuildingQuestReached => activeQuestIndex >= buildingQuestIndex;
    public bool SpaceRaceQuestReached => activeQuestIndex >= spaceRaceQuestIndex;
    public bool AllowSellFarmables => activeQuestIndex >= buildingQuestIndex; // quest completion point where farmables are no longer required
    public bool AllowSellBuildMaterial => activeQuestIndex >= spaceRaceQuestIndex; // quest completion point where building is no longer required

    // quest info for other scripts
    private int deadTreesToSpawn;
    public int DeadTreesToSpawn => deadTreesToSpawn;

    public bool QuestLogActive => MainUIManager.Instance.QuestPanelActive;
    public string[] GemstoneNames => gemstoneNames;

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
        OpenBuildMode,
        PlaceABuild,
        UndoLastBuild,
        PlaceMoreBuilds,
        SpaceRace,
    }

    private Quest[] introQuests;
    
    private void Awake()
    {
        Instance = this;

        introQuests = new Quest[]
        {
            new Quest("Remove Dead Trees", IntroQuest.RemoveDeadTrees, 10, null),
            new Quest("Sell Dead Trees", IntroQuest.SellDeadTrees, 10, RewardForSellDeadTrees),
            new Quest("Plant New Trees", IntroQuest.PlantNewTrees, 6, RewardForPlantNewTrees, PlantNewTreesIntroAction),
            new Quest("Place Rocks", IntroQuest.PlaceRocks, 2, null),
            new Quest("Farm Tree", IntroQuest.FarmTree, 1, null),
            new Quest("Collect Wood", IntroQuest.CollectWood, 1, null),
            new Quest("Collect More Wood", IntroQuest.CollectMoreWood, 10, null),
            new Quest("Collect Stones", IntroQuest.CollectStones, 5, null),
            new Quest("Place A Build", IntroQuest.PlaceABuild, 1, RewardForPlaceABuild),
            new Quest("Undo Last Build", IntroQuest.UndoLastBuild, 1, RewardForUndoLastBuild, UndoLastBuildIntroAction),
            new Quest("Place More Builds", IntroQuest.PlaceMoreBuilds, 20, RewardForPlaceMoreBuilds, PlaceMoreBuildsIntroAction),
            new Quest("Try Space Race", IntroQuest.SpaceRace, 1, RewardForSpaceRace)
        };

        // get quest index and progress from data manager
        activeQuestIndex = DataManager.Instance.PlayerStats.QuestIndex;
        questProgress = DataManager.Instance.PlayerStats.QuestProgress;

        // set indices here so the get quest index loop only has to run one time per quest index
        inventoryQuestIndex = GetQuestIndex(IntroQuest.SellDeadTrees);
        farmingQuestIndex = GetQuestIndex(IntroQuest.FarmTree);
        buildingQuestIndex = GetQuestIndex(IntroQuest.PlaceABuild);
        spaceRaceQuestIndex = GetQuestIndex(IntroQuest.SpaceRace);

        // if on the remove dead trees quest, set the number for use by the DeadTreeSpawner script
        if (GetCurrentQuest() == IntroQuest.RemoveDeadTrees)
        {
            int deadTreesQuestIndex = GetQuestIndex(IntroQuest.RemoveDeadTrees);
            Quest deadTreesQuest = introQuests[deadTreesQuestIndex];

            // set amount accounting for any previously completed removed trees
            deadTreesToSpawn = deadTreesQuest.TotalNeeded - questProgress;
        }
    }

    private void Start()
    {
        HandleInitialGrassColor();
        StartCoroutine(LoadQuest());
    }

    public IntroQuest? GetCurrentQuest()
    {
        if (activeQuestIndex >= 0 && activeQuestIndex < introQuests.Length)
        {
            return introQuests[activeQuestIndex].QuestType;
        }
        else
        {
            return null;
        }
    }

    private void HandleInitialGrassColor()
    {
        // create clone of grass material
        changeableGrassMaterial = Instantiate(groundRenderer.material);

        int plantTreesIndex = GetQuestIndex(IntroQuest.PlantNewTrees);
        // if player has already reached grass growing quest, set color to alive color instantly
        if (activeQuestIndex > plantTreesIndex || (activeQuestIndex == plantTreesIndex && questProgress > 0))
        {
            changeableGrassMaterial.color = aliveGrassColor;
        }

        // set renderer's material to the changeableMaterial, which will either start as alive or be eligible to be changed later
        groundRenderer.material = changeableGrassMaterial;
    }

    private IEnumerator LoadQuest()
    {
        if (activeQuestIndex < introQuests.Length)
        {
            // activate and update tutorial progress bar
            MainUIManager.Instance.ActivateTutorialProgressPanel();
            MainUIManager.Instance.UpdateTutorialProgressBar(activeQuestIndex, introQuests.Length);

            if (questProgress > 0 || DataManager.Instance.RaceStats.RaceCompleted)
            {
                // activate and update quest log while load screen is still active
                StartNewQuest(false);
            }
            else
            {
                // wait for load screen to mostly finish then start dialogue of new quest
                yield return new WaitForSeconds(MainGameManager.Instance.LoadingWaitTime);
                StartNewQuest();
            }
        }
    }

    private IEnumerator StartNewQuestCoroutine(bool showDialogue)
    {
        // get current quest using the index
        Quest currentQuest = introQuests[activeQuestIndex];

        // update questlog with new quest details
        MainUIManager.Instance.UpdateQuestLogWithNewQuest(currentQuest.QuestTitle, questProgress, currentQuest.TotalNeeded);

        if (showDialogue)
        {
            // get intro dialogue for new quest
            List<string> introDialogue = DialogueManager.Instance.GetDialogue(currentQuest.IntroDialoguePath);

            // display dialogue and wait for the user to either press enter or for the timer to elapse
            yield return DialogueManager.Instance.ShowDialogue(introDialogue, true);

            // execute intro action if available -- if null nothing occurs
            currentQuest.InitIntroAction();
        }

        // display UI QuestLog with new quest details
        MainUIManager.Instance.ActivateQuestLog();
    }

    public void UpdateCurrentQuest(int changeAmount = 1)
    {
        questProgress += changeAmount;

        // update data manager
        DataManager.Instance.UpdateQuestStatus(activeQuestIndex, questProgress);

        // update UI
        MainUIManager.Instance.UpdateQuestProgress(questProgress, introQuests[activeQuestIndex].TotalNeeded, changeAmount);

        if (questProgress >= introQuests[activeQuestIndex].TotalNeeded)
        {
            CompleteQuest();
        }
        else
        {
            // play quest progress sound
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.QuestProgress);
        }
    }

    private void StartNewQuest(bool showDialogue = true)
    {
        if (startNewQuestCoroutine != null)
        {
            StopCoroutine(startNewQuestCoroutine);
            startNewQuestCoroutine = null;
        }
        startNewQuestCoroutine = StartCoroutine(StartNewQuestCoroutine(showDialogue));
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

        // play quest completion sound
        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.QuestComplete);

        // update UI with overall tutorial progress
        MainUIManager.Instance.UpdateTutorialProgressBar(activeQuestIndex + 1, introQuests.Length);

        // if this will be the last quest completion, show tutorial bar success animation
        if (activeQuestIndex + 1 >= introQuests.Length)
        {
            // deactivate tutorial progress bar with success shown
            MainUIManager.Instance.ShowTutorialSuccess();
        }

        // give quest rewards to player and enact any necessary post-quest-completion logic
        completedQuest.CompleteQuest();

        // get completion dialogue from dialogue manager (fetches .txt file)
        List<string> completionDialogue = DialogueManager.Instance.GetDialogue(completedQuest.CompletionDialoguePath);

        // display dialogue and wait for the user to either press enter or the timer to elapse
        yield return DialogueManager.Instance.ShowDialogue(completionDialogue, true);

        // reset quest progress and change the quest index to the next intro quest
        questProgress = 0;
        activeQuestIndex++;

        // update data manager
        DataManager.Instance.UpdateQuestStatus(activeQuestIndex, questProgress);

        if (activeQuestIndex < introQuests.Length)
        {
            StartNewQuest();
        }
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

    private void RewardForSellDeadTrees()
    {
        // toggle off inventory menu if active
        if (InventoryManager.Instance.IsMenuActive)
        {
            InventoryManager.Instance.ToggleInventoryMenu(false);
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

    private void RewardForPlaceABuild()
    {
        // close out of build mode, with small delay to leave time for build to 100% finish building
        BuildManager.Instance.ToggleOffBuildMode();
    }

    private void RewardForUndoLastBuild()
    {
        // give build material reward to insure player has enough for next quest, and close out of build mode for aesthetics
        RewardItems(buildMaterialPrefabs, 0.4f);
        BuildManager.Instance.ToggleOffBuildMode();
    }

    private void RewardForPlaceMoreBuilds()
    {
        BuildManager.Instance.ToggleOffBuildMode();
    }

    private void RewardForSpaceRace()
    {
        RewardItems(tutorialRewardPrefabs, 0.3f);
    }

    // INTRO ACTIONS

    private void PlantNewTreesIntroAction()
    {
        changeGrassColorCoroutine ??= StartCoroutine(ChangeGrassColorCoroutine());
    }

    private void UndoLastBuildIntroAction()
    {
        BuildManager.Instance.ToggleOnBuildMode(0.05f);
    }

    private void PlaceMoreBuildsIntroAction()
    {
        BuildManager.Instance.ToggleOnBuildMode(0.05f);
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

    private int GetQuestIndex(IntroQuest quest)
    {
        for (int i = 0; i < introQuests.Length; i++)
        {
            if (introQuests[i].QuestType == quest)
            {
                return i;
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

    private void RewardItems(GameObject[] prefabArray, float percentageOfMaxStack = 1.0f)
    {
        if (prefabArray == null || prefabArray.Length == 0) return;

        foreach (GameObject prefab in prefabArray)
        {
            // instantiate new item
            GameObject newObject = Instantiate(prefab);

            // get item component and pickup item into player inventory
            if (newObject.TryGetComponent(out Item newItem))
            {
                // set quantity to max stack
                if (newItem.MaxStackQuantity > 1 && percentageOfMaxStack > 0)
                {
                    int newQuantity = Mathf.CeilToInt(newItem.MaxStackQuantity * percentageOfMaxStack);
                    int newQuantitySafe = Mathf.Min(newQuantity, newItem.MaxStackQuantity);

                    newItem.SetQuantity(newQuantitySafe);
                }
                
                // add to inventory
                newItem.PickupItem();
            }
        }
    }
}
