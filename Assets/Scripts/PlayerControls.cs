using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode axeKeybind = KeyCode.E;
    public KeyCode pickupKeybind = KeyCode.F;
    public KeyCode inventoryKeybind = KeyCode.B;
    public KeyCode shootingKeybind = KeyCode.C;

    private readonly float pickupRange = 1.5f;
    private readonly float shotAngle = 30.0f;
    private readonly float projectileSpeed = 20.0f;

    [Header("References")]
    [SerializeField] private PlayerAxe axe;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform playerObject;
    [SerializeField] private Camera mainCamera;

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
        if (Input.GetKeyDown(inventoryKeybind) && !InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.ToggleInventoryMenu();
        }

        // SHOOTING
        if (Input.GetKeyDown(shootingKeybind))
        {
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        GameObject projectile = ProjectilePool.Instance.GetPooledObject();

        // calc direction
        Vector3 direction = new Vector3(playerObject.forward.x, 0, playerObject.forward.z).normalized;
        Debug.Log(direction);

        // adjust to add a slight lob
        //direction = Quaternion.Euler(-shotAngle, 0, 0) * direction;
        //Debug.Log(direction);

        // ignore collision with player object
        if (playerObject.TryGetComponent(out Collider playerCollider) && projectile.TryGetComponent(out Collider projectileCollider))
        {
            Physics.IgnoreCollision(playerCollider, projectileCollider);
        }

        // reposition projectile
        projectile.transform.position = playerObject.position;
        // activate projectile
        projectile.SetActive(true);

        // apply force to projectile
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);
        }
    }
}
