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
    [SerializeField] private Material buildPreviewMaterial;

    private GameObject currentPreview;
    private Material originalMaterial;

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
            currentPreview.transform.position = targetPosition;
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
