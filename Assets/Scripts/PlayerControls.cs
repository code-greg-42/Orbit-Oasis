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
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000); // arbitrary distance
        }

        Vector3 direction = (targetPoint - playerObject.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, playerObject.position, Quaternion.identity);

        // calc direction
        //Vector3 direction = playerObject.forward;
        // adjust to add a slight lob
        direction = Quaternion.Euler(shotAngle, 0, 0) * direction;

        // ignore collision with player object
        if (playerObject.TryGetComponent(out Collider playerCollider) && projectile.TryGetComponent(out Collider projectileCollider))
        {
            Debug.Log("Ignoring collision!");
            Physics.IgnoreCollision(playerCollider, projectileCollider);
        }

        // apply force to projectile
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(direction * projectileSpeed, ForceMode.Impulse);
        }
    }
}
