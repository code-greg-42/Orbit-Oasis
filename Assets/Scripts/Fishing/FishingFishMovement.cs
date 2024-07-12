using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FishingFishMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float intervalRangeMin = 2.0f;
    [SerializeField] private float intervalRangeMax = 4.0f;

    private const float xBoundary = 8.0f;
    private const float yBoundary = 4.0f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private bool outOfBounds = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(ChangeDirectionCoroutine());
    }

    private void FixedUpdate()
    {
        BoundaryCheck();
    }

    private IEnumerator ChangeDirectionCoroutine()
    {
        while (true)
        {
            float randomInterval = Random.Range(intervalRangeMin, intervalRangeMax);
            yield return new WaitForSeconds(randomInterval);
            if (!outOfBounds)
            {
                ChooseRandomDirection();
                rb.AddForce(moveDirection * moveSpeed, ForceMode2D.Impulse);
            }
        }
    }

    private void ChooseRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private void BoundaryCheck()
    {
        if (transform.position.x < -xBoundary)
        {
            outOfBounds = true;
            rb.AddForce(Vector2.right * moveSpeed);
        }
        else if (transform.position.x > xBoundary)
        {
            outOfBounds = true;
            rb.AddForce(Vector2.left * moveSpeed);
        }
        else if (transform.position.y < -yBoundary)
        {
            outOfBounds = true;
            rb.AddForce(Vector2.up * moveSpeed);
        }
        else if (transform.position.y > yBoundary)
        {
            outOfBounds = true;
            rb.AddForce(Vector2.down * moveSpeed);
        }
        else
        {
            outOfBounds = false;
        }
    }
}
