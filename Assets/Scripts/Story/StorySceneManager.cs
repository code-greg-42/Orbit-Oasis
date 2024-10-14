using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySceneManager : MonoBehaviour
{
    public static StorySceneManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(TestProgressBar());
    }

    private IEnumerator TestProgressBar()
    {
        float totalTime = 10.0f;

        float timer = 0.0f;

        while (true)
        {
            timer += Time.deltaTime;

            float timePercentage = timer / totalTime;

            StoryUIManager.Instance.UpdateProgressBar(timePercentage);

            yield return null;
        }
    }
}
