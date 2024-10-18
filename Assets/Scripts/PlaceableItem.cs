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

    // raycasting settings
    private const float raycastBuffer = 0.1f;
    private float distanceToGround;
    private float rayLength;
    private float minHitLength;

    // flag for registering collision events
    private bool isColliding;

    public float ItemHeight => itemHeight;
    public BuildEnums.BuildType AttachmentType => attachmentType;
    public override bool IsDroppable { get; } = false;

    private void Awake()
    {
        localBounds = new Vector3[5]
        {
            new(0, 0, 0),
            new(leftBound, 0, 0),
            new(rightBound, 0, 0),
            new(0, 0, frontBound),
            new(0, 0, backBound)
        };
        ignoreLayer = ~LayerMask.GetMask("BuildAttachmentPoint");

        // init last position and movement threshold
        lastPosition = transform.position;
        movementThresholdSqr = movementThreshold * movementThreshold;
        distanceToGround = itemHeight / 2;
        rayLength = distanceToGround + raycastBuffer;
        minHitLength = distanceToGround - raycastBuffer;
    }

    public bool IsPlaceable()
    {
        // if object is colliding, it's not placeable
        if (isColliding)
        {
            lastIsPlaceable = false;
            return false;
        }

        // if object hasn't moved, return most recent result
        if (!HasMoved())
        {
            return lastIsPlaceable;
        }

        // counter for valid hits
        int validHits = 0;

        foreach (Vector3 localPoint in localBounds)
        {
            // transform to world position
            Vector3 worldPoint = transform.TransformPoint(localPoint);

            // create ray, aimed down
            Ray ray = new(worldPoint, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, 1f);

            // --- PERFORM RAYCAST ---

            // --- no hit ---
            if (!Physics.Raycast(ray, out RaycastHit hit, rayLength, ignoreLayer))
            {
                lastIsPlaceable = false;
                return false;
            }

            // --- yes hit ---

            // calculate whether or not the distance of the raycast hit is within the minimum range
            bool isValidHitLength = (worldPoint.y - hit.point.y) > minHitLength;

            if (isValidHitLength)
            {
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
            else
            {
                // hit was too close --- there is an obstruction 
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

    private void OnTriggerStay(Collider other)
    {
        // ignore interactions with ground
        if (other.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // ignore interactions with floor objects
        if (other.gameObject.TryGetComponent(out BuildableObject buildable))
        {
            if (buildable.BuildType == BuildEnums.BuildType.Floor)
            {
                return;
            }
        }

        // ignore interactions with the BuildAttachment layer
        if (other.gameObject.layer == LayerMask.NameToLayer("BuildAttachmentPoint"))
        {
            return;
        }

        isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // reset flag upon exiting the trigger
        isColliding = false;
    }
}
