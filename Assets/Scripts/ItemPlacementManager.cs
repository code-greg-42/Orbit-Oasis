using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacementManager : MonoBehaviour
{
    public static ItemPlacementManager Instance;

    public bool ItemPlacementActive { get; private set; }

    [SerializeField] private Material itemPreviewMaterial;
    [SerializeField] private Transform orientation;

    // placement settings
    private const float placementDistance = 4.6f;
    private const float cameraVerticalOffset = 0.25f;

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

        // set active in scene
        currentItem.gameObject.SetActive(true);
    }

    public void UpdateItemPlacementPos()
    {
        // calc target placement
        Vector3 targetPosition = CalcTargetPosition();

        currentItem.transform.position = targetPosition;
    }

    private Vector3 CalcTargetPosition()
    {
        // get original position as the position out in front of the camera, adjusted for where the camera is looking left/right
        Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

        // get camera's forward direction for up/down calculation
        Vector3 cameraForward = Camera.main.transform.forward;

        // adjust slightly to make the position slightly higher than where the camera is looking (allows for building up)
        float verticalAdjustment = (cameraForward.y + cameraVerticalOffset) * placementDistance;

        return targetPosition;
    }
}
