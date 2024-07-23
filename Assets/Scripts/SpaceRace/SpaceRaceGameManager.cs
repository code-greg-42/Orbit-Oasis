using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpaceRaceGameManager : MonoBehaviour
{
    public static SpaceRaceGameManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;

    private const float checkpointBoundaryX = 100.0f;
    private const float checkpointBoundaryY = 75.0f;
    private const float distanceBetweenCheckpoints = 300.0f;
    private const int initialCheckpointsToLoad = 4;

    private int checkpointsLoaded = 0;

    private List<SpaceRaceCheckpoint> activeCheckpoints = new List<SpaceRaceCheckpoint>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < initialCheckpointsToLoad; i++)
        {
            SpawnNewCheckpoint();
        }
    }

    private void Update()
    {
        CheckCheckpoints();
    }

    private void CheckCheckpoints()
    {
        for (int i = activeCheckpoints.Count - 1; i >= 0; i--)
        {
            SpaceRaceCheckpoint checkpoint = activeCheckpoints[i];

            if (!checkpoint.CheckpointSuccess && checkpoint.transform.position.z < playerTransform.position.z)
            {
                Debug.Log("Checkpoint missed!");
            }
        }
    }

    private Vector3 GetCheckpointLocation(float xBoundary, float yBoundary)
    {
        // randomize x and y values
        float randomX = Random.Range(-xBoundary, xBoundary);
        float randomY = Random.Range(-yBoundary, yBoundary);

        // calculate z positioning based on how many checkpoints have been loaded in scene
        float zPosition = checkpointsLoaded * distanceBetweenCheckpoints + distanceBetweenCheckpoints;

        return new Vector3(randomX, randomY, zPosition);
    }

    public void SpawnNewCheckpoint()
    {
        // get checkpoint object from object pool
        GameObject checkpoint = CheckpointPool.Instance.GetPooledObject();

        if (checkpoint != null)
        {
            // calculate randomized positioning
            Vector3 newPosition = GetCheckpointLocation(checkpointBoundaryX, checkpointBoundaryY);

            // set checkpoint object to newly calculated position
            checkpoint.transform.position = newPosition;

            // activate in scene
            checkpoint.SetActive(true);

            // update counter
            checkpointsLoaded++;
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
}
