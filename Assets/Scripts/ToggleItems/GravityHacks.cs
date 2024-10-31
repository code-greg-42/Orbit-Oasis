using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityHacks : ToggleItem
{
    public override void ToggleAbility()
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
            string alertMessage = alertPrefix + " gravity hacks...";
            MainUIManager.Instance.ShowAlertText(alertMessage, 2.5f);

            // play sound effect
            MainSoundManager.Instance.PlaySoundEffect(soundEffect);

            // call gravity hacks method from main game manager
            MainGameManager.Instance.ToggleGravityHacks();

            recentToggleTime = Time.time;
        }
    }
}
