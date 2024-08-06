using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceSoundManager : MonoBehaviour
{
    public static SpaceRaceSoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource shipEngineAudioSource;
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioSource checkpointPassedAudioSource;
    [SerializeField] private AudioSource checkpointMissedAudioSource;
    [SerializeField] private AudioSource gameWinAudioSource;
    [SerializeField] private AudioSource shipCrashAudioSource;
    [SerializeField] private AudioSource fireRocketsAudioSource;
    [SerializeField] private AudioSource typingKeyAudioSource;
    [SerializeField] private AudioSource[] gameMusicAudioSources;

    [Header("Sound Effect Prefabs")]
    [SerializeField] private GameObject asteroidExplosionSoundPrefab;
    [SerializeField] private GameObject backgroundAsteroidExplosionSoundPrefab;

    // countdown sound settings
    private const float countdownAudioStartTime = 0.15f;
    private const float finalCountdownAudioStartTime = 0.08f;
    private const float countdownPitch = 0.6f;
    private const float finalCountdownPitch = 0.9f;
    private const float countdownVolume = 0.15f;
    private const float finalCountdownVolume = 0.25f;

    // engine pitch settings
    private const float regularEnginePitch = 1.0f;
    private const float boostEnginePitch = 1.5f;
    private const float pitchTransitionTime = 0.2f;

    // asteroid explosion pitch settings
    private const float minimumAsteroidPitch = 0.5f; // lowest pitch that an asteroid explosion sound can be
    private const float asteroidPitchRate = 0.1f; // rate that the pitch decreases per 1 unit of asteroid scale

    // typing sound settings
    private const float typingMinPitch = 0.88f;
    private const float typingMaxPitch = 1.12f;

    // music settings
    private const float musicFadeInDuration = 5.0f;
    private const float musicFadeOutDuration = 5.0f;
    private int chosenTrackIndex;
    private float[] musicTrackVolumes = { 0.175f, 0.075f };

    private Coroutine enginePitchTransitionCoroutine;
    private Coroutine fadeMusicCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChooseTrackAndStartMusic();
    }

    public void PlayCountdownSound(bool finalSound = false)
    {
        if (countdownAudioSource.isPlaying)
        {
            countdownAudioSource.Stop();
        }

        // skips delay at beginning of audio clip --- start earlier for final sound as playback speed is faster
        countdownAudioSource.time = finalSound ? finalCountdownAudioStartTime : countdownAudioStartTime;

        // make final countdown sound a little louder
        countdownAudioSource.volume = finalSound ? finalCountdownVolume : countdownVolume;

        // set to pitch based on whether it's the last sound in the countdown
        countdownAudioSource.pitch = finalSound ? finalCountdownPitch : countdownPitch;

        // play clip
        countdownAudioSource.Play();
    }

    public void PlayCheckpointPassedSound()
    {
        PlaySoundEffect(checkpointPassedAudioSource);
    }

    public void PlayCheckpointMissedSound()
    {
        PlaySoundEffect(checkpointMissedAudioSource);
    }

    public void PlayWinSound()
    {
        PlaySoundEffect(gameWinAudioSource);
    }

    public void PlayShipCrashSound()
    {
        PlaySoundEffect(shipCrashAudioSource);
    }

    public void PlayFireRocketsSound()
    {
        PlaySoundEffect(fireRocketsAudioSource);
    }

    public void PlayTypingKeySound()
    {
        if (typingKeyAudioSource.isPlaying)
        {
            typingKeyAudioSource.Stop();
        }

        // randomize and set a pitch between the min and max settings
        float pitch = Random.Range(typingMinPitch, typingMaxPitch);
        typingKeyAudioSource.pitch = pitch;

        typingKeyAudioSource.Play();
    }

    private void FadeInMusic()
    {
        float targetVolume = musicTrackVolumes[chosenTrackIndex];

        FadeMusic(targetVolume, musicFadeInDuration);
    }

    public void FadeOutMusic()
    {
        FadeMusic(0f, musicFadeOutDuration);
    }

    public void PlayExplosionSound(Vector3 position, Vector3 adjustedScale)
    {
        // instantiate main sound effect
        GameObject soundEffect = Instantiate(asteroidExplosionSoundPrefab, position, Quaternion.identity);

        // set pitch based on size if asteroid is larger than normal
        if (soundEffect != null && adjustedScale.magnitude > 1)
        {
            float pitch = 1.0f - (adjustedScale.magnitude - 1.0f) * asteroidPitchRate;

            pitch = Mathf.Max(pitch, minimumAsteroidPitch);

            if (soundEffect.TryGetComponent(out InstantiatedSoundEffect sound))
            {
                sound.SetPitch(pitch);
            }
            else
            {
                Debug.LogWarning("Unable to get InstantiatedSoundEffect component from asteroid explosion sound effect prefab.");
            }
        }

        // instantiate background sound effect
        Instantiate(backgroundAsteroidExplosionSoundPrefab, position, Quaternion.identity);
    }

    public void SetEnginePitch(bool boost = false)
    {
        float targetPitch = boost ? boostEnginePitch : regularEnginePitch;

        if (enginePitchTransitionCoroutine != null)
        {
            StopCoroutine(enginePitchTransitionCoroutine);
            enginePitchTransitionCoroutine = null;
        }

        enginePitchTransitionCoroutine = StartCoroutine(SmoothPitchTransition(targetPitch));
    }

    private void ChooseTrackAndStartMusic()
    {
        // roll a random number to select a track to play
        chosenTrackIndex = Random.Range(0, gameMusicAudioSources.Length);

        AudioSource chosenTrack = gameMusicAudioSources[chosenTrackIndex];

        // start track
        chosenTrack.Play();

        // fade volume from default of 0 to specified volume in volume settings (different for each track)
        FadeInMusic();
    }

    private IEnumerator SmoothPitchTransition(float targetPitch)
    {
        float startPitch = shipEngineAudioSource.pitch;
        float elapsedTime = 0.0f;

        while (elapsedTime < pitchTransitionTime)
        {
            shipEngineAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, elapsedTime / pitchTransitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shipEngineAudioSource.pitch = targetPitch;
    }

    private void FadeMusic(float targetVolume, float fadeDuration)
    {
        if (fadeMusicCoroutine != null)
        {
            StopCoroutine(fadeMusicCoroutine);
        }

        fadeMusicCoroutine = StartCoroutine(FadeMusicCoroutine(gameMusicAudioSources[chosenTrackIndex], targetVolume, fadeDuration));
    }

    private IEnumerator FadeMusicCoroutine(AudioSource audioSource, float targetVolume, float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private void PlaySoundEffect(AudioSource audioSource)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.Play();
    }
}
