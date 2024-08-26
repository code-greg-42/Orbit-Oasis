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

    private Vector3[] localBounds;

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
    }

    public bool IsPlaceable()
    {
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

            // perform raycast

            // no hit
            if (!Physics.Raycast(ray, out RaycastHit hit, rayLength))
            {
                return false;
            }

            // surface is a valid build
            if (hit.collider.TryGetComponent(out BuildableObject buildable))
            {
                if (buildable.BuildType == AttachmentType)
                {
                    validHits++;
                }
            } // surface is ground
            else if (hit.collider.CompareTag("Ground"))
            {
                validHits++;
            }
            else
            {
                return false;
            }
        }

        // return true if each bound area found a valid surface
        if (validHits == localBounds.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
