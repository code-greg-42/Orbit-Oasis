using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode toolKeybind = KeyCode.E;
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
    private const float timeToMidAxeSwing = 0.612f;

    // mining pick variables
    private const float miningFinishTime = 0.955225f;
    private const float timeToMidMiningSwing = 0.628525f;

    // general tool variables
    private bool toolSwingReady = true;
    private bool isMidToolSwing = false;
    private bool toolHitRegistered = false;
    private float timeOfLastToolSwing;
    private bool isSwinging = false;

    // farming proximity check variables
    private float farmableSearchRadius = 2.5f;

    // properties used by FarmingTool script
    public bool IsMidToolSwing => isMidToolSwing;
    public bool ToolHitRegistered => toolHitRegistered;
    public float TimeOfLastToolSwing => timeOfLastToolSwing;

    // property used by movement script
    public bool IsSwinging => isSwinging;

    private float shootingChargeTime;

    [Header("References")]
    [SerializeField] private FarmingTool playerAxe;
    [SerializeField] private FarmingTool playerMiningPick;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMovement playerMovement;

    void Update()
    {
        if (!BuildManager.Instance.BuildModeActive && !DialogueManager.Instance.DialogueWindowActive
            && !ItemPlacementManager.Instance.ItemPlacementActive)
        {
            // FARMING
            if (Input.GetKey(toolKeybind) && toolSwingReady && playerMovement.IsGrounded)
            {
                SwingTool(false);
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

    private void CheckForNearbyFarmableObject()
    {
        Collider[] results = new Collider[8];
        int size = Physics.OverlapSphereNonAlloc(transform.position, farmableSearchRadius, results, LayerMask.GetMask("FarmableObject"));
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
        }
    }

    private void SwingTool(bool isAxe)
    {
        toolSwingReady = false;
        isSwinging = true;

        // set time of initial swing for use by playerAxe despawn coroutine
        timeOfLastToolSwing = Time.time;

        if (isAxe)
        {
            // activate axe gameobject
            playerAxe.gameObject.SetActive(true);

            // start axe swing animation
            playerAnimation.TriggerAxeSwing();
        }
        else
        {
            // activate mining pick gameobject
            playerMiningPick.gameObject.SetActive(true);

            // start mining animation
            playerAnimation.StartMiningLoop();
        }

        StartCoroutine(ResetToolSwing(isAxe));
    }

    public void GetClosestFarmableObject()
    {

    }

    public void RegisterToolHit()
    {
        toolHitRegistered = true;
    }

    private IEnumerator ResetToolSwing(bool isAxe)
    {
        float timeToMid = isAxe ? timeToMidAxeSwing : timeToMidMiningSwing;
        float timeToFinish = isAxe ? axeSwingFinishTime : miningFinishTime;

        // wait for animation to reach 'hit capable' part of swing
        yield return new WaitForSeconds(timeToMid);
        isMidToolSwing = true;

        // wait for rest of swing animation to finish and reset bools
        yield return new WaitForSeconds(timeToFinish);
        isMidToolSwing = false;
        toolHitRegistered = false;

        // reset swing ready bools only if it's an axe swing, or if the user is no longer pressing the 'farming/use tool' button
        if (isAxe || !Input.GetKey(toolKeybind))
        {
            if (!isAxe)
            {
                playerAnimation.StopMiningLoop();
            }

            isSwinging = false;
            toolSwingReady = true;
        }
        else
        {
            // set time of last swing to now to keep mining pick from despawning early
            timeOfLastToolSwing = Time.time;

            // start the reset process again --- makes it so it only resets if the player is not actively holding the 'farming/use tool' button
            StartCoroutine(ResetToolSwing(false));
        }
    }
}
