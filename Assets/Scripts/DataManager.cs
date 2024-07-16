using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public float PlayerCurrency { get; private set; }
    public float PlayerFood { get; private set; }

    public float PlayerBuildMaterial { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddBuildMaterial(float amount) { PlayerBuildMaterial += amount; Debug.Log("Build Material: " + PlayerBuildMaterial); }

    public void AddCurrency(float amount) { PlayerCurrency += amount; }

    public void AddFood(float amount) {  PlayerFood += amount; }

    public void SubtractBuildMaterial(float amount) { PlayerBuildMaterial -= amount; }

    public void SubtractCurrency(float amount) { PlayerCurrency -= amount; }

    public void SubtractFood(float amount) { PlayerFood -= amount; }
}
