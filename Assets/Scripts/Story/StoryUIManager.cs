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
    private const float uploadingCharDelayOne = 0.10f;
    private const float uploadingCharDelayTwo = 0.46f;
    private const float uploadingEndDelay = 0.5f;
    private Coroutine animateUploadingText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animateUploadingText ??= StartCoroutine(AnimateUploadingText());
    }

    public void UpdateProgressBar(float percentage)
    {
        if (percentage > 0)
        {
            if (percentage < 1.0f)
            {
                progressBar.fillAmount = percentage;
                int percentageInt = (int)Mathf.Round(percentage * 100);
                string percentageString = $"{percentageInt}%";
                uploadPercentageText.text = percentageString;
            }
            else
            {
                progressBar.fillAmount = 1.0f;
                uploadPercentageText.text = "100%";
                StopCoroutine(animateUploadingText);
                uploadingText.text = "upload complete!";
            }
        }
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
}
