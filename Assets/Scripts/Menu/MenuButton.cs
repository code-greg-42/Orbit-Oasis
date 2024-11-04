using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Button button;
    private TMP_Text buttonText;
    private Image buttonGraphic;

    private Color textColorHighlighted = Color.white;
    private Color textColorNormal = new(0.8f, 0.8f, 0.8f);
    private Color textColorPressed = new(0.6f, 0.6f, 0.6f);

    private bool isPressed;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
        buttonGraphic = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && !isPressed && buttonText.color != textColorHighlighted)
        {
            buttonText.color = textColorHighlighted;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable && !isPressed && buttonText.color != textColorNormal)
        {
            buttonText.color = textColorNormal;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button.interactable && !isPressed)
        {
            isPressed = true;
            buttonText.color = textColorPressed;

            // send click to MenuManager for processing
            MenuManager.Instance.OnButtonClick(this);
        }
    }

    public void DisableButton()
    {
        button.interactable = false;
        button.enabled = false;
    }

    public void EnableButton()
    {
        button.interactable = true;
        button.enabled = true;
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeUI.Fade(buttonText, 0f, duration, false));
        StartCoroutine(FadeUI.Fade(buttonGraphic, 0f, duration, false));
    }
}
