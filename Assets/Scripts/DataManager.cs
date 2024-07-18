using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement; // TEMPORARY FOR TESTING PURPOSES

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public float PlayerCurrency { get; private set; }
    public float PlayerFood { get; private set; }
    public float PlayerBuildMaterial { get; private set; }
    public List<BuildableObject> BuildList { get; private set; }
    public List<Item> InventoryItems { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize lists
            BuildList = new List<BuildableObject>();
            InventoryItems = new List<Item>();
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
        BuildList.Add(build);
    }

    public void RemoveBuild(BuildableObject build)
    {
        BuildList.RemoveAll(b => b.PlacementPosition == build.PlacementPosition && b.PlacementRotation == build.PlacementRotation);
    }

    public void AddItem(Item item)
    {
        InventoryItems.Add(item);
    }

    public void RemoveItem(Item item)
    {
        int index = InventoryItems.FindIndex(x => x.ItemName == item.ItemName && x.Quantity == item.Quantity);

        if (index != -1)
        {
            InventoryItems.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Attempted to remove item, but item not found in InventoryItems list.");
        }
    }

    public void AddBuildMaterial(float amount) { PlayerBuildMaterial += amount; Debug.Log("Build Material: " + PlayerBuildMaterial); }

    public void AddCurrency(float amount)
    {
        PlayerCurrency += amount;
    }

    public void AddFood(float amount)
    {
        PlayerFood += amount;
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
}
