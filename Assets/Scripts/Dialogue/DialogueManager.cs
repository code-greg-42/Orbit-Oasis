using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject goNextIndicator;

    // keybinds
    private KeyCode nextDialogueKey = KeyCode.Return;
    private KeyCode nextDialogueKeyAlt = KeyCode.F;
    private KeyCode nextDialogueKeyAltTwo = KeyCode.Escape;

    // dialogue window word display
    private const float maxDelayTime = 10.0f;
    private const float wordDisplayDelay = 0.1f;
    private const float initialDelay = 0.3f;

    public enum PlaceholderType
    {
        PlayerName,
        Money,
    }

    private readonly Dictionary<PlaceholderType, string> placeholderPatterns = new()
    {
        { PlaceholderType.PlayerName, @"\[PLAYERNAME_PH\]" },
        { PlaceholderType.Money, @"\[MONEY_PH\]" }
    };

    private Coroutine dialogueCoroutine;

    public bool DialogueWindowActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public List<string> GetDialogue(string path, Dictionary<PlaceholderType, string> replacements = null)
    {
        // prepend the "dialogue/" base path
        path = "Dialogue/" + path;

        // load all text in the specified folder
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(path);

        // extract text from each asset and return as array of strings
        List<string> dialogues = new();

        // loop through each asset and add string to list
        foreach (TextAsset textAsset in textAssets)
        {
            dialogues.Add(textAsset.text);
        }

        // if replacement values were given, replace the placeholders
        if (replacements != null && replacements.Count > 0)
        {
            dialogues = ReplacePlaceholders(dialogues, replacements);
        }

        return dialogues;
    }

    private List<string> ReplacePlaceholders(List<string> dialogues, Dictionary<PlaceholderType, string> replacements)
    {
        List<string> updatedDialogues = new(dialogues);

        foreach (var kvp in replacements)
        {
            // get corresponding regex pattern from pattern dictionary
            if (placeholderPatterns.TryGetValue(kvp.Key, out string pattern))
            {
                string replacementText = kvp.Value;

                // loop through and replace all instances of the corresponding pattern with the replacement value
                for (int i = 0; i < updatedDialogues.Count; i++)
                {
                    updatedDialogues[i] = Regex.Replace(updatedDialogues[i], pattern, replacementText);
                }
            }
        }
        return updatedDialogues;
    }

    public void ShowDialogue(List<string> dialogues)
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        // start display coroutine
        dialogueCoroutine = StartCoroutine(ShowDialogueCoroutine(dialogues));
    }

    // overloaded method for being part of a larger coroutine -- waits for dialogue coroutine to finish
    public IEnumerator ShowDialogue(List<string> dialogues, bool await)
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        // start display coroutine and wait to finish
        yield return dialogueCoroutine = StartCoroutine(ShowDialogueCoroutine(dialogues));
    }

    private IEnumerator ShowDialogueCoroutine(List<string> dialogues)
    {
        // activate dialogue window
        DialogueWindowActive = true;
        dialoguePanel.SetActive(true);

        // wait allotted amount for eyes to adjust to scene transition
        yield return new WaitForSeconds(initialDelay);

        // loop through and display all strings in list
        foreach (string dialogue in dialogues)
        {
            // clear dialogue text
            dialogueText.text = "";

            string[] words = dialogue.Split(' '); // split sentences into array of words

            foreach (string word in words)
            {
                dialogueText.text += word + " ";

                // PLACE SOUND EFFECT HERE LATER

                yield return new WaitForSeconds(wordDisplayDelay);
            }

            // activate go next indicator
            goNextIndicator.SetActive(true);

            // start timer for dialogue to move to next sentence, with user key working as well
            float elapsedTime = 0f;
            while (elapsedTime < maxDelayTime)
            {
                elapsedTime += Time.deltaTime;
                if (Input.GetKeyDown(nextDialogueKey) || Input.GetKeyDown(nextDialogueKeyAlt) || Input.GetKeyDown(nextDialogueKeyAltTwo))
                {
                    elapsedTime = maxDelayTime;
                }

                yield return null;
            }

            // reset indicator
            goNextIndicator.SetActive(false);
        }

        // reset text, deactivate and set bool back to false
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        DialogueWindowActive = false;
    }
}
