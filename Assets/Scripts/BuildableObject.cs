using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    [SerializeField] private BuildEnums.BuildType buildType;
    [SerializeField] private BuildAttachmentPoint[] attachmentPoints;

    [SerializeField] private GameObject[] attachmentSlots;

    public bool IsPlaced { get; private set; }
    public BuildEnums.BuildType BuildType => buildType;

    public Vector3 PlaceObject()
    {
        foreach (GameObject attachmentSlot in attachmentSlots)
        {
            attachmentSlot.SetActive(true);
        }
        IsPlaced = true;

        return transform.position;
    }

    public void CheckAndDisableAttachmentPoints(LayerMask buildLayer)
    {
        Collider[] overlaps = new Collider[10];

        foreach (GameObject slot in attachmentSlots)
        {
            // skip inactive slots
            if (!slot.activeSelf) continue;

            // cycle through points in slot
            foreach (Transform attachmentPoint in slot.transform)
            {
                if (attachmentPoint.TryGetComponent(out BuildAttachmentPoint buildAttachmentPoint))
                {
                    if (buildAttachmentPoint.CheckForNearbyBuild(overlaps, buildLayer))
                    {
                        slot.SetActive(false);
                        break;
                    }
                }
            }
        }
    }
}
