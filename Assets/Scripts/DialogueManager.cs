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

    public void DisplayDialogue(string text)
    {
        dialoguePanel.SetActive(true);
    }
}
