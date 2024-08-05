using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRacePlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("References")]
    [SerializeField] private Transform shipObjectTransform;
    [SerializeField] private ParticleSystem[] boosterEffects;
    [SerializeField] private Color boosterEffectRegularColor;
    [SerializeField] private Color boosterEffectBoostedColor;
    [SerializeField] private GameObject boosterEffectHolder;
    [SerializeField] private GameObject crashEffectHolder;
    [SerializeField] private GameObject checkpointPassedEffectHolder;

    // active use speed variables
    private float forwardSpeed;
    private float minForwardSpeed;
    private float boostSpeed;
    private float regularSpeed;
    private Vector3 moveDirection;

    // const multipliers
    private const float minForwardSpeedMultiplier = 0.8f;
    private const float boostSpeedMultiplier = 2.0f;

    // boost variables
    private bool boostActive;
    private float boostAvailable = 100.0f;
    private float boostUsageRate = 10.0f; // usage % per second
    private float boostRechargeRate = 20.0f; // recharge % per second
    private const float boostUseThreshold = 15.0f; // % of boost available necessary for initiating boost
    private const float rechargeDelayTime = 2.0f; // delay recharging if boost hits 0
    private bool rechargeReady = true;

    // upgrade amounts
    private readonly float[] boostUpgradeUsageRates = { 7.5f, 5.0f, 2.5f };
    private readonly float[] boostUpgradeRechargeRates = { 30.0f, 40.0f, 50.0f };

    // const variables
    private const KeyCode boostKey = KeyCode.LeftShift;
    private const float accelMultiplier = 20.0f;
    private const float moveSpeed = 20.0f; // speed for directional movement
    private const float introSpeed = 20.0f; // forward speed for intro
    private const float maxRotation = 45.0f;
    private const float rotationSpeed = 7.0f;

    // effect variables
    private const float boosterEffectRegularSpeed = 6.0f;
    private const float boosterEffectBoostedSpeed = 16.0f;
    private const float checkpointPassedEffectDuration = 2.2f;

    // input variables
    private float horizontalInput;
    private float verticalInput;

    private bool isCrashing;
    private Coroutine checkpointPassedEffectCoroutine;

    public float ForwardSpeed => forwardSpeed;
    
    void Start()
    {
        // set initial speed to intro speed
        forwardSpeed = introSpeed;

        // get rigidbody component and set velocity to forward speed
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * forwardSpeed;
    }

    void Update()
    {
        if (SpaceRaceGameManager.Instance.IsGameActive)
        {
            GetInput();
            SetRotation();
            UpdateBoost();
        }

        // speed control always last as it's dependent on speed variables that change from other methods
        SpeedControl();
    }

    private void FixedUpdate()
    {
        if (!isCrashing)
        {
            if (SpaceRaceGameManager.Instance.IsGameActive && horizontalInput != 0 || verticalInput != 0)
            {
                moveDirection = transform.up * verticalInput + transform.right * horizontalInput;

                rb.AddForce(accelMultiplier * moveSpeed * moveDirection, ForceMode.Force);
            }
            else
            {
                rb.AddForce(accelMultiplier * forwardSpeed * transform.forward, ForceMode.Force);
            }
        }
    }

    public void SetRaceSpeed(float speed)
    {
        forwardSpeed = speed;
        minForwardSpeed = speed * minForwardSpeedMultiplier;
        boostSpeed = speed * boostSpeedMultiplier;
        regularSpeed = speed;
    }

    public void SetBoostUpgradeLevel(int boostUpgradeLevel)
    {
        if (boostUpgradeLevel >= 1 && boostUpgradeLevel <= boostUpgradeUsageRates.Length)
        {
            boostUsageRate = boostUpgradeUsageRates[boostUpgradeLevel - 1];
            boostRechargeRate = boostUpgradeRechargeRates[boostUpgradeLevel - 1];
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (!boostActive && boostAvailable >= boostUseThreshold && Input.GetKey(boostKey))
        {
            ActivateBoost();
        }

        if (boostActive && Input.GetKeyUp(boostKey))
        {
            DeactivateBoost();
        }
    }

    private void SetRotation()
    {
        if (!isCrashing)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, -horizontalInput * maxRotation);
            //shipObjectTransform.rotation = targetRotation;

            // Interpolate between the current rotation and the target rotation
            shipObjectTransform.rotation = Quaternion.Lerp(shipObjectTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void UpdateBoost()
    {
        if (boostActive && boostAvailable > 0)
        {
            // USE
            float newBoostAmount = boostAvailable - boostUsageRate * Time.deltaTime;

            if (newBoostAmount > 0)
            {
                boostAvailable = newBoostAmount;
            }
            else
            {
                boostAvailable = 0;
                DeactivateBoost();
                StartCoroutine(DelayRecharge());
            }
            // update UI with new value
            SpaceRaceUIManager.Instance.UpdateBoostAmount(boostAvailable);
        }
        else if (rechargeReady)
        {
            // RECHARGE
            if (boostAvailable < 100)
            {
                boostAvailable = Mathf.Min(boostAvailable + boostRechargeRate * Time.deltaTime, 100);

                // update UI with new value
                SpaceRaceUIManager.Instance.UpdateBoostAmount(boostAvailable, boostUseThreshold);
            }
        }
    }

    private IEnumerator DelayRecharge()
    {
        rechargeReady = false;
        SpaceRaceUIManager.Instance.ChangeBoostTextColor();

        yield return new WaitForSeconds(rechargeDelayTime);

        rechargeReady = true;
        // don't change back as it'll be changed back once boostAvailable > the use threshold
    }

    private void SpeedControl()
    {
        // directional movement speed control
        Vector3 directionalVel = new(rb.velocity.x, rb.velocity.y, 0);

        if (directionalVel.magnitude > moveSpeed)
        {
            Vector3 newDirectionalVel = directionalVel.normalized * moveSpeed;
            rb.velocity = new(newDirectionalVel.x, newDirectionalVel.y, rb.velocity.z);
        }

        // limit forward velocity if needed
        Vector3 forwardVel = new(0, 0, rb.velocity.z);

        if (forwardVel.magnitude > forwardSpeed)
        {
            Vector3 newForwardVel = new(rb.velocity.x, rb.velocity.y, forwardSpeed);
            rb.velocity = newForwardVel;
        }
        else if (forwardVel.magnitude < minForwardSpeed)
        {
            Vector3 newMinForwardVel = new(rb.velocity.x, rb.velocity.y, minForwardSpeed);
            rb.velocity = newMinForwardVel;
        }
    }

    private void ActivateBoost()
    {
        boostActive = true;
        forwardSpeed = boostSpeed;
        minForwardSpeed = boostSpeed * minForwardSpeedMultiplier;

        // apply effects
        UpdateBoosterEffects(true);
        SpaceRaceSoundManager.Instance.SetEnginePitch(true);
    }

    private void DeactivateBoost()
    {
        forwardSpeed = regularSpeed;
        minForwardSpeed = regularSpeed * minForwardSpeedMultiplier;
        boostActive = false;

        // apply effects
        UpdateBoosterEffects(false);
        SpaceRaceSoundManager.Instance.SetEnginePitch();
    }

    private void UpdateBoosterEffects(bool isBoosting)
    {
        foreach (ParticleSystem boosterEffect in boosterEffects)
        {
            // get reference to main module in particle system
            ParticleSystem.MainModule main = boosterEffect.main;

            // change start color and start speed
            main.startColor = isBoosting ? boosterEffectBoostedColor : boosterEffectRegularColor;
            main.startSpeed = isBoosting ? boosterEffectBoostedSpeed : boosterEffectRegularSpeed;
        }
    }

    public void CueCheckpointPassedEffect()
    {
        if (checkpointPassedEffectCoroutine != null)
        {
            StopCoroutine(checkpointPassedEffectCoroutine);
            checkpointPassedEffectCoroutine = null;
        }

        checkpointPassedEffectCoroutine = StartCoroutine(CheckpointPassedEffectCoroutine());
    }

    private IEnumerator CheckpointPassedEffectCoroutine()
    {
        checkpointPassedEffectHolder.SetActive(true);

        yield return new WaitForSeconds(checkpointPassedEffectDuration);

        checkpointPassedEffectHolder.SetActive(false);
    }

    private void Crash()
    {
        isCrashing = true;

        // settings for realistic crash
        rb.useGravity = true;
        rb.drag = 1.0f;

        // activate crash effects
        crashEffectHolder.SetActive(true);
        SpaceRaceSoundManager.Instance.PlayShipCrashSound();

        // deactivate booster effects
        boosterEffectHolder.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            if (!isCrashing)
            {
                Crash();
                SpaceRaceGameManager.Instance.EndGame();
            }
        }
    }
}
