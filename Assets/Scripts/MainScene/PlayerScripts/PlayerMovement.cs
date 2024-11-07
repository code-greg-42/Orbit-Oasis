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
    private float jumpForce = 12f;
    private const float jumpDuration = 0.6f;
    private bool jumpReady = true;
    private bool isJumping = false;
    private const float originalJumpForce = 12f;
    private const float upgradedJumpForce = 18f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform playerObject;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private CinemachineControls camControls;

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

    // custom position/rotation set variables
    private bool queuedMove;
    private bool queuedRotation;
    private Vector3 moveTarget;
    private Quaternion rotationTarget;

    // jump animation variables
    private float airTime;
    private const float airTimeThreshold = 0.2f;

    // custom gravity
    private const float customGravity = 20.0f;
    private bool gravityHacksActive;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    public Vector3 PlayerPosition => transform.position;
    public Quaternion PlayerRotation => playerObject.rotation;

    // properties used in playercontrols script
    public bool IsGrounded => isGrounded;
    public bool IsMoving => verticalInput != 0 || horizontalInput != 0;
    public bool GravityHacksActive => gravityHacksActive;
    // private properties
    private bool IsReadyToMove => !playerControls.IsSwinging && !playerControls.IsPickingUpItem;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (IsReadyToMove)
        {
            MovePlayer();
        }

        // custom gravity
        if (!isJumping && !gravityHacksActive)
        {
            rb.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
        }

        if (queuedMove)
        {
            queuedMove = false;
            rb.MovePosition(moveTarget);
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

    private void LateUpdate()
    {
        if (queuedRotation)
        {
            queuedRotation = false;
            playerObject.rotation = rotationTarget;
        }
    }

    public void ToggleGravityHacks()
    {
        gravityHacksActive = !gravityHacksActive;
        jumpForce = gravityHacksActive ? upgradedJumpForce : originalJumpForce;
    }

    public void LoadPlayerPosition()
    {
        Vector3 playerPos = DataManager.Instance.PlayerStats.PlayerPosition;
        if (playerPos != Vector3.zero)
        {
            SetPlayerPosition(playerPos);
        }

        Quaternion playerRot = DataManager.Instance.PlayerStats.PlayerRotation;
        if (playerRot != Quaternion.identity)
        {
            SetPlayerRotation(playerRot);
        }
    }

    public void SetPlayerPosition(Vector3 playerPos)
    {
        moveTarget = playerPos;
        queuedMove = true;
    }

    public bool HasFallenOffEdge(float yBoundary)
    {
        return transform.position.y < yBoundary;
    }

    private void SetPlayerRotation(Quaternion playerRot)
    {
        rotationTarget = playerRot;
        queuedRotation = true;
    }

    private void MyInput()
    {
        if (IsReadyToMove)
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
                Jump();
            }
        }
        else
        {
            horizontalInput = 0;
            verticalInput = 0;
        }
    }

    private void MovePlayer()
    {
        // calc movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            // raycast origin position and distance
            Vector3 rayAheadPlayer = transform.position + transform.forward * 0.5f; // 1
            float rayDistance = playerHeight;

            // variable to store slope value
            Vector3 slopeAheadPlayer = Vector3.up;
            bool isSlopeAhead = false;

            // raycast in front to detect upcoming terrain
            if (Physics.Raycast(rayAheadPlayer, Vector3.down, out RaycastHit hitAhead, rayDistance, groundLayer))
            {
                slopeAheadPlayer = hitAhead.normal;
                isSlopeAhead = true;

                if (hitAhead.collider.CompareTag("Ground"))
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.Ground);
                }
                else if (hitAhead.collider.CompareTag("Buildable") || hitAhead.collider.CompareTag("MainSpaceship"))
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.Wood);
                }
                else
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.NoSound);
                }
            }

            if (isSlopeAhead)
            {
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, playerHeight * 0.5f + groundCheckBuffer, groundLayer);

        if (isGrounded)
        {
            rb.drag = groundDrag;

            if (airTime > 0)
            {
                // update sound manager with new ground type
                if (hit.collider.CompareTag("Ground"))
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.Ground);
                }
                else if (hit.collider.CompareTag("Buildable") || hit.collider.CompareTag("MainSpaceship"))
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.Wood);
                }
                else
                {
                    MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.NoSound);
                }

                playerAnimation.TriggerLanding();
            }

            airTime = 0f;
        }
        else
        {
            rb.drag = airDrag;
            MainSoundManager.Instance.SetFootstepType(MainSoundManager.FootstepType.NoSound);
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
            timer -= Time.deltaTime;
            yield return null;
        }
        isJumping = false;
        jumpReady = true;
    }

    private void HandleAirState()
    {
        if (!isGrounded)
        {
            // increment air time
            airTime += Time.deltaTime;

            // raycast downward to check distance to the ground
            float raycastDistance = playerHeight * 1.3f;

            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit _, raycastDistance, groundLayer))
            {
                if (airTime > airTimeThreshold && !playerAnimation.IsFalling)
                {
                    playerAnimation.TriggerFallingLoop();
                }
            }
        }
    }
}
