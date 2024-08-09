using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionPanelButton : MonoBehaviour
{
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private TMP_Text buttonText;

    public bool IsSelected { get; private set; }

    public void SelectButton()
    {
        if (!IsSelected)
        {
            IsSelected = true;
            selectionHighlight.SetActive(true);
            buttonText.text = "<color=#FF0000>> </color>" + buttonText.text + "<color=#FF0000> <</color>";
        }
    }

    public void DeselectButton()
    {
        if (IsSelected)
        {
            IsSelected = false;
            selectionHighlight.SetActive(false);
            buttonText.text = buttonText.text.Replace("<color=#FF0000>> </color>", "").Replace("<color=#FF0000> <</color>", "");
        }
    }

    public void ActivateButton()
    {
        gameObject.SetActive(true);
    }

    public void DeactivateButton()
    {
        gameObject.SetActive(false);
    }
}
