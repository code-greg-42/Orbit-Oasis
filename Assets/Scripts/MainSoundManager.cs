using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance;

    [SerializeField] private AudioSource mainAudioSource;

    [Header("AudioClips")]
    [SerializeField] private AudioClip questProgressClip;
    [SerializeField] private AudioClip moneyClip;
    [SerializeField] private AudioClip farmTreeSound;
    [SerializeField] private AudioClip farmRockSound;

    public enum SoundEffect
    {
        QuestProgress,
        Money,
        FarmTree,
        FarmRock
    }

    private Dictionary<SoundEffect, AudioClip> audioClips;

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
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (audioClips.TryGetValue(effect, out AudioClip clip))
        {
            mainAudioSource.PlayOneShot(clip);
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
