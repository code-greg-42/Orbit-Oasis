using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipSelection : MonoBehaviour
{
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SelectionPanelButton[] mainMenuButtons; // 2
    [SerializeField] private SelectionPanelButton[] difficultySettingButtons; // 3

    private bool isSpaceshipSelectionActive;
    private Coroutine walkAwayDeactivation;

    private const float walkAwayCheckTime = 1.0f;
    private const float deactivationDistance = 5.0f;
    private float sqrDeactivationDistance;

    private SelectionPanelButton[][] menuStages;

    private delegate void ButtonAction();
    private ButtonAction[][] buttonActions;

    private int currentSelection;
    private int currentMenuStage;

    private void Start()
    {
        sqrDeactivationDistance = deactivationDistance * deactivationDistance;

        menuStages = new SelectionPanelButton[][]
        {
            mainMenuButtons,
            difficultySettingButtons
        };

        buttonActions = new ButtonAction[][]
        {
            new ButtonAction[] { OnStartRacePressed, OnUpgradesPressed },
            new ButtonAction[] { StartSpaceRace, StartSpaceRace, StartSpaceRace }
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
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int newSelection = currentSelection + 1;
            if (AreIndicesValid(currentMenuStage, newSelection))
            {
                ChangeSelection(newSelection);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int newSelection = currentSelection - 1;
            if (AreIndicesValid (currentMenuStage, newSelection))
            {
                ChangeSelection(newSelection);
            }
        }

        // BUTTON PRESS
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PressSelectedButton();
        }

        // EXIT MENU
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

            float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
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
            Debug.LogWarning("ActivateMenuStage called with an invalid stage: " + stage);
        }
    }

    private void DeactivateCurrentMenu()
    {
        if (currentMenuStage >= 0 && currentMenuStage < menuStages.Length)
        {
            // remove selection from current
            DeselectCurrentSelection();

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
            Debug.LogWarning("DeactivateCurrentMenu called with an invalid currentMenuStage: " + currentMenuStage);
        }
    }

    private bool AreIndicesValid(int menuStage, int selectionIndex)
    {
        return menuStage >= 0 && menuStage < menuStages.Length &&
               selectionIndex >= 0 && selectionIndex < menuStages[menuStage].Length;
    }

    private void OnStartRacePressed()
    {
        Debug.Log("Start Race has been pressed!");

        ChangeMenuStage(1);
    }

    private void OnUpgradesPressed()
    {
        Debug.Log("Upgrades has been pressed!");
    }

    private void StartSpaceRace()
    {
        Debug.Log("Space Race started with difficulty: " + currentSelection);
    }

    private void ResetState()
    {
        // reset selection first to comply with AreIndicesValid
        currentSelection = 0;
        currentMenuStage = 0;
    }
}
