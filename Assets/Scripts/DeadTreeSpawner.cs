using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] deadTreePrefabs;
    private const int treesToSpawn = 2; // TEMPORARY: normal setting 10
    private const float boundary = 30.0f;
    private const float xAvoid = -10.0f;
    private const float zAvoid = -17.0f;

    private const float distanceCheck = 3.0f;

    private List<Vector2> treePositions;

    private void Awake()
    {
        treePositions = new List<Vector2>();
    }

    private void Start()
    {
        if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.RemoveDeadTrees)
        {
            SpawnDeadTrees();
        }
    }

    private Vector2 GenerateRandomSpawnPosition(int stopCounter = 0)
    {
        // prevents infinite loop
        if (stopCounter >= treesToSpawn)
        {
            Debug.LogWarning($"GenerateRandomSpawnPosition called {treesToSpawn} times but was unable to find an uncontested spawn position.");
            return Vector2.zero;
        }

        // randomize x and z values, excluding the corner where the spaceship is
        float randomX = Random.Range(-boundary, boundary);
        float randomZ = randomX < xAvoid ? Random.Range(zAvoid, boundary) : Random.Range(-boundary, boundary);

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

    private void SpawnDeadTrees()
    {
        for (int i = 0; i < treesToSpawn; i++)
        {
            // randomize selection of which prefab to spawn
            int randomSelection = Random.Range(0, deadTreePrefabs.Length);
            GameObject selectedTree = deadTreePrefabs[randomSelection];

            // generate random XZ (excluding avoid zone)
            Vector2 randomSpawn = GenerateRandomSpawnPosition();

            if (randomSpawn == Vector2.zero)
            {
                Debug.LogWarning("Unable to find valid random spawn.");
                continue;
            }

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
}
