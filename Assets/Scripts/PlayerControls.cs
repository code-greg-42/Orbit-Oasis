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

    // item pickup variables
    private const float timeToMidItemPickup = 0.819878f / 1.2f;
    private const float itemPickupFinishTime = 1.414122f / 1.2f;
    private Coroutine itemPickupCoroutine;
    private bool isPickingUpItem;

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
    private const float farmableSearchDistance = 2.6f;
    private FarmableObject.ObjectType nearbyFarmableObjectType;
    private const float raycastSideOffset = 0.3f;

    // properties used by FarmingTool script
    public bool IsMidToolSwing => isMidToolSwing;
    public bool ToolHitRegistered => toolHitRegistered;
    public float TimeOfLastToolSwing => timeOfLastToolSwing;

    // property used by movement script
    public bool IsSwinging => isSwinging;
    public bool IsPickingUpItem => isPickingUpItem;
    public bool ReadyForAction => !isSwinging && !isPickingUpItem && playerMovement.IsGrounded && !BuildManager.Instance.BuildModeActive &&
        !DialogueManager.Instance.DialogueWindowActive && !ItemPlacementManager.Instance.ItemPlacementActive;

    private float shootingChargeTime;

    [Header("References")]
    [SerializeField] private FarmingTool playerAxe;
    [SerializeField] private FarmingTool playerMiningPick;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Layer Assignments")]
    [SerializeField] private LayerMask farmableObjectLayer;

    void Update()
    {
        if (!BuildManager.Instance.BuildModeActive && !DialogueManager.Instance.DialogueWindowActive
            && !ItemPlacementManager.Instance.ItemPlacementActive)
        {
            // FARMING
            if (toolSwingReady && playerMovement.IsGrounded && !isPickingUpItem)
            {
                // CHECK FOR NEARBY FARMABLE OBJECTS
                bool farmableObjectIsNearby = CheckForFarmableObject();

                if (farmableObjectIsNearby)
                {
                    if (!isSwinging)
                    {
                        MainUIManager.Instance.ActivateFarmingIndicator();
                    }

                    // USER INPUT FOR FARMING
                    if (Input.GetKey(toolKeybind))
                    {
                        // deactivate with success set to true for green tint
                        MainUIManager.Instance.DeactivateFarmingIndicator(true);

                        if (nearbyFarmableObjectType == FarmableObject.ObjectType.Tree)
                        {
                            SwingTool(true);
                        }
                        else if (nearbyFarmableObjectType == FarmableObject.ObjectType.Rock)
                        {
                            SwingTool(false);
                        }
                        else
                        {
                            Debug.LogWarning("Tool swing keybind pressed, but nearby farmable object is not correctly set.");
                        }
                    }
                }
                else
                {
                    MainUIManager.Instance.DeactivateFarmingIndicator();
                }
            }

            if (ReadyForAction)
            {
                // PICKUP ITEM PROCESSING
                (Collider[] _, int _, bool foundAction, bool _, int _) = ScanForActions();

                if (foundAction)
                {
                    MainUIManager.Instance.ActivateItemPickupIndicator();
                }
                else
                {
                    MainUIManager.Instance.DeactivateItemPickupIndicator(false);
                }
            }
            else
            {
                MainUIManager.Instance.DeactivateItemPickupIndicator(false);
            }

            // ITEM PICKUP/ACTION KEYBIND
            if (Input.GetKeyDown(pickupKeybind) && !IsPickingUpItem)
            {
                Collider[] results = new Collider[16];
                int size = Physics.OverlapSphereNonAlloc(transform.position, pickupRange, results);
                bool actionFound = false;

                // trader and spaceship should never be in range of each other
                for (int i = 0; i < size; i++)
                {
                    var collider = results[i];

                    // check for trader
                    if (collider.CompareTag("Trader"))
                    {
                        if (!InventoryManager.Instance.IsMenuActive)
                        {
                            TraderMenuManager.Instance.ToggleTraderMenu();
                            actionFound = true;
                        }
                    }
                    else if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out SpaceshipSelection selection))
                    {
                        selection.ActivateSpaceshipSelection();
                        actionFound = true;
                    }
                }

                // if neither was found, run the item pickup coroutine to pickup all items (up to 16 at once) in the pickup range
                if (!actionFound)
                {
                    if (itemPickupCoroutine != null)
                    {
                        StopCoroutine(itemPickupCoroutine);
                        itemPickupCoroutine = null;
                    }

                    itemPickupCoroutine = StartCoroutine(ItemPickupCoroutine(results, size));
                }

                //foreach (Collider collider in Physics.OverlapSphere(transform.position, pickupRange))
                //{
                //    if (collider.gameObject.TryGetComponent<Item>(out var item))
                //    {
                //        if (item is not Animal)
                //        {
                //            item.PickupItem();
                //        }
                //    }
                //    else if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out SpaceshipSelection selection))
                //    {
                //        selection.ActivateSpaceshipSelection();
                //    }
                //    else if (collider.CompareTag("Trader"))
                //    {
                //        if (!InventoryManager.Instance.IsMenuActive)
                //        {
                //            TraderMenuManager.Instance.ToggleTraderMenu();
                //        }
                //    }
                //}
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

    private (Collider[], int, bool, bool, int) ScanForActions()
    {
        // variables to return
        Collider[] results = new Collider[16];
        int size = Physics.OverlapSphereNonAlloc(transform.position, pickupRange, results);
        bool nonItemActionFound = false;
        bool actionFound = false;
        int nonItemActionIndex = 0;

        // trader and spaceship should never be in range of each other
        for (int i = 0; i < size; i++)
        {
            var collider = results[i];

            // check for trader
            if (collider.CompareTag("Trader"))
            {
                if (!InventoryManager.Instance.IsMenuActive)
                {
                    nonItemActionFound = true;
                    actionFound = true;
                    nonItemActionIndex = i;
                }
            }
            // check for spaceship
            else if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out SpaceshipSelection selection))
            {
                nonItemActionFound = true;
                actionFound = true;
                nonItemActionIndex = i;
            }
            // check for items
            else if (collider.gameObject.TryGetComponent(out Item item))
            {
                if (item.IsReadyForPickup)
                {
                    actionFound = true;
                }
            }
        }

        return (results, size, actionFound, nonItemActionFound, nonItemActionIndex);
    }

    private IEnumerator ItemPickupCoroutine(Collider[] results, int size)
    {
        // set bool for use by movement script
        isPickingUpItem = true;

        // play pickup animation
        playerAnimation.TriggerItemPickup();

        // wait for animation to reach the ground
        yield return new WaitForSeconds(timeToMidItemPickup);

        for (int i = 0; i < size; i++)
        {
            Collider collider = results[i];

            // get item component and pick it up if it's not an animal
            if (collider.gameObject.TryGetComponent(out Item item))
            {
                if (item is not Animal)
                {
                    item.PickupItem();
                }
            }
        }

        // wait for rest of animation to finish
        yield return new WaitForSeconds(itemPickupFinishTime);

        // reset bool to enable further item pickup and other actions
        isPickingUpItem = false;

        // ensure coroutine is set back to null
        itemPickupCoroutine = null;
    }

    private bool CheckForFarmableObject()
    {
        // set origins for 3 raycasts
        // ordered with priority to the most middle ray, then the right ray (because the tool swings come from the right side)
        Vector3[] rayOrigins =
        {
            playerObject.position,
            playerObject.position + playerObject.right * raycastSideOffset,
            playerObject.position - playerObject.right * raycastSideOffset,
        };

        // loop through each origin and perform raycast
        foreach (Vector3 rayOrigin in rayOrigins)
        {
            Debug.DrawRay(rayOrigin, playerObject.forward * farmableSearchDistance, Color.red, 1.0f);

            // cast a ray forwards from the specified position
            if (Physics.Raycast(rayOrigin, playerObject.forward, out RaycastHit hit, farmableSearchDistance, farmableObjectLayer))
            {
                if (hit.collider.TryGetComponent(out FarmableObject farmable))
                {
                    nearbyFarmableObjectType = farmable.Type;
                    return true;
                }
            }
        }

        return false;
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
