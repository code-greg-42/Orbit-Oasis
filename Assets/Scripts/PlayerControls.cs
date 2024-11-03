using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public KeyCode traderMenuKeybind = KeyCode.T;

    // bow shot variables
    private readonly float bowShotAnimationTime = 0.967f;
    private readonly float timeToBowShotRelease = 0.413876f;
    private Coroutine bowShotCoroutine;
    private bool isShooting = false;

    // item pickup variables
    private readonly float pickupRange = 1.5f;
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

    // unstuck variables
    private const float unstuckHoldLength = 3.0f;
    private float unstuckTimer = 0.0f;

    // farming proximity check variables
    private const float farmableSearchDistance = 2.6f;
    private FarmableObject.ObjectType nearbyFarmableObjectType;
    private bool farmableObjectNearby = false;
    private bool lastFarmableCheckWasFalse = false;
    private const float raycastSideOffset = 0.25f;

    private const float checkFrequency = 0.1f; // to prevent repetitive uneccessary proximity checks
    private float checkFrequencyTimer = 0.0f; // timer used for both farming and item pickup proximity checks
    //private Coroutine checkFarmableObjectCoroutine;

    // item proximity check variables
    private Collider[] cachedItemResults;
    private int cachedItemResultSize;
    private bool cachedFoundAction;
    private bool cachedFoundNonItemAction;
    private int cachedNonItemActionIndex;

    // properties used by FarmingTool script
    public bool IsMidToolSwing => isMidToolSwing;
    public bool ToolHitRegistered => toolHitRegistered;
    public float TimeOfLastToolSwing => timeOfLastToolSwing;

    // property used by movement script
    public bool IsSwinging => isSwinging;
    public bool IsPickingUpItem => isPickingUpItem;

    // main action property
    //public bool ReadyForAction => !isSwinging && !isShooting && !isPickingUpItem && playerMovement.IsGrounded && !BuildManager.Instance.BuildModeActive &&
    //    !DialogueManager.Instance.DialogueWindowActive && !ItemPlacementManager.Instance.ItemPlacementActive && !InventoryManager.Instance.IsMenuActive &&
    //    !TraderMenuManager.Instance.IsMenuActive && !SpaceshipSelection.Instance.IsMenuActive;

    [Header("References")]
    [SerializeField] private FarmingTool playerAxe;
    [SerializeField] private FarmingTool playerMiningPick;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject playerBow;

    [Header("Layer Assignments")]
    [SerializeField] private LayerMask farmableObjectLayer;

    // control bools
    private bool IsLoaded => !MainGameManager.Instance.IsSwappingScenes && !MainGameManager.Instance.IsLoadingIn;
    private bool NoMenusActive => !BuildManager.Instance.BuildModeActive && !InventoryManager.Instance.IsMenuActive &&
        !TraderMenuManager.Instance.IsMenuActive && !SpaceshipSelection.Instance.IsMenuActive; // discludes ItemPlacement as that's handled separately
    private bool ReadyForAction => playerMovement.IsGrounded && !isPickingUpItem && !isShooting;

    void Update()
    {
        if (IsLoaded)
        {
            if (!DialogueManager.Instance.DialogueWindowActive && !ItemPlacementManager.Instance.ItemPlacementActive)
            {
                ActionButtons();
                UserMenuToggles();
            }
            else
            {
                ResetAllIndicators();
            }
            UserMenuEscape();
        }
    }

    public void EscapeMenusAndBuildMode()
    {
        if (InventoryManager.Instance.IsMenuActive)
        {
            if (!InventoryManager.Instance.IsDragging)
            {
                InventoryManager.Instance.ToggleInventoryMenu();
            }
        }

        if (TraderMenuManager.Instance.IsMenuActive)
        {
            if (!TraderMenuManager.Instance.IsDragging)
            {
                TraderMenuManager.Instance.ToggleTraderMenu();
            }
        }
        
        if (BuildManager.Instance.BuildModeActive)
        {
            BuildManager.Instance.ToggleBuildMode();
        }
    }

    private void ActionButtons()
    {
        if (NoMenusActive)
        {
            checkFrequencyTimer += Time.deltaTime;

            if (ReadyForAction)
            {
                Farming(checkFrequencyTimer);

                // disclude !isSwinging from Farming to allow for continuous farming
                if (!isSwinging)
                {
                    PickupItemsButton(checkFrequencyTimer);

                    // bow shot --- only while playing isn't moving
                    if (Input.GetKeyDown(shootingKeybind) && !playerMovement.IsMoving)
                    {
                        ShootBow();
                        ResetAllIndicators();
                    }
                }
            }

            if (checkFrequencyTimer >= checkFrequency)
            {
                checkFrequencyTimer = 0.0f;
            }
        }
        else
        {
            ResetAllIndicators();
        }
    }

    private void ResetAllIndicators()
    {
        farmableObjectNearby = false;
        ResetCachedItemVariables();
        MainUIManager.Instance.DeactivateFarmingIndicator();
        MainUIManager.Instance.DeactivateItemPickupIndicator();
    }

    private void UserMenuToggles()
    {
        // INVENTORY
        if (Input.GetKeyDown(inventoryKeybind) && QuestManager.Instance.InventoryQuestReached && !InventoryManager.Instance.IsDragging &&
            !TraderMenuManager.Instance.IsDragging)
        {
            // deactivate other menus if active
            if (BuildManager.Instance.BuildModeActive)
            {
                BuildManager.Instance.ToggleBuildMode();
            }
            if (TraderMenuManager.Instance.IsMenuActive)
            {
                TraderMenuManager.Instance.ToggleTraderMenu();
            }

            InventoryManager.Instance.ToggleInventoryMenu();
        }

        // TRADER MENU
        else if (Input.GetKeyDown(traderMenuKeybind) && !TraderMenuManager.Instance.IsDragging && !InventoryManager.Instance.IsDragging &&
            QuestManager.Instance.GetCurrentQuest() == null)
        {
            // deactivate other menus if active
            if (BuildManager.Instance.BuildModeActive)
            {
                BuildManager.Instance.ToggleBuildMode();
            }
            if (InventoryManager.Instance.IsMenuActive)
            {
                InventoryManager.Instance.ToggleInventoryMenu();
            }

            TraderMenuManager.Instance.ToggleTraderMenu();
        }

        // BUILD MODE -- only allow if no menus active
        else if (Input.GetKeyDown(buildModeKeybind) && !InventoryManager.Instance.IsMenuActive && !TraderMenuManager.Instance.IsMenuActive &&
            QuestManager.Instance.BuildingQuestReached)
        {
            BuildManager.Instance.ToggleBuildMode();
        }
    }

    private void UserMenuEscape()
    {
        // ALTERNATE ESCAPE OF MENUS AND MODES
        if (Input.GetKeyDown(escapeKeybind))
        {
            EscapeMenusAndBuildMode();
        }

        // UNSTUCK PLAYER
        if (Input.GetKey(escapeKeybind))
        {
            unstuckTimer += Time.deltaTime;

            if (unstuckTimer >= unstuckHoldLength)
            {
                unstuckTimer = 0.0f;
                MainGameManager.Instance.UnstuckPlayer();
            }
        }

        if (Input.GetKeyUp(escapeKeybind))
        {
            unstuckTimer = 0.0f;
        }
    }

    private void Farming(float checkTimer)
    {
        // FARMING
        if (QuestManager.Instance.FarmingQuestReached && toolSwingReady)
        {
            if (checkTimer >= checkFrequency)
            {
                // CHECK FOR NEARBY FARMABLE OBJECTS
                bool objectNearby = CheckForFarmableObject();

                if (objectNearby)
                {
                    farmableObjectNearby = true;
                    lastFarmableCheckWasFalse = false;
                }
                else
                {
                    if (lastFarmableCheckWasFalse)
                    {
                        farmableObjectNearby = false;
                    }
                    else
                    {
                        lastFarmableCheckWasFalse = true;
                    }
                }
            }

            if (farmableObjectNearby)
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

                    // deactivate item pickup indicator (normal instead of success) and reset variables
                    MainUIManager.Instance.DeactivateItemPickupIndicator();
                    ResetCachedItemVariables();

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
    }

    private void PickupItemsButton(float checkTimer)
    {
        if (QuestManager.Instance.FarmingQuestReached)
        {
            if (checkTimer >= checkFrequency)
            {
                // PICKUP ITEM PROCESSING
                //(Collider[] results, int size, bool foundAction, bool foundNonItemAction, int nonItemActionIndex) = ScanForActions();
                (cachedItemResults, cachedItemResultSize, cachedFoundAction, cachedFoundNonItemAction, cachedNonItemActionIndex) = ScanForActions();
            }

            if (cachedFoundAction)
            {
                MainUIManager.Instance.ActivateItemPickupIndicator();
                if (Input.GetKeyDown(pickupKeybind))
                {
                    // deactivate with success = true for green glow
                    MainUIManager.Instance.DeactivateItemPickupIndicator(true);

                    // deactivate farming indicator and reset bool
                    MainUIManager.Instance.DeactivateFarmingIndicator();
                    farmableObjectNearby = false;

                    // process results based on whether there is a menu action found
                    ProcessFoundActions(cachedItemResults, cachedItemResultSize, cachedFoundNonItemAction, cachedNonItemActionIndex);
                }
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
    }

    private void ResetCachedItemVariables()
    {
        cachedItemResults = null;
        cachedItemResultSize = 0;
        cachedFoundAction = false;
        cachedFoundNonItemAction = false;
        cachedNonItemActionIndex = -1;
    }

    private void ProcessFoundActions(Collider[] results, int size, bool nonItemAction, int nonItemActionIndex)
    {
        // always prioritize menu actions before items. trader and spaceship should never be in range of each other, thus should never overlap
        if (nonItemAction)
        {
            Collider actionCollider = results[nonItemActionIndex];

            if (actionCollider != null)
            {
                // changed for game design reasons
                //if (actionCollider.CompareTag("Trader"))
                //{
                //    TraderMenuManager.Instance.ToggleTraderMenu();
                //}
                // end changed code

                if (actionCollider.CompareTag("MainSpaceship"))
                {
                    // play menu open sound effect
                    MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.SpaceMenuEnter);

                    // open space race menu
                    SpaceshipSelection.Instance.ActivateSpaceshipSelection();
                }
                else
                {
                    Debug.LogWarning("Non item action index led to a collider where neither the trader nor the spaceship was found.");
                }
            }
            else
            {
                Debug.LogWarning("Non item action index led to a null collider.");
            }
        }
        else
        {
            // if no non-item actions were found, pickup items
            if (itemPickupCoroutine != null)
            {
                StopCoroutine(itemPickupCoroutine);
                itemPickupCoroutine = null;
            }

            itemPickupCoroutine = StartCoroutine(ItemPickupCoroutine(results, size));
        }

        // reset once done processing
        ResetCachedItemVariables();
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

            // --- CHANGED FOR GAME DESIGN REASONS
            // check for trader
            //if (collider.CompareTag("Trader"))
            //{
            //    if (!InventoryManager.Instance.IsMenuActive)
            //    {
            //        nonItemActionFound = true;
            //        actionFound = true;
            //        nonItemActionIndex = i;
            //    }
            //}
            // --- END CHANGED CODE

            // check for spaceship
            if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out SpaceshipSelection _) &&
                QuestManager.Instance.SpaceRaceQuestReached)
            {
                nonItemActionFound = true;
                actionFound = true;
                nonItemActionIndex = i;
            }
            // check for items
            else if (collider.gameObject.TryGetComponent(out Item item))
            {
                if (item is not Animal && item is not PlaceableItem && item is not DeadTree && item.IsReadyForPickup)
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

        int numberOfQuestItems = 0;
        bool atLeastOneItemPickedUp = false;

        // loop through collider results and pickup all items
        for (int i = 0; i < size; i++)
        {
            Collider collider = results[i];

            // get item component and pick it up if it's not an animal/placeableitem/deadtree
            if (collider.gameObject.TryGetComponent(out Item item))
            {
                if (item is not Animal && item is not PlaceableItem && item is not DeadTree)
                {
                    item.PickupItem();
                    atLeastOneItemPickedUp = true;
                    
                    if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.CollectMoreWood && item.ItemName == "Wood")
                    {
                        numberOfQuestItems++;
                    }
                    else if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.CollectStones &&
                        QuestManager.Instance.GemstoneNames.Contains(item.ItemName))
                    {
                        numberOfQuestItems++;
                    }
                }
            }
        }

        if (atLeastOneItemPickedUp)
        {
            // play item pickup sound effect
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.PickupItem);
        }

        // if player is on one of the collecting quests and at least 1 quest item was collected
        if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.CollectMoreWood ||
            QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.CollectStones &&
            numberOfQuestItems > 0)
        {
            // update quest manager with amount
            // this is done as a batch so that the UI floating text component properly updates +2, +3 etc instead of spawning multiple overlapping +1s
            QuestManager.Instance.UpdateCurrentQuest(numberOfQuestItems);
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
            playerObject.position + Vector3.up * 0.25f,
            playerObject.position + playerObject.right * raycastSideOffset + Vector3.up * 0.25f,
            playerObject.position - playerObject.right * raycastSideOffset + Vector3.up * 0.25f,
        };

        // loop through each origin and perform raycast
        foreach (Vector3 rayOrigin in rayOrigins)
        {
            Debug.DrawRay(rayOrigin, playerObject.forward * farmableSearchDistance, Color.red, 0.1f); // Short duration for live debugging

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

    private void ShootBow()
    {
        if (bowShotCoroutine != null)
        {
            StopCoroutine(bowShotCoroutine);
            bowShotCoroutine = null;
        }

        bowShotCoroutine = StartCoroutine(BowShotCoroutine());
    }

    private IEnumerator BowShotCoroutine()
    {
        isShooting = true;
        playerBow.SetActive(true);
        playerAnimation.TriggerBowShot();

        // wait for animation to progress to the release state
        while (!playerAnimation.GetBowShotAnimationState())
        {
            yield return null;
        }

        // get projectile from pool, set position to bow, and set active in scene
        GameObject projectile = ProjectilePool.Instance.GetPooledObject();

        if (projectile != null)
        {
            projectile.transform.position = playerBow.transform.position;
            projectile.SetActive(true);

            // set shot direction to player object's forward direction
            Vector3 direction = playerObject.transform.forward;

            // get rigidbody component and set velocity
            if (projectile.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(15f * direction, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(bowShotAnimationTime - timeToBowShotRelease);

        playerBow.SetActive(false);
        isShooting = false;
    }
}
