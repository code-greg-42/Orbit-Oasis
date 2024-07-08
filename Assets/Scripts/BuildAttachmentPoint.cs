using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildAttachmentPoint : MonoBehaviour
{
    [SerializeField] private BuildEnums.BuildType attachmentType;

    public bool IsUsed { get; private set; }

    public BuildEnums.BuildType AttachmentType => attachmentType;

    public void MarkUsed()
    {
        IsUsed = true;
    }

    public void MarkUnsused()
    {
        IsUsed = false;
    }
}
