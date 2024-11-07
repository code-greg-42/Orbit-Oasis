using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SoundEffectSettings
{
    [SerializeField] private MainSoundManager.SoundEffect soundName;
    [Range(0f, 5f)][SerializeField] private float volume;
    [Range(0.5f, 2f)][SerializeField] private float pitch;

    public MainSoundManager.SoundEffect Name => soundName;
    public float Volume => volume;
    public float Pitch => pitch;

    public abstract UnityEngine.Object GetAudio();
}
