using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    public bool BuildModeActive { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform orientation;
    [SerializeField] private float placementDistance = 5.0f;
    [SerializeField] private Material buildPreviewMaterial;
    [SerializeField] private float rotationSpeed = 60.0f; // degrees per second
    [SerializeField] private float previewMoveSpeed = 2.0f; // units per second

    private KeyCode placeBuildKey = KeyCode.F;
    private KeyCode rotateLeftKey = KeyCode.Q;
    private KeyCode rotateRightKey = KeyCode.E;
    private KeyCode moveUpKey = KeyCode.UpArrow;
    private KeyCode moveDownKey = KeyCode.DownArrow;
    private KeyCode moveLeftKey = KeyCode.LeftArrow;
    private KeyCode moveRightKey = KeyCode.RightArrow;
    private KeyCode resetKey = KeyCode.R;

    // add boundary variables to keep the preview on the screen

    private GameObject currentPreview;
    private Material originalMaterial;
    private float userRotation;
    private float verticalOffset;
    private float horizontalOffset;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (BuildModeActive)
        {
            if (currentPreview == null)
            {
                // instantiate preview
                currentPreview = Instantiate(wallPrefab);
                SetPreviewMaterial(true);
            }

            UpdatePreviewPosition();

            if (Input.GetKeyDown(placeBuildKey))
            {
                PlaceBuild();
            }

            HandleUserInput();
        }
    }

    public void ToggleBuildMode()
    {
        if (BuildModeActive)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
            originalMaterial = null;
            ResetPreviewState();
        }

        // toggle bool
        BuildModeActive = !BuildModeActive;
    }

    private void UpdatePreviewPosition()
    {
        if (BuildModeActive)
        {
            // calc the position in front of the camera ( - 1 accounts for the height of the orientation game object)
            Vector3 targetPosition = orientation.position +
                orientation.forward * placementDistance +
                Vector3.up * ((wallPrefab.transform.position.y - 1) + verticalOffset) +
                orientation.right * horizontalOffset;
            
            // calc rotation offset based on orientation and user input
            Quaternion targetRotation = Quaternion.LookRotation(orientation.forward) * Quaternion.Euler(0, userRotation, 0);

            // set position and rotation
            currentPreview.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }

    private void PlaceBuild()
    {
        if (BuildModeActive && currentPreview != null)
        {
            // change material to solid
            SetPreviewMaterial(false);

            // finalize placement
            currentPreview = null;
            originalMaterial = null;
        }
    }

    private void SetPreviewMaterial(bool isPreview)
    {
        if (currentPreview.TryGetComponent<Renderer>(out var renderer))
        {
            if (isPreview)
            {
                originalMaterial = renderer.material;
                renderer.material = buildPreviewMaterial;
            }
            else
            {
                renderer.material = originalMaterial;
            }
        }
    }

    // add boundaries to keep the preview on the screen later
    private void HandleUserInput()
    {
        if (Input.GetKey(rotateLeftKey))
        {
            userRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rotateRightKey))
        {
            userRotation += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveLeftKey))
        {
            horizontalOffset -= previewMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveRightKey))
        {
            horizontalOffset += previewMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveUpKey))
        {
            verticalOffset += previewMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveDownKey))
        {
            verticalOffset -= previewMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(resetKey))
        {
            ResetPreviewState();
        }
    }

    private void ResetPreviewState()
    {
        userRotation = 0f;
        verticalOffset = 0f;
        horizontalOffset = 0f;
    }
}
