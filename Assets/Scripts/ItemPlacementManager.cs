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

    private Color validPreviewColor = new(166 / 255f, 166 / 255f, 166 / 255f, 40 / 255f); // gray transparent color
    private Color invalidPreviewColor = new(255 / 255f, 0 / 255f, 0 / 255f, 65 / 255f); // red transparent color

    // placement settings
    private const float placementDistance = 5.0f;
    private const float cameraVerticalOffset = 0.25f;
    private const float attachmentSearchRadius = 2.5f;
    private const float orientationDefaultY = 1.0f; // normal height on ground of the orientation game object -- used for adjusting spawn height of items

    // for use references
    private Material itemOriginalMaterial;
    private PlaceableItem currentItem;

    private void Awake()
    {
        Instance = this;
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
            }
            else if (Input.GetKeyDown(escapeKey))
            {
                ReturnItemToInventory();
            }
        }
    }

    public void ActivateItemPlacement(PlaceableItem item)
    {
        ItemPlacementActive = true;
        InventoryManager.Instance.ToggleInventoryMenu();
        item.gameObject.transform.SetParent(null);
        currentItem = item;

        // update start position
        UpdateItemPlacementPos();

        // set to transparent preview material (true)
        SetPreviewMaterial(true);

        // set active in scene
        currentItem.gameObject.SetActive(true);
    }

    private void DeactivateItemPlacement()
    {
        currentItem = null;
        ItemPlacementActive = false;
    }

    public void PlaceItem()
    {
        if (ItemPlacementActive && currentItem != null)
        {
            // restore original material to currentItem
            SetPreviewMaterial(false);

            // remove from player inventory in data manager
            DataManager.Instance.RemoveItem(currentItem);

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

            currentItem.transform.position = targetPosition;
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
            if (currentItem.TryGetComponent<Renderer>(out var renderer))
            {
                if (isPreview)
                {
                    itemOriginalMaterial = renderer.material;
                    renderer.material = itemPreviewMaterial;
                }
                else
                {
                    renderer.material = itemOriginalMaterial;
                }
            }
        }
    }

    private void UpdatePreviewColor()
    {
        if (ItemPlacementActive && currentItem != null)
        {
            if (currentItem.TryGetComponent(out Renderer renderer))
            {
                Color targetColor = currentItem.IsPlaceable() ? validPreviewColor : invalidPreviewColor;

                if (renderer.material.color != targetColor)
                {
                    renderer.material.color = targetColor;
                }
            }
        }
    }
}
