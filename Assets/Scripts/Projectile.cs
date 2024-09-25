using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const float captureRadius = 3.0f;
    private const float groundSequenceDelay = 1.5f;
    private Coroutine groundSequenceCoroutine;

    [SerializeField] private GameObject detonationEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Item item))
        {
            // add item to player inventory
            item.PickupItem();

            // update navmesh with the lack of the object if item was a placeable item such as a tree
            if (item is PlaceableItem placeableItem)
            {
                // update data manager with lack of placed item
                DataManager.Instance.RemovePlacedItem(placeableItem);

                // update navmesh with lack of placed item
                NavMeshManager.Instance.UpdateNavMesh();
            }

            // deactivate projectile and return to pool
            Deactivate();
        }
        else if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.TryGetComponent(out BuildableObject _)) && groundSequenceCoroutine == null)
        {
            if (gameObject.activeInHierarchy)
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

        // instantiate detonation effect on position
        GameObject detonationEffectInstance = Instantiate(detonationEffect);
        detonationEffectInstance.transform.position = transform.position;

        // set movement back to zero
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Deactivate the projectile game object and return to pool
        gameObject.SetActive(false);
    }
}
