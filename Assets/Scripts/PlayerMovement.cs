using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement speed variables
    private float moveSpeed = 7.0f;
    private const float baseSpeed = 7.0f;
    private const float boostMultiplier = 1.5f;

    // jump variables
    private const float jumpForce = 12f;
    private const float jumpDuration = 0.6f;
    private bool jumpReady = true;
    private bool isJumping = false;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode boostKey = KeyCode.LeftShift;

    // ground check variables
    private const float playerHeight = 2.0f;
    private const float groundCheckBuffer = 0.2f;
    private const float groundDrag = 5.0f;
    private const float airDrag = 1.0f;
    private const float airMultiplier = 0.5f;
    private bool isGrounded;

    // jump animation variables
    private float airTime;
    private const float airTimeThreshold = 0.2f;

    // custom gravity
    private float customGravity = 20.0f;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    public Vector3 PlayerPosition => transform.position;
    public Quaternion PlayerRotation => playerObject.rotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void LoadPlayerPosition()
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
    }

    private void FixedUpdate()
    {
        MovePlayer();

        // custom gravity
        if (!isJumping)
        {
            rb.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
        }
    }

    private void Update()
    {
        GroundCheck();
        MyInput();
        SpeedControl();
        UpdateRunningAnimation();
        HandleAirState();
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
        if (Input.GetKeyDown(jumpKey) && isGrounded && jumpReady)
        {
            Debug.Log("Jump!");
            Jump();
        }
    }

    private void MovePlayer()
    {
        // calc movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            // raycast origin positions and distance
            Vector3 rayBelowPlayer = transform.position;
            Vector3 rayAheadPlayer = transform.position + transform.forward;
            float rayDistance = playerHeight;

            // variables to store slope values
            Vector3 slopeBelowPlayer = Vector3.up;
            Vector3 slopeAheadPlayer = Vector3.up;
            bool isSlopeAhead = false;

            // raycast below
            if (Physics.Raycast(rayBelowPlayer, Vector3.down, out RaycastHit hitBelow, rayDistance, groundLayer))
            {
                slopeBelowPlayer = hitBelow.normal;
            }

            // raycast in front to detect upcoming terrain
            if (Physics.Raycast(rayAheadPlayer, Vector3.down, out RaycastHit hitAhead, rayDistance, groundLayer))
            {
                slopeAheadPlayer = hitAhead.normal;
                isSlopeAhead = true;
            }

            if (isSlopeAhead)
            {
                // calculate diff in slope between current surface and upcoming surface
                float slopeDifference = Vector3.Angle(slopeBelowPlayer, slopeAheadPlayer);

                if (slopeDifference > 5.0f)
                {
                    if (rb.velocity.y > 0)
                    {
                        rb.AddForce(10f * Vector3.down, ForceMode.Force);
                    }
                    else if (rb.velocity.y < 0)
                    {
                        rb.AddForce(10f * Vector3.up, ForceMode.Force);
                    }
                }
                moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeAheadPlayer);
            }

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

            if (airTime > 0)
            {
                playerAnimation.TriggerLanding();
            }

            airTime = 0f;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void Jump()
    {
        if (jumpReady)
        {
            jumpReady = false;

            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // trigger player jump animation
            playerAnimation.TriggerJumpUp();
            Debug.Log("Jump Animation Triggered");

            StartCoroutine(JumpCoroutine());
        }
    }

    private void UpdateRunningAnimation()
    {
        if (verticalInput == 0 && horizontalInput == 0)
        {
            playerAnimation.SetPlayerSpeed(PlayerAnimation.PlayerSpeed.Idle);
        }
        else
        {
            if (moveSpeed > baseSpeed)
            {
                playerAnimation.SetPlayerSpeed(PlayerAnimation.PlayerSpeed.Sprint);
            }
            else
            {
                playerAnimation.SetPlayerSpeed(PlayerAnimation.PlayerSpeed.Run);
            }
        }
    }

    private IEnumerator JumpCoroutine()
    {
        float timer = jumpDuration;
        isJumping = true;
        rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        while (timer > 0)
        {
            //float logValue = Mathf.Log(timer + 1, jumpDuration + 1);
            //float logOfLogValue = Mathf.Log(logValue + 1, 2);
            //float smoothForce = jumpForce * Mathf.Log(logOfLogValue + 1, 2);

            //rb.AddForce(smoothForce * transform.up, ForceMode.Force);
            //rb.AddForce(3.5f * transform.up, ForceMode.Acceleration);
            timer -= Time.deltaTime;
            yield return null;
        }
        isJumping = false;
        jumpReady = true;
        Debug.Log("Jump Ready Set To: " + jumpReady);
    }

    private void HandleAirState()
    {
        if (!isGrounded)
        {
            // increment air time
            airTime += Time.deltaTime;

            // trigger falling animation if threshold is reached
            if (airTime > airTimeThreshold && !playerAnimation.IsFalling)
            {
                playerAnimation.TriggerFallingLoop();
            }
        }
    }
}
