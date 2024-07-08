using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    [SerializeField] private BuildEnums.BuildType buildType;
    [SerializeField] private BuildAttachmentPoint[] attachmentPoints;
    public bool IsPlaced { get; private set; }
    public BuildEnums.BuildType BuildType => buildType;

    public void PlaceObject()
    {
        foreach (BuildAttachmentPoint attachmentPoint in attachmentPoints)
        {
            attachmentPoint.gameObject.SetActive(true);
        }
        IsPlaced = true;
    }
}
