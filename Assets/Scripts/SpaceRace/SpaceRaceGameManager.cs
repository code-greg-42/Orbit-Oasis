using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceRaceGameManager : MonoBehaviour
{
    public static SpaceRaceGameManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform checkpointIndicator;
    [SerializeField] private SpaceRacePlayerMovement playerMovement;
    [SerializeField] private SpaceRacePlayerAttack playerAttack;

    // rewards for winning
    private readonly float[] currencyRewards = { 500, 1500, 6000 };
    private const float bestTimeRewardMultiplier = 2.0f;

    // checkpoint variables
    private float distanceBetweenCheckpoints = 300.0f;
    private int finalCheckpoint = 20; // last checkpoint to win the race
    private const float checkpointBoundaryX = 80.0f;
    private const float checkpointBoundaryY = 60.0f;
    private const int initialCheckpointsToLoad = 6;
    private float checkpointBuffer = 25.0f;
    private int checkpointsLoaded = 0;
    private List<SpaceRaceCheckpoint> activeCheckpoints = new List<SpaceRaceCheckpoint>();
    private SpaceRaceCheckpoint nextCheckpoint;
    private float missedCheckpointBuffer = 25.0f;

    // asteroid variables
    private const float asteroidBoundaryX = 150.0f;
    private const float asteroidBoundaryY = 100.0f;
    private const float asteroidBuffer = 25.0f;
    private int asteroidsPerCheckpoint = 35;
    private List<SpaceRaceAsteroid> activeAsteroids = new List<SpaceRaceAsteroid>();
    private int[] asteroidPrefabWeights = { 15, 15, 15, 15, 40 };
    private float asteroidMovementModifier = 1.0f;

    // indicator/navigation variables
    private const float indicatorRotationSpeed = 100.0f;

    // game management variables
    private float playerMovementSpeed = 40.0f;
    private float gameClock = 0.0f;
    private float gameClockInterval = 0.01f; // interval that gameclock changes in
    private const float endGameSequenceTime = 5.0f;
    private Coroutine gameClockCoroutine;
    private Coroutine updateClockCoroutine;
    private int selectedDifficulty;

    // difficulty settings
    private readonly float[] difficultyMovementSpeeds = { 40f, 50f, 60f };
    private readonly int[] difficultyAsteroidAmounts = { 20, 35, 50 };
    private readonly float[] difficultyCheckpointDistances = { 300f, 350f, 450f };
    private readonly float[] difficultyMissedCheckpointBuffer = { 25f, 35f, 50f };
    private readonly float[] difficultyAsteroidMovementModifier = { 0.8f, 1.0f, 1.4f };
    private readonly int[] difficultyFinalCheckpointSettings = { 12, 16, 20 };

    // separate bool from IsGameActive that is set to true once and that's it -- used only for starting the race
    private bool hasRaceStarted;

    public bool IsGameActive { get; private set; }
    public float FinalAsteroidBoundary
    {
        get { return distanceBetweenCheckpoints * finalCheckpoint + checkpointBuffer; } // boundary for z end of asteroid field
    }
    public float AsteroidBoundaryX => asteroidBoundaryX;
    public float AsteroidBoundaryY => asteroidBoundaryY;
    public int[] AsteroidPrefabWeights => asteroidPrefabWeights;
    public List<SpaceRaceCheckpoint> ActiveCheckpoints => activeCheckpoints;
    public int FinalCheckpoint => finalCheckpoint;
    public int CheckpointsLoaded => checkpointsLoaded;
    public float AsteroidMovementModifier => asteroidMovementModifier;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // set difficulty and upgrade levels from data manager
        selectedDifficulty = DataManager.Instance.RaceStats.SelectedDifficulty;
        SetDifficulty(selectedDifficulty);
        playerMovement.SetBoostUpgradeLevel(DataManager.Instance.RaceStats.BoostUpgradeLevel);
        playerAttack.SetRocketUpgradeLevel(DataManager.Instance.RaceStats.RocketUpgradeLevel);

        // spawn initial checkpoints and asteroids
        SpawnInitialScene();
    }

    private void Update()
    {
        if (!IsGameActive)
        {
            // positional setting set for intro speed of 20 and a 3 second countdown (20 x 4)
            if (!SpaceRaceUIManager.Instance.CountdownStarted && playerTransform.position.z >= -80)
            {
                SpaceRaceUIManager.Instance.StartCountdown();
            }

            if (!hasRaceStarted && playerTransform.position.z > 0)
            {
                StartRace();
            }
        }
        else
        {
            CheckCheckpoints();
            UpdateCheckpointIndicator();
        }
    }

    private void SetDifficulty(int difficulty)
    {
        if (difficulty >= 0 && difficulty <= difficultyMovementSpeeds.Length)
        {
            // set movement speed for passing to PlayerMovement script
            playerMovementSpeed = difficultyMovementSpeeds[difficulty];

            // set checkpoint and asteroid spawn variables
            asteroidsPerCheckpoint = difficultyAsteroidAmounts[difficulty];
            distanceBetweenCheckpoints = difficultyCheckpointDistances[difficulty];
            missedCheckpointBuffer = difficultyMissedCheckpointBuffer[difficulty];

            // update movement modifier for asteroid script
            asteroidMovementModifier = difficultyAsteroidMovementModifier[difficulty];

            // set finish line number
            finalCheckpoint = difficultyFinalCheckpointSettings[difficulty];
        }
        else
        {
            Debug.LogWarning("difficulty set incorrectly in game manager script.");
        }
    }

    private void StartRace()
    {
        IsGameActive = true;
        hasRaceStarted = true;

        playerMovement.SetRaceSpeed(playerMovementSpeed);

        // disable intro UI components
        SpaceRaceUIManager.Instance.DisableIntroText();

        // activate in game UI components
        checkpointIndicator.gameObject.SetActive(true);
        StartGameClock();

        // adjust engine sound effect
        SpaceRaceSoundManager.Instance.SetEnginePitch();
    }

    private void StartGameClock()
    {
        if (gameClockCoroutine == null && updateClockCoroutine == null)
        {
            gameClockCoroutine = StartCoroutine(TrackGameClock());
            updateClockCoroutine = StartCoroutine(UpdateGameClock());
        }
    }

    private IEnumerator TrackGameClock()
    {
        while (IsGameActive)
        {
            gameClock += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator UpdateGameClock()
    {
        while (IsGameActive)
        {
            yield return new WaitForSeconds(gameClockInterval);

            // update UI
            SpaceRaceUIManager.Instance.UpdateGameClock(gameClock);
        }
    }

    private void UpdateCheckpointIndicator()
    {
        // ignore adjusting indicator when player gets too close to the checkpoint for aesthetics
        if (nextCheckpoint != null && nextCheckpoint.transform.position.z - playerTransform.position.z > checkpointBuffer)
        {
            // calc direction from the player to next checkpoint
            Vector3 checkpointDirection = nextCheckpoint.transform.position - playerTransform.position;

            // create target rotation based on direction
            Quaternion targetRotation = Quaternion.LookRotation(checkpointDirection);

            // smoothly rotate
            checkpointIndicator.rotation = Quaternion.RotateTowards(checkpointIndicator.rotation, targetRotation, indicatorRotationSpeed * Time.deltaTime);
        }
    }

    private void CheckCheckpoints()
    {
        if (IsGameActive)
        {
            // backwards in case of list change
            for (int i = activeCheckpoints.Count - 1; i >= 0; i--)
            {
                SpaceRaceCheckpoint checkpoint = activeCheckpoints[i];

                if (!checkpoint.CheckpointSuccess && checkpoint.transform.position.z + missedCheckpointBuffer < playerTransform.position.z)
                {
                    MissedCheckpoint();
                }
            }
        }
    }

    public void CheckpointPassed()
    {
        if (activeCheckpoints.Count > 1)
        {
            nextCheckpoint = activeCheckpoints[1]; // 2nd checkpoint in list because current one has not despawned/been removed yet

            // checkpoint number is calculated before new checkpoint is loaded
            int checkpointNumber = checkpointsLoaded - initialCheckpointsToLoad;

            // skip display message and visual effect for the initial checkpoint (checkpoint number 0)
            if (checkpointNumber > 0)
            {
                SpaceRaceUIManager.Instance.UpdateCheckpointStatusWindow();

                // display checkpoint passed visual effect
                playerMovement.CueCheckpointPassedEffect();

                // play checkpoint passed sound effect
                SpaceRaceSoundManager.Instance.PlayCheckpointPassedSound();
            }
        }
        else
        {
            // EndGame with bool win == true
            EndGame(true);
        }

        if (checkpointsLoaded < finalCheckpoint)
        {
            SpawnNewWave();
        }
    }

    private void MissedCheckpoint()
    {
        // display checkpoint missed status update -- false turns it red and says 'missed'
        SpaceRaceUIManager.Instance.UpdateCheckpointStatusWindow(false);

        // tell sound manager to play sound
        SpaceRaceSoundManager.Instance.PlayCheckpointMissedSound();

        // change coloring of in game future checkpoints to red
        ChangeCheckpointsToRed();

        EndGame();
    }

    private void SpawnNewWave()
    {
        SpawnNewCheckpoint();
        SpawnNewAsteroids();
        DespawnOldAsteroids();
    }

    private void SpawnInitialCheckpoint()
    {
        GameObject checkpoint = CheckpointPool.Instance.GetPooledObject();

        if (checkpoint != null)
        {
            checkpoint.transform.position = Vector3.zero;

            checkpoint.SetActive(true);

            checkpointsLoaded++;
        }
    }

    private void SpawnInitialScene()
    {
        SpawnInitialCheckpoint();
        SpawnNewAsteroids();

        for (int i = 0; i < initialCheckpointsToLoad - 1; i++)
        {
            SpawnNewCheckpoint();
            SpawnNewAsteroids();
        }
    }

    public void SpawnNewCheckpoint()
    {
        // get checkpoint object from object pool
        GameObject checkpoint = CheckpointPool.Instance.GetPooledObject();

        if (checkpoint != null)
        {
            // calculate randomized positioning
            Vector3 newPosition = GetSpawnLocation(checkpointBoundaryX, checkpointBoundaryY);

            // set checkpoint object to newly calculated position
            checkpoint.transform.position = newPosition;

            // activate in scene
            checkpoint.SetActive(true);

            // update counter
            checkpointsLoaded++;
        }
    }

    private void SpawnNewAsteroids()
    {
        for (int i = 0; i < asteroidsPerCheckpoint; i++)
        {
            // get asteroid from pool
            GameObject asteroid = AsteroidPool.Instance.GetPooledObject();

            if (asteroid != null)
            {
                // calculate randomized positioning
                Vector3 newPosition = GetSpawnLocation(asteroidBoundaryX, asteroidBoundaryY, true);

                // set asteroid object to newly calculated position
                asteroid.transform.position = newPosition;

                // activate in scene
                asteroid.SetActive(true);
            }
            else
            {
                Debug.LogError("Unable to get asteroid from asteroid pool.");
            }
        }
    }

    private void DespawnOldAsteroids()
    {
        if (IsGameActive)
        {
            // backwards in case of list change
            for (int i = activeAsteroids.Count - 1; i >= 0; i--)
            {
                SpaceRaceAsteroid asteroid = activeAsteroids[i];

                // if asteroid is behind the player (buffer amount included so asteroid is off screen)
                if (asteroid.transform.position.z < playerTransform.position.z - asteroidBuffer)
                {
                    // disable asteroid (send back to pool)
                    asteroid.gameObject.SetActive(false);
                }
            }
        }
    }

    private Vector3 GetSpawnLocation(float xBoundary, float yBoundary, bool randomizeZ = false)
    {
        // randomize x and y values
        float randomX = Random.Range(-xBoundary, xBoundary);
        float randomY = Random.Range(-yBoundary, yBoundary);
        float zPos;

        if (randomizeZ)
        {
            // calculate min based on checkpoint area + a buffer around the checkpoint
            float zMin = checkpointsLoaded * distanceBetweenCheckpoints + asteroidBuffer;
            // calculate max based on next checkpoint area - buffer around the checkpoint (x2 for near and far buffer)
            float zMax = zMin + distanceBetweenCheckpoints - asteroidBuffer * 2;
            zPos = Random.Range(zMin, zMax);
        }
        else
        {
            // calculate z positioning based on how many checkpoints have been loaded in scene
            zPos = checkpointsLoaded * distanceBetweenCheckpoints + distanceBetweenCheckpoints;
        }

        return new Vector3(randomX, randomY, zPos);
    }

    private void ChangeCheckpointsToRed()
    {
        foreach (SpaceRaceCheckpoint checkpoint in activeCheckpoints)
        {
            checkpoint.ChangeColor(false);
        }
    }

    public void RegisterCheckpoint(SpaceRaceCheckpoint checkpoint)
    {
        activeCheckpoints.Add(checkpoint);
    }

    public void UnregisterCheckpoint(SpaceRaceCheckpoint checkpoint)
    {
        activeCheckpoints.Remove(checkpoint);
    }

    public void RegisterAsteroid(SpaceRaceAsteroid asteroid)
    {
        activeAsteroids.Add(asteroid);
    }

    public void UnregisterAsteroid(SpaceRaceAsteroid asteroid)
    {
        activeAsteroids.Remove(asteroid);
    }

    public float GetCurrentPlayerSpeed()
    {
        return playerMovement.ForwardSpeed;
    }

    public void EndGame(bool win = false)
    {
        IsGameActive = false;
        virtualCamera.Follow = null;
        nextCheckpoint = null;

        checkpointIndicator.gameObject.SetActive(false);

        // initiate music fade out
        SpaceRaceSoundManager.Instance.FadeOutMusic();

        DataManager.Instance.SetRaceCompleted();

        StartCoroutine(EndGameSequence());

        if (win)
        {
            float currentBestTime = DataManager.Instance.RaceStats.BestTimes[DataManager.Instance.RaceStats.SelectedDifficulty];
            float rewardAmount = currencyRewards[selectedDifficulty];

            if (currentBestTime == 0 || gameClock < currentBestTime)
            {
                // display victory text with the 'new best time' message attached
                SpaceRaceUIManager.Instance.DisplayVictoryText(true);

                // play victory sound with a different pitch to notify new best time
                SpaceRaceSoundManager.Instance.PlayWinSound(true);

                // update best time UI component
                SpaceRaceUIManager.Instance.UpdateBestTime(gameClock);

                // set best time in data manager (will always apply to the current difficulty only)
                DataManager.Instance.SetRaceBestTime(gameClock);

                // set race as won and set reward amount with multiplier for achieving a best time
                DataManager.Instance.SetRaceWon(rewardAmount * bestTimeRewardMultiplier);
            }
            else
            {
                // display regular victory text and play regular victory sound
                SpaceRaceUIManager.Instance.DisplayVictoryText();
                SpaceRaceSoundManager.Instance.PlayWinSound();

                // set race as won with normal reward amount
                DataManager.Instance.SetRaceWon(rewardAmount);
            }
        }
    }

    private IEnumerator EndGameSequence()
    {
        // wait for alloted amount of time (for crash scene or exit scene)
        yield return new WaitForSeconds(endGameSequenceTime);

        // deactivate player object
        playerTransform.gameObject.SetActive(false);

        // load the main scene
        SceneManager.LoadScene("Main");
    }
}
