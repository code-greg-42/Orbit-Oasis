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
    [SerializeField] private KeyCode placeBuildKey = KeyCode.F;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.RightArrow;
    [SerializeField] private Material buildPreviewMaterial;
    [SerializeField] private float rotationSpeed = 60.0f; // degrees per second

    private GameObject currentPreview;
    private Material originalMaterial;
    private float currentRotation;

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

            // handle rotation
            if (Input.GetKey(rotateLeftKey))
            {
                currentRotation -= rotationSpeed * Time.deltaTime;
            }

            if (Input.GetKey(rotateRightKey))
            {
                currentRotation += rotationSpeed * Time.deltaTime;
            }
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
        }

        // toggle bool
        BuildModeActive = !BuildModeActive;
    }

    private void UpdatePreviewPosition()
    {
        if (BuildModeActive)
        {
            // calc the position in front of the camera
            Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

            // calc rotation offset based on orientation and user input
            Quaternion targetRotation = Quaternion.LookRotation(orientation.forward) * Quaternion.Euler(0, currentRotation, 0);

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
}
