using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffectSettings2D: SoundEffectSettings
{
    [SerializeField] private AudioClip audioClip;

    public override UnityEngine.Object GetAudio()
    {
        return audioClip;
    }
}
