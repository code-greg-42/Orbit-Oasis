using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] Animator playerAnim;

    public enum PlayerSpeed
    {
        Idle,
        Run,
        Sprint,
    }

    public void SetPlayerSpeed(PlayerSpeed speed)
    {
        float targetSpeed = 0.0f;

        switch (speed)
        {
            case PlayerSpeed.Idle:
                break;

            case PlayerSpeed.Run:
                targetSpeed = 0.5f;
                break;

            case PlayerSpeed.Sprint:
                targetSpeed = 1.0f;
                break;
        }

        // get current value of the speed parameter to check against the new targetSpeed
        float currentSpeed = playerAnim.GetFloat("speed");

        // if values are different, set the speed parameter equal to the targetSpeed
        if (!Mathf.Approximately(currentSpeed, targetSpeed))
        {
            playerAnim.SetFloat("speed", targetSpeed);
        }
    }

    public void TriggerPlayerJump()
    {
        playerAnim.SetTrigger("jump");
    }
}
