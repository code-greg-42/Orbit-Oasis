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

    public enum SoundEffect
    {
        QuestProgress,
        Money
    }

    private Dictionary<SoundEffect, AudioClip> audioClips;

    private void Awake()
    {
        Instance = this;

        audioClips = new Dictionary<SoundEffect, AudioClip>()
        {
            { SoundEffect.QuestProgress, questProgressClip },
            { SoundEffect.Money, moneyClip }
        };
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (audioClips.TryGetValue(effect, out AudioClip clip))
        {
            mainAudioSource.PlayOneShot(clip);
        }
    }
}
