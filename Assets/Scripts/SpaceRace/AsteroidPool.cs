using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidPool : ObjectPool
{
    public static AsteroidPool Instance;

    protected override void Awake()
    {
        // initialize singleton instance
        Instance = this;
        // call base awake method
        base.Awake();
    }
}
