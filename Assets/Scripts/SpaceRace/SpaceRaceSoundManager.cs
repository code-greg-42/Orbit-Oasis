using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceSoundManager : MonoBehaviour
{
    public static SpaceRaceSoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource countdownAudioSource;

    // countdown sound settings
    private float countdownAudioStartTime = 0.15f;
    private float finalCountdownAudioStartTime = 0.08f;
    private float countdownPitch = 0.6f;
    private float finalCountdownPitch = 0.9f;
    private float countdownVolume = 0.15f;
    private float finalCountdownVolume = 0.25f;

    // engine pitch settings
    private const float regularEnginePitch = 1.0f;
    private const float boostEnginePitch = 1.5f;
    private const float pitchTransitionTime = 0.2f;

    private Coroutine enginePitchTransitionCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayCountdownSound(bool finalSound = false)
    {
        // if clip is playing, stop and restart it
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
        float startPitch = playerAudioSource.pitch;
        float elapsedTime = 0.0f;

        while (elapsedTime < pitchTransitionTime)
        {
            playerAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, elapsedTime / pitchTransitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerAudioSource.pitch = targetPitch;
    }
}
