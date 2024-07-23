using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpaceRaceGameManager : MonoBehaviour
{
    public static SpaceRaceGameManager Instance { get; private set; }
    public bool IsGameActive { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    // checkpoint variables
    private const float checkpointBoundaryX = 100.0f;
    private const float checkpointBoundaryY = 75.0f;
    private const float distanceBetweenCheckpoints = 300.0f;
    private const int initialCheckpointsToLoad = 4;
    private const float checkpointBuffer = 25.0f;
    private int checkpointsLoaded = 0;
    private List<SpaceRaceCheckpoint> activeCheckpoints = new List<SpaceRaceCheckpoint>();

    // asteroid variables
    private const float asteroidBoundaryX = 150.0f;
    private const float asteroidBoundaryY = 100.0f;
    private const float asteroidBuffer = 25.0f;
    private const int asteroidsPerCheckpoint = 50;

    private const float endGameSequenceTime = 5.0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsGameActive = true;

        for (int i = 0; i < initialCheckpointsToLoad; i++)
        {
            SpawnNewCheckpoint();
            SpawnNewAsteroids();
        }
    }

    private void Update()
    {
        CheckCheckpoints();
    }

    private void CheckCheckpoints()
    {
        if (IsGameActive)
        {
            for (int i = activeCheckpoints.Count - 1; i >= 0; i--)
            {
                SpaceRaceCheckpoint checkpoint = activeCheckpoints[i];

                if (!checkpoint.CheckpointSuccess && checkpoint.transform.position.z + checkpointBuffer < playerTransform.position.z)
                {
                    Debug.Log("Checkpoint missed!");
                    ChangeCheckpointsToRed();
                    EndGame();
                }
            }
        }
    }

    private void ChangeCheckpointsToRed()
    {
        foreach (SpaceRaceCheckpoint checkpoint in activeCheckpoints)
        {
            checkpoint.ChangeColor(false);
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
            // calculate max based on next checkpoint area - buffer around the checkpoint
            float zMax = zMin + distanceBetweenCheckpoints - asteroidBuffer;
            zPos = Random.Range(zMin, zMax);
        }
        else
        {
            // calculate z positioning based on how many checkpoints have been loaded in scene
            zPos = checkpointsLoaded * distanceBetweenCheckpoints + distanceBetweenCheckpoints;
        }

        return new Vector3(randomX, randomY, zPos);
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

    public void EndGame()
    {
        IsGameActive = false;
        virtualCamera.Follow = null;

        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // wait for alloted amount of time (for crash scene or exit scene)
        yield return new WaitForSeconds(endGameSequenceTime);

        // deactivate player object
        playerTransform.gameObject.SetActive(false);

        Debug.Log("End game and scene here.");
    }
}
