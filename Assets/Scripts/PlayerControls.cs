using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public KeyCode axeKeybind = KeyCode.E;
    public KeyCode pickupKeybind = KeyCode.F;

    // temporary solution
    public float axeRange = 3.0f;

    // Update is called once per frame
    void Update()
    {
        // change this later to incorporate the axe hitting the tree etc
        if (Input.GetKeyDown(axeKeybind))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, axeRange);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<FarmableObject>(out var farmableObject))
                {
                    farmableObject.FarmObject();
                    return;
                }
            }
        }
    }
}
