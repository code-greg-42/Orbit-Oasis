using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private int questStatus;
    private int questTotalNeeded = 10;
    
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCurrentQuest()
    {
        questStatus++;
        Debug.Log($"Quest updated! Completion: {questStatus}/{questTotalNeeded}");
    }
}
