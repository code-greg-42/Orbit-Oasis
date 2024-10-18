using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor.Build.Reporting;
using UnityEngine;

// script execution time of -100 to run before other scripts
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    private readonly string saveFolderName = "/SaveData/";

    // data classes
    public PlayerData PlayerStats { get; private set; }
    public RaceData RaceStats { get; private set; }
    public TraderData TraderStats { get; private set; }
    public SerializableList<BuildableObjectData> BuildList { get; private set; }
    public SerializableList<ItemData> InventoryItems { get; private set; }
    public SerializableList<PlaceableItemData> PlacedItems { get; private set; }
    public SerializableList<AnimalData> SavedActiveAnimals { get; private set; }

    // tracking variables --- do not need to be saved to file
    public float PlayerBuildMaterial { get; private set; }
    public List<int> CaughtFishIndex { get; private set; }
    public bool NewGameStarted { get; private set; }
    public bool IntroLoadingTextShown { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CaughtFishIndex = new List<int>();

            CreateSaveDirectory();
            LoadAllData();

            Debug.Log("Player Currency: " + PlayerStats.PlayerCurrency);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        // set bool for story scene's manager
        NewGameStarted = true;

        // ADD THIS LATER --- WIPE ALL EXISTING FILE DATA
    }

    public void ResetNewGameStarted()
    {
        NewGameStarted = false;
    }

    public void SetIntroLoadingTextShown()
    {
        IntroLoadingTextShown = true;
    }

    public void SetRaceWon(float currencyAmount)
    {
        RaceStats.RaceWon = true;
        RaceStats.RewardCurrency = currencyAmount;
    }

    public void SetRaceCompleted()
    {
        RaceStats.RaceCompleted = true;
    }

    public void ResetRaceRewards()
    {
        RaceStats.RaceCompleted = false;
        RaceStats.RaceWon = false;
        RaceStats.RewardCurrency = 0;
        SaveRaceStats();
    }

    public void SetRaceBestTime(float raceTime)
    {
        if (RaceStats.BestTimes[RaceStats.SelectedDifficulty] == 0f || raceTime < RaceStats.BestTimes[RaceStats.SelectedDifficulty])
        {
            RaceStats.BestTimes[RaceStats.SelectedDifficulty] = raceTime;
            SaveRaceStats();
        }
    }

    public void UpgradeBoost()
    {
        if (RaceStats.BoostUpgradeLevel < RaceStats.MaxBoostLevel)
        {
            RaceStats.BoostUpgradeLevel++;
            SaveRaceStats();
        }
    }

    public void UpgradeRockets()
    {
        if (RaceStats.RocketUpgradeLevel < RaceStats.MaxRocketLevel)
        {
            RaceStats.RocketUpgradeLevel++;
            SaveRaceStats();
        }
    }

    public void SetRaceDifficulty(int difficulty)
    {
        if (difficulty >= 0 && difficulty <= 2)
        {
            RaceStats.SelectedDifficulty = difficulty;
        }
    }

    //public void SetPlayerPosition(Vector3 playerPos, Quaternion playerRot)
    //{
    //    PlayerStats.PlayerPosition = playerPos;
    //    PlayerStats.PlayerRotation = playerRot;
    //}

    //public void SetCameraValues(float mouseX, float mouseY)
    //{
    //    PlayerStats.CameraX = mouseX;
    //    PlayerStats.CameraY = mouseY;
    //}

    public void SetPlayerAndCamera(Vector3 playerPos, Quaternion playerRot, CinemachineFreeLook cinemachineCam)
    {
        PlayerStats.PlayerPosition = playerPos;
        PlayerStats.PlayerRotation = playerRot;

        PlayerStats.CameraX = cinemachineCam.m_XAxis.Value;
        PlayerStats.CameraY = cinemachineCam.m_YAxis.Value;
    }

    public void AddBuild(BuildableObject build)
    {
        // create new build data instance with the values from the build
        BuildableObjectData buildData = new(build.transform.position, build.transform.rotation, build.BuildPrefabIndex);

        // add to list
        BuildList.ItemList.Add(buildData);

        // save to file
        SaveBuildList();
    }

    public void RemoveBuild(BuildableObject build)
    {
        // accounts for floating point inconsistencies
        BuildList.ItemList.RemoveAll(b => (b.placementPosition - build.transform.position).sqrMagnitude < 0.0001f
            && Quaternion.Angle(b.placementRotation, build.transform.rotation) < 0.01f);

        // save to file
        SaveBuildList();
    }

    public void AddItem(Item item)
    {
        // create new item data instance with the values from the item
        ItemData itemData = new(item.ItemName, item.PrefabIndex, item.Quantity);

        // add to list
        InventoryItems.ItemList.Add(itemData);

        // save to file
        SaveInventory();
    }

    public void AddPlacedItem(PlaceableItem placeableItem)
    {
        // create new placeable item data instance with the values from the item
        PlaceableItemData placeableItemData = new(placeableItem.transform.position, placeableItem.transform.rotation, placeableItem.PrefabIndex);

        // add to list
        PlacedItems.ItemList.Add(placeableItemData);

        Debug.Log("Added a placed item. Number of placed items: " + PlacedItems.ItemList.Count);

        // save to file
        SavePlacedItems();
    }

    public void AddTraderItems(List<Item> items, float timer)
    {
        // clear the existing list
        TraderStats.TraderItems.ItemList.Clear();

        // add each item to the serializable list
        foreach (Item item in items)
        {
            ItemData itemData = new(item.ItemName, item.PrefabIndex, item.Quantity);
            TraderStats.TraderItems.ItemList.Add(itemData);
        }

        // update refresh timer -- also saves all trader data to file
        SetTraderRefreshTimer(timer);
    }

    public void RemoveItem(Item item)
    {
        int index = InventoryItems.ItemList.FindIndex(x => x.itemName == item.ItemName && x.quantity == item.Quantity);

        if (index != -1)
        {
            InventoryItems.ItemList.RemoveAt(index);

            // save to file
            SaveInventory();
        }
        else
        {
            Debug.LogWarning("Attempted to remove item, but item not found in InventoryItems list.");
        }
    }

    public void RemovePlacedItem(PlaceableItem placeableItem)
    {
        // accounts for floating point inconsistencies
        PlacedItems.ItemList.RemoveAll(x => (x.placementPosition - placeableItem.transform.position).sqrMagnitude < 0.0001f
            && Quaternion.Angle(x.placementRotation, placeableItem.transform.rotation) < 0.01f);

        // save to file
        SavePlacedItems();
    }

    public void RemoveSingleTraderItem(Item item, float timer)
    {
        int index = TraderStats.TraderItems.ItemList.FindIndex(x => x.itemName == item.ItemName && x.quantity == item.Quantity);

        if (index != -1)
        {
            TraderStats.TraderItems.ItemList.RemoveAt(index);

            // update refresh timer (also saves all trader-data to file)
            SetTraderRefreshTimer(timer);
        }
        else
        {
            Debug.LogWarning("Attempted to remove item, but item not found in TraderItems list.");
        }
    }

    public void SetTraderRefreshTimer(float timer)
    {
        TraderStats.RefreshTimer = timer;

        // save to file
        SaveTraderData();
    }

    public void ChangeItemQuantity(Item item, int newQuantity)
    {
        int index = InventoryItems.ItemList.FindIndex(x => x.itemName == item.ItemName && x.quantity == item.Quantity);

        if (index != -1)
        {
            InventoryItems.ItemList[index].quantity = newQuantity;
        }
        else
        {
            Debug.LogWarning("Attempted to change item quantity, but item was not found in InventoryItems list.");
        }

        // save to file
        SaveInventory();
    }

    // build material only used as a tracker for how much material is in inventory --- no need to save
    public void AddBuildMaterial(float amount)
    {
        PlayerBuildMaterial += amount;
    }

    public void AddCurrency(float amount)
    {
        PlayerStats.PlayerCurrency += amount;

        // update in-game UI
        MainUIManager.Instance.UpdateCurrencyDisplay(PlayerStats.PlayerCurrency, amount);

        // save to file
        SavePlayerStats();
    }

    // used only on load --- DO NOT SAVE TO FILE
    public void ClearInventoryItems()
    {
        InventoryItems.ItemList.Clear();
    }

    // used only on load --- DO NOT SAVE TO FILE
    public void ClearPlayerBuildMaterial()
    {
        PlayerBuildMaterial = 0;
    }

    // build material only used as a tracker for how much material is in inventory --- no need to save
    public void SubtractBuildMaterial(float amount)
    {
        PlayerBuildMaterial -= amount;
    }

    public void SubtractCurrency(float amount)
    {
        PlayerStats.PlayerCurrency -= amount;

        // update in-game UI --- use set amount to negative for correct floating text
        MainUIManager.Instance.UpdateCurrencyDisplay(PlayerStats.PlayerCurrency, -amount);

        // save to file
        SavePlayerStats();
    }

    private void CreateSaveDirectory()
    {
        string directoryPath = Application.persistentDataPath + saveFolderName;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private string GetSaveFilePath(string variableName)
    {
        string filePath = Application.persistentDataPath + saveFolderName + variableName + ".json";
        return filePath;
    }

    private void SaveToFile(object data, string variableName)
    {
        string filePath = GetSaveFilePath(variableName);
        string json = JsonUtility.ToJson(data);

        File.WriteAllText(filePath, json);
    }

    private T LoadFromFile<T>(string variableName)
    {
        string filePath = GetSaveFilePath(variableName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(json);
        }
        return default;
    }

    private void SavePlayerStats(Vector3? playerPos = null, Quaternion? playerRot = null, CinemachineFreeLook cam = null)
    {
        if (playerPos.HasValue && playerRot.HasValue && cam != null)
        {
            SetPlayerAndCamera(playerPos.Value, playerRot.Value, cam);
        }

        SaveToFile(PlayerStats, nameof(PlayerStats));
    }

    private void LoadPlayerStats()
    {
        PlayerStats = LoadFromFile<PlayerData>(nameof(PlayerStats)) ?? new PlayerData();
    }

    private void SaveRaceStats()
    {
        SaveToFile(RaceStats, nameof(RaceStats));
    }

    private void LoadRaceStats()
    {
        RaceStats = LoadFromFile<RaceData>(nameof(RaceStats)) ?? new RaceData();
    }

    private void SaveBuildList()
    {
        SaveToFile(BuildList, nameof(BuildList));
    }

    private void LoadBuildList()
    {
        // load list from file, otherwise init new list
        BuildList = LoadFromFile<SerializableList<BuildableObjectData>>(nameof(BuildList))
            ?? new SerializableList<BuildableObjectData>(new List<BuildableObjectData>());
    }

    private void SaveInventory()
    {
        SaveToFile(InventoryItems, nameof(InventoryItems));
    }

    private void LoadInventory()
    {
        // load list from file, otherwise init new list
        InventoryItems = LoadFromFile<SerializableList<ItemData>>(nameof(InventoryItems))
            ?? new SerializableList<ItemData>(new List<ItemData>());
    }

    private void SavePlacedItems()
    {
        SaveToFile(PlacedItems, nameof(PlacedItems));
    }

    private void LoadPlacedItems()
    {
        // load list from file, otherwise init new list
        PlacedItems = LoadFromFile<SerializableList<PlaceableItemData>>(nameof(PlacedItems))
            ?? new SerializableList<PlaceableItemData>(new List<PlaceableItemData>());
    }

    private void SaveTraderData()
    {
        SaveToFile(TraderStats, nameof(TraderStats));
    }

    private void LoadTraderData()
    {
        TraderStats = LoadFromFile<TraderData>(nameof(TraderStats))
                      ?? new TraderData();

        // Ensure TraderItems is instantiated even if the file is not found
        if (TraderStats.TraderItems == null)
        {
            TraderStats.TraderItems = new SerializableList<ItemData>(new List<ItemData>());
        }
    }

    public void SaveActiveAnimals(List<Animal> activeAnimals = null)
    {
        if (activeAnimals != null)
        {
            // step 1: create new animal data list
            List<AnimalData> newSavedActiveAnimals = new();

            // step 2: loop through active animals list, and create a new <AnimalData> for each entry
            foreach (Animal animal in activeAnimals)
            {
                AnimalData animalData = new(animal.transform.position, animal.transform.rotation, animal.PrefabIndex);
                newSavedActiveAnimals.Add(animalData);
            }

            // step 3: update SavedAnimals and save to file
            SavedActiveAnimals.ItemList = newSavedActiveAnimals;
        }
        
        SaveToFile(SavedActiveAnimals, nameof(SavedActiveAnimals));
    }

    private void LoadActiveAnimals()
    {
        // load list from file, otherwise init new list
        SavedActiveAnimals = LoadFromFile<SerializableList<AnimalData>>(nameof(SavedActiveAnimals))
            ?? new SerializableList<AnimalData>(new List<AnimalData>());
    }

    public void SaveDynamicData(Vector3 playerPos, Quaternion playerRot, CinemachineFreeLook cam, List<Animal> activeAnimals)
    {
        SavePlayerStats(playerPos, playerRot, cam);
        SaveActiveAnimals(activeAnimals);
    }

    private void LoadAllData()
    {
        // load/instantiate all data components
        LoadPlayerStats();
        LoadRaceStats();
        LoadBuildList();
        LoadPlacedItems();
        LoadInventory();
        LoadTraderData();
        LoadActiveAnimals();
    }








    // FISHING --- CURRENTLY UNUSED --- POSSIBLY IMPLEMENT LATER

    public void AddCaughtFish(int index)
    {
        CaughtFishIndex.Add(index);
    }

    public void ClearCaughtFish()
    {
        CaughtFishIndex.Clear();
    }

    // FOOD MANAGEMENT SYSTEM --- CURRENTLY UNUSED --- GAME DESIGN DECISION

    //public void SubtractFood(float amount)
    //{
    //    PlayerStats.PlayerFood -= amount;

    //    // save to file
    //    SavePlayerStats();
    //}

    //public void AddFood(float amount)
    //{
    //    PlayerStats.PlayerFood += amount;

    //    // save to file
    //    SavePlayerStats();
    //}
}
