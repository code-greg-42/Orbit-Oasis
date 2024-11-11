using System;
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

    [Header("Alert Text Field")]
    [SerializeField] private TMP_Text alertText;
    private Coroutine alertTextCoroutine;
    private float alertFadeDuration = 0.5f;

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

    [Header("Quest Log")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Image mainQuestPanelImage;
    [SerializeField] private Image[] extraQuestPanelImages;
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text questProgressText;
    [SerializeField] private Color questPanelDefaultColor;
    [SerializeField] private Color questPanelSuccessColor;

    [Header("Tutorial Progress")]
    [SerializeField] private Image tutorialProgressBar;
    [SerializeField] private Image tutorialProgressBarBackground;
    [SerializeField] private Image tutorialProgressPanel;
    [SerializeField] private TMP_Text tutorialProgressText;

    [Header("Build Mode Indicators")]
    [SerializeField] private GameObject buildModeIndicator;
    [SerializeField] private GameObject deleteModeIndicator;

    [Header("Controls Display")]
    [SerializeField] private GameObject controlsDisplayPanel;
    [SerializeField] private GameObject inventoryControlsPanel;
    [SerializeField] private GameObject buildModeControlsPanel;
    [SerializeField] private GameObject traderMenuControlsPanel;
    [SerializeField] private GameObject itemPlacementControlsPanel;

    // tutorial progress settings
    private float tutorialProgressFadeDuration = 0.3f;
    private Coroutine tutorialProgressBarCoroutine;

    // quest log settings
    private float questSuccessFadeDuration = 1.4f;
    private float questFadeInDuration = 0.8f;
    private Coroutine questFadeInCoroutine;
    private Coroutine showQuestSuccessCoroutine;
    private Color questTextStartColor;

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

    public bool QuestPanelActive => questPanel.activeInHierarchy;

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

        questTextStartColor = questProgressText.color;

        UpdateCurrencyDisplay(DataManager.Instance.PlayerStats.PlayerCurrency);
        InitControlsDisplay();
    }

    public void UpdateCurrencyDisplay(float newAmount, float changeAmount = 0)
    {
        // format with commas
        string formattedAmount = newAmount.ToString("N0");

        // update text component
        currencyText.text = currencySymbol + formattedAmount;

        if (changeAmount != 0)
        {
            CreateFloatingCurrencyText(changeAmount);
        }
    }

    public void ActivateControlsDisplay()
    {
        if (!controlsDisplayPanel.activeInHierarchy)
        {
            controlsDisplayPanel.SetActive(true);
        }
    }

    public void DeactivateControlsDisplay()
    {
        if (controlsDisplayPanel.activeInHierarchy)
        {
            controlsDisplayPanel.SetActive(false);
        }
    }

    public void ActivateItemPlacementControlsPanel()
    {
        if (!itemPlacementControlsPanel.activeInHierarchy)
        {
            itemPlacementControlsPanel.SetActive(true);
        }
    }

    public void DeactivateItemPlacementControlsPanel()
    {
        if (itemPlacementControlsPanel.activeInHierarchy)
        {
            itemPlacementControlsPanel.SetActive(false);
        }
    }

    public void UpdateControlsDisplay(QuestManager.IntroQuest introQuest)
    {
        // activate panels based on the quest started/completed

        switch (introQuest)
        {
            case QuestManager.IntroQuest.SellDeadTrees:
                inventoryControlsPanel.SetActive(true);
                break;

            case QuestManager.IntroQuest.PlaceABuild:
                buildModeControlsPanel.SetActive(true);
                break;

            case QuestManager.IntroQuest.SpaceRace:
                traderMenuControlsPanel.SetActive(true);
                break;

            default:
                Debug.LogWarning("IntroQuest given to MainUIManager.UpdateControlsDisplay() does not match.");
                break;
        }
    }

    private void InitControlsDisplay()
    {
        // activate panels based on which quests have been reached

        if (QuestManager.Instance.InventoryQuestReached)
        {
            inventoryControlsPanel.SetActive(true);
        }

        if (QuestManager.Instance.BuildingQuestReached)
        {
            buildModeControlsPanel.SetActive(true);
        }

        if (QuestManager.Instance.AllQuestsCompleted)
        {
            traderMenuControlsPanel.SetActive(true);
        }
    }

    public void ActivateTutorialProgressPanel()
    {
        tutorialProgressPanel.gameObject.SetActive(true);
    }

    public void ShowTutorialSuccess()
    {
        Color newColor = new(0, 1, 0, tutorialProgressPanel.color.a);
        tutorialProgressPanel.color = newColor;
        StartCoroutine(FadeUI.Fade(tutorialProgressText, 0f, questSuccessFadeDuration));
        StartCoroutine(FadeUI.Fade(tutorialProgressBarBackground, 0f, questSuccessFadeDuration));
        StartCoroutine(FadeUI.Fade(tutorialProgressBar, 0f, questSuccessFadeDuration));
        StartCoroutine(FadeUI.Fade(tutorialProgressPanel, 0f, questSuccessFadeDuration));
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

    public void UpdateTutorialProgressBar(int questsCompleted, int totalTutorialQuests)
    {
        if (tutorialProgressBarCoroutine != null)
        {
            StopCoroutine(tutorialProgressBarCoroutine);
            tutorialProgressBarCoroutine = null;
        }

        // get target fill by calculating how many quests have been completed out of the total amount
        float targetFill = (float)questsCompleted / totalTutorialQuests;

        // start the coroutine
        tutorialProgressBarCoroutine = StartCoroutine(TutorialProgressBarCoroutine(targetFill));

        // --- this should cover all cases. if a 2nd quest is completed back to back it will simply fill the bar faster ---
    }

    public void ActivateQuestLog()
    {
        if (questFadeInCoroutine != null)
        {
            StopCoroutine(questFadeInCoroutine);
            questFadeInCoroutine = null;
        }
        StartCoroutine(FadeQuestLog(true));
    }

    public void UpdateQuestLogWithNewQuest(string title, int progress, int total)
    {
        questTitleText.text = title;
        questProgressText.text = $"{progress}/{total}";
    }

    public void UpdateQuestProgress(int progress, int total, int changeAmount = 1)
    {
        questProgressText.text = $"{progress}/{total}";

        // format string for floating text effect
        string floatingString = $"+{changeAmount}";

        // create floating text effect
        CreateFloatingText(questProgressText, floatingString, Color.green);
    }

    public void ShowQuestSuccess()
    {
        if (showQuestSuccessCoroutine != null)
        {
            StopCoroutine(showQuestSuccessCoroutine);
            showQuestSuccessCoroutine = null;

            ReturnQuestPanelToOriginalColor();
        }
        showQuestSuccessCoroutine = StartCoroutine(FadeQuestLog(false));
    }

    public void ActivateBuildModeIndicator()
    {
        buildModeIndicator.SetActive(true);
    }

    public void DeactivateBuildModeIndicator()
    {
        buildModeIndicator.SetActive(false);
    }

    public void ActivateDeleteModeIndicator()
    {
        deleteModeIndicator.SetActive(true);
    }

    public void DeactivateDeleteModeIndicator()
    {
        deleteModeIndicator.SetActive(false);
    }

    public void ShowAlertText(string text, float duration)
    {
        // if called subsequently, will overwrite the old text with new text, but fade from the existing alpha rather than resetting
        if (alertTextCoroutine != null)
        {
            StopCoroutine(alertTextCoroutine);
            alertTextCoroutine = null;
        }
        alertTextCoroutine = StartCoroutine(ShowAlertTextCoroutine(text, duration));
    }

    private IEnumerator ShowAlertTextCoroutine(string text, float duration)
    {
        // set text and fade in
        alertText.text = text;
        yield return FadeUI.Fade(alertText, 1.0f, alertFadeDuration);

        // wait for allotted duration
        yield return new WaitForSeconds(duration);

        // fade out
        yield return FadeUI.Fade(alertText, 0.0f, alertFadeDuration);

        alertTextCoroutine = null;
    }

    private IEnumerator FadeQuestLog(bool fadeIn = false)
    {
        float timer = 0f;
        float duration = fadeIn ? questFadeInDuration : questSuccessFadeDuration;
        float startBackgroundAlpha = fadeIn ? 0f : questPanelDefaultColor.a;
        float targetBackgroundAlpha = fadeIn ? questPanelDefaultColor.a : 0f; 
        float startTextAlpha = fadeIn ? 0f : questTextStartColor.a;
        float targetTextAlpha = fadeIn ? questTextStartColor.a : 0f;
        Color mainPanelStartColor = fadeIn ? questPanelDefaultColor : questPanelSuccessColor;

        if (!fadeIn)
        {
            mainQuestPanelImage.color = questPanelSuccessColor;
        }

        while (timer < questSuccessFadeDuration)
        {
            timer += Time.deltaTime;

            // fade main background
            mainQuestPanelImage.color = GetFadedColor(mainPanelStartColor, startBackgroundAlpha, targetBackgroundAlpha, timer, duration);

            // fade out other backgrounds
            foreach (Image questPanelImage in extraQuestPanelImages)
            {
                questPanelImage.color = GetFadedColor(questPanelDefaultColor, startBackgroundAlpha, targetBackgroundAlpha, timer, duration);
            }

            // fade out the text assets
            foreach (TMP_Text textAsset in new[] { questTitleText, questProgressText })
            {
                textAsset.color = GetFadedColor(questTextStartColor, startTextAlpha, targetTextAlpha, timer, duration);
            }

            // activate panel if it has not already been activated and fade in has been called
            if (fadeIn && !questPanel.activeInHierarchy)
            {
                questPanel.SetActive(true);
            }

            yield return null;
        }

        if (!fadeIn)
        {
            questPanel.SetActive(false);
        }
        ReturnQuestPanelToOriginalColor();
    }

    private void ReturnQuestPanelToOriginalColor()
    {
        // loop through each type and return to original color

        mainQuestPanelImage.color = questPanelDefaultColor;

        foreach (Image questPanelImage in extraQuestPanelImages)
        {
            questPanelImage.color = questPanelDefaultColor;
        }

        foreach (TMP_Text textAsset in new[] { questTitleText, questProgressText })
        {
            textAsset.color = questTextStartColor;
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

    private IEnumerator TutorialProgressBarCoroutine(float targetFill)
    {
        float timer = 0f;
        float startFill = tutorialProgressBar.fillAmount;

        while (timer < tutorialProgressFadeDuration)
        {
            timer += Time.deltaTime;

            // smoothly get new fill amount based on length of time passed
            float newFill = Mathf.Lerp(startFill, targetFill, timer / tutorialProgressFadeDuration);

            // set progress bar fill amount to desired fill
            tutorialProgressBar.fillAmount = newFill;

            yield return null;
        }

        tutorialProgressBar.fillAmount = targetFill;
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

    //private void ResetItemPickupIndicator()
    //{
    //    itemPickupIndicatorPanel.color = indicatorPanelStartColor;
    //    itemPickupIndicatorText.color = indicatorTextStartColor;
    //    itemPickupIndicator.transform.localScale = indicatorOriginalScale;
    //    itemPickupSuccessIndicator.color = successIndicatorStartColor;
    //    itemPickupSuccessIndicator.gameObject.SetActive(false);
    //}

    private void CreateFloatingText(TMP_Text textBoxOrigin, string text, Color color)
    {
        // instantiate floating text prefab
        GameObject floatingTextInstance = Instantiate(floatingTextPrefab,
            textBoxOrigin.transform.position + floatingTextSpawnOffset, Quaternion.identity, textBoxOrigin.transform.parent);

        if (floatingTextInstance.TryGetComponent(out FloatingText floatingText))
        {
            floatingText.Init(text, color);
        }
    }

    private void CreateFloatingCurrencyText(float changeAmount)
    {
        // set text values and color based on whether changeAmount is positive or negative
        string symbol = changeAmount > 0 ? "+" : "-";
        Color color = changeAmount > 0 ? Color.green : Color.red;
        string text = symbol + currencySymbol + Mathf.Abs(changeAmount).ToString("N0");

        CreateFloatingText(currencyText, text, color);
    }
}
