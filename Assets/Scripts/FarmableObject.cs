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
        // set start position
        Vector3 dropPosition = new(transform.position.x, transform.position.y + dropHeight, transform.position.z);

        // randomize material
        if (materialPrefabs.Length == materialWeights.Length)
        {
            // get prefab index based on assigned weights
            int prefabIndex = WeightedRandom.GetWeightedRandomIndex(materialWeights);

            GameObject material = Instantiate(materialPrefabs[prefabIndex], dropPosition, Quaternion.identity);
            material.transform.position = dropPosition;
            material.SetActive(true);

            // apply force in random direction
            if (material.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
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
}
