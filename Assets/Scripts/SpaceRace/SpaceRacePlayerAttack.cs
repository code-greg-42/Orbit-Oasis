using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRacePlayerAttack : MonoBehaviour
{
    private const float attackCooldown = 1.0f;
    private const KeyCode attackKey = KeyCode.Space;

    private bool attackReady = true;

    // references
    [SerializeField] private Transform spaceshipObject;

    private void Update()
    {
        if (attackReady && Input.GetKey(attackKey))
        {
            Attack();
        }
    }

    private void Attack()
    {
        attackReady = false;

        // get bullet from bullet pool
        GameObject bullet = BulletPool.Instance.GetPooledObject();

        // ignore collision between bullet colliders and player ship
        if (spaceshipObject.TryGetComponent(out Collider shipCollider))
        {
            // get collider array from bullet gameobject children
            Collider[] bulletColliders = bullet.GetComponentsInChildren<Collider>();

            // loop through and ignore
            foreach (var bulletCollider in bulletColliders)
            {
                Physics.IgnoreCollision(shipCollider, bulletCollider);
            }
        }
        else
        {
            Debug.Log("Ship collider not found.");
        }

        // set bullet position to ship position
        bullet.transform.position = transform.position;

        // set active
        bullet.SetActive(true);

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        attackReady = true;
    }
}
