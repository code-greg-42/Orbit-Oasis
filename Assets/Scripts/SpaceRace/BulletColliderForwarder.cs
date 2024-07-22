using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletColliderForwarder : MonoBehaviour
{
    private SpaceRaceBullet bullet;

    void Start()
    {
        bullet = GetComponentInParent<SpaceRaceBullet>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (bullet != null)
        {
            bullet.HandleCollision(other);
        }
    }
}
