using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance;

    // centralized references
    [Header("Script References")]
    //[SerializeField] private CinemachineFreeLook cinemachineCam;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private CinemachineControls cameraControls;

    [Header("Loading Screen References")]
    [SerializeField] private Image loadingScreenPanel;
    [SerializeField] private TMP_Text loadingText;

    // variables for handling player falling off edge
    private int numberOfFalls;
    private bool playerResetInProgress;
    private Vector3 playerResetLocation = new(0, 250, 0);
    private const string fallOffDialoguePath = "Robot/FallOffEdge/Regular";
    private const string fallOffDialoguePathAlt = "Robot/FallOffEdge/Repetitive";
    private const string stuckDialoguePath = "Robot/PlayerStuck";
    private const float resetDelayTime = 2.0f;
    private const int altDialogueThreshold = 3;
    private const float fallYBoundary = -50;

    // load screen variables
    private const string introLoadingString = "loading in";
    private const string spaceRaceLoadingString = "initiating race";
    private const string menuLoadingString = "exiting oasis";
    private const string loadingTextEnding = "...";
    private const float initialLoadingDelay = 0.15f;
    private const float charDelayOne = 0.08f;
    private const float charDelayTwo = 0.42f;
    private const float loadingTextEndDelay = 1.0f;
    private const float loadingTextFadeDuration = 0.8f;
    private const float loadingScreenFadeDuration = 2.0f;
    private const float spaceRaceFadeDuration = 2.46f;
    private const float spaceRaceTextFadeDuration = 0.2f;

    private Coroutine startNewSceneCoroutine;
    private float saveTimer = 0.0f;
    private const float saveInterval = 180.0f;

    // start cam settings
    private float startCamX = 0f;
    private float startCamY = 0.6f;

    public bool IsSwappingScenes { get; private set; }
    public bool IsLoadingIn { get; private set; } = true;

    // start dialogue once scene is 50% through the fade in
    public float LoadingWaitTime => initialLoadingDelay + (introLoadingString.Length * charDelayOne) +
        (loadingTextEnding.Length * charDelayTwo) + loadingTextEndDelay + (loadingScreenFadeDuration * 0.5f);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartSceneCoroutine());
    }

    private void Update()
    {
        CheckForFallOffEdge();
        AutoSaveDynamicData();
    }

    public void UnstuckPlayer()
    {
        playerMovement.SetPlayerPosition(playerResetLocation);

        // get dialogue
        List<string> dialogue = DialogueManager.Instance.GetDialogue(stuckDialoguePath);
        // show dialogue
        DialogueManager.Instance.ShowDialogue(dialogue);
    }

    public void StartSpaceRaceScene(int difficulty)
    {
        // set data manager's difficulty variable to carry over into new scene
        DataManager.Instance.SetRaceDifficulty(difficulty);

        startNewSceneCoroutine ??= StartCoroutine(StartNewSceneCoroutine(true));
    }

    // added to OnClick of button in the inspector
    public void ReturnToMenu()
    {
        // close all open menus
        playerControls.EscapeMenusAndBuildMode();

        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Select);

        startNewSceneCoroutine ??= StartCoroutine(StartNewSceneCoroutine(false));
    }

    // used by double clicking item in inventory slot
    public void ToggleGravityHacks()
    {
        playerMovement.ToggleGravityHacks();
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
        cameraControls.DisableCam();

        // wait for reset delay time
        yield return new WaitForSeconds(resetDelayTime);

        // reset player position to reset location
        playerMovement.SetPlayerPosition(playerResetLocation);

        // wait short amount before turning cam back on
        yield return new WaitForSeconds(0.5f);

        // turn cam back on
        cameraControls.EnableCam();

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

    private IEnumerator StartSceneCoroutine()
    {
        // prevent camera movement from any initial sporadic mouse movement
        cameraControls.ToggleMouseMovement(true, false);

        // slight initial delay
        yield return new WaitForSeconds(initialLoadingDelay);

        // only load player position if done the first intro quest
        if (DataManager.Instance.PlayerStats.QuestIndex > 0)
        {
            // load in player and camera positioning from data manager
            playerMovement.LoadPlayerPosition();
            cameraControls.LoadCameraPos(DataManager.Instance.PlayerStats.CameraX, DataManager.Instance.PlayerStats.CameraY);
        }
        else
        {
            // adjust start cam for a better first look
            cameraControls.LoadCameraPos(startCamX, startCamY);
        }

        // show and wait for loading text
        yield return StartCoroutine(ShowLoadingText(introLoadingString));

        // fade out text
        StartCoroutine(FadeUI.Fade(loadingText, 0f, loadingTextFadeDuration));

        // re-enable camera movement from mouse movement to allow player to look around as game fades in
        cameraControls.ToggleMouseMovement(false, false);

        // fade in scene by fading out loading panel
        yield return FadeUI.Fade(loadingScreenPanel, 0f, loadingScreenFadeDuration);

        IsLoadingIn = false;
    }

    private IEnumerator StartNewSceneCoroutine(bool isSpaceRace)
    {
        IsSwappingScenes = true;

        string loadScreenString = isSpaceRace ? spaceRaceLoadingString : menuLoadingString;

        // fade to black
        StartCoroutine(FadeUI.Fade(loadingScreenPanel, 1f, spaceRaceFadeDuration));

        // fade in loading text
        StartCoroutine(FadeUI.Fade(loadingText, 1f, spaceRaceTextFadeDuration));

        // update DataManager with player and animal positioning
        SaveDynamicData();

        // show loading text
        yield return ShowLoadingText(loadScreenString);
        yield return new WaitForSeconds(0.2f);

        if (isSpaceRace)
        {
            // load space race scene
            SceneManager.LoadScene("SpaceRace");
        }
        else
        {
            // load menu
            SceneManager.LoadScene("Menu");
        }
    }

    private void SaveDynamicData()
    {
        DataManager.Instance.SaveDynamicData(playerMovement.PlayerPosition, playerMovement.PlayerRotation,
            cameraControls.CameraReference, AnimalManager.Instance.ActiveAnimals);
    }

    private void AutoSaveDynamicData()
    {
        saveTimer += Time.deltaTime;

        if (saveTimer >= saveInterval)
        {
            if (playerMovement.IsGrounded)
            {
                SaveDynamicData();
                saveTimer = 0.0f;
            }
        }
    }

    private IEnumerator ShowLoadingText(string text)
    {
        // clear existing text
        loadingText.text = "";

        // split string into chars
        char[] chars = text.ToCharArray();
        char[] periods = loadingTextEnding.ToCharArray();

        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.LoadingSound);

        foreach (char c in chars)
        {
            loadingText.text += c;
            yield return new WaitForSeconds(charDelayOne);
        }

        MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.LoadingSound);

        foreach (char p in periods)
        {
            loadingText.text += p;
            yield return new WaitForSeconds(charDelayTwo);
        }

        yield return new WaitForSeconds(loadingTextEndDelay);
    }
}
