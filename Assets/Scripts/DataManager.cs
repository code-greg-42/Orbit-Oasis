using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // TEMPORARY FOR TESTING PURPOSES

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public float PlayerCurrency { get; private set; }
    public float PlayerFood { get; private set; }

    public float PlayerBuildMaterial { get; private set; }

    public List<GameObject> BuildList { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // initialize build list
            BuildList = new List<GameObject>();
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

    public void AddBuild(GameObject build)
    {
        BuildList.Add(build);
    }

    public void RemoveBuild(GameObject build)
    {
        BuildList.Remove(build);
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
