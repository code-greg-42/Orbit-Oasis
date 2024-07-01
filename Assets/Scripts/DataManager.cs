using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public float PlayerCurrency { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddCurrency(float amount)
    {
        PlayerCurrency += amount;
    }

    public void SubtractCurrency(float amount)
    {
        PlayerCurrency -= amount;
    }
}
