using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAxe : MonoBehaviour
{
    //[Header("References")]
    //[SerializeField] private PlayerControls playerControls;

    //private const float checkDespawnFrequency = 0.1f;
    //private const float despawnTimer = 3.5f;

    //private Coroutine checkForDespawnCoroutine;

    //private void OnEnable()
    //{
    //    StopDespawn();

    //    checkForDespawnCoroutine = StartCoroutine(CheckForDespawnCoroutine());
    //}

    //private void OnDisable()
    //{
    //    StopDespawn();
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    bool validHit = !playerControls.AxeHitRegistered && playerControls.IsMidAxeSwing && other.CompareTag("FarmableObject");

    //    if (validHit)
    //    {
    //        // register the hit with the player control script to disallow multiple farms from one swing
    //        playerControls.RegisterAxeHit();

    //        if (other.TryGetComponent(out FarmableObject farmable))
    //        {
    //            farmable.FarmObject();
    //        }
    //    }
    //}

    //private IEnumerator CheckForDespawnCoroutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(checkDespawnFrequency);

    //        // get timestamp of most recent axe swing from player controls script
    //        float timeOfLastToolSwing = playerControls.TimeOfLastToolSwing;

    //        // if a time of last axe swing has been set and the elapsed time since then is longer than the despawn timer
    //        bool despawnNeeded = timeOfLastToolSwing != 0 && Time.time - timeOfLastToolSwing > despawnTimer;

    //        if (despawnNeeded)
    //        {
    //            gameObject.SetActive(false);
    //        }
    //    }
    //}

    //private void StopDespawn()
    //{
    //    if (checkForDespawnCoroutine != null)
    //    {
    //        StopCoroutine(checkForDespawnCoroutine);
    //        checkForDespawnCoroutine = null;
    //    }
    //}
}
