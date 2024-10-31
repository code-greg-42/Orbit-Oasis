using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance;

    [Header("UI AudioSources")]
    [SerializeField] private AudioSource[] uiAudioSources;

    [Header("Sound Effect Settings")]
    [SerializeField] private List<SoundEffectSettings2D> soundEffectSettings2D;
    [SerializeField] private List<SoundEffectSettings3D> soundEffectSettings3D;

    private Dictionary<SoundEffect, SoundEffectSettings> soundEffects = new();

    public enum SoundEffect
    {
        Money,
        FarmTree,
        FarmRock,
        PlaceBuild,
        DeleteBuild,
        NoSell,
        HacksOn,
        HacksOff
    }

    private void Awake()
    {
        Instance = this;

        // map all 2d effects
        foreach (SoundEffectSettings2D effect2D in soundEffectSettings2D)
        {
            soundEffects[effect2D.Name] = effect2D;
        }

        // mad all 3d effects and set volume/pitch settings to inspector settings
        foreach (SoundEffectSettings3D effect3D in soundEffectSettings3D)
        {
            soundEffects[effect3D.Name] = effect3D;
            effect3D.InitAudioSettings();
        }
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (soundEffects.TryGetValue(effect, out SoundEffectSettings settings))
        {
            if (settings is SoundEffectSettings2D sound2D && sound2D.GetAudio() is AudioClip audioClip)
            {
                AudioSource uiAudioSource = GetAvailableAudioSource();
                uiAudioSource.volume = settings.Volume;
                uiAudioSource.pitch = settings.Pitch;
                uiAudioSource.PlayOneShot(audioClip);
            }
            else if (settings is SoundEffectSettings3D sound3D && sound3D.GetAudio() is AudioSource audioSource)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("Tried to play Sound Effect: " + effect.ToString() + ", but could not find corresponding settings in dictionary.");
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource audioSource in uiAudioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // return oldest source
        return uiAudioSources[0];
    }
}
