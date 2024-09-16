using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance;

    [Header("Static Text Field")]
    [SerializeField] private TMP_Text currencyText;

    [Header("Floating Text Prefab")]
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Farming Indicator")]
    [SerializeField] private GameObject farmingIndicator;
    [SerializeField] private Image farmingIndicatorPanel;
    [SerializeField] private TMP_Text farmingIndicatorText;
    [SerializeField] private Image successIndicator;

    // farming indicator settings
    private const float farmingIndicatorFadeTime = 0.25f;
    private const float farmingSuccessFadeTime = 0.5f;
    private Vector3 farmingIndicatorOriginalScale;
    private float farmingSuccessScaleAmount = 1.2f;
    private Color farmingIndicatorPanelStartColor;
    private Color farmingIndicatorTextStartColor;
    private Color successIndicatorStartColor;
    private Coroutine deactivateFarmingIndicatorCoroutine;

    // floating text settings
    private Vector3 floatingTextSpawnOffset = new(0, 30, 0);

    private const string currencySymbol = "$";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        farmingIndicatorPanelStartColor = farmingIndicatorPanel.color;
        farmingIndicatorTextStartColor = farmingIndicatorText.color;
        successIndicatorStartColor = successIndicator.color;
        farmingIndicatorOriginalScale = farmingIndicator.transform.localScale;

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

    public void ActivateFarmingIndicator()
    {
        if (!farmingIndicator.activeInHierarchy)
        {
            farmingIndicator.SetActive(true);
        }
        else
        {
            if (deactivateFarmingIndicatorCoroutine != null)
            {
                StopCoroutine(deactivateFarmingIndicatorCoroutine);
                deactivateFarmingIndicatorCoroutine = null;

                ResetFarmingIndicator();
                farmingIndicator.SetActive(true);
            }
        }
    }

    public void DeactivateFarmingIndicator(bool success = false)
    {
        if (farmingIndicator.activeInHierarchy && deactivateFarmingIndicatorCoroutine == null)
        {
            deactivateFarmingIndicatorCoroutine = StartCoroutine(DeactivateFarmingIndicatorCoroutine(success));
        }
    }

    private IEnumerator DeactivateFarmingIndicatorCoroutine(bool success)
    {
        float timer = 0f;
        float duration = success ? farmingSuccessFadeTime : farmingIndicatorFadeTime;

        Vector3 targetScale = farmingIndicatorOriginalScale * farmingSuccessScaleAmount;

        if (success)
        {
            // activate highlight gameobject
            successIndicator.gameObject.SetActive(true);

            
        }

        // while loop to perform the fade
        while (timer < duration)
        {
            timer += Time.deltaTime;

            farmingIndicatorPanel.color = GetFadedColor(farmingIndicatorPanelStartColor, farmingIndicatorPanelStartColor.a, 0f, timer, duration);
            farmingIndicatorText.color = GetFadedColor(farmingIndicatorTextStartColor, farmingIndicatorTextStartColor.a, 0f, timer, duration);

            if (success)
            {
                successIndicator.color = GetFadedColor(successIndicatorStartColor, successIndicatorStartColor.a, 0f, timer, duration);

                // adjust scale of the indicator for effect
                farmingIndicator.transform.localScale = Vector3.Lerp(farmingIndicatorOriginalScale, targetScale, timer / duration);
            }

            yield return null;
        }

        farmingIndicator.SetActive(false);
        ResetFarmingIndicator();

        // manually set coroutine reference to null once it's done
        deactivateFarmingIndicatorCoroutine = null;
    }

    private Color GetFadedColor(Color startColor, float startAlpha, float targetAlpha, float timer, float duration)
    {
        float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
        Color newColor = new(startColor.r, startColor.g, startColor.b, newAlpha);
        return newColor;
    }

    private void ResetFarmingIndicator()
    {
        farmingIndicatorPanel.color = farmingIndicatorPanelStartColor;
        farmingIndicatorText.color = farmingIndicatorTextStartColor;
        farmingIndicator.transform.localScale = farmingIndicatorOriginalScale;
        successIndicator.color = successIndicatorStartColor;
        successIndicator.gameObject.SetActive(false);
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
