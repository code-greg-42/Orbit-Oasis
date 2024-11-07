using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipSelection : MonoBehaviour
{
    public static SpaceshipSelection Instance { get; private set; }

    [Header("General")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Buttons")]
    [SerializeField] private SelectionPanelButton[] mainMenuButtons; // 2
    [SerializeField] private SelectionPanelButton[] difficultySettingButtons; // 3
    [SerializeField] private SelectionPanelButton[] upgradeButtons; // 2

    [Header("Upgrade Panels")]
    [SerializeField] private GameObject upgradeButtonsPanel;
    [SerializeField] private GameObject upgradeDisplayPanel;

    [Header("Upgrade Display Objects")]
    [SerializeField] private GameObject[] boostDisplayExtraLines; // 6
    [SerializeField] private GameObject[] rocketDisplayExtraLines; // 6
    private readonly int extraLinesPerUpgradeLevel = 2;

    private bool isSpaceshipSelectionActive;
    private Coroutine walkAwayDeactivation;

    private const float walkAwayCheckTime = 1.0f;
    private const float deactivationDistance = 10.0f;
    private float sqrDeactivationDistance;

    private SelectionPanelButton[][] menuStages;
    private int upgradeStage = 2; // must match index of upgrade stage in menuStages

    private readonly float[] boostUpgradeCosts = { 1000, 3000, 9000 };
    private readonly float[] rocketUpgradeCosts = { 800, 2400, 7200 };

    private delegate void ButtonAction();
    private ButtonAction[][] buttonActions;

    private int currentSelection;
    private int currentMenuStage;

    public bool IsMenuActive => isSpaceshipSelectionActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        sqrDeactivationDistance = deactivationDistance * deactivationDistance;

        menuStages = new SelectionPanelButton[][]
        {
            mainMenuButtons,
            difficultySettingButtons,
            upgradeButtons
        };

        buttonActions = new ButtonAction[][]
        {
            new ButtonAction[] { OnStartRacePressed, OnUpgradesPressed },
            new ButtonAction[] { StartSpaceRace, StartSpaceRace, StartSpaceRace },
            new ButtonAction[] { OnUpgradeBoostPressed, OnUpgradeRocketsPressed }
        };
    }

    private void Update()
    {
        if (isSpaceshipSelectionActive)
        {
            HandleUserInput();
        }
    }

    public void PressSelectedButton()
    {
        if (AreIndicesValid(currentMenuStage, currentSelection))
        {
            ButtonAction action = buttonActions[currentMenuStage][currentSelection];
            action?.Invoke();
        }
        else
        {
            Debug.LogWarning($"PressSelectedButton called with invalid indices: currentMenuStage={currentMenuStage}, currentSelection={currentSelection}");
        }
    }

    private void HandleUserInput()
    {
        // SELECTION
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            int newSelection = currentSelection + 1;
            if (AreIndicesValid(currentMenuStage, newSelection))
            {
                ChangeSelection(newSelection);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            int newSelection = currentSelection - 1;
            if (AreIndicesValid (currentMenuStage, newSelection))
            {
                ChangeSelection(newSelection);
            }
        }

        // BUTTON PRESS
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F))
        {
            // play sound
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.SpaceMenuEnter);

            // execute button press
            PressSelectedButton();
        }

        // EXIT MENU
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.SpaceMenuBack);
            int newMenuStage = currentMenuStage - 1;

            // go to main menu if escape is pressed from a further menu, otherwise exit the menu altogether
            if (AreIndicesValid(newMenuStage, 0))
            {
                ChangeMenuStage(0);
            }
            else
            {
                DeactivateSpaceshipSelection();
            }
        }
    }

    private void ChangeMenuStage(int stage)
    {
        if (AreIndicesValid(stage, 0))
        {
            // deactivate current menu before changing menu stage variable
            DeactivateCurrentMenu();

            // reset selection and change menu stage variable
            currentSelection = 0;
            currentMenuStage = stage;

            // activate new menu and update selection visuals
            ActivateMenuStage(stage);
            UpdateSelectionHighlight();
        }
        else
        {
            Debug.LogWarning("ChangeMenuStage called with an invalid stage: " + stage);
        }
    }

    private void ChangeSelection(int selection)
    {
        if (AreIndicesValid(currentMenuStage, selection))
        {
            // play sound
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.SpaceMenuSelect);

            // call deselect before changing currentSelection
            DeselectCurrentSelection();

            // set current selection to new value
            currentSelection = selection;

            // update selection visuals
            UpdateSelectionHighlight();
        }
        else
        {
            Debug.LogWarning($"ChangeSelection called with an invalid selection index: {selection} for currentMenuStage: {currentMenuStage}");
        }
    }

    private void UpdateSelectionHighlight()
    {
        if (AreIndicesValid(currentMenuStage, currentSelection))
        {
            SelectionPanelButton button = menuStages[currentMenuStage][currentSelection];

            if (button != null)
            {
                button.SelectButton();
            }
        }
    }

    public void ActivateSpaceshipSelection()
    {
        if (!isSpaceshipSelectionActive)
        {
            isSpaceshipSelectionActive = true;
            selectionPanel.SetActive(true);

            // make sure current ints are reset
            ResetState();

            // activate first menu and update selection highlight
            ActivateMenuStage(0);
            UpdateSelectionHighlight();

            if (walkAwayDeactivation != null)
            {
                StopCoroutine(walkAwayDeactivation);
                walkAwayDeactivation = null;
            }

            walkAwayDeactivation = StartCoroutine(WalkAwayDeactivationCoroutine());
        }
    }

    public void DeactivateSpaceshipSelection()
    {
        if (isSpaceshipSelectionActive)
        {
            isSpaceshipSelectionActive = false;

            // deactivate current menu stage so it properly starts fresh next time
            DeactivateCurrentMenu();

            // reset bools for same reason
            ResetState();

            // deactivate main panel background
            selectionPanel.SetActive(false);

            // deactivate distance check coroutine
            if (walkAwayDeactivation != null)
            {
                StopCoroutine(walkAwayDeactivation);
                walkAwayDeactivation = null;
            }
        }
    }

    private IEnumerator WalkAwayDeactivationCoroutine()
    {
        while (isSpaceshipSelectionActive)
        {
            yield return new WaitForSeconds(walkAwayCheckTime);

            float sqrDistance = (playerMovement.PlayerPosition - transform.position).sqrMagnitude;
            if (sqrDistance > sqrDeactivationDistance)
            {
                DeactivateSpaceshipSelection();
                break;
            }
        }
    }

    private void DeselectCurrentSelection()
    {
        if (AreIndicesValid(currentMenuStage, currentSelection))
        {
            SelectionPanelButton button = menuStages[currentMenuStage][currentSelection];

            if (button != null)
            {
                button.DeselectButton();
            }
        }
        else
        {
            Debug.LogWarning($"DeselectCurrentSelection called with invalid indices: currentMenuStage={currentMenuStage}, currentSelection={currentSelection}");
        }
    }

    private void ActivateMenuStage(int stage)
    {
        if (stage >= 0 && stage < menuStages.Length)
        {
            if (stage != upgradeStage)
            {
                SelectionPanelButton[] menuStage = menuStages[stage];

                foreach (SelectionPanelButton button in menuStage)
                {
                    if (button != null)
                    {
                        button.ActivateButton();
                    }
                }
            }
            else
            {
                UpdateUpgradeDisplay();
                upgradeButtonsPanel.SetActive(true);
                upgradeDisplayPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("ActivateMenuStage called with an invalid stage: " + stage);
        }
    }

    private void DeactivateCurrentMenu()
    {
        if (currentMenuStage >= 0 && currentMenuStage < menuStages.Length)
        {
            // remove selection from current
            DeselectCurrentSelection();

            if (currentMenuStage != upgradeStage)
            {
                SelectionPanelButton[] menuStage = menuStages[currentMenuStage];

                foreach (SelectionPanelButton button in menuStage)
                {
                    if (button != null)
                    {
                        button.DeactivateButton();
                    }
                }
            }
            else
            {
                upgradeButtonsPanel.SetActive(false);
                upgradeDisplayPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("DeactivateCurrentMenu called with an invalid currentMenuStage: " + currentMenuStage);
        }
    }

    private void UpdateUpgradeDisplay()
    {
        if (currentMenuStage == upgradeStage)
        {
            // set levels based on data manager stats
            int boostUpgradeLevel = DataManager.Instance.RaceStats.BoostUpgradeLevel;
            int rocketUpgradeLevel = DataManager.Instance.RaceStats.RocketUpgradeLevel;

            // set ordered arrays usable by the loop
            int[] upgradeLevels = { boostUpgradeLevel, rocketUpgradeLevel };
            float[][] upgradeCosts = { boostUpgradeCosts, rocketUpgradeCosts };
            GameObject[][] extraLinesDisplays = { boostDisplayExtraLines, rocketDisplayExtraLines };

            // loop through and display either the upgrade cost or a 'maxed' message
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (upgradeButtons[i] is UpgradePanelButton upgradeButton)
                {
                    // get upgrade cost or null
                    float? upgradeCost = GetUpgradeCostOrNull(upgradeCosts[i], upgradeLevels[i]);

                    // if a cost was found, set it, otherwise show 'maxed'
                    if (upgradeCost.HasValue)
                    {
                        upgradeButton.SetCurrencyAmount(upgradeCost.Value);
                    }
                    else
                    {
                        upgradeButton.ShowMaxText();
                    }

                    // activate extra lines in display based on upgrade level
                    ActivateExtraDisplayLines(extraLinesDisplays[i], upgradeLevels[i]);
                }
            }
        }
    }

    private void ActivateExtraDisplayLines(GameObject[] extraLines, int upgradeLevel)
    {
        // deactivate all extra lines first
        foreach(GameObject line in extraLines)
        {
            line.SetActive(false);
        }

        //// activate X number of lines per upgrade level
        int numLines = upgradeLevel * extraLinesPerUpgradeLevel;

        // loop through and activate
        for (int i = 0; i < numLines; i++)
        {
            extraLines[i].SetActive(true);
        }
    }

    private void OnStartRacePressed()
    {
        ChangeMenuStage(1);
    }

    private void OnUpgradesPressed()
    {
        ChangeMenuStage(2);
    }

    private void StartSpaceRace()
    {
        MainGameManager.Instance.StartSpaceRaceScene(currentSelection);
        DeactivateSpaceshipSelection();
    }

    private void OnUpgradeBoostPressed()
    {
        int boostLevel = DataManager.Instance.RaceStats.BoostUpgradeLevel;

        if (boostLevel < DataManager.Instance.RaceStats.MaxBoostLevel && boostLevel >= 0 && boostLevel < boostUpgradeCosts.Length)
        {
            float boostCost = boostUpgradeCosts[boostLevel];

            if (DataManager.Instance.PlayerStats.PlayerCurrency >= boostCost)
            {
                DataManager.Instance.SubtractCurrency(boostCost);
                DataManager.Instance.UpgradeBoost();
                UpdateUpgradeDisplay();
            }
        }
    }

    private void OnUpgradeRocketsPressed()
    {
        int rocketLevel = DataManager.Instance.RaceStats.RocketUpgradeLevel;

        if (rocketLevel < DataManager.Instance.RaceStats.MaxRocketLevel && rocketLevel >= 0 && rocketLevel < rocketUpgradeCosts.Length)
        {
            float rocketCost = rocketUpgradeCosts[rocketLevel];

            if (DataManager.Instance.PlayerStats.PlayerCurrency >= rocketCost)
            {
                DataManager.Instance.SubtractCurrency(rocketCost);
                DataManager.Instance.UpgradeRockets();
                UpdateUpgradeDisplay();
            }
        }
    }

    private void ResetState()
    {
        // reset selection first to comply with AreIndicesValid
        currentSelection = 0;
        currentMenuStage = 0;
    }

    private float? GetUpgradeCostOrNull(float[] costs, int level)
    {
        return level < costs.Length ? costs[level] : (float?)null;
    }

    private bool AreIndicesValid(int menuStage, int selectionIndex)
    {
        return menuStage >= 0 && menuStage < menuStages.Length &&
               selectionIndex >= 0 && selectionIndex < menuStages[menuStage].Length;
    }
}
