using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffectSettings3D : SoundEffectSettings
{
    [SerializeField] private AudioSource audioSource;

    public override UnityEngine.Object GetAudio()
    {
        return audioSource;
    }

    public void InitAudioSettings()
    {
        audioSource.volume = Volume;
        audioSource.pitch = Pitch;
    }
}
