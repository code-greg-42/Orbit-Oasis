using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRacePlayerAttack : MonoBehaviour
{
    private const float attackCooldown = 0.25f;
    private const KeyCode attackKey = KeyCode.Space;
    private Vector3 bulletSpawnOffset = new(0, 0, 1);

    private bool attackReady = true;

    // references
    [SerializeField] private Transform spaceshipObject;
    [SerializeField] private Collider leftWingCollider;
    [SerializeField] private Collider rightWingCollider;

    private int numRockets = 8;

    private readonly int[] rocketUpgradeAmounts = { 9, 10, 11 };

    private void Update()
    {
        if (SpaceRaceGameManager.Instance.IsGameActive)
        {
            if (attackReady && numRockets > 0 && Input.GetKey(attackKey))
            {
                Attack();
            }
        }
    }

    public void SetRocketUpgradeLevel(int rocketUpgrade)
    {
        if (rocketUpgrade >= 1 && rocketUpgrade <= rocketUpgradeAmounts.Length)
        {
            numRockets = rocketUpgradeAmounts[rocketUpgrade - 1];
        }

        // update UI
        SpaceRaceUIManager.Instance.UpdateRocketAmount(numRockets);
    }

    private void Attack()
    {
        attackReady = false;

        numRockets--;
        SpaceRaceUIManager.Instance.UpdateRocketAmount(numRockets);

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
                Physics.IgnoreCollision(leftWingCollider, bulletCollider);
                Physics.IgnoreCollision(rightWingCollider, bulletCollider);
            }
        }
        else
        {
            Debug.Log("Ship collider not found.");
        }

        // set bullet position to ship position plus offset
        bullet.transform.position = transform.position + bulletSpawnOffset;

        // set active
        bullet.SetActive(true);

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        attackReady = true;
    }
}
