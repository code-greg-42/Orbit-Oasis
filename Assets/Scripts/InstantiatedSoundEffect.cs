using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatedSoundEffect : MonoBehaviour
{
    private AudioSource audioSource;
    private float audioLength;

    private void Awake()
    {
        if (TryGetComponent(out AudioSource audio))
        {
            audioSource = audio;

            if (audioSource.clip != null)
            {
                audioLength = audioSource.clip.length;
            }
            else
            {
                Debug.LogWarning("Audio clip not attached to audio source.");
            }
        }
        else
        {
            Debug.LogWarning("Audio source component not found.");
        }
    }

    private void Start()
    {
        StartCoroutine(DestroyAfterPlaying());
    }

    private IEnumerator DestroyAfterPlaying()
    {
        float effectiveLength = audioLength / audioSource.pitch;

        yield return new WaitForSeconds(effectiveLength);
        Destroy(gameObject);
    }

    public void SetPitch(float pitch)
    {
        Debug.Log("setting pitch!");
        if (audioSource != null)
        {
            audioSource.pitch = pitch;

            Debug.Log("Explosion Pitch: " + audioSource.pitch);
        }
    }
}
