using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacementManager : MonoBehaviour
{
    public static ItemPlacementManager Instance;

    public bool ItemPlacementActive { get; private set; }

    [SerializeField] private Material itemPreviewMaterial;
    [SerializeField] private Transform orientation;

    private KeyCode placeKey = KeyCode.F;
    private KeyCode escapeKey = KeyCode.Escape;
    private KeyCode backToInventoryKey = KeyCode.Tab;

    private Color validPreviewColor = new(166 / 255f, 166 / 255f, 166 / 255f, 40 / 255f); // gray transparent color
    private Color invalidPreviewColor = new(255 / 255f, 0 / 255f, 0 / 255f, 65 / 255f); // red transparent color

    // placement settings
    private const float placementDistance = 5.0f;
    private const float orientationDefaultY = 1.0f; // normal height on ground of the orientation game object -- used for adjusting spawn height of items

    // for use references
    private Material[][] itemOriginalMaterials;
    private PlaceableItem currentItem;
    private Renderer[] currentItemRenderers;

    // ---  Similarly to BuildManager, this script could be modified to have a more realistic feel if needed.
    // ----- 1. Attach references to the build an item was placed on, and vice versa
    // ----- 2. Before deleting a build/item, either also delete the attached objects, or allow them to have gravity enabled to fall
    // ----- 3. This would also require re-setting the references when re-creating the saved scene at launch
    // ----- 4. Because of this, it would be highly recommended to make a .PlaceItem() method in PlaceableItem, similar to what's in BuildableObject
    // ----- 5. This would allow for a single-use raycast when called, to check for a buildable object, and add the appropriate references if found

    // --- Additionally, this script could be optimized for better performance if needed, by running less frequent placeable/not placeable checks

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // instantiate any existing placed items from data manager's list of PlacedItems
        LoadPlacedItems();
    }

    private void Update()
    {
        if (ItemPlacementActive)
        {
            UpdateItemPlacementPos();
            UpdatePreviewColor();
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (ItemPlacementActive)
        {
            if (Input.GetKeyDown(placeKey) && currentItem != null && currentItem.IsPlaceable())
            {
                PlaceItem();
                MainUIManager.Instance.ActivateControlsDisplay();
            }
            else if (Input.GetKeyDown(escapeKey))
            {
                ReturnItemToInventory();
                MainUIManager.Instance.ActivateControlsDisplay();
            }
            else if (Input.GetKeyDown(backToInventoryKey))
            {
                ReturnItemToInventory();

                if (!InventoryManager.Instance.IsMenuActive)
                {
                    InventoryManager.Instance.ToggleInventoryMenu();
                }
            }
        }
    }

    private void LoadPlacedItems()
    {
        if (DataManager.Instance.PlacedItems.ItemList.Count > 0)
        {
            // copy of list
            List<PlaceableItemData> placedItems = new(DataManager.Instance.PlacedItems.ItemList);

            // get array of item prefabs from inventory manager
            GameObject[] itemPrefabs = InventoryManager.Instance.ItemPrefabs;

            foreach (PlaceableItemData placedItemData in placedItems)
            {
                // instantiate new prefab at the saved position and rotation
                Instantiate(itemPrefabs[placedItemData.prefabIndex], placedItemData.placementPosition,
                    placedItemData.placementRotation);
            }

            // update navmesh with newly placed items
            NavMeshManager.Instance.UpdateNavMesh();
        }
    }

    public void ActivateItemPlacement(PlaceableItem item)
    {
        ItemPlacementActive = true;
        InventoryManager.Instance.ToggleInventoryMenu(false);

        // force deactivate main controls panel and activate item placement controls
        MainUIManager.Instance.DeactivateControlsDisplay();
        MainUIManager.Instance.ActivateItemPlacementControlsPanel();

        item.gameObject.transform.SetParent(null);
        currentItem = item;

        // update start position
        UpdateItemPlacementPos();

        // set to transparent preview material (true)
        SetPreviewMaterial(true);

        // set collider to trigger --- used for determining whether it's placeable
        if (currentItem.TryGetComponent(out Collider collider))
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("Could not find collider component on placeable item.");
        }

        // set active in scene
        currentItem.gameObject.SetActive(true);
    }

    private void DeactivateItemPlacement()
    {
        // reset collider
        if (currentItem.TryGetComponent(out Collider collider))
        {
            collider.isTrigger = false;
        }
        else
        {
            Debug.LogWarning("Could not find collider component on placeable item.");
        }

        MainUIManager.Instance.DeactivateItemPlacementControlsPanel();
        currentItem = null;
        ItemPlacementActive = false;
    }

    public void PlaceItem()
    {
        if (ItemPlacementActive && currentItem != null)
        {
            // restore original material to currentItem
            SetPreviewMaterial(false);

            // play sound effect
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.PlaceBuild);

            // update quest manager if player is on the place trees quest -- no placeable items other than trees available at this point in game
            if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.PlantNewTrees)
            {
                QuestManager.Instance.UpdateCurrentQuest();
            }

            // update quest manager if player is on the place rocks quest
            if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.PlaceRocks)
            {
                // make sure item placed is a rock, as it's possible for a user to pickup and replace a tree
                if (currentItem.TryGetComponent(out FarmableObject farmable))
                {
                    if (farmable.Type == FarmableObject.ObjectType.Rock)
                    {
                        QuestManager.Instance.UpdateCurrentQuest();
                    }
                }
            }

            // remove from player inventory in data manager
            DataManager.Instance.RemoveItem(currentItem);

            // add to placeable items list in data manager
            DataManager.Instance.AddPlacedItem(currentItem);

            // update navmesh with newly placed object
            NavMeshManager.Instance.UpdateNavMesh();
        }

        DeactivateItemPlacement();
    }

    public void ReturnItemToInventory()
    {
        if (ItemPlacementActive && currentItem != null)
        {
            // remove from player inventory as it will be added back in .PickupItem()
            // ----- DONE THIS WAY TO PREVENT THE OCCURRENCE OF LOSING AN ITEM FROM A GAME CRASH DURING PLACEMENT MODE ----- //
            DataManager.Instance.RemoveItem(currentItem);

            // add item back to player inventory
            currentItem.PickupItem();

            // reset material to original --- currentItem should still have its reference despite the item being in player inventory
            // ---- done this way to prevent any split second material change on the screen --- .PickupItem() will deactivate the object
            SetPreviewMaterial(false);
        }

        DeactivateItemPlacement();
    }

    private void UpdateItemPlacementPos()
    {
        if (ItemPlacementActive && currentItem != null)
        {
            // calc target placement
            Vector3 targetPosition = CalcTargetPosition();

            // calc target rotation
            Quaternion targetRotation = Quaternion.LookRotation(orientation.forward);

            // set position of and rotation of preview
            currentItem.transform.SetPositionAndRotation(targetPosition, targetRotation);

            //currentItem.transform.position = targetPosition;
        }
    }

    private Vector3 CalcTargetPosition()
    {
        // get original position as the position out in front of the camera, adjusted for where the camera is looking left/right
        Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

        float heightAdjustment = currentItem.ItemHeight / 2 - orientationDefaultY;

        // adjust Y position for height of the item
        targetPosition.y += heightAdjustment;

        return targetPosition;
    }

    private void SetPreviewMaterial(bool isPreview)
    {
        if (ItemPlacementActive && currentItem != null)
        {
            if (isPreview)
            {
                // get all renderers on current item
                currentItemRenderers = currentItem.GetComponentsInChildren<Renderer>();
            }

            if (currentItemRenderers.Length > 0)
            {
                if (isPreview)
                {
                    // init array of arrays to store all original materials for all renderers
                    itemOriginalMaterials = new Material[currentItemRenderers.Length][];

                    for (int i = 0; i < currentItemRenderers.Length; i++)
                    {
                        // get all materials from the renderer
                        Material[] rendererMaterials = currentItemRenderers[i].materials;

                        // initialize the inner array with how many materials are used
                        itemOriginalMaterials[i] = new Material[rendererMaterials.Length];

                        // store the original materials
                        for (int j = 0; j < rendererMaterials.Length; j++)
                        {
                            itemOriginalMaterials[i][j] = rendererMaterials[j];

                            // set the renderer's material to the preview material
                            rendererMaterials[j] = itemPreviewMaterial;
                        }

                        // assign updated materials array back to the renderer
                        currentItemRenderers[i].materials = rendererMaterials;
                    }
                }
                else
                {
                    if (currentItemRenderers.Length == itemOriginalMaterials.Length)
                    {
                        // revert all renderers back to original materials
                        for (int i = 0; i < currentItemRenderers.Length; i++)
                        {
                            currentItemRenderers[i].materials = itemOriginalMaterials[i];
                        }
                    }
                    else
                    {
                        Debug.LogError("Error: Original materials array does not have the same length as the current item renderers array.");
                    }

                    // empty cached renderers and materials arrays
                    currentItemRenderers = new Renderer[0];
                    itemOriginalMaterials = new Material[0][];
                }
            }
            else
            {
                Debug.LogWarning("No renderer components found on current item or its children. Item: " + currentItem.name);
            }
        }
    }

    private void UpdatePreviewColor()
    {
        if (ItemPlacementActive && currentItem != null && currentItemRenderers.Length > 0)
        {
            Color targetColor = currentItem.IsPlaceable() ? validPreviewColor : invalidPreviewColor;

            foreach (Renderer renderer in currentItemRenderers)
            {
                // Get the array of materials for the current renderer
                Material[] materials = renderer.materials;

                // Iterate through each material and update the color
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].color != targetColor)
                    {
                        materials[i].color = targetColor;
                    }
                }

                // Assign the modified materials back to the renderer
                renderer.materials = materials;
            }
        }
    }
}
