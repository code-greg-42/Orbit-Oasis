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

        // if this is final checkpoint, change color
        if (SpaceRaceGameManager.Instance.CheckpointsLoaded + 1 == SpaceRaceGameManager.Instance.FinalCheckpoint)
        {
            ChangeToFinishLineColor();
        }

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
                asteroid.PushRandomDirection();
            }
        }
    }

    private void CheckpointCompleted()
    {
        CheckpointSuccess = true;

        // load future part of race (and despawn old asteroids)
        SpaceRaceGameManager.Instance.CheckpointPassed();

        if (SpaceRaceGameManager.Instance.ActiveCheckpoints.Count > 1)
        {
            // change color to green to show success
            ChangeColor(true);

            // deactivate after timer
            StartCoroutine(DeactivationCoroutine());
        }
        else
        {
            // immediately deactivate last checkpoint to make visual space for victory text
            gameObject.SetActive(false);
        }
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

    private void ChangeToFinishLineColor()
    {
        Color finishLineColor = Color.white;
        finishLineColor.a = 0.65f;

        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = finishLineColor;
        }
    }
}
