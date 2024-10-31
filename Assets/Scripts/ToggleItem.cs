using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ToggleItem : Item
{
    protected float recentToggleTime;
    protected float toggleCooldown = 1.0f;
    protected bool isActive;

    public override bool IsDroppable { get; } = false;

    public void ToggleAbility()
    {
        if (Time.time - recentToggleTime > toggleCooldown)
        {
            isActive = !isActive;

            string alertPrefix;
            MainSoundManager.SoundEffect soundEffect;

            if (isActive)
            {
                alertPrefix = "enabling";
                soundEffect = MainSoundManager.SoundEffect.HacksOn;
            }
            else
            {
                alertPrefix = "disabling";
                soundEffect = MainSoundManager.SoundEffect.HacksOff;
            }

            // show alert message
            string alertMessage = $"{alertPrefix} {ItemName.ToLower()}...";
            MainUIManager.Instance.ShowAlertText(alertMessage, 2.5f);

            // play sound effect
            MainSoundManager.Instance.PlaySoundEffect(soundEffect);

            recentToggleTime = Time.time;

            // execute specific toggle logic
            OnToggle();
        }
    }

    protected abstract void OnToggle();
}
