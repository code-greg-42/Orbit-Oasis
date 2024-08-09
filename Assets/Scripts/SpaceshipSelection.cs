using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipSelection : MonoBehaviour
{
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SelectionPanelButton[] mainMenuButtons;
    [SerializeField] private SelectionPanelButton[] difficultySettingButtons;

    private bool isSpaceshipSelectionActive;
    private Coroutine walkAwayDeactivation;

    private const float walkAwayCheckTime = 1.0f;
    private const float deactivationDistance = 5.0f;
    private float sqrDeactivationDistance;

    private SelectionPanelButton[][] menuStages;

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
    }

    private void Update()
    {
        if (isSpaceshipSelectionActive)
        {
            HandleUserInput();
        }
    }

    private void HandleUserInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeSelection(currentSelection + 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeSelection(currentSelection - 1);
        }
    }

    private void ChangeMenuStage(int stage)
    {
        if (AreIndicesValid(stage, 0))
        {
            DeactivateCurrentMenu();

            SelectionPanelButton[] newMenu = menuStages[stage];

            foreach (SelectionPanelButton button in newMenu)
            {
                button.ActivateButton();
            }

            // always reset currentSelection first
            currentSelection = 0;
            currentMenuStage = stage;
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
            DeselectCurrentSelection();

            SelectionPanelButton button = menuStages[currentMenuStage][selection];

            if (button != null)
            {
                currentSelection = selection;
                button.SelectButton();
            }
        }
        else
        {
            Debug.LogWarning($"ChangeSelection called with an invalid selection index: {selection} for currentMenuStage: {currentMenuStage}");
        }
    }

    public void ActivateSpaceshipSelection()
    {
        if (!isSpaceshipSelectionActive)
        {
            isSpaceshipSelectionActive = true;
            selectionPanel.SetActive(true);

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
            selectionPanel.SetActive(false);

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

    private void DeactivateCurrentMenu()
    {
        if (currentMenuStage >= 0 && currentMenuStage < menuStages.Length)
        {
            // remove selection from current
            DeselectCurrentSelection();

            SelectionPanelButton[] menuStage = menuStages[currentMenuStage];

            foreach (SelectionPanelButton button in menuStage)
            {
                button.DeactivateButton();
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
}
