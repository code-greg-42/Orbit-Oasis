using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement; // TEMPORARY FOR TESTING PURPOSES

// script execution time of -100 to run before other scripts
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private readonly string saveFolderName = "/SaveData/";

    // datasets
    public PlayerData PlayerStats { get; private set; }
    public RaceData RaceStats { get; private set; }
    public SerializableList<BuildableObjectData> BuildableList { get; private set; }
    public SerializableList<ItemData> InventoryItemList { get; private set; }


    // player variables
    public float PlayerCurrency { get; private set; }
    public float PlayerFood { get; private set; }
    public Vector3 PlayerPosition { get; private set; }
    public Quaternion PlayerRotation { get; private set; }


    // lists
    public List<BuildableObjectData> BuildList { get; private set; }
    public List<ItemData> InventoryItems { get; private set; }
    

    // race variables
    //public int RaceSelectedDifficulty { get; private set; }
    //public int RaceBoostUpgradeLevel { get; private set; }
    //public int RaceRocketUpgradeLevel { get; private set; }
    //public float[] RaceBestTimes { get; private set; }


    // unsaved variables
    public float PlayerBuildMaterial { get; private set; }
    public List<int> CaughtFishIndex { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize new list
            PlayerStats = new PlayerData();
            RaceStats = new RaceData();
            BuildableList = new SerializableList<BuildableObjectData>(new List<BuildableObjectData>());
            InventoryItemList = new SerializableList<ItemData>(new List<ItemData>());

            // initialize lists
            BuildList = new List<BuildableObjectData>();
            InventoryItems = new List<ItemData>();
            CaughtFishIndex = new List<int>();
            //RaceBestTimes = new float[] { 0f, 0f, 0f };

            CreateSaveDirectory();

            LoadRaceStats();

            LoadGameFromFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // TEMPORARY FOR TESTING PURPOSES
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SwapScenes();
        }
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
        if (RaceStats.BoostUpgradeLevel < 3)
        {
            RaceStats.BoostUpgradeLevel++;
            SaveRaceStats();
        }
    }

    public void UpgradeRockets()
    {
        if (RaceStats.RocketUpgradeLevel < 3)
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

    public void SetPlayerPosition(Vector3 playerPos, Quaternion playerRot)
    {
        PlayerStats.PlayerPosition = playerPos;
        PlayerStats.PlayerRotation = playerRot;
    }

    public void SetCameraValues(float mouseX, float mouseY)
    {
        PlayerStats.CameraX = mouseX;
        PlayerStats.CameraY = mouseY;
    }

    public void AddBuild(BuildableObject build)
    {
        // create new build data instance with the values from the build
        BuildableObjectData buildData = new(build.transform.position, build.transform.rotation, build.BuildPrefabIndex);

        // add to list
        BuildList.Add(buildData);
    }

    public void RemoveBuild(BuildableObject build)
    {
        // accounts for float point inconsistencies
        BuildList.RemoveAll(b => (b.placementPosition - build.transform.position).sqrMagnitude < 0.0001f
            && Quaternion.Angle(b.placementRotation, build.transform.rotation) < 0.01f);
    }

    public void AddItem(Item item)
    {
        // create new item data instance with the values from the item
        ItemData itemData = new(item.ItemName, item.PrefabIndex, item.Quantity);

        // add to list
        InventoryItems.Add(itemData);
    }

    public void RemoveItem(Item item)
    {
        int index = InventoryItems.FindIndex(x => x.itemName == item.ItemName && x.quantity == item.Quantity);

        if (index != -1)
        {
            InventoryItems.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Attempted to remove item, but item not found in InventoryItems list.");
        }
    }

    public void ChangeItemQuantity(Item item, int newQuantity)
    {
        int index = InventoryItems.FindIndex(x => x.itemName == item.ItemName && x.quantity == item.Quantity);

        if (index != -1)
        {
            InventoryItems[index].quantity = newQuantity;
        }
        else
        {
            Debug.LogWarning("Attempted to change item quantity, but item was not found in InventoryItems list.");
        }
    }

    public void AddBuildMaterial(float amount)
    {
        PlayerBuildMaterial += amount; Debug.Log("Build Material: " + PlayerBuildMaterial);
    }

    public void AddCaughtFish(int index)
    {
        CaughtFishIndex.Add(index);
    }

    public void ClearCaughtFish()
    {
        CaughtFishIndex.Clear();
    }

    public void AddCurrency(float amount)
    {
        PlayerCurrency += amount;
    }

    public void AddFood(float amount)
    {
        PlayerFood += amount;
    }

    // used only on load
    public void ClearInventoryItems()
    {
        InventoryItems.Clear();
    }

    // used only on load
    public void ClearPlayerBuildMaterial()
    {
        PlayerBuildMaterial = 0;
    }

    public void SubtractBuildMaterial(float amount)
    {
        PlayerBuildMaterial -= amount;
    }

    public void SubtractCurrency(float amount)
    {
        PlayerCurrency -= amount;
    }

    public void SubtractFood(float amount)
    {
        PlayerFood -= amount;
    }

    // TEMPORARY METHOD TO TEST DATA PERSISTENCE
    private void SwapScenes()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex == 0)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    // add places to save game periodically later
    private void SaveGameToFile()
    {
        // convert DataManager values to <GameData> serializable class
        GameData gameData = new(PlayerCurrency, PlayerFood, BuildList, InventoryItems);

        // convert to json format
        string json = JsonUtility.ToJson(gameData);

        // write json to file
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        Debug.Log("Game Saved Successfully.");
    }

    private void LoadGameFromFile()
    {
        string path = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(path))
        {
            // read json from file
            string json = File.ReadAllText(path);

            // convert json back to <GameData>
            GameData gameData = JsonUtility.FromJson<GameData>(json);

            // set DataManager values to <GameData> values
            PlayerCurrency = gameData.playerCurrency;
            PlayerFood = gameData.playerFood;
            BuildList = gameData.buildList;
            InventoryItems = gameData.inventoryItems;

            Debug.Log("Game Loaded Successfully.");
        }
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

    private void SavePlayerStats()
    {
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
}
