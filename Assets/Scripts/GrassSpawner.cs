using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private GameObject grassPrefab;
    private const float spacing = 0.5f;
    private const float boundaryMin = -50.0f;
    private const float boundaryMax = 50.0f;

    [ContextMenu("Spawn Grass in Hierarchy")]
    private void SpawnGrass()
    {
        // Clear existing grass patches to avoid duplication
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // loop through and instantiate at all locations
        for (float x = boundaryMin; x <= boundaryMax; x+= spacing)
        {
            for (float z = boundaryMin; z <= boundaryMax; z += spacing)
            {
                Vector3 position = new(x, 0, z);
                GameObject grassPatch = Instantiate(grassPrefab, position, Quaternion.identity, transform);
                grassPatch.name = $"GrassPatch_x{x}_z{z}";
            }
        }
    }
}
