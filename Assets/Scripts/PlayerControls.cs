using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode axeKeybind = KeyCode.E;
    public KeyCode pickupKeybind = KeyCode.F;
    public KeyCode inventoryKeybind = KeyCode.B;

    private readonly float pickupRange = 1.5f;

    [Header("References")]
    [SerializeField] private PlayerAxe axe;

    void Update()
    {
        // FARMING
        if (Input.GetKeyDown(axeKeybind))
        {
            axe.SwingAxe();
        }

        // CHANGE LATER TO INCLUDE OVERLAPSPHERENONALLOC WITH AN ITEMS LAYER
        if (Input.GetKeyDown(pickupKeybind))
        {
            foreach (Collider collider in Physics.OverlapSphere(transform.position, pickupRange))
            {
                if (collider.gameObject.TryGetComponent<Item>(out var item))
                {
                    item.PickupItem();
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
