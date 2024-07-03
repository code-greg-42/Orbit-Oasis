using System.Collections;
using System.Collections.Generic;
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

    private GameObject currentPreview;

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
        if (currentPreview != null)
        {
            // change material to solid
            SetPreviewMaterial(false);

            // finalize placement
            currentPreview = null;
        }
    }

    private void SetPreviewMaterial(bool isPreview)
    {
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (isPreview)
            {
                // Make material see-through
                Color color = renderer.material.color;
                color.a = 0.5f;
                renderer.material.color = color;
            }
            else
            {
                // Make material solid
                Color color = renderer.material.color;
                color.a = 1.0f;
                renderer.material.color = color;
            }
        }
    }
}
