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
    private const float clickedButtonFadeDelay = 0.5f;
    private const float clickedButtonFadeDuration = 1.0f;

    private Coroutine buttonWasClickedCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartSceneCoroutine());
    }

    public void TestClick()
    {
        buttonWasClickedCoroutine ??= StartCoroutine(TestClickCoroutine());
    }

    public void OnButtonClick(MenuButton clickedButton)
    {
        
    }

    private IEnumerator ButtonClickCoroutine(MenuButton clickedButton)
    {
        foreach (MenuButton menuButton in menuButtons)
        {
            menuButton.DisableInteractivity();

            if (menuButton != clickedButton)
            {
                menuButton.FadeOut(otherButtonFadeDuration);
            }
        }

        yield return new WaitForSeconds(clickedButtonFadeDelay);

        // fade clicked button
        clickedButton.FadeOut(clickedButtonFadeDuration);

        yield return new WaitForSeconds(clickedButtonFadeDuration);

        // fade to black

        yield return FadeUI.Fade(loadingScreenPanel, 1.0f, 1.5f);
    }

    private IEnumerator TestClickCoroutine()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (MenuButton menuButton in menuButtons)
        {
            menuButton.DisableInteractivity();
            menuButton.FadeOut(1.0f);
        }
        StartCoroutine(FadeUI.Fade(loadingScreenPanel, 1.0f, 2.0f));
    }

    private IEnumerator StartSceneCoroutine()
    {
        yield return new WaitForSeconds(introDelay);

        yield return ShowIntroText();

        yield return FadeUI.Fade(introText, 0f, 1.0f);

        StartCoroutine(FadeUI.Fade(loadingScreenPanel, 0f, 1.5f));
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
