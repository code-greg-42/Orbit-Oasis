using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("References")]
    [SerializeField] private MenuButton[] menuButtons;
    [SerializeField] private Image loadingScreenPanel;
    [SerializeField] private TMP_Text introText;

    [Header("Disabled Button Sprite")]
    [SerializeField] private Sprite disabledButtonSprite;

    [Header("Custom Cursor")]
    [SerializeField] private Texture2D customCursorTexture;
    private Vector2 cursorHotspot = new(10, 4);

    // intro settings
    private const string introTextString = "GamesByGreg presents";
    private const string introTextEnding = "...";
    private const float introDelay = 0.3f;
    private const float charDelayOne = 0.08f;
    private const float charDelayTwo = 0.42f;
    private const float introTextEndDelay = 1.0f;

    // button fade settings
    private const float otherButtonFadeDuration = 0.5f;

    private Coroutine fadeLoadingScreenCoroutine;
    private Coroutine buttonClickCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DisableCursor();

        // set cursor to custom cursor
        Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);

        // load in main menu
        StartCoroutine(StartSceneCoroutine());
    }

    public void OnButtonClick(MenuButton clickedButton)
    {
        Debug.Log("Button Clicked: " + clickedButton.gameObject.name);
        buttonClickCoroutine ??= StartCoroutine(ButtonClickCoroutine(clickedButton));
    }

    private IEnumerator ButtonClickCoroutine(MenuButton clickedButton)
    {
        // lock and hide the cursor upon click
        DisableCursor();

        // loop through and disable buttons
        foreach (MenuButton menuButton in menuButtons)
        {
            menuButton.DisableButton();

            // fade out all non-clicked buttons to leave selected button as the obvious clicked button
            if (menuButton != clickedButton)
            {
                menuButton.FadeOut(otherButtonFadeDuration);
            }
        }

        // update Data Manager if new game was started, with enough time for data to reset before scene swap
        if (clickedButton.gameObject.name == "NewGameButton")
        {
            DataManager.Instance.StartNewGame();
        }

        // fade to black
        // if loading screen is still fading in, stop the coroutine and fade it out from the current alpha
        if (fadeLoadingScreenCoroutine != null)
        {
            StopCoroutine(fadeLoadingScreenCoroutine);
            fadeLoadingScreenCoroutine = null;
        }
        yield return fadeLoadingScreenCoroutine = StartCoroutine(FadeUI.Fade(loadingScreenPanel, 1.0f, 2.0f));

        // process scene swap based on which button is clicked
        SwapScene(clickedButton);
    }

    private void SwapScene(MenuButton menuButton)
    {
        if (menuButton.gameObject.name == "NewGameButton" || menuButton.gameObject.name == "StoryButton")
        {
            // load story scene
            SceneManager.LoadScene("Story");
        }
        else if (menuButton.gameObject.name == "ContinueButton")
        {
            // load main game
            SceneManager.LoadScene("Main");
        }
        else if (menuButton.gameObject.name == "ExitButton")
        {
            Application.Quit();
        }
    }

    private IEnumerator StartSceneCoroutine()
    {
        yield return new WaitForSeconds(introDelay);

        yield return ShowIntroText();

        yield return FadeUI.Fade(introText, 0f, 1.0f);

        fadeLoadingScreenCoroutine ??= StartCoroutine(FadeUI.Fade(loadingScreenPanel, 0f, 1.5f));

        // brief wait before enabling buttons and cursor
        yield return new WaitForSeconds(0.1f);

        EnableButtons();
        EnableCursor();
    }

    private void EnableButtons()
    {
        foreach (MenuButton menuButton in menuButtons)
        {
            // TEMPORARY --- ONLY TO TEST --- ACTUAL WILL BE BASED ON ACTIVE QUEST INDEX / LACK OF A SAVED GAME
            if (menuButton.gameObject.name == "ContinueButton" && DataManager.Instance.PlayerStats.PlayerCurrency > 0)
            {
                if (menuButton.TryGetComponent(out Image buttonImage))
                {
                    buttonImage.sprite = disabledButtonSprite;
                }
            }
            else
            {
                menuButton.EnableButton();
            }
        }
    }

    private IEnumerator ShowIntroText()
    {
        // split string into chars
        char[] chars = introTextString.ToCharArray();

        foreach (char c in chars)
        {
            introText.text += c;
            yield return new WaitForSeconds(charDelayOne);
        }

        char[] periods = introTextEnding.ToCharArray();

        foreach (char p in periods)
        {
            introText.text += p;
            yield return new WaitForSeconds(charDelayTwo);
        }

        yield return new WaitForSeconds(introTextEndDelay);
    }

    private void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
