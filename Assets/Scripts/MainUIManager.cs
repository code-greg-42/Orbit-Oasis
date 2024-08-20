using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance;

    [Header("Static Text Field")]
    [SerializeField] private TMP_Text currencyText;

    [Header("Floating Text Prefab")]
    [SerializeField] private GameObject floatingTextPrefab;

    // floating text settings
    private Vector3 floatingTextSpawnOffset = new(0, 30, 0);

    private const string currencySymbol = "$";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateCurrencyDisplay(DataManager.Instance.PlayerStats.PlayerCurrency);
    }

    public void UpdateCurrencyDisplay(float newAmount, float changeAmount = 0)
    {
        // format with commas
        string formattedAmount = newAmount.ToString("N0");

        // update text component
        currencyText.text = currencySymbol + formattedAmount;

        if (changeAmount != 0)
        {
            CreateFloatingText(changeAmount);
        }
    }

    private void CreateFloatingText(float changeAmount)
    {
        // set text values and color based on whether changeAmount is positive or negative
        string symbol = changeAmount > 0 ? "+" : "-";
        Color color = changeAmount > 0 ? Color.green : Color.red;
        string text = symbol + currencySymbol + Mathf.Abs(changeAmount).ToString("N0");

        // instantiate floating text prefab
        GameObject floatingTextInstance = Instantiate(floatingTextPrefab,
            currencyText.transform.position + floatingTextSpawnOffset, Quaternion.identity, currencyText.transform.parent);

        if (floatingTextInstance.TryGetComponent(out FloatingText floatingText))
        {
            floatingText.Init(text, color);
        }
    }
}
