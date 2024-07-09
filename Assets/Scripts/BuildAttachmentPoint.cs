using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildAttachmentPoint : MonoBehaviour
{
    [SerializeField] private BuildEnums.BuildType attachmentType;

    private const float checkRadius = 0.15f;

    public BuildEnums.BuildType AttachmentType => attachmentType;

    public bool CheckForNearbyBuild(Collider[] overlaps, LayerMask buildLayer)
    {
        // clear array
        System.Array.Clear(overlaps, 0, overlaps.Length);

        // check for any nearby buildable objects
        int size = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, overlaps, buildLayer);

        // loop through array and return true if buildable object is found
        for (int i = 0; i < size; i++)
        {
            if (overlaps[i].TryGetComponent(out BuildableObject buildable))
            {
                return true;
            }
        }

        return false;
    }
}
