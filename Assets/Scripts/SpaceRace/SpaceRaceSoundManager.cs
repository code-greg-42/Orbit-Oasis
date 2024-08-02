using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceSoundManager : MonoBehaviour
{
    public static SpaceRaceSoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource playerAudioSource;

    // engine pitch settings
    private const float regularEnginePitch = 1.0f;
    private const float boostEnginePitch = 1.5f;
    private const float pitchTransitionTime = 0.2f;

    private Coroutine enginePitchTransitionCoroutine;

    private void Awake()
    {
        Instance = this;
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
