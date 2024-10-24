using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] deadTreePrefabs;
    private const float boundary = 30.0f;
    private const float xAvoid = -10.0f;
    private const float zAvoid = -17.0f;
    private const float middleAvoid = 5.0f;

    private const float distanceCheck = 3.0f;
    private const int loopCheck = 10;

    private List<Vector2> treePositions;

    private void Awake()
    {
        treePositions = new List<Vector2>();
    }

    private void Start()
    {
        if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.RemoveDeadTrees)
        {
            SpawnDeadTrees(QuestManager.Instance.DeadTreesToSpawn);
        }
    }

    private Vector2 GenerateRandomSpawnPosition(int stopCounter = 0)
    {
        // prevents infinite loop
        if (stopCounter >= loopCheck)
        {
            // place just outside boundary --- incredibly unlikely this would even happen once, let alone more than once
            Debug.LogWarning($"GenerateRandomSpawnPosition called {stopCounter} times but was unable to find an uncontested spawn position. " +
                "Placing just outside boundary");
            Vector2 justOutsideBoundary = new(boundary + 1.5f, boundary + 1.5f);
            return justOutsideBoundary;
        }

        // randomize x and z values, explicitly excluding the corner where the spaceship is and the middle where the player is
        // this avoids additional recursive calls
        float randomX = Random.Range(-boundary, boundary);
        float randomZ = RandomizeZBasedOnX(randomX);

        Vector2 randomSpawn = new(randomX, randomZ);

        // loop through each tree position in the list of current positions
        foreach (Vector2 treePos in treePositions)
        {
            if (Vector2.Distance(randomSpawn, treePos) < distanceCheck)
            {
                return GenerateRandomSpawnPosition(++stopCounter);
            }
        }

        return randomSpawn;
    }

    private void SpawnDeadTrees(int treesToSpawn)
    {
        for (int i = 0; i < treesToSpawn; i++)
        {
            // randomize selection of which prefab to spawn
            int randomSelection = Random.Range(0, deadTreePrefabs.Length);
            GameObject selectedTree = deadTreePrefabs[randomSelection];

            // generate random XZ (excluding avoid zone)
            Vector2 randomSpawn = GenerateRandomSpawnPosition();

            // add random spawn to list of current positions (to avoid trees spawning on top of each other)
            treePositions.Add(randomSpawn);

            // convert to vector3, using the prefabs original Y value (makes object sit at ground height)
            Vector3 spawnPos = new(randomSpawn.x, selectedTree.transform.position.y, randomSpawn.y);

            // instantiate prefab into scene
            GameObject deadTree = Instantiate(selectedTree);

            // set position to randomized spawn position
            deadTree.transform.position = spawnPos;
        }
    }

    private float RandomizeZBasedOnX(float xValue)
    {
        if (xValue <= xAvoid)
        {
            // avoid bottom left quadrant by spaceship
            return Random.Range(zAvoid, boundary);
        }
        else if (xValue >= -middleAvoid && xValue <= middleAvoid)
        {
            // avoid middle by player
            if (Random.value < 0.5f)
            {
                return Random.Range(-boundary, -middleAvoid);
            }
            else
            {
                return Random.Range(middleAvoid, boundary);
            }
        }
        else
        {
            // allow full range
            return Random.Range(-boundary, boundary);
        }
    }
}
