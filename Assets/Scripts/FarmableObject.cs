using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableObject : Item
{
    [Header("Material Settings")]
    [SerializeField] private GameObject materialPrefab;
    [SerializeField] private int farmableMaterialCount;
    [SerializeField] private float dropForce;
    [SerializeField] private float dropHeight;

    public void FarmObject()
    {
        Vector3 dropPosition = new(transform.position.x, dropHeight, transform.position.z);
        GameObject material = Instantiate(materialPrefab, dropPosition, Quaternion.identity);
        material.transform.position = dropPosition;
        material.SetActive(true);

        // apply force in random direction
        if (material.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
        }
    }
}
