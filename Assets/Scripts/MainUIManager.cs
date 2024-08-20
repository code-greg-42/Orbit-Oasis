using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance;

    [Header("Static Text Fields")]
    [SerializeField] private TMP_Text currencyText;
    [Header("Floating Text Animations")]
    [SerializeField] private TMP_Text floatingCurrencyText;

    private const string currencySymbol = "$";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateCurrencyDisplay(DataManager.Instance.PlayerStats.PlayerCurrency);
    }

    public void UpdateCurrencyDisplay(float newAmount)
    {
        // format with commas
        string formattedAmount = newAmount.ToString("N0");

        // update text component
        currencyText.text = currencySymbol + formattedAmount;
    }
}
