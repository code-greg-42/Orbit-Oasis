using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class UpgradePanelButton : SelectionPanelButton
{
    public void SetCurrencyAmount(float amount)
    {
        ReplaceCurrencyText(amount.ToString(), true);
    }

    public void ShowMaxText()
    {
        ReplaceCurrencyText("MAXED", false);
    }

    private void ReplaceCurrencyText(string text, bool isDollarAmount)
    {
        string pattern = @"\(\$[^\)]+\)";

        string newText = isDollarAmount ? $"(${text})" : $"({text})";

        buttonText.text = Regex.Replace(buttonText.text, pattern, newText);
    }
}
