using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceCheckpoint : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color originalColor;

    private const float deactivationTime = 2.0f;

    public bool CheckpointSuccess { get; private set; }

    private void OnEnable()
    {
        CheckpointSuccess = false;
        SpaceRaceGameManager.Instance.RegisterCheckpoint(this);
    }

    private void OnDisable()
    {
        SpaceRaceGameManager.Instance.UnregisterCheckpoint(this);
    }

    public void ChangeColor(bool success)
    {
        foreach (Renderer renderer in renderers)
        {
            if (success)
            {
                renderer.material.color = new Color(0, 1, 0, originalColor.a); // green
            }
            else
            {
                renderer.material.color = new Color(1, 0, 0, originalColor.a); // red
            }
        }
    }

    public void HandleCollision(Collider other)
    {
        if (other.gameObject.CompareTag("RacePlayerShip"))
        {
            CheckpointCompleted();
        }
        else if (other.gameObject.CompareTag("Asteroid"))
        {
            if (other.gameObject.TryGetComponent(out SpaceRaceAsteroid asteroid))
            {
                // move away from checkpoint
                asteroid.ReverseZMovement();
            }
        }
    }

    private void CheckpointCompleted()
    {
        CheckpointSuccess = true;

        // change color to green to show success
        ChangeColor(true);

        // load future part of race (and despawn old asteroids)
        SpaceRaceGameManager.Instance.CheckpointPassed();

        // deactivate after timer
        StartCoroutine(DeactivationCoroutine());
    }

    private IEnumerator DeactivationCoroutine()
    {
        // wait until checkpoint leaves player viewpoint
        yield return new WaitForSeconds(deactivationTime);

        // reset color to original color
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = originalColor;
        }

        // deactivate gameobject
        gameObject.SetActive(false);
    }
}
