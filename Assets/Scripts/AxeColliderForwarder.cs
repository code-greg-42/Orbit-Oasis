using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeColliderForwarder : MonoBehaviour
{
    private PlayerAxe playerAxe;

    void Start()
    {
        playerAxe = GetComponentInParent<PlayerAxe>();
    }

    private void OnTriggerEnter(Collider other)
    {
        playerAxe.HandleCollision(other);
    }
}
