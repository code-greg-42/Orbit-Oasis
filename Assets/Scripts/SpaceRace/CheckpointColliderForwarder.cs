using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointColliderForwarder : MonoBehaviour
{
    private SpaceRaceCheckpoint checkpoint;

    private void Start()
    {
        checkpoint = GetComponentInParent<SpaceRaceCheckpoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (checkpoint != null)
        {
            checkpoint.HandleCollision(other);
        }
    }
}
