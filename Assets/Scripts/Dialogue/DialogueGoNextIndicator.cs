using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueGoNextIndicator : MonoBehaviour
{
    private RectTransform indicator;
    private Vector2 initialPosition;
    private const float targetHeight = 15.0f;
    private const float interval = 0.4f; // # seconds it takes for a single bounce to reach target height

    private Coroutine bounceCoroutine;

    private void Awake()
    {
        indicator = GetComponent<RectTransform>();
        
        if (indicator != null)
        {
            initialPosition = indicator.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }
        bounceCoroutine = StartCoroutine(BounceCoroutine());
    }

    private void OnDisable()
    {
        ResetIndicator();
    }

    private IEnumerator BounceCoroutine()
    {
        while (true)
        {
            // move up
            yield return StartCoroutine(MoveIndicator(targetHeight));

            // move down
            yield return StartCoroutine(MoveIndicator(-targetHeight));
        }
    }

    private IEnumerator MoveIndicator(float moveBy)
    {
        Vector2 targetPosition = indicator.anchoredPosition + new Vector2(0, moveBy);

        float elapsedTime = 0f;

        while (elapsedTime < interval)
        {
            indicator.anchoredPosition = Vector2.Lerp(indicator.anchoredPosition, targetPosition, elapsedTime / interval);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        indicator.anchoredPosition = targetPosition;
    }

    private void ResetIndicator()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }

        if (indicator != null)
        {
            indicator.anchoredPosition = initialPosition;
        }
    }
}
