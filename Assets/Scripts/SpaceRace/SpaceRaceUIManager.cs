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

    // sentence display variables
    private float wordDisplayDelay = 0.1f; // rate of words being added to the sentence
    private float sentenceDisplayDelay = 5.0f; // total time of pause after sentence is complete

    private readonly List<string> introSentences = new List<string>
    {
        "You are approaching the asteroid field. Fly through the checkpoints (yellow boxes) to complete the race.",
        "[WASD] to move, [shift] to boost, [space] for rockets. Rockets are limited! Good luck!"
    };

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

    public void DisableIntroText()
    {
        introTextBox.SetActive(false);
    }
}
