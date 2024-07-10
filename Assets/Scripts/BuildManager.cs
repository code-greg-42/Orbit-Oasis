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
    [SerializeField] private float placementDistance = 5.0f;
    [SerializeField] private Material buildPreviewMaterial;
    [SerializeField] private float rotationSpeed = 60.0f; // degrees per second
    [SerializeField] private float previewMoveSpeed = 2.0f; // units per second
    [SerializeField] private float attachmentSearchRadius = 2.0f;
    [SerializeField] private float attachmentDisableRadius = 7.0f;
    [SerializeField] private LayerMask attachmentLayer;
    [SerializeField] private LayerMask buildLayer;

    private float cameraVerticalOffset = 0.25f;

    private KeyCode placeBuildKey = KeyCode.F;
    private KeyCode rotateLeftKey = KeyCode.Q;
    private KeyCode rotateRightKey = KeyCode.E;
    private KeyCode moveUpKey = KeyCode.UpArrow;
    private KeyCode moveDownKey = KeyCode.DownArrow;
    private KeyCode moveLeftKey = KeyCode.LeftArrow;
    private KeyCode moveRightKey = KeyCode.RightArrow;
    private KeyCode resetKey = KeyCode.R;

    private GameObject currentPreview;
    private Material originalMaterial;
    private float userRotation;
    private float verticalOffset;
    private float horizontalOffset;
    private int currentPrefabIndex = 0; // index to track current selection

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

                // calc rotation offset based on orientation and user input
                targetRotation = Quaternion.LookRotation(orientation.forward) *
                    Quaternion.Euler(buildPrefabs[currentPrefabIndex].transform.rotation.eulerAngles.x, userRotation, buildPrefabs[currentPrefabIndex].transform.rotation.eulerAngles.z);
            }

            // set position and rotation
            currentPreview.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }

    private void PlaceBuild()
    {
        if (BuildModeActive && currentPreview != null)
        {
            Vector3 placedPosition;

            // change material to solid
            SetPreviewMaterial(false);
            if (currentPreview.TryGetComponent<BuildableObject>(out var buildable))
            {
                placedPosition = buildable.PlaceObject();

                // deactivate any impacted attachment points
                CheckAndDisableAttachmentPoints(placedPosition);
            }

            // finalize placement
            currentPreview = null;
            originalMaterial = null;
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

    private void CheckAndDisableAttachmentPoints(Vector3 placedPosition)
    {
        Collider[] overlapResults = new Collider[12];

        int numResults = Physics.OverlapSphereNonAlloc(placedPosition, attachmentDisableRadius, overlapResults, buildLayer);

        for (int i = 0; i < numResults; i++)
        {
            if (overlapResults[i].TryGetComponent(out BuildableObject buildObject))
            {
                buildObject.CheckAndDisableAttachmentPoints(buildLayer);
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

    private void ResetPreviewState()
    {
        userRotation = 0f;
        verticalOffset = 0f;
        horizontalOffset = 0f;
    }

    private Vector3 CalcTargetPosition()
    {
        // calc the position in front of the camera ( - 1 accounts for the height of the orientation game object)
        //Vector3 targetPosition = orientation.position +
        //    orientation.forward * placementDistance +
        //    Vector3.up * ((buildPrefabs[currentPrefabIndex].transform.position.y - 1) + verticalOffset) +
        //    orientation.right * horizontalOffset;

        //return targetPosition;

        Vector3 targetPosition = orientation.position + orientation.forward * placementDistance;

        // get camera's forward direction
        Vector3 cameraForward = Camera.main.transform.forward;

        float verticalAdjustment = (cameraForward.y + cameraVerticalOffset) * placementDistance;

        targetPosition.y = Mathf.Max(buildPrefabs[currentPrefabIndex].transform.position.y, targetPosition.y + verticalAdjustment);

        return targetPosition;
    }

    void OnDrawGizmos()
    {
        if (BuildModeActive && currentPreview != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentPreview.transform.position, attachmentSearchRadius);
        }
    }
}
