using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceAsteroid : MonoBehaviour
{
    private float moveSpeed;

    private const float boundarySpeedMinimum = 20.0f; // used for moving an asteroid with a movespeed of 0
    private const float boundarySpeedModifier = 2.0f;

    private float movePercentage = 0.6f; // percentage of asteroids that move

    private int[] speeds = { 20, 40, 60, 100 }; // list of different asteroid speeds
    private int[] speedWeights = { 35, 35, 25, 5 }; // percentages each speed will be picked

    private float[] sizes = { 0.9f, 1f, 1.05f, 1.1f, 1.15f, 1.2f, 1.3f, 1.45f, 1.6f, 2.2f }; // list of different asteroid sizes
    private int[] sizeWeights = { 5, 5, 15, 25, 20, 10, 10, 5, 3, 2 }; // percentages each size will be picked

    private Rigidbody rb;

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

        SetSizeAndMovement();
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        SpaceRaceGameManager.Instance.UnregisterAsteroid(this);
    }

    private void SetSizeAndMovement()
    {
        // weighted roll for size
        int weightIndex = WeightedRandom.GetWeightedRandomIndex(sizeWeights);
        float sizeAdjustment = sizes[weightIndex];

        // set scale
        transform.localScale *= sizeAdjustment;
        rb.mass *= sizeAdjustment;

        // roll for whether or not asteroid should move
        float shouldMove = Random.Range(0f, 1f);

        if (shouldMove <= movePercentage)
        {
            // get random direction
            Vector3 randomDirection = GenerateRandomDirection();

            // get random speed index based on weights
            int speedIndex = WeightedRandom.GetWeightedRandomIndex(speedWeights);

            // set to moveSpeed and adjust for difficulty from game manager
            moveSpeed = speedWeights[speedIndex];
            moveSpeed *= SpaceRaceGameManager.Instance.AsteroidMovementModifier;
            
            // set velocity
            rb.velocity = randomDirection * moveSpeed;
        }
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
        // set to a minimum amount for asteroids with a move speed of 0 that were bumped by other asteroids
        float boundarySpeedBase = Mathf.Max(boundarySpeedMinimum, moveSpeed);
        float boundarySpeed = boundarySpeedBase * boundarySpeedModifier;

        if (transform.position.x < -SpaceRaceGameManager.Instance.AsteroidBoundaryX)
        {
            rb.AddForce(Vector3.right * boundarySpeed);
        }
        else if (transform.position.x > SpaceRaceGameManager.Instance.AsteroidBoundaryX)
        {
            rb.AddForce(Vector3.left * boundarySpeed);
        }
        else if (transform.position.y < -SpaceRaceGameManager.Instance.AsteroidBoundaryY)
        {
            rb.AddForce(Vector3.up * boundarySpeed);
        }
        else if (transform.position.y > SpaceRaceGameManager.Instance.AsteroidBoundaryY)
        {
            rb.AddForce(Vector3.down * boundarySpeed);
        }
        else if (transform.position.z > SpaceRaceGameManager.Instance.FinalAsteroidBoundary)
        {
            rb.AddForce(Vector3.back * (boundarySpeed / 2));
        }
    }

    public void PushRandomDirection()
    {
        Vector3 randomDirection = GenerateRandomDirection();

        float boundarySpeedBase = Mathf.Max(boundarySpeedMinimum, moveSpeed);
        float boundarySpeed = boundarySpeedBase * boundarySpeedModifier;

        rb.AddForce(randomDirection * boundarySpeed);
    }
}
