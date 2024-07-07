using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    [SerializeField] private Transform attachmentPoint;

    public Transform AttachmentPoint => attachmentPoint;

    public bool IsPlaced { get; private set; }

    public void PlaceBuildableObject()
    {
        IsPlaced = true;
    }
}
