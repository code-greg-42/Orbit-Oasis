using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private MenuButton[] menuButtons;
    [SerializeField] private Image loadingScreenPanel;
    [SerializeField] private TMP_Text introText;

    // intro settings
    private const string introTextString = "GamesByGreg presents";
    private const string introTextEnding = "...";
    private const float introDelay = 0.3f;
    private const float charDelayOne = 0.08f;
    private const float charDelayTwo = 0.42f;
    private const float introTextEndDelay = 1.0f;

    // button fade settings
    private const float otherButtonFadeDuration = 0.3f;
    private const float clickedButtonFadeDuration = 1.5f;

    private Coroutine buttonClickCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartSceneCoroutine());
    }

    public void OnButtonClick(MenuButton clickedButton)
    {
        Debug.Log("Button Clicked: " + clickedButton.gameObject.name);
        buttonClickCoroutine ??= StartCoroutine(ButtonClickCoroutine(clickedButton));
    }

    private IEnumerator ButtonClickCoroutine(MenuButton clickedButton)
    {
        foreach (MenuButton menuButton in menuButtons)
        {
            menuButton.DisableButton();

            if (menuButton != clickedButton)
            {
                menuButton.FadeOut(otherButtonFadeDuration);
            }
        }

        // fade clicked button
        clickedButton.FadeOut(clickedButtonFadeDuration);

        // fade to black
        yield return FadeUI.Fade(loadingScreenPanel, 1.0f, 1.5f);
    }

    private IEnumerator StartSceneCoroutine()
    {
        yield return new WaitForSeconds(introDelay);

        yield return ShowIntroText();

        yield return FadeUI.Fade(introText, 0f, 1.0f);

        StartCoroutine(FadeUI.Fade(loadingScreenPanel, 0f, 1.5f));

        // brief wait before enabling buttons
        yield return new WaitForSeconds(0.1f);

        EnableButtons();
    }

    private void EnableButtons()
    {
        foreach (MenuButton menuButton in menuButtons)
        {
            // TEMPORARY --- ONLY TO TEST --- ACTUAL WILL BE BASED ON ACTIVE QUEST INDEX / LACK OF A SAVED GAME
            if (menuButton.gameObject.name == "ContinueButton" && DataManager.Instance.PlayerStats.PlayerCurrency > 0)
            {
                menuButton.DisableInteractivity();
            }

            menuButton.EnableButton();
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
}
