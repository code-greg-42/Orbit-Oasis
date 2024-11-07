using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishingGameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] fishPrefabs;
    [SerializeField] private TMP_Text fishClock;

    private const float playerBoundaryX = 2.5f; // used for avoiding spawning on the player

    // define boundaries of spawn area
    private const float spawnBoundaryX = 8;
    private const float spawnBoundaryY = 4;

    // weights for probabilities of how many fish to spawn and what type of fish to spawn
    private int[] fishCountWeights = { 30, 45, 20, 5 }; // weights for 1 fish, 2 fish, 3 fish, 4 fish
    private int[] fishTypeWeights = { 35, 25, 5, 35 }; // weights for order of fish prefabs (small, medium, large, explosive)

    private void Start()
    {
        SpawnFish();
        StartCoroutine(ClockCoroutine());
    }

    private void SpawnFish()
    {
        int numFish = WeightedRandom.GetWeightedRandomIndex(fishCountWeights) + 1; // ensure between 1 and 4

        for (int i = 0; i < numFish; i++)
        {
            Vector2 spawnPos = GenerateRandomSpawn();
            int fishIndex = WeightedRandom.GetWeightedRandomIndex(fishTypeWeights);
            Instantiate(fishPrefabs[fishIndex], spawnPos, fishPrefabs[fishIndex].transform.rotation);
        }
    }

    private IEnumerator ClockCoroutine()
    {
        int clock = 5;

        while (clock >= 0)
        {
            fishClock.text = clock.ToString();
            yield return new WaitForSeconds(1f);
            clock--;
        }

        Debug.Log("Fishing scene end.");
    }

    private Vector2 GenerateRandomSpawn()
    {
        float randomX = GetRandomX();
        float randomY = Random.Range(-spawnBoundaryY, spawnBoundaryY);

        return new Vector2(randomX, randomY);
    }

    private float GetRandomX()
    {
        // generate random numbers between the valid spawn points, then randomize which number is picked
        if (Random.Range(0, 2) == 0)
        {
            return Random.Range(-spawnBoundaryX, -playerBoundaryX);
        }
        else
        {
            return Random.Range(playerBoundaryX, spawnBoundaryX);
        }
    }
}
