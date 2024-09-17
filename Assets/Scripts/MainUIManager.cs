using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private Image farmingSuccessIndicator;

    [Header("Item Pickup Indicator")]
    [SerializeField] private GameObject itemPickupIndicator;
    [SerializeField] private Image itemPickupIndicatorPanel;
    [SerializeField] private TMP_Text itemPickupIndicatorText;
    [SerializeField] private Image itemPickupSuccessIndicator;

    // farming indicator settings
    private const float farmingIndicatorFadeTime = 0.25f;
    private const float farmingSuccessFadeTime = 0.5f;
    private Vector3 farmingIndicatorOriginalScale;
    private float farmingSuccessScaleAmount = 1.2f;
    private Color farmingIndicatorPanelStartColor;
    private Color farmingIndicatorTextStartColor;
    private Color successIndicatorStartColor;
    private Coroutine deactivateFarmingIndicatorCoroutine;

    private const float indicatorFadeTime = 0.25f;
    private const float indicatorSuccessFadeTime = 0.5f;
    private Vector3 indicatorOriginalScale;
    private float successScaleAmount = 1.2f;
    private Color indicatorPanelStartColor;
    private Color indicatorTextStartColor;
    private Coroutine deactivateItemPickupIndicatorCoroutine;

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
        successIndicatorStartColor = farmingSuccessIndicator.color;
        farmingIndicatorOriginalScale = farmingIndicator.transform.localScale;

        indicatorPanelStartColor = farmingIndicatorPanel.color;
        indicatorTextStartColor = farmingIndicatorText.color;
        successIndicatorStartColor = farmingSuccessIndicator.color;
        indicatorOriginalScale = farmingIndicator.transform.localScale;

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

    public void ActivateItemPickupIndicator()
    {
        if (!itemPickupIndicator.activeInHierarchy)
        {
            itemPickupIndicator.SetActive(true);
        }
        else
        {
            if (deactivateItemPickupIndicatorCoroutine != null)
            {
                StopCoroutine(deactivateItemPickupIndicatorCoroutine);
                deactivateItemPickupIndicatorCoroutine = null;

                itemPickupIndicatorPanel.color = indicatorPanelStartColor;
                itemPickupIndicatorText.color = indicatorTextStartColor;
                itemPickupIndicator.transform.localScale = indicatorOriginalScale;
                itemPickupSuccessIndicator.color = successIndicatorStartColor;
                itemPickupSuccessIndicator.gameObject.SetActive(false);
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

    public void DeactivateItemPickupIndicator(bool success = false)
    {
        if (itemPickupIndicator.activeInHierarchy && deactivateItemPickupIndicatorCoroutine == null)
        {
            deactivateItemPickupIndicatorCoroutine = StartCoroutine(DeactivateIndicatorCoroutine(success,
                itemPickupIndicator, itemPickupSuccessIndicator, itemPickupIndicatorPanel, itemPickupIndicatorText, false));
        }
    }

    private IEnumerator DeactivateIndicatorCoroutine(bool success, GameObject indicator, Image successIndicator,
        Image indicatorPanel, TMP_Text indicatorText, bool isFarming = true)
    {
        float timer = 0f;
        float duration = success ? indicatorSuccessFadeTime : indicatorFadeTime;

        Vector3 targetScale = indicatorOriginalScale * successScaleAmount;

        if (success)
        {
            // activate highlight gameobject
            successIndicator.gameObject.SetActive(true);
        }

        // while loop to perform the fade
        while (timer < duration)
        {
            timer += Time.deltaTime;

            indicatorPanel.color = GetFadedColor(indicatorPanelStartColor, indicatorPanelStartColor.a, 0f, timer, duration);
            indicatorText.color = GetFadedColor(indicatorTextStartColor, indicatorTextStartColor.a, 0f, timer, duration);

            if (success)
            {
                successIndicator.color = GetFadedColor(successIndicatorStartColor, successIndicatorStartColor.a, 0f, timer, duration);

                // adjust scale of the indicator for effect
                indicator.transform.localScale = Vector3.Lerp(farmingIndicatorOriginalScale, targetScale, timer / duration);
            }

            yield return null;
        }

        indicator.SetActive(false);
        indicatorPanel.color = indicatorPanelStartColor;
        indicatorText.color = indicatorTextStartColor;
        indicator.transform.localScale = indicatorOriginalScale;
        successIndicator.color = successIndicatorStartColor;
        successIndicator.gameObject.SetActive(false);

        // manually set coroutine reference to null once it's done
        if (isFarming)
        {
            deactivateFarmingIndicatorCoroutine = null;
        }
        else
        {
            deactivateItemPickupIndicatorCoroutine = null;
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
            farmingSuccessIndicator.gameObject.SetActive(true);

            
        }

        // while loop to perform the fade
        while (timer < duration)
        {
            timer += Time.deltaTime;

            farmingIndicatorPanel.color = GetFadedColor(farmingIndicatorPanelStartColor, farmingIndicatorPanelStartColor.a, 0f, timer, duration);
            farmingIndicatorText.color = GetFadedColor(farmingIndicatorTextStartColor, farmingIndicatorTextStartColor.a, 0f, timer, duration);

            if (success)
            {
                farmingSuccessIndicator.color = GetFadedColor(successIndicatorStartColor, successIndicatorStartColor.a, 0f, timer, duration);

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
        farmingSuccessIndicator.color = successIndicatorStartColor;
        farmingSuccessIndicator.gameObject.SetActive(false);
    }

    private void ResetItemPickupIndicator()
    {
        itemPickupIndicatorPanel.color = indicatorPanelStartColor;
        itemPickupIndicatorText.color = indicatorTextStartColor;
        itemPickupIndicator.transform.localScale = indicatorOriginalScale;
        itemPickupSuccessIndicator.color = successIndicatorStartColor;
        itemPickupSuccessIndicator.gameObject.SetActive(false);
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
