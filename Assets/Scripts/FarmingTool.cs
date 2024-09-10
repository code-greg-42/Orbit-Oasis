using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingTool : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControls playerControls;

    private const float checkDespawnFrequency = 0.1f;
    private const float despawnTimer = 2.5f;

    private Coroutine checkForDespawnCoroutine;

    private void OnEnable()
    {
        StopDespawn();

        checkForDespawnCoroutine = StartCoroutine(CheckForDespawnCoroutine());
    }

    private void OnDisable()
    {
        StopDespawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool validHit = !playerControls.ToolHitRegistered && playerControls.IsMidToolSwing && other.CompareTag("FarmableObject");
        if (validHit)
        {
            // register the hit with the player control script to disallow multiple farms from one swing
            playerControls.RegisterToolHit();

            if (other.TryGetComponent(out FarmableObject farmable))
            {
                farmable.FarmObject();
            }
        }
    }

    private IEnumerator CheckForDespawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkDespawnFrequency);

            // get timestamp of most recent axe swing from player controls script
            float timeOfLastAxeSwing = playerControls.TimeOfLastToolSwing;

            // if a time of last axe swing has been set and the elapsed time since then is longer than the despawn timer
            bool despawnNeeded = timeOfLastAxeSwing != 0 && Time.time - timeOfLastAxeSwing > despawnTimer;

            if (despawnNeeded)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void StopDespawn()
    {
        if (checkForDespawnCoroutine != null)
        {
            StopCoroutine(checkForDespawnCoroutine);
            checkForDespawnCoroutine = null;
        }
    }
}
