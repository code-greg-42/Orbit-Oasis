using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidPool : ObjectPool
{
    public static AsteroidPool Instance { get; private set; }
    [SerializeField] private GameObject[] asteroidPrefabs; // array of asteroid prefabs

    protected override void Awake()
    {
        // initialize singleton instance
        Instance = this;
        // call base awake method
        base.Awake();
    }

    protected override void InitializePool()
    {
        pooledObjects = new List<GameObject>();
        GameObject obj;

        for (int i = 0; i < amountToPool; i++)
        {
            int prefabIndex = WeightedRandom.GetWeightedRandomIndex(SpaceRaceGameManager.Instance.AsteroidPrefabWeights);
            obj = Instantiate(asteroidPrefabs[prefabIndex]);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }
}
