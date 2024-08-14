using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;

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
}
