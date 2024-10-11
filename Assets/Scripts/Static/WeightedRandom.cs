using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WeightedRandom
{
    public static int GetWeightedRandomIndex(int[] weights)
    {
        int randomValue = Random.Range(0, 100); // weights must add up to 100

        for (int i = 0; i < weights.Length; i++)
        {
            if (randomValue < weights[i])
            {
                return i;
            }
            randomValue -= weights[i];
        }

        return -1; // should never reach here
    }
}
