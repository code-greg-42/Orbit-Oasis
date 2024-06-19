using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public KeyCode axeKeybind = KeyCode.E;
    public KeyCode pickupKeybind = KeyCode.F;
    public KeyCode inventoryKeybind = KeyCode.B;

    // temporary solution
    public float axeRange = 3.0f;
    public float pickupRange = 0.5f;

    [Header("References")]
    [SerializeField] private PlayerAxe axe;

    // Update is called once per frame
    void Update()
    {
        // change this later to incorporate the axe hitting the tree etc
        if (Input.GetKeyDown(axeKeybind))
        {
            axe.SwingAxe();
            //foreach (Collider collider in Physics.OverlapSphere(transform.position, axeRange))
            //{
            //    if (collider.gameObject.TryGetComponent<FarmableObject>(out var farmableObject))
            //    {
            //        farmableObject.FarmObject();
            //        return;
            //    }
            //}
        }

        // CHANGE LATER TO INCLUDE OVERLAPSPHERENONALLOC WITH A PICKUP OBJECTS LAYER

        // change this later to include inventory
        if (Input.GetKeyDown(pickupKeybind))
        {
            foreach (Collider collider in Physics.OverlapSphere(transform.position, pickupRange))
            {
                if (collider.gameObject.TryGetComponent<FarmableMaterial>(out var farmableMaterial))
                {
                    farmableMaterial.PickupMaterial();
                }
            }
        }

        // INVENTORY
        if (Input.GetKeyDown(inventoryKeybind))
        {
            InventoryManager.Instance.ToggleInventoryMenu();
        }
    }
}
