using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryUIManager : MonoBehaviour
{
    public static StoryUIManager Instance;

    [Header("UploadBar Components")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text uploadingText;
    [SerializeField] private TMP_Text uploadPercentageText;

    // upload bar variables
    private const string uploadingTextString = "uploading";
    private const string uploadingTextEnding = "...";
    private const float uploadingCharDelayOne = 0.20f;
    private const float uploadingCharDelayTwo = 0.75f;
    private const float uploadingEndDelay = 0.85f;
    private Coroutine animateUploadingText;

    [Header("Story Components")]
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Image storyImage;
    [SerializeField] private Image storyImagePanel;

    [Header("Loading Screen")]
    [SerializeField] private Image loadingScreenPanel;
    private const float loadingScreenFadeDuration = 2.0f;

    // story text variables
    private const float storyTextWordDelay = 0.15f;
    private Coroutine displayStoryTextCoroutine;

    // story image variables
    private const float imageFadeDuration = 2.0f;
    private Coroutine storyImageCoroutine;

    private Coroutine displaySceneCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animateUploadingText ??= StartCoroutine(AnimateUploadingText());
    }

    public IEnumerator DisplayScene(string text, Sprite imageSprite, float sceneDuration, float endOfSceneDelay, bool isLastScene)
    {
        yield return displaySceneCoroutine ??= StartCoroutine(DisplayStorySceneCoroutine(text, imageSprite, sceneDuration, endOfSceneDelay, isLastScene));
    }

    public IEnumerator FadeInScene()
    {
        yield return FadeUI.Fade(loadingScreenPanel, 0.0f, loadingScreenFadeDuration);
    }

    public IEnumerator FadeOutScene()
    {
        yield return FadeUI.Fade(loadingScreenPanel, 1.0f, loadingScreenFadeDuration);
    }

    private IEnumerator DisplayStorySceneCoroutine(string text, Sprite imageSprite, float sceneDuration, float endOfSceneDelay, bool isLastScene)
    {
        float startTime = Time.time;

        DisplayStoryText(text);
        DisplayStoryImage(imageSprite);

        // wait until both coroutines have completed
        yield return new WaitUntil(() => displayStoryTextCoroutine == null && storyImageCoroutine == null);

        float timer = Time.time - startTime;

        float pauseTime = sceneDuration - imageFadeDuration - timer;

        if (pauseTime < sceneDuration - imageFadeDuration)
        {
            yield return new WaitForSeconds(pauseTime);
        }
        
        if (!isLastScene)
        {
            yield return FadeOutImageCoroutine();
        }
        else
        {
            yield return new WaitForSeconds(imageFadeDuration);
        }

        yield return new WaitForSeconds(endOfSceneDelay);

        displaySceneCoroutine = null;
    }

    public void UpdateProgressBar(float percentage)
    {
        if (percentage > 0)
        {
            if (percentage < 1.0f)
            {
                // set fill amount to percentage to show progress
                progressBar.fillAmount = percentage;

                // convert percentage into a text string and display
                int percentageInt = (int)Mathf.Round(percentage * 100);
                string percentageString = $"{percentageInt}%";
                uploadPercentageText.text = percentageString;
            }
            else
            {
                if (animateUploadingText != null)
                {
                    // stop coroutine and set to null
                    StopCoroutine(animateUploadingText);
                    animateUploadingText = null;

                    // set progress bar and text to fully uploaded values
                    progressBar.fillAmount = 1.0f;
                    uploadingText.text = "upload complete!";
                }
            }
        }
    }

    public void StartUploadingText()
    {
        animateUploadingText ??= StartCoroutine(AnimateUploadingText());
    }

    private IEnumerator AnimateUploadingText()
    {
        while (true)
        {
            // clear existing text
            uploadingText.text = "";

            // split string into chars
            char[] chars = uploadingTextString.ToCharArray();

            foreach (char c in chars)
            {
                uploadingText.text += c;
                yield return new WaitForSeconds(uploadingCharDelayOne);
            }

            char[] periods = uploadingTextEnding.ToCharArray();

            foreach (char p in periods)
            {
                uploadingText.text += p;
                yield return new WaitForSeconds(uploadingCharDelayTwo);
            }

            yield return new WaitForSeconds(uploadingEndDelay);
        }
    }

    private void DisplayStoryText(string text)
    {
        displayStoryTextCoroutine ??= StartCoroutine(DisplayStoryTextCoroutine(text));
    }

    private IEnumerator DisplayStoryTextCoroutine(string text)
    {
        // clear textbox
        storyText.text = "";

        // split text string into words
        string[] words = text.Split(" ");

        foreach (string word in words)
        {
            storyText.text += word + " ";
            yield return new WaitForSeconds(storyTextWordDelay);
        }

        displayStoryTextCoroutine = null;
    }

    private void DisplayStoryImage(Sprite imageSprite)
    {
        storyImageCoroutine ??= StartCoroutine(DisplayStoryImageCoroutine(imageSprite));
    }

    private IEnumerator DisplayStoryImageCoroutine(Sprite imageSprite)
    {
        storyImage.sprite = imageSprite;
        StartCoroutine(FadeUI.Fade(storyImagePanel, 0.4f, imageFadeDuration));

        yield return FadeUI.Fade(storyImage, 1.0f, imageFadeDuration);

        storyImageCoroutine = null;
    }

    private IEnumerator FadeOutImageCoroutine()
    {
        StartCoroutine(FadeUI.Fade(storyImagePanel, 0.0f, imageFadeDuration, false));

        yield return FadeUI.Fade(storyImage, 0.0f, imageFadeDuration, false);
    }
}
