using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;

    private Coroutine dialogueCoroutine;

    private KeyCode nextDialogueKey = KeyCode.Return;
    private KeyCode nextDialogueKeyAlt = KeyCode.F;
    private const float maxDelayTime = 10.0f;
    private const float wordDisplayDelay = 0.1f;
    private const float initialDelay = 0.3f;

    public bool DialogueWindowActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public List<string> GetDialogue(string path)
    {
        // prepend the "dialogue/" base path
        path = "Dialogue/" + path;

        // load all text in the specified folder
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(path);

        // extract text from each asset and return as array of strings
        List<string> dialogues = new List<string>();

        foreach (TextAsset textAsset in textAssets)
        {
            dialogues.Add(textAsset.text);
        }

        return dialogues;
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

            // start timer for dialogue to move to next sentence, with user key working as well
            float elapsedTime = 0f;
            while (elapsedTime < maxDelayTime)
            {
                elapsedTime += Time.deltaTime;
                if (Input.GetKeyDown(nextDialogueKey) || Input.GetKeyDown(nextDialogueKeyAlt))
                {
                    elapsedTime = maxDelayTime;
                }

                yield return null;
            }
        }

        // reset text, deactivate and set bool back to false
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        DialogueWindowActive = false;
    }
}
