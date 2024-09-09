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
    public KeyCode escapeKeybind = KeyCode.Escape;

    private readonly float pickupRange = 1.5f;
    private readonly float projectileLobHeight = 0.35f;
    private readonly float baseProjectileForce = 10.0f;
    private readonly float maxChargeTime = 2.0f;
    private readonly float maxAdditionalForce = 20.0f;

    // axe swing variables
    private const float axeSwingFinishTime = 1.008f;
    private const float timeToMidSwing = 0.612f;
    private bool axeSwingReady = true;
    private bool isMidAxeSwing = false;
    private bool axeHitRegistered = false;
    private float timeOfLastAxeSwing;

    // mining pick variables

    // properties used by PlayerAxe script
    public bool IsMidAxeSwing => isMidAxeSwing;
    public bool AxeHitRegistered => axeHitRegistered;
    public float TimeOfLastAxeSwing => timeOfLastAxeSwing;

    private float shootingChargeTime;

    [Header("References")]
    [SerializeField] private PlayerAxe axe;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;

    void Update()
    {
        if (!BuildManager.Instance.BuildModeActive && !DialogueManager.Instance.DialogueWindowActive
            && !ItemPlacementManager.Instance.ItemPlacementActive)
        {
            // FARMING
            if (Input.GetKey(axeKeybind) && axeSwingReady)
            {
                SwingAxe();
            }

            // ITEM PICKUP
            if (Input.GetKeyDown(pickupKeybind))
            {
                foreach (Collider collider in Physics.OverlapSphere(transform.position, pickupRange))
                {
                    if (collider.gameObject.TryGetComponent<Item>(out var item))
                    {
                        if (item is Animal)
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
                    else if (collider.CompareTag("Trader"))
                    {
                        if (!InventoryManager.Instance.IsMenuActive)
                        {
                            TraderMenuManager.Instance.ToggleTraderMenu();
                        }
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

        // ESCAPE MENUS
        if (Input.GetKeyDown(escapeKeybind))
        {
            if (InventoryManager.Instance.IsMenuActive)
            {
                if (!InventoryManager.Instance.IsDragging)
                {
                    InventoryManager.Instance.ToggleInventoryMenu();
                }
            }
            else if (TraderMenuManager.Instance.IsMenuActive)
            {
                if (!TraderMenuManager.Instance.IsDragging)
                {
                    TraderMenuManager.Instance.ToggleTraderMenu();
                }
            }
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

    private void SwingAxe()
    {
        axeSwingReady = false;

        // activate axe gameobject
        axe.gameObject.SetActive(true);

        // set time of initial swing for use by playerAxe despawn coroutine
        timeOfLastAxeSwing = Time.time;

        // start animation
        playerAnimation.TriggerAxeSwing();

        StartCoroutine(ResetAxeSwing());
    }

    public void RegisterAxeHit()
    {
        axeHitRegistered = true;
    }

    private IEnumerator ResetAxeSwing()
    {
        // wait for animation to reach 'hit capable' part of swing
        yield return new WaitForSeconds(timeToMidSwing);
        isMidAxeSwing = true;

        // wait for rest of swing animation to finish
        yield return new WaitForSeconds(axeSwingFinishTime);
        isMidAxeSwing = false;
        axeHitRegistered = false;
        axeSwingReady = true;
    }
}
