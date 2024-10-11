using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private MenuButton[] menuButtons;
    [SerializeField] private Image loadingScreenPanel;

    private Coroutine buttonWasClickedCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void TestClick()
    {
        buttonWasClickedCoroutine ??= StartCoroutine(TestClickCoroutine());
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
}
