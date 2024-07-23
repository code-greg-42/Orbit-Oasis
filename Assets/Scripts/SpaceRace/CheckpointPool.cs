using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointPool : ObjectPool
{
    public static CheckpointPool Instance { get; private set; }

    protected override void Awake()
    {
        // initialize singleton instance
        Instance = this;
        // call base awake method
        base.Awake();
    }
}
