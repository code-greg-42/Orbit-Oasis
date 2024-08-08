using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceUIManager : MonoBehaviour
{
    public static SpaceRaceUIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TMP_Text introText;
    [SerializeField] private GameObject introTextBox;
    [SerializeField] private TMP_Text rocketsAmountText;
    [SerializeField] private Image boostBar;
    [SerializeField] private TMP_Text countdownTimerText;
    [SerializeField] private TMP_Text boostText;
    [SerializeField] private TMP_Text rocketsText;
    [SerializeField] private TMP_Text gameClockText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text checkpointStatusWindow;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private Color outOfResourceTextColor;
    [SerializeField] private Color checkpointPassedColor;
    [SerializeField] private Color checkpointMissedColor;
    [SerializeField] private Color victoryTextColor;

    // ui setting variables
    private float introWordDelay = 0.2f; // delay before the words start in the intro
    private float wordDisplayDelay = 0.1f; // rate of words being added to the sentence
    private float sentenceDisplayDelay = 4.0f; // total time of pause after sentence is complete
    private float statusDisplayDuration = 1.2f; // total time of the display at normal alpha
    private float statusFadeDuration = 2.0f; // duration of the fade effect for the status window

    private readonly List<string> introSentences = new List<string>
    {
        "You are approaching the asteroid field. Fly through the <color=#FFF300>checkpoints</color> to complete the race.",
        "<color=#00FF00>[WASD]</color> to move, <color=#77B5FF>[shift]</color> to boost, <color=#FF0000>[space]</color> for rockets. Rockets are limited! Good luck!"
    };

    private Coroutine countdownCoroutine;
    private Coroutine statusWindowCoroutine;
    private Coroutine victoryTextCoroutine;

    public bool CountdownStarted { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        float bestTime = DataManager.Instance.RaceStats.BestTimes[DataManager.Instance.RaceStats.SelectedDifficulty];

        if (bestTime != 0f)
        {
            UpdateBestTime(bestTime);
        }

        StartCoroutine(DisplayIntroText());
    }

    private IEnumerator DisplayIntroText()
    {
        yield return new WaitForSeconds(introWordDelay);

        for (int i = 0; i < introSentences.Count; i++)
        {
            introText.text = "";
            string[] words = introSentences[i].Split(' '); // split sentences into array of words

            foreach (string word in words)
            {
                introText.text += word + " ";
                SpaceRaceSoundManager.Instance.PlayTypingKeySound();
                yield return new WaitForSeconds(wordDisplayDelay);
            }
            // pause to allow reading
            yield return new WaitForSeconds(sentenceDisplayDelay);
        }
    }

    public void UpdateCheckpointStatusWindow(bool checkpointCompleted = true)
    {
        if (statusWindowCoroutine != null)
        {
            StopCoroutine(statusWindowCoroutine);
            statusWindowCoroutine = null;
        }

        // set message and color passed on checkpoint status

        string message = checkpointCompleted ? "Checkpoint Passed!" : "Checkpoint Missed!";
        Color statusColor = checkpointCompleted ? checkpointPassedColor : checkpointMissedColor;

        // start the coroutine to show and fade the text
        statusWindowCoroutine = StartCoroutine(ShowAndFadeText(checkpointStatusWindow, message, statusColor,
                statusDisplayDuration, statusFadeDuration));
    }

    public void UpdateGameClock(float gameTime)
    {
        gameClockText.text = FormatClockTime(gameTime);
    }

    public void UpdateBestTime(float gameTime)
    {
        string formattedTime = FormatClockTime(gameTime);
        bestTimeText.text = "Best Time:\n" + formattedTime;
    }

    public void DisplayVictoryText(bool newBestTime = false)
    {
        if (victoryTextCoroutine == null)
        {
            string victoryTextString = newBestTime ? "Race Completed!\nNew Best Time!" : "Race Completed!";
            victoryTextCoroutine = StartCoroutine(ShowAndFadeText(victoryText, victoryTextString, victoryTextColor, 2.0f, 3.0f));
        }
    }

    private IEnumerator DisplayCountdown()
    {
        int countdown = 3;

        while (countdown > 0)
        {
            // update UI
            countdownTimerText.text = countdown.ToString();

            // update local bool
            countdown--;

            // play sound
            SpaceRaceSoundManager.Instance.PlayCountdownSound();

            yield return new WaitForSeconds(1);
        }

        countdownTimerText.text = "GO!";

        // play final sound
        SpaceRaceSoundManager.Instance.PlayCountdownSound(true);

        // wait for 2 seconds then disable
        yield return new WaitForSeconds(2);
        countdownTimerText.gameObject.SetActive(false);
    }

    public void StartCountdown()
    {
        if (countdownCoroutine == null && !CountdownStarted)
        {
            CountdownStarted = true;
            countdownCoroutine = StartCoroutine(DisplayCountdown());
        }
    }

    public void UpdateRocketAmount(int amount)
    {
        if (amount > 0)
        {
            rocketsAmountText.text = new string('I', amount);
        }
        else
        {
            rocketsAmountText.fontSize = 20;
            rocketsAmountText.characterSpacing = 0;
            rocketsAmountText.text = "OUT OF ROCKETS";
            rocketsText.color = outOfResourceTextColor;
        }
    }

    public void UpdateBoostAmount(float amount)
    {
        ChangeBoostFill(amount);
    }

    public void UpdateBoostAmount(float amount, float useThreshold)
    {
        ChangeBoostFill(amount);

        // change color if needed
        if (boostText.color != Color.white && amount >= useThreshold)
        {
            boostText.color = Color.white;
        }
    }

    public void ChangeBoostTextColor()
    {
        boostText.color = outOfResourceTextColor;
    }

    public void DisableIntroText()
    {
        introTextBox.SetActive(false);
    }

    private void ChangeBoostFill(float amount)
    {
        // change fill
        if (amount >= 0 && amount <= 100)
        {
            boostBar.fillAmount = amount / 100;
        }
    }

    private IEnumerator ShowAndFadeText(TMP_Text textBox, string text, Color startColor, float displayDuration, float fadeDuration)
    {
        // save original color to be able to return it at the end
        Color originalColor = textBox.color;

        // change text color to assigned color
        textBox.color = startColor;

        // add assigned text
        textBox.text = text;

        textBox.gameObject.SetActive(true);

        // set fadeColor to start color with an alpha of 0
        Color fadeColor = startColor;
        fadeColor.a = 0;

        // wait for display time
        yield return new WaitForSeconds(displayDuration);

        // gradually reduce alpha over time
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textBox.color = Color.Lerp(startColor, fadeColor, elapsedTime / fadeDuration);
            yield return null;
        }

        // set fully faded
        textBox.color = fadeColor;

        // reset text
        textBox.text = "";

        // disable textbox
        textBox.gameObject.SetActive(false);

        // reset original textbox color
        textBox.color = originalColor;
    }

    private string FormatClockTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int fraction = Mathf.FloorToInt((time * 100) % 100);

        string clockString = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);

        return clockString;
    }
}
