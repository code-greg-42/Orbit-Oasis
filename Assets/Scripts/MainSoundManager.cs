using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script execution time of -10 to be able to set master volume
public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance;

    [Header("Volume Settings")]
    [Range(0f, 2f)][SerializeField] private float masterVolume;

    [Header("AudioSources")]
    [SerializeField] private AudioSource[] uiAudioSources;
    [SerializeField] private AudioSource[] playerAudioSources;

    [Header("Sound Effect Settings")]
    [SerializeField] private List<SoundEffectSettings2D> soundEffectSettings2D;
    [SerializeField] private List<SoundEffectSettings2DRandomPitch> soundEffectSettings2DRandomPitch;
    [SerializeField] private List<SoundEffectSettings3D> soundEffectSettings3D;
    [SerializeField] private List<SoundEffectSettingsFootstep> soundEffectSettingsFootsteps;

    [Header("Additional Effect Volumes")] // effects/audio sources that are part of a prefab, and are set when instantiated into a pool
    [Range(0f, 5f)][SerializeField] private float projectileVolume;
    [Range(0f, 5f)][SerializeField] private float detonationVolume;

    private Dictionary<SoundEffect, SoundEffectSettings> soundEffects = new();

    private FootstepType footstepType;

    public float MasterVolume => masterVolume; // used by sound effects in projectile pool
    public float ProjectileVolume => projectileVolume;
    public float DetonationVolume => detonationVolume;

    public enum SoundEffect
    {
        NoSound,
        Money,
        FarmTree,
        FarmRock,
        PlaceBuild,
        DeleteBuild,
        NoSell,
        HacksOn,
        HacksOff,
        QuestProgress,
        QuestComplete,
        PickupItem,
        DropItem,
        Sheep,
        Click,
        Select,
        EndDrag,
        SpaceMenuSelect,
        SpaceMenuEnter,
        SpaceMenuBack,
        Footstep,
        JumpLand,
        JumpStart,
        Typing,
        NextDialogue,
        ToggleMenu,
        ToggleBuildMode
    }

    public enum FootstepType
    {
        NoSound,
        Ground,
        Wood,
    }

    // master volume will be adjustable from the main menu scene, so the 3d audio settings only need to be set once
    private void Awake()
    {
        Instance = this;

        // get master volume setting from data manager

        // map all 2d effects --- choosing not to modify with mastervolume yet to keep things easier for controlling mid-test in the inspector
        foreach (SoundEffectSettings2D effect2D in soundEffectSettings2D)
        {
            soundEffects[effect2D.Name] = effect2D;
        }

        // map 2d effects with random pitch requirements
        foreach (SoundEffectSettings2DRandomPitch effectRandomPitch in soundEffectSettings2DRandomPitch)
        {
            soundEffects[effectRandomPitch.Name] = effectRandomPitch;
        }

        // map all 3d effects and set volume/pitch settings to inspector settings
        foreach (SoundEffectSettings3D effect3D in soundEffectSettings3D)
        {
            soundEffects[effect3D.Name] = effect3D;
            effect3D.InitAudioSettings(masterVolume);
        }

        foreach (SoundEffectSettingsFootstep effectFootstep in soundEffectSettingsFootsteps)
        {
            soundEffects[effectFootstep.Name] = effectFootstep;
        }
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (soundEffects.TryGetValue(effect, out SoundEffectSettings settings))
        {
            if (settings is SoundEffectSettings2D sound2D && sound2D.GetAudio() is AudioClip uiClip)
            {
                // randomize pitch if needed
                float pitch = settings is SoundEffectSettings2DRandomPitch randomPitchSettings ?
                    randomPitchSettings.GetRandomPitch() : settings.Pitch;

                PlayClip(uiAudioSources, uiClip, settings.Volume, pitch);
            }
            else if (settings is SoundEffectSettingsFootstep soundFootstep && soundFootstep.GetAudio(footstepType) is AudioClip footstepClip)
            {
                PlayClip(playerAudioSources, footstepClip, settings.Volume, settings.Pitch);
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

    public void SetFootstepType(FootstepType type)
    {
        footstepType = type;
    }

    private void PlayClip(AudioSource[] audioSources, AudioClip audioClip, float volume, float pitch)
    {
        AudioSource audioSource = GetAvailableAudioSource(audioSources);

        if (audioSource != null && audioClip != null & volume > 0)
        {
            audioSource.volume = volume * masterVolume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
        else
        {
            Debug.LogWarning("Tried to play an audio clip without proper source, clip, or volume settings.");
        }
    }

    private AudioSource GetAvailableAudioSource(AudioSource[] audioSources)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // log info and return oldest source
        Debug.LogWarning("Entire AudioSource array is being used, you may want to add an additional source to the array.");
        return uiAudioSources[0];
    }
}
