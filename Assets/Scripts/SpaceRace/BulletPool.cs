using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ObjectPool
{
    public static BulletPool Instance;

    protected override void Awake()
    {
        // initialize singleton instance
        Instance = this;
        // call base awake method
        base.Awake();
    }
}
