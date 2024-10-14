using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneManager : MonoBehaviour
{
    public static StorySceneManager Instance;

    [Header("Story Text Excerpts")]
    [TextArea(2, 4)]
    [SerializeField] private string[] storyTexts;

    [Header("Story Sprites")]
    [SerializeField] private Sprite[] storySprites;

    private Dictionary<Sprite, string> storyScenes;

    private const float sceneDuration = 12.0f;
    private const float endOfSceneDelay = 1.0f;

    private void Awake()
    {
        Instance = this;

        // init dictionary
        storyScenes = new Dictionary<Sprite, string>();

        // populate dictionary
        int length = Mathf.Min(storySprites.Length, storyTexts.Length);
        for (int i = 0; i < length; i++)
        {
            storyScenes[storySprites[i]] = storyTexts[i];
        }
    }

    private void Start()
    {
        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene()
    {
        DisableCursor();

        yield return StoryUIManager.Instance.FadeInScene();

        StartCoroutine(StartProgressBar());

        StoryUIManager.Instance.StartUploadingText();

        yield return PlayStoryScenes();

        yield return StoryUIManager.Instance.FadeOutScene();

        // go to main game if a new game click initiated the story scene, otherwise go back to main menu
        if (DataManager.Instance.NewGameStarted)
        {
            // reset bool in data manager first
            DataManager.Instance.ResetNewGameStarted();
            SceneManager.LoadScene("Main");
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    private IEnumerator StartProgressBar()
    {
        // dont include end of scene delay in total time so that it completes a bit before the final scene ends
        float totalTime = sceneDuration * storyScenes.Count;
        float timer = 0.0f;

        while (true)
        {
            timer += Time.deltaTime;
            float timePercentage = timer / totalTime;
            StoryUIManager.Instance.UpdateProgressBar(timePercentage);
            yield return null;
        }
    }

    private IEnumerator PlayStoryScenes()
    {
        int lastSceneNumber = storyScenes.Count;
        int sceneNumber = 1;

        foreach (KeyValuePair<Sprite, string> scene in storyScenes)
        {
            Sprite image = scene.Key;
            string text = scene.Value;

            bool isLastScene = sceneNumber == lastSceneNumber;
            yield return StoryUIManager.Instance.DisplayScene(text, image, sceneDuration, endOfSceneDelay, isLastScene);
            sceneNumber++;
        }
    }

    private void DisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
