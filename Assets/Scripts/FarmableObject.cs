using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmableObject : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private GameObject[] materialPrefabs;
    [SerializeField] private int farmableMaterialCount;
    [SerializeField] private float dropForce;
    [SerializeField] private float dropHeight;
    [SerializeField] private float offsetLength;
    [SerializeField] private float dropTorqueAmount;
    [SerializeField] private int[] materialWeights; // must have same number of ints as materialPrefab, and equal 100

    [Header("Type Setting")]
    [SerializeField] private ObjectType type;

    public enum ObjectType
    {
        Tree,
        Rock
    }

    public ObjectType Type => type;

    public void FarmObject()
    {
        // randomize material
        if (materialPrefabs.Length == materialWeights.Length)
        {
            // get prefab index based on assigned weights
            int prefabIndex = WeightedRandom.GetWeightedRandomIndex(materialWeights);

            // get material prefab
            GameObject materialPrefab = materialPrefabs[prefabIndex];

            // get necessary vector3's from the helper method
            (Vector3 dropPosition, Vector3 randomDirection, Vector3 dropTorque) = GetDropVectors();

            // spawn material and set active
            GameObject material = Instantiate(materialPrefab, dropPosition, materialPrefab.transform.rotation);

            // get rigidbody component
            if (material.TryGetComponent<Rigidbody>(out var rb))
            {
                // apply force in the randomized direction
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);

                // apply torque to material
                rb.AddTorque(dropTorque, ForceMode.Impulse);
            }

            // update quest manager if on the farm tree quest and object is of tree type
            if (QuestManager.Instance.GetCurrentQuest() == QuestManager.IntroQuest.FarmTree && type == ObjectType.Tree)
            {
                QuestManager.Instance.UpdateCurrentQuest();
            }
        }
        else
        {
            Debug.LogWarning("Material prefabs/weights not set up correctly.");
        }
    }

    private (Vector3, Vector3, Vector3) GetDropVectors()
    {
        // set start position
        Vector3 dropPosition = new(transform.position.x, transform.position.y + dropHeight, transform.position.z);

        // randomize a new direction, with 0.5f as the Y value to keep it going to the side
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)).normalized;

        // adjust start position according to random direction
        dropPosition += randomDirection * offsetLength;

        // randomize torque
        Vector3 torque = new(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f) * dropTorqueAmount);

        return (dropPosition, randomDirection, torque);
    }
}
