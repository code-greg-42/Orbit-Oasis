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
    public float PlayerCurrency { get; private set; }
    public float PlayerFood { get; private set; }
    public float PlayerBuildMaterial { get; private set; }
    public List<BuildableObjectData> BuildList { get; private set; }
    public List<ItemData> InventoryItems { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize lists
            BuildList = new List<BuildableObjectData>();
            InventoryItems = new List<ItemData>();

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
}
