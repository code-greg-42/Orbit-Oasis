using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode axeKeybind = KeyCode.E;
    public KeyCode pickupKeybind = KeyCode.F;
    public KeyCode inventoryKeybind = KeyCode.Tab;
    public KeyCode shootingKeybind = KeyCode.C;
    public KeyCode buildModeKeybind = KeyCode.B;

    private readonly float pickupRange = 1.5f;
    private readonly float projectileLobHeight = 0.35f;
    private readonly float baseProjectileForce = 10.0f;
    private readonly float maxChargeTime = 2.0f;
    private readonly float maxAdditionalForce = 20.0f;

    private float shootingChargeTime;

    [Header("References")]
    [SerializeField] private PlayerAxe axe;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform playerObject;

    void Update()
    {
        if (!BuildManager.Instance.BuildModeActive && !DialogueManager.Instance.DialogueWindowActive)
        {
            // FARMING
            if (Input.GetKeyDown(axeKeybind))
            {
                axe.SwingAxe();
            }

            // ITEM PICKUP
            if (Input.GetKeyDown(pickupKeybind))
            {
                foreach (Collider collider in Physics.OverlapSphere(transform.position, pickupRange))
                {
                    if (collider.gameObject.TryGetComponent<Item>(out var item))
                    {
                        if (item is FarmableObject)
                        {
                            Debug.Log("Shake tree!");
                            // shake farmable object
                        }
                        else if (item is Animal)
                        {
                            Debug.Log("Pet animal! Hello fren");
                            // pet animal
                        }
                        else
                        {
                            item.PickupItem();
                        }
                    }
                    else if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out SpaceshipSelection selection))
                    {
                        selection.ActivateSpaceshipSelection();
                    }
                }
            }

            // SHOOTING
            if (Input.GetKeyDown(shootingKeybind))
            {
                shootingChargeTime = 0.0f;
            }

            if (Input.GetKey(shootingKeybind))
            {
                shootingChargeTime += Time.deltaTime;
                shootingChargeTime = Mathf.Min(shootingChargeTime, maxChargeTime); // cap at max charge time
            }

            if (Input.GetKeyUp(shootingKeybind))
            {
                // calc additional force amount
                float additionalForce = shootingChargeTime / maxChargeTime * maxAdditionalForce;
                // shoot with additional force and slight lob added
                ShootProjectile(additionalForce, true);
            }
        }

        // INVENTORY
        if (Input.GetKeyDown(inventoryKeybind) && !InventoryManager.Instance.IsDragging)
        {
            InventoryManager.Instance.ToggleInventoryMenu();
        }

        // BUILD MODE
        if (Input.GetKeyDown(buildModeKeybind))
        {
            BuildManager.Instance.ToggleBuildMode();
        }
    }

    private void ShootProjectile(float additionalForce, bool addLob)
    {
        // get projectile from pool
        GameObject projectile = ProjectilePool.Instance.GetPooledObject();

        // calc direction
        Vector3 direction = playerObject.forward;

        if (addLob)
        {
            // adjust y value for a slight lob
            direction.y = projectileLobHeight;
        }

        // ignore collision with player object
        if (playerObject.TryGetComponent(out Collider playerCollider) && projectile.TryGetComponent(out Collider projectileCollider))
        {
            Physics.IgnoreCollision(playerCollider, projectileCollider);
        }

        // reposition projectile to player object
        projectile.transform.position = playerObject.position;

        // apply force to projectile
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // activate projectile
            projectile.SetActive(true);

            rb.AddForce(direction * (baseProjectileForce + additionalForce), ForceMode.Impulse);
            Debug.Log("Projectile shot with force: " + (baseProjectileForce + additionalForce));
        }
    }
}
