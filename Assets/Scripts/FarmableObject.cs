using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableObject : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private GameObject materialPrefab;
    [SerializeField] private int farmableMaterialCount;

    private List<GameObject> materials;
    private readonly float dropForce = 2.0f;
    private readonly float dropHeight = 1.0f;

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        materials = new List<GameObject>();
        GameObject material;

        for (int i = 0; i < farmableMaterialCount; i++)
        {
            material = Instantiate(materialPrefab);
            Physics.IgnoreCollision(material.GetComponent<Collider>(), GetComponent<Collider>());
            material.SetActive(false);
            materials.Add(material);
        }
    }

    private GameObject GetMaterial()
    {
        // loop through and return the first inactive material
        for (int i = 0; i < farmableMaterialCount; i++)
        {
            if (!materials[i].activeInHierarchy)
            {
                return materials[i];
            }
        }
        return null;
    }

    public void FarmObject()
    {
        Debug.Log("Farming!");

        GameObject material = GetMaterial();

        if (material != null)
        {
            // set position to farmable object position and raise a bit off the ground
            Vector3 dropPosition = new(transform.position.x, dropHeight, transform.position.z);
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
}
