using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRacePlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform shipObjectTransform;

    // active use speed variables
    private float forwardSpeed = 40.0f;
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
    private float boostUsageRate = 15.0f; // usage % per second
    private float boostRechargeRate = 10.0f; // recharge % per second
    private float boostUseThreshold = 15.0f; // % of boost available necessary for initiating boost
    private float rechargeDelayTime = 2.0f; // delay recharging if boost hits 0
    private bool rechargeReady = true;

    // const variables
    private const KeyCode boostKey = KeyCode.LeftShift;
    private const float accelMultiplier = 20.0f;
    private const float moveSpeed = 20.0f; // speed for directional movement

    // input variables
    private float horizontalInput;
    private float verticalInput;

    private bool isCrashing;
    

    void Start()
    {
        // set initial variables based on default forwardSpeed
        minForwardSpeed = forwardSpeed * minForwardSpeedMultiplier;
        boostSpeed = forwardSpeed * boostSpeedMultiplier;
        regularSpeed = forwardSpeed;

        // get rigidbody component and set velocity to forward speed
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * forwardSpeed;
    }

    void Update()
    {
        if (SpaceRaceGameManager.Instance.IsGameActive)
        {
            GetInput();
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
    }

    private void DeactivateBoost()
    {
        forwardSpeed = regularSpeed;
        minForwardSpeed = regularSpeed * minForwardSpeedMultiplier;
        boostActive = false;
    }

    private void Crash()
    {
        isCrashing = true;
        rb.useGravity = true;
        rb.drag = 1.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Crash();
            SpaceRaceGameManager.Instance.EndGame();
        }
    }
}
