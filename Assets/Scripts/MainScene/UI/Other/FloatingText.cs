using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private const float effectDuration = 1.2f;
    private readonly Vector3 offset = new(0, 30, 0);

    private RectTransform rectTransform;
    private TMP_Text textComponent;
    private Coroutine floatingEffectCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TMP_Text>();
    }

    public void Init(string text, Color color)
    {
        // set text component and color
        textComponent.text = text;
        textComponent.color = color;

        // start floating/fading effect
        floatingEffectCoroutine ??= StartCoroutine(FloatingEffect());
    }

    private IEnumerator FloatingEffect()
    {
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + offset;
        Color startColor = textComponent.color;

        float timer = 0f;

        while (timer < effectDuration)
        {
            // move text
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, timer / effectDuration);

            // fade out
            float alpha = Mathf.Lerp(1f, 0f, timer / effectDuration);
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // destroy after effect
        Destroy(gameObject);
    }
}
