using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement speed variables
    private float moveSpeed = 7.0f;
    private const float baseSpeed = 7.0f;
    private const float boostMultiplier = 1.5f;
    private float jumpForce = 5.0f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform playerObject;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode boostKey = KeyCode.LeftShift;

    // ground check variables
    private const float playerHeight = 2.0f;
    private const float groundCheckBuffer = 0.2f;
    private const float groundDrag = 5.0f;
    private const float airDrag = 0.1f;
    private const float airMultiplier = 0.5f;
    private bool isGrounded;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    void Start()
    {
        Vector3 playerPos = DataManager.Instance.PlayerStats.PlayerPosition;
        if (playerPos != Vector3.zero)
        {
            transform.position = playerPos;
        }

        Quaternion playerRot = DataManager.Instance.PlayerStats.PlayerRotation;
        if (playerRot != Quaternion.identity)
        {
            playerObject.rotation = playerRot;
        }

        Quaternion playerOrientation = DataManager.Instance.PlayerStats.PlayerOrientation;
        if (playerOrientation != Quaternion.identity)
        {
            Debug.Log("PlayerOrientation: " + playerOrientation);
            orientation.rotation = playerOrientation;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        GroundCheck();
        MyInput();
        SpeedControl();
    }

    private void MyInput()
    {
        // get user input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // check for speed boost
        if (Input.GetKey(boostKey) && isGrounded)
        {
            moveSpeed = baseSpeed * boostMultiplier;
        }
        else
        {
            moveSpeed = baseSpeed;
        }

        // check for jump
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
    }

    private void MovePlayer()
    {
        // calc movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
        else
        {
            // move based on air multiplier
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + groundCheckBuffer, groundLayer);

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // add jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}
