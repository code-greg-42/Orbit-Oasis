using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceCheckpoint : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private ParticleSystem[] glowEffects;
    [SerializeField] private Color originalColor;

    private const float finishLineAlpha = 0.65f;
    private const float glowEffectAlpha = 0.5f;
    private const float deactivationTime = 2.0f;

    private Gradient originalGlowGradient;

    public bool CheckpointSuccess { get; private set; }

    private void Start()
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = glowEffects[0].colorOverLifetime;
        originalGlowGradient = new Gradient();
        originalGlowGradient.SetKeys(colorOverLifetime.color.gradient.colorKeys, colorOverLifetime.color.gradient.alphaKeys);
    }

    private void OnEnable()
    {
        CheckpointSuccess = false;

        // if this is final checkpoint, change color
        if (SpaceRaceGameManager.Instance.CheckpointsLoaded + 1 == SpaceRaceGameManager.Instance.FinalCheckpoint)
        {
            ChangeToFinishLineColor();
        }

        SpaceRaceGameManager.Instance.RegisterCheckpoint(this);
    }

    private void OnDisable()
    {
        SpaceRaceGameManager.Instance.UnregisterCheckpoint(this);
    }

    public void ChangeColor(bool success)
    {
        Color newColor = success ? Color.green : Color.red;

        ProcessColorChange(newColor, originalColor.a);
    }

    public void HandleCollision(Collider other)
    {
        if (other.gameObject.CompareTag("RacePlayerShip"))
        {
            CheckpointCompleted();
        }
        else if (other.gameObject.CompareTag("Asteroid"))
        {
            if (other.gameObject.TryGetComponent(out SpaceRaceAsteroid asteroid))
            {
                asteroid.PushRandomDirection();
            }
        }
    }

    private void CheckpointCompleted()
    {
        CheckpointSuccess = true;

        // load future part of race (and despawn old asteroids)
        SpaceRaceGameManager.Instance.CheckpointPassed();

        if (SpaceRaceGameManager.Instance.ActiveCheckpoints.Count > 1)
        {
            // change color to green to show success
            ChangeColor(true);

            // deactivate after timer
            StartCoroutine(DeactivationCoroutine());
        }
        else
        {
            // immediately deactivate last checkpoint to make visual space for victory text
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DeactivationCoroutine()
    {
        // wait until checkpoint leaves player viewpoint
        yield return new WaitForSeconds(deactivationTime);

        // reset color to original color
        ProcessColorChange(originalColor, originalColor.a);

        // deactivate gameobject
        gameObject.SetActive(false);
    }

    private void ChangeToFinishLineColor()
    {
        Color finishLineColor = Color.white;

        ProcessColorChange(finishLineColor, finishLineAlpha);
    }

    private void ProcessColorChange(Color newColor, float cubeAlpha)
    {
        // loop through both parts of the checkpoint, modifying the alpha of the new color first for each one

        // PHYSICAL CUBES
        newColor.a = cubeAlpha;
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = newColor;
        }

        // GLOW EFFECTS
        newColor.a = glowEffectAlpha;
        foreach (ParticleSystem glowEffect in glowEffects)
        {
            ChangeGlowEffectColor(glowEffect, newColor);
        }
    }

    private void ChangeGlowEffectColor(ParticleSystem glowEffect, Color newColor)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = glowEffect.colorOverLifetime;

        // Use original alpha keys
        GradientAlphaKey[] originalAlphaKeys = originalGlowGradient.alphaKeys;

        // Create new gradient with new color but keep original alpha keys
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new(newColor, 0.0f), new(newColor, 1.0f) },
            originalAlphaKeys
        );

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }
}
