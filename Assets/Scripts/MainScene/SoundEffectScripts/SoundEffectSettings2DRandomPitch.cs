using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffectSettings2DRandomPitch : SoundEffectSettings2D
{
    [Header("Pitch Settings")]
    [Range(0.5f, 2f)][SerializeField] private float minPitch;
    [Range(0.5f, 2f)][SerializeField] private float maxPitch;

    public float GetRandomPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }
}
