using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceCheckpoint : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color originalColor;

    private float deactivationTime = 2.0f;

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
            ChangeColor(true);
            StartCoroutine(DeactivationCoroutine());
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
}
