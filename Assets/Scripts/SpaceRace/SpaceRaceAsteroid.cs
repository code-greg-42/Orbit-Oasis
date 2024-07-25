using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceAsteroid : MonoBehaviour
{
    private Rigidbody rb;

    private float moveSpeed;

    private float movePercentage = 0.6f; // percentage of asteroids that move

    private int[] speeds = { 15, 30, 45, 80 }; // list of different asteroid speeds
    private int[] speedWeights = { 35, 35, 25, 5 }; // percentages each speed will be picked

    private float[] sizes = { 0.7f, 0.8f, 0.9f, 1.0f, 1.05f, 1.1f, 1.15f, 1.2f, 1.5f, 2f };
    private int[] sizeWeights = { 5, 5, 15, 25, 20, 10, 5, 5, 5, 5 };

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        BoundaryCheck();
    }

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // add to active asteroid list
        SpaceRaceGameManager.Instance.RegisterAsteroid(this);

        // weighted roll for size
        int weightIndex = WeightedRandom.GetWeightedRandomIndex(sizeWeights);
        float sizeAdjustment = sizes[weightIndex];
        transform.localScale *= sizeAdjustment;

        // adjust mass to scale
        rb.mass *= sizeAdjustment;

        // roll for whether or not asteroid should move
        float shouldMove = Random.Range(0f, 1f);

        if (shouldMove <= movePercentage)
        {
            // get random direction
            Vector3 randomDirection = GenerateRandomDirection();

            // get random speed index based on weights
            int speedIndex = WeightedRandom.GetWeightedRandomIndex(speedWeights);

            // set to moveSpeed and set velocity
            moveSpeed = speedWeights[speedIndex];
            rb.velocity = randomDirection * moveSpeed;
        }
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        SpaceRaceGameManager.Instance.UnregisterAsteroid(this);
    }

    private Vector3 GenerateRandomDirection()
    {
        float randomX = Random.Range(-1.0f, 1.0f);
        float randomY = Random.Range(-1.0f, 1.0f);
        float randomZ = Random.Range(-1.0f, 1.0f);

        Vector3 randomDirection = new Vector3(randomX, randomY, randomZ);

        return randomDirection.normalized;
    }

    private void BoundaryCheck()
    {
        if (transform.position.x < -SpaceRaceGameManager.Instance.AsteroidBoundaryX)
        {
            rb.AddForce(Vector3.right * moveSpeed);
        }
        else if (transform.position.x > SpaceRaceGameManager.Instance.AsteroidBoundaryX)
        {
            rb.AddForce(Vector3.left * moveSpeed);
        }
        else if (transform.position.y < -SpaceRaceGameManager.Instance.AsteroidBoundaryY)
        {
            rb.AddForce(Vector3.up * moveSpeed);
        }
        else if (transform.position.y > SpaceRaceGameManager.Instance.AsteroidBoundaryY)
        {
            rb.AddForce(Vector3.down * moveSpeed);
        }
    }
}
