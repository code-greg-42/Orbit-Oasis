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

    private Coroutine enginePitchTransitionCoroutine;

    private void Awake()
    {
        Instance = this;
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

    private void PlaySoundEffect(AudioSource audioSource)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.Play();
    }
}
