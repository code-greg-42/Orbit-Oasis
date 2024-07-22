using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRacePlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform shipObjectTransform;

    private const float moveSpeed = 20.0f;
    private const float accelMultiplier = 20.0f;
    private const float boostMultiplier = 2.0f;

    private float forwardSpeed = 40.0f;
    private float minForwardSpeed = 32.0f;
    private bool boostActive;

    private const KeyCode boostKey = KeyCode.LeftShift;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * forwardSpeed;
    }

    void Update()
    {
        GetInput();
        SpeedControl();

        if (!boostActive && Input.GetKey(boostKey))
        {
            boostActive = true;
            forwardSpeed *= boostMultiplier;
            minForwardSpeed *= boostMultiplier;
        }

        if (Input.GetKeyUp(boostKey))
        {
            forwardSpeed /= boostMultiplier;
            minForwardSpeed /= boostMultiplier;
            boostActive = false;
        }
    }

    private void FixedUpdate()
    {
        if (horizontalInput != 0 || verticalInput != 0)
        {
            moveDirection = transform.up * verticalInput + transform.right * horizontalInput;

            rb.AddForce(accelMultiplier * moveSpeed * moveDirection, ForceMode.Force);
        }
        else
        {
            rb.AddForce(accelMultiplier * forwardSpeed * transform.forward, ForceMode.Force);
        }

        Debug.Log("Velocity: " + rb.velocity);
        Debug.Log("Magnitude: " + rb.velocity.magnitude);
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
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
}
