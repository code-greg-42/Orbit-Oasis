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

    [Header("Farming Limits")]
    [SerializeField] private int maxFarms = 8; // maximum number of farms before the object becomes unavailable to get more materials from
    [SerializeField] private float regenInterval = 60.0f; // time in seconds to regenerate 1 available farm

    private int availableFarms;
    private Coroutine regenCoroutine;

    [Header("Type Setting")]
    [SerializeField] private ObjectType type;

    public enum ObjectType
    {
        Tree,
        Rock
    }

    public ObjectType Type => type;

    private void Start()
    {
        // intentionally resetting all farming between scene and session changes
        availableFarms = maxFarms;
    }

    private void OnEnable()
    {
        // handling case where an object is picked up into inventory and re-placed
        RegenerateFarms();

        // intentionally not stopping the coroutine on disable, as this way the availableFarms continue to regenerate even when a tree is in inventory
    }

    public void FarmObject()
    {
        if (availableFarms > 0)
        {
            availableFarms--;
            SpawnMaterial();
        }

        RegenerateFarms();
    }

    private void RegenerateFarms()
    {
        if (availableFarms < maxFarms && regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(FarmingRegenerationCoroutine());
        }
    }

    private void SpawnMaterial()
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

    private IEnumerator FarmingRegenerationCoroutine()
    {
        while (availableFarms < maxFarms)
        {
            yield return new WaitForSeconds(regenInterval);

            availableFarms++;
        }

        // set to null to indicate it's no longer running
        regenCoroutine = null;
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
        float randomX = Random.Range(-1, 1f);
        float randomY = Random.Range(-1, 1f);
        float randomZ = Random.Range(-1, 1f);
        Vector3 torque = new Vector3(randomX, randomY, randomZ) * dropTorqueAmount;

        return (dropPosition, randomDirection, torque);
    }
}
