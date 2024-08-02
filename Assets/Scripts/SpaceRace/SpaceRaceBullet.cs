using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceBullet : MonoBehaviour
{
    private const float launchSpeed = 200.0f;
    private const float lifetime = 3.0f;
    private Rigidbody rb;
    private Coroutine deactivationCoroutine;

    private const float asteroidStartingScale = 800.0f;
    private const float desiredExplosionScale = 5.0f;

    [SerializeField] private GameObject explosionPrefab; // used for when a bullet hits an asteroid

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (SpaceRaceGameManager.Instance != null)
        {
            // calc bullet speed from the current forward movement of the player/spaceship
            float bulletSpeed = launchSpeed + SpaceRaceGameManager.Instance.GetCurrentPlayerSpeed();
            rb.velocity = Vector3.forward * bulletSpeed;

            // start deactivation coroutine
            if (deactivationCoroutine == null)
            {
                deactivationCoroutine = StartCoroutine(DeactivateCoroutine());
            }
        }
    }

    private void OnDisable()
    {
        // Ensure the coroutine is stopped when the object is disabled
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }
    }

    private IEnumerator DeactivateCoroutine()
    {
        // wait for lifetime amount
        yield return new WaitForSeconds(lifetime);

        // set inactive and return to pool
        gameObject.SetActive(false);
    }

    public void HandleCollision(Collider collider)
    {
        if (collider.gameObject.CompareTag("Asteroid"))
        {
            // instantiate effect
            GameObject explosionEffect = Instantiate(explosionPrefab, collider.transform.position, Quaternion.identity);

            // immediately set scale of effect ( adjusted for starting scales )
            explosionEffect.transform.localScale = collider.transform.localScale / asteroidStartingScale * desiredExplosionScale;

            // deactivate asteroid
            collider.gameObject.SetActive(false);

            // stop coroutine before deactivating
            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
                deactivationCoroutine = null;
            }

            // set to inactive (return to pool)
            gameObject.SetActive(false);
        }
    }
}
