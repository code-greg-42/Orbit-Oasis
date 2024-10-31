using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.1f;

    [Header("UI AudioSource")]
    [SerializeField] private AudioSource uiAudioSource;

    [Header("AudioClips - 2D")]
    [SerializeField] private AudioClip questProgressClip;
    [SerializeField] private AudioClip moneyClip;
    [SerializeField] private AudioClip farmTreeSound;
    [SerializeField] private AudioClip farmRockSound;

    [Header("AudioSources - 3D")]
    [SerializeField] private AudioSource axeSource;
    [SerializeField] private AudioSource miningSource;

    public enum SoundEffect
    {
        QuestProgress,
        Money,
        FarmTree,
        FarmRock
    }

    public enum SoundEffect3D
    {
        FarmTree,
        FarmRock
    }

    private Dictionary<SoundEffect, AudioClip> audioClips;
    private Dictionary<SoundEffect3D, AudioSource> audioSources;

    private void Awake()
    {
        Instance = this;

        audioClips = new Dictionary<SoundEffect, AudioClip>()
        {
            { SoundEffect.QuestProgress, questProgressClip },
            { SoundEffect.Money, moneyClip },
            { SoundEffect.FarmTree, farmTreeSound },
            { SoundEffect.FarmRock, farmRockSound },
        };

        audioSources = new Dictionary<SoundEffect3D, AudioSource>()
        {
            { SoundEffect3D.FarmTree, axeSource },
            { SoundEffect3D.FarmRock, miningSource }
        };
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (audioClips.TryGetValue(effect, out AudioClip clip))
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundEffect3D(SoundEffect3D effect)
    {
        if (audioSources.TryGetValue(effect, out AudioSource source))
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
            source.Play();
        }
    }

    public void PlayFarmingSound(FarmableObject.ObjectType farmableObjectType)
    {
        if (farmableObjectType == FarmableObject.ObjectType.Tree)
        {
            PlaySoundEffect(SoundEffect.FarmTree);
        }
        else if (farmableObjectType == FarmableObject.ObjectType.Rock)
        {
            PlaySoundEffect(SoundEffect.FarmRock);
        }
        else
        {
            Debug.LogWarning("Tried to play farming sound effect, but object type did not match.");
        }
    }
}
