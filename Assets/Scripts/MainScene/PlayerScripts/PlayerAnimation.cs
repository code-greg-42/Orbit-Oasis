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
    private bool isRunning;
    private bool isSprinting;

    private const float jumpAnimationTime = 0.533f;
    private const float landingAnimationTime = 0.6f;
    private const float axeSwingAnimationTime = 1.61084f;
    private const float itemPickupAnimationTime = 2.234f / 1.2f;
    private const float shootingAnimationTime = 0.967f;
    private const float shootingAnimationReleasePct = 0.428f;

    private const float runningFirstFootstepPct = 0.358f; // .378
    private const float runningSecondFootstepPct = 0.837f; // .867
    private const float sprintingFirstFootstepPct = 0.402f; // .42
    private const float sprintingSecondFootstepPct = 0.902f; // .937
    private const float footstepPctThreshold = 0.08f; // .062

    private bool firstFootstepPlayed = false;
    private bool secondFootstepPlayed = false;

    private bool justLanded;

    public bool IsFalling => isFalling;
    public bool JustLanded => justLanded;

    public enum PlayerSpeed
    {
        Idle,
        Run,
        Sprint,
    }

    private void Update()
    {
        PlayFootstepSounds();
    }

    public void SetPlayerSpeed(PlayerSpeed speed)
    {
        float targetSpeed = 0.0f;
        isRunning = false;
        isSprinting = false;

        switch (speed)
        {
            case PlayerSpeed.Idle:
                break;

            case PlayerSpeed.Run:
                targetSpeed = 0.5f;
                isRunning = true;
                break;

            case PlayerSpeed.Sprint:
                targetSpeed = 1.0f;
                isSprinting = true;
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

            // play sound
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.JumpStart);

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
            justLanded = true;

            // play sound
            MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.JumpLand);

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

    private void PlayFootstepSounds()
    {
        if (isRunning || isSprinting)
        {
            // get current state of running/sprinting animation
            (PlayerSpeed speed, float animationPct) = GetRunningAnimationState();

            // use only the decimal portion, as the number of cycles does not matter
            animationPct %= 1.0f;

            // ensure character is actually in running/sprinting animation state
            if (speed == PlayerSpeed.Idle)
            {
                ResetFootstepsPlayed();
                return;
            }

            // set percentages based on running/sprinting
            float firstFootstepPct = speed == PlayerSpeed.Run ? runningFirstFootstepPct : sprintingFirstFootstepPct;
            float secondFootstepPct = speed == PlayerSpeed.Run ? runningSecondFootstepPct : sprintingSecondFootstepPct;

            // ensure bools are reset
            if (animationPct < firstFootstepPct)
            {
                ResetFootstepsPlayed();
            }
            else if (animationPct < secondFootstepPct)
            {
                secondFootstepPlayed = false;
            }

            // play footstep sounds if within thresholds
            if (!firstFootstepPlayed && animationPct >= firstFootstepPct && animationPct <= firstFootstepPct + footstepPctThreshold)
            {
                firstFootstepPlayed = true;
                // play sound
                MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Footstep);
            }
            else if (!secondFootstepPlayed && animationPct >= secondFootstepPct && animationPct <= secondFootstepPct + footstepPctThreshold)
            {
                secondFootstepPlayed = true;
                // play sound
                MainSoundManager.Instance.PlaySoundEffect(MainSoundManager.SoundEffect.Footstep);
            }
        }
        else
        {
            // reset bools when not running or sprinting
            ResetFootstepsPlayed();
        }
    }

    private void ResetFootstepsPlayed()
    {
        firstFootstepPlayed = false;
        secondFootstepPlayed = false;
    }

    private (PlayerSpeed, float) GetRunningAnimationState()
    {
        AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);
        float animationPct = stateInfo.normalizedTime;
        PlayerSpeed speed = PlayerSpeed.Idle;

        if (stateInfo.IsName("RunForward"))
        {
            speed = PlayerSpeed.Run;
        }
        else if (stateInfo.IsName("Sprint"))
        {
            speed = PlayerSpeed.Sprint;
        }

        return (speed, animationPct);
    }

    private IEnumerator ResetAxeSwing()
    {
        yield return new WaitForSeconds(axeSwingAnimationTime);
        isSwinging = false;
    }

    private IEnumerator ResetLanding()
    {
        yield return new WaitForSeconds(landingAnimationTime * 0.6f);
        justLanded = false;
        yield return new WaitForSeconds(landingAnimationTime * 0.4f);
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
