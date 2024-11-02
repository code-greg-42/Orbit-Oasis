using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundEffectSettingsFootstep: SoundEffectSettings
{
    [Header("Ground")]
    [SerializeField] private AudioClip[] groundClips;

    [Header("Wood")]
    [SerializeField] private AudioClip[] woodClips;

    public override UnityEngine.Object GetAudio()
    {
        return groundClips[0];
    }

    public AudioClip GetAudio(MainSoundManager.FootstepType footstepType)
    {
        AudioClip audioClip = null;

        if (footstepType == MainSoundManager.FootstepType.Ground && groundClips.Length > 0)
        {
            int index = Random.Range(0, groundClips.Length);
            audioClip = groundClips[index];
        }
        else if (footstepType == MainSoundManager.FootstepType.Wood && woodClips.Length > 0)
        {
            int index = Random.Range(0, woodClips.Length);
            audioClip = woodClips[index];
        }

        return audioClip;
    }
}
