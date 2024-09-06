using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAxe : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControls playerControls;

    private void OnTriggerEnter(Collider other)
    {
        bool validHit = !playerControls.AxeHitRegistered && playerControls.IsMidAxeSwing && other.CompareTag("FarmableObject");

        if (validHit)
        {
            // register the hit with the player control script to disallow multiple farms from one swing
            playerControls.RegisterAxeHit();

            if (other.TryGetComponent(out FarmableObject farmable))
            {
                farmable.FarmObject();
            }
        }
    }
}
