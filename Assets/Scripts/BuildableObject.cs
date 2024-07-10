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

    public void PlaceObject()
    {
        foreach (GameObject attachmentSlot in attachmentSlots)
        {
            attachmentSlot.SetActive(true);
        }
        IsPlaced = true;
    }

    public void DeleteObject()
    {
        Destroy(gameObject);
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

    public void CheckAndEnableAttachmentPoints(LayerMask buildLayer)
    {
        Collider[] overlaps = new Collider[10];

        foreach (GameObject slot in attachmentSlots)
        {
            // skip active slots
            if (slot.activeSelf) continue;

            bool allClear = true;

            // cycle through points in slot
            foreach (Transform attachmentPoint in slot.transform)
            {
                if (attachmentPoint.TryGetComponent(out BuildAttachmentPoint buildAttachmentPoint))
                {
                    if (buildAttachmentPoint.CheckForNearbyBuild(overlaps, buildLayer))
                    {
                        allClear = false;
                        break;
                    }
                }
            }

            // if no nearby builds were found, activate slot
            if (allClear)
            {
                slot.SetActive(true);
            }
        }
    }
}
