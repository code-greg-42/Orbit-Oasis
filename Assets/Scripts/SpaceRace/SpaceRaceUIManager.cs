using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceUIManager : MonoBehaviour
{
    public static SpaceRaceUIManager Instance { get; private set; }

    [SerializeField] private TMP_Text introText;
    [SerializeField] private GameObject introTextBox;
    [SerializeField] private TMP_Text rocketsAmountText;
    [SerializeField] private Image boostBar;
    [SerializeField] private TMP_Text countdownTimerText;

    // sentence display variables
    private float wordDisplayDelay = 0.1f; // rate of words being added to the sentence
    private float sentenceDisplayDelay = 5.0f; // total time of pause after sentence is complete

    private readonly List<string> introSentences = new List<string>
    {
        "You are approaching the asteroid field. Fly through the <color=#FFF300>checkpoints</color> to complete the race.",
        "<color=#00FF00>[WASD]</color> to move, <color=#77B5FF>[shift]</color> to boost, <color=#FF0000>[space]</color> for rockets. Rockets are limited! Good luck!"
    };

    private Coroutine countdownCoroutine;

    public bool CountdownStarted { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(DisplayIntroText());
    }

    private IEnumerator DisplayIntroText()
    {
        for (int i = 0; i < introSentences.Count; i++)
        {
            introText.text = "";
            string[] words = introSentences[i].Split(' '); // split sentences into array of words

            foreach (string word in words)
            {
                introText.text += word + " ";
                yield return new WaitForSeconds(wordDisplayDelay);
            }
            // pause to allow reading
            yield return new WaitForSeconds(sentenceDisplayDelay);
        }
    }

    private IEnumerator DisplayCountdown()
    {
        int countdown = 3;

        while (countdown > 0)
        {
            countdownTimerText.text = countdown.ToString();
            countdown--;
            yield return new WaitForSeconds(1);
        }

        countdownTimerText.text = "GO!";

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
        }
    }

    public void DisableIntroText()
    {
        introTextBox.SetActive(false);
    }
}
