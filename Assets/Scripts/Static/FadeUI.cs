using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FadeUI
{
    public static IEnumerator Fade(Image image, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;
        float startAlpha = color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            color.a = newAlpha;
            image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }
}
