using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlaceableItem : Item
{
    [Header("Placeable Item Settings")]
    [SerializeField] private float itemHeight;
    [SerializeField] private BuildEnums.BuildType attachmentType;

    [Header("Placeability Check Settings")]
    [SerializeField] private float leftBound;
    [SerializeField] private float rightBound;
    [SerializeField] private float frontBound;
    [SerializeField] private float backBound;

    // initialize in awake
    private Vector3[] localBounds;
    private int ignoreLayer;

    // for optimizing the IsPlaceable method
    private bool lastIsPlaceable;
    private Vector3 lastPosition;
    private const float movementThreshold = 0.01f;
    private float movementThresholdSqr;

    public float ItemHeight => itemHeight;
    public BuildEnums.BuildType AttachmentType => attachmentType;

    private void Awake()
    {
        localBounds = new Vector3[4]
        {
            new(leftBound, 0, 0),
            new(rightBound, 0, 0),
            new(0, 0, frontBound),
            new(0, 0, backBound)
        };
        ignoreLayer = ~LayerMask.GetMask("BuildAttachmentPoint");

        // init last position and movement threshold
        lastPosition = transform.position;
        movementThresholdSqr = movementThreshold * movementThreshold;
    }

    public bool IsPlaceable()
    {
        if (!HasMoved())
        {
            return lastIsPlaceable;
        }

        // length of the ray: half item's height plus a buffer
        float rayLength = (itemHeight / 2) + 0.1f;

        // counter for valid hits
        int validHits = 0;

        foreach (Vector3 localPoint in localBounds)
        {
            // transform to world position
            Vector3 worldPoint = transform.TransformPoint(localPoint);

            // create ray, aimed down
            Ray ray = new(worldPoint, Vector3.down);

            // draw line for debugging
            Debug.DrawLine(worldPoint, worldPoint + Vector3.down * rayLength, Color.red);

            // --- PERFORM RAYCAST ---

            // --- no hit ---
            if (!Physics.Raycast(ray, out RaycastHit hit, rayLength, ignoreLayer))
            {
                lastIsPlaceable = false;
                return false;
            }
            
            // --- yes hit ---
            if (hit.collider.TryGetComponent(out BuildableObject buildable))
            {
                // surface is a valid build
                if (buildable.BuildType == AttachmentType)
                {
                    validHits++;
                }
            }
            else if (hit.collider.CompareTag("Ground"))
            {
                // surface is ground
                validHits++;
            }
            else
            {
                // surface is something else
                lastIsPlaceable = false;
                return false;
            }
        }

        // return true if each bound area found a valid surface
        bool isPlaceable = validHits == localBounds.Length;
        lastIsPlaceable = isPlaceable;
        return isPlaceable;
    }

    private bool HasMoved()
    {
        // check distance to see if it has moved past threshold
        if ((transform.position - lastPosition).sqrMagnitude > movementThresholdSqr)
        {
            lastPosition = transform.position;
            return true;
        }
        return false;
    }
}
