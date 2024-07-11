using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const float captureRadius = 3.0f;
    private const float groundSequenceDelay = 1.5f;
    private Coroutine groundSequenceCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        if (BuildManager.Instance.BuildModeActive)
        {
            if (collision.gameObject.TryGetComponent(out BuildableObject buildable))
            {
                buildable.DeleteObject();
            }
            Deactivate();
        }
        else
        {
            if (collision.gameObject.TryGetComponent(out Item item))
            {
                item.PickupItem();
                Deactivate();
            }
            else if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.TryGetComponent(out BuildableObject _)) && groundSequenceCoroutine == null)
            {
                groundSequenceCoroutine = StartCoroutine(GroundSequence());
            }
        }
    }

    private IEnumerator GroundSequence()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(groundSequenceDelay);

        // check for nearby items
        Collider[] colliders = Physics.OverlapSphere(transform.position, captureRadius);

        // find any items in collider array
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Item item))
            {
                item.PickupItem();
                Deactivate();
                yield break;
            }
        }

        // deactivate if no items captured
        Deactivate();
    }

    private void Deactivate()
    {
        // Stop the ground sequence coroutine if it is running
        if (groundSequenceCoroutine != null)
        {
            StopCoroutine(groundSequenceCoroutine);
            groundSequenceCoroutine = null;
        }

        // Deactivate the projectile game object and return to pool
        gameObject.SetActive(false);
    }
}
