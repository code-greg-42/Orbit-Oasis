using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    public bool BuildModeActive { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject[] buildPrefabs;
    [SerializeField] private Transform orientation;
    [SerializeField] private Material buildPreviewMaterial;
    [SerializeField] private LayerMask attachmentLayer;
    [SerializeField] private LayerMask buildLayer;

    // build settings
    private float placementDistance = 5.0f;
    private float attachmentSearchRadius = 2.5f;
    private float attachmentDisableRadius = 7.0f;
    private float cameraVerticalOffset = 0.25f;

    // keybinds
    private KeyCode placeBuildKey = KeyCode.F;
    private KeyCode undoBuildKey = KeyCode.Q;

    private GameObject currentPreview;
    private GameObject lastPlacedBuild;
    private Material originalMaterial;
    private int currentPrefabIndex = 0; // index to track build object type selection

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
                Vector3 targetPosition = CalcTargetPosition();
                currentPreview = Instantiate(buildPrefabs[currentPrefabIndex], targetPosition, buildPrefabs[currentPrefabIndex].transform.rotation);
                SetPreviewMaterial(true);
            }

            UpdatePreviewPosition();

            if (Input.GetKeyDown(placeBuildKey) && currentPreview != null)
            {
                PlaceBuild();
            }

            if (Input.GetKeyDown(undoBuildKey) && lastPlacedBuild != null)
            {
                UndoRecentBuild();
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
        }
        BuildModeActive = !BuildModeActive;
    }

    private void UpdatePreviewPosition()
    {
        if (BuildModeActive)
        {
            Transform closestAttachmentPoint = FindClosestAttachmentPoint();

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (closestAttachmentPoint != null)
            {
                targetPosition = closestAttachmentPoint.position;
                targetRotation = closestAttachmentPoint.rotation;
            }
            else
            {
                targetPosition = CalcTargetPosition();
                targetRotation = Quaternion.LookRotation(orientation.forward) *
                                    Quaternion.Euler(buildPrefabs[currentPrefabIndex].transform.rotation.eulerAngles);
            }

            currentPreview.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }

    private void PlaceBuild()
    {
        if (BuildModeActive && currentPreview != null)
        {
            lastPlacedBuild = currentPreview;

            // change material to solid
            SetPreviewMaterial(false);
            if (currentPreview.TryGetComponent<BuildableObject>(out var buildable))
            {
                // place build
                buildable.PlaceObject();

                // deactivate any impacted attachment points
                CheckAttachmentPoints(lastPlacedBuild.transform.position);
            }

            // finalize placement
            currentPreview = null;
            originalMaterial = null;
        }
    }

    private void UndoRecentBuild()
    {
        if (BuildModeActive && lastPlacedBuild != null)
        {
            // get position of build to use for attachment update
            Vector3 lastBuildPosition = lastPlacedBuild.transform.position;

            // deactivate before destroying so CheckAttachmentPoints runs as expected
            lastPlacedBuild.SetActive(false);
            Destroy(lastPlacedBuild);
            lastPlacedBuild = null;

            // enable any free attachment points
            CheckAttachmentPoints(lastBuildPosition, true);
        }
    }

    private Transform FindClosestAttachmentPoint()
    {
        Vector3 previewPosition = CalcTargetPosition();
        BuildableObject currentBuildableObject = currentPreview.GetComponent<BuildableObject>();

        Collider[] results = new Collider[16];
        int size = Physics.OverlapSphereNonAlloc(previewPosition, attachmentSearchRadius, results, attachmentLayer);

        Transform closestAttachmentPoint = null;
        float closestSqrDistance = attachmentSearchRadius;

        for (int i = 0; i < size; i++)
        {
            if (results[i].TryGetComponent(out BuildAttachmentPoint attachmentPoint))
            {
                if (attachmentPoint.AttachmentType == currentBuildableObject.BuildType)
                {
                    float sqrDistance = (previewPosition - attachmentPoint.transform.position).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestAttachmentPoint = attachmentPoint.transform;
                    }
                }
            }
        }
        return closestAttachmentPoint;
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

    private void CheckAttachmentPoints(Vector3 placedPosition, bool enable = false)
    {
        Collider[] overlapResults = new Collider[12];

        int numResults = Physics.OverlapSphereNonAlloc(placedPosition, attachmentDisableRadius, overlapResults, buildLayer);

        if (!enable)
        {
            // disable any used attachment points
            for (int i = 0; i < numResults; i++)
            {
                if (overlapResults[i].TryGetComponent(out BuildableObject buildObject))
                {
                    if (buildObject.IsPlaced)
                    {
                        buildObject.CheckAndDisableAttachmentPoints(buildLayer);
                    }
                }
            }
        }
        else
        {
            // enable any unused attachment points
            for (int i = 0; i < numResults; i++)
            {
                if (overlapResults[i].TryGetComponent(out BuildableObject buildObject))
                {
                    if (buildObject.IsPlaced)
                    {
                        buildObject.CheckAndEnableAttachmentPoints(buildLayer);
                    }
                }
            }
        }
    }

    private void HandleUserInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangePrefab(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangePrefab(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangePrefab(2);
        }
    }

    private void ChangePrefab(int index)
    {
        if (index >= 0 && index < buildPrefabs.Length)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            currentPrefabIndex = index;

            Vector3 targetPosition = CalcTargetPosition();
            Debug.Log(buildPrefabs[currentPrefabIndex].transform.rotation);
            currentPreview = Instantiate(buildPrefabs[currentPrefabIndex], targetPosition, buildPrefabs[currentPrefabIndex].transform.rotation);
            SetPreviewMaterial(true);
        }
    }

    private Vector3 CalcTargetPosition()
    {
        Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

        // get camera's forward direction
        Vector3 cameraForward = Camera.main.transform.forward;

        float verticalAdjustment = (cameraForward.y + cameraVerticalOffset) * placementDistance;

        targetPosition.y = Mathf.Max(buildPrefabs[currentPrefabIndex].transform.position.y, targetPosition.y + verticalAdjustment);

        return targetPosition;
    }
}
