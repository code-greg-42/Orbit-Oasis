using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FadeUI
{
    public static IEnumerator Fade(Graphic graphic, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = graphic.color;
        float startAlpha = color.a;

        graphic.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            color.a = newAlpha;
            graphic.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        graphic.color = color;

        if (targetAlpha == 0f)
        {
            graphic.gameObject.SetActive(false);
        }
    }
}
