using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance;

    // centralized references
    [SerializeField] CinemachineFreeLook cinemachineCam;
    [SerializeField] PlayerMovement playerMovement;

    // variables for handling player falling off edge
    private int numberOfFalls;
    private bool playerResetInProgress;
    private Vector3 playerResetLocation = new(0, 300, 0);
    private const string fallOffDialoguePath = "Robot/FallOffEdge/Regular";
    private const string fallOffDialoguePathAlt = "Robot/FallOffEdge/Repetitive";
    private const string stuckDialoguePath = "Robot/PlayerStuck";
    private const float resetDelayTime = 2.0f;
    private const int altDialogueThreshold = 3;
    private const float fallYBoundary = -50;

    // scene start variables
    private const float startSceneLoadDelay = 1.0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerMovement.LoadPlayerPosition();
        LoadCameraPos();
    }

    private void Update()
    {
        CheckForFallOffEdge();
    }

    public void SetPlayerAndCameraPos()
    {
        DataManager.Instance.SetPlayerPosition(playerMovement.PlayerPosition, playerMovement.PlayerRotation);
        DataManager.Instance.SetCameraValues(cinemachineCam.m_XAxis.Value, cinemachineCam.m_YAxis.Value);
    }

    public void UnstuckPlayer()
    {
        playerMovement.SetPlayerPosition(playerResetLocation);

        // get dialogue
        List<string> dialogue = DialogueManager.Instance.GetDialogue(stuckDialoguePath);
        // show dialogue
        DialogueManager.Instance.ShowDialogue(dialogue);
    }

    private void CheckForFallOffEdge()
    {
        if (playerMovement.HasFallenOffEdge(fallYBoundary) && !playerResetInProgress)
        {
            playerResetInProgress = true;
            numberOfFalls++;
            StartCoroutine(FallOffEdgeCoroutine());
        }
    }

    private IEnumerator FallOffEdgeCoroutine()
    {
        // turn off camera for falling effect
        cinemachineCam.enabled = false;

        // wait for reset delay time
        yield return new WaitForSeconds(resetDelayTime);

        // reset player position to reset location
        playerMovement.SetPlayerPosition(playerResetLocation);

        // wait short amount before turning cam back on
        yield return new WaitForSeconds(0.5f);

        // turn cam back on
        cinemachineCam.enabled = true;

        // get dialogue path
        string dialoguePath = fallOffDialoguePath;

        // if player has fallen off repetitively, get alt dialogue path
        if (numberOfFalls >= altDialogueThreshold)
        {
            dialoguePath = fallOffDialoguePathAlt;
        }

        // get dialogue
        List<string> dialogue = DialogueManager.Instance.GetDialogue(dialoguePath);

        // display dialogue
        DialogueManager.Instance.ShowDialogue(dialogue);

        // small delay before resetting bool
        yield return new WaitForSeconds(0.5f);

        playerResetInProgress = false;
    }

    private void LoadCameraPos()
    {
        float camX = DataManager.Instance.PlayerStats.CameraX;
        float camY = DataManager.Instance.PlayerStats.CameraY;

        if (camX != 0 || camY != 0)
        {
            cinemachineCam.m_XAxis.Value = camX;
            cinemachineCam.m_YAxis.Value = camY;
        }
    }

    private IEnumerator StartSceneCoroutine()
    {
        yield return new WaitForSeconds(startSceneLoadDelay);

        playerMovement.LoadPlayerPosition();
        LoadCameraPos();
    }
}
