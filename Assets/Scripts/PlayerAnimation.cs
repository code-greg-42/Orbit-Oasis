using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] Animator playerAnim;

    private bool isFalling;
    private bool isJumping;
    private bool isLanding;
    private bool isSwinging;
    private bool isMining;
    private bool isPickingUpItem;
    private bool isShooting;

    private const float jumpAnimationTime = 0.533f;
    private const float landingAnimationTime = 0.6f;
    private const float axeSwingAnimationTime = 1.61084f;
    private const float itemPickupAnimationTime = 2.234f / 1.2f;
    private const float shootingAnimationTime = 0.967f;
    private const float shootingAnimationReleasePct = 0.428f;

    public bool IsFalling => isFalling;

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

    public void TriggerJumpUp()
    {
        if (!isJumping)
        {
            isJumping = true;
            isFalling = false;
            isLanding = false;

            playerAnim.SetTrigger("jumpUp");
            StartCoroutine(ResetJumpUp());
        }
    }

    private IEnumerator ResetJumpUp()
    {
        yield return new WaitForSeconds(jumpAnimationTime);
        isJumping = false;
    }

    public void TriggerFallingLoop()
    {
        if (!isFalling)
        {
            isFalling = true;
            isLanding = false;

            playerAnim.SetBool("isFalling", true);
        }
    }

    public void TriggerLanding()
    {
        if (!isLanding && !isJumping && isFalling)
        {
            isLanding = true;
            playerAnim.SetTrigger("jumpDown");

            isFalling = false;
            playerAnim.SetBool("isFalling", false);

            StartCoroutine(ResetLanding());
        }
    }

    public void TriggerAxeSwing()
    {
        if (!isSwinging)
        {
            isSwinging = true;
            playerAnim.SetTrigger("axeSwing");

            StartCoroutine(ResetAxeSwing());
        }
    }

    public void TriggerBowShot()
    {
        if (!isShooting)
        {
            isShooting = true;
            playerAnim.SetTrigger("shootBow");

            StartCoroutine(ResetBowShot());
        }
    }

    public void TriggerItemPickup()
    {
        if (!isPickingUpItem)
        {
            isPickingUpItem = true;
            playerAnim.SetTrigger("pickupItem");

            StartCoroutine(ResetItemPickup());
        }
    }

    public void StartMiningLoop()
    {
        if (!isMining)
        {
            isMining = true;
            playerAnim.SetBool("isMining", true);
        }
    }

    public void StopMiningLoop()
    {
        if (isMining)
        {
            isMining = false;
            playerAnim.SetBool("isMining", false);
        }
    }

    public bool GetBowShotAnimationState()
    {
        if (isShooting)
        {
            if (playerAnim.GetCurrentAnimatorStateInfo(0).IsName("BowShot"))
            {
                AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.normalizedTime >= shootingAnimationReleasePct)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator ResetAxeSwing()
    {
        yield return new WaitForSeconds(axeSwingAnimationTime);
        isSwinging = false;
    }

    private IEnumerator ResetLanding()
    {
        yield return new WaitForSeconds(landingAnimationTime);
        isLanding = false;
    }

    private IEnumerator ResetItemPickup()
    {
        yield return new WaitForSeconds(itemPickupAnimationTime);
        isPickingUpItem = false;
    }

    private IEnumerator ResetBowShot()
    {
        yield return new WaitForSeconds(shootingAnimationTime);
        isShooting = false;
    }
}
