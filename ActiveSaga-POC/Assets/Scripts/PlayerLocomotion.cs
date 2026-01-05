using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("The XR Rig or parent object to move.")]
    [SerializeField] private Transform playerRoot;
    
    [Tooltip("Usually the Main Camera - determines movement direction.")]
    [SerializeField] private Transform forwardReference;

    [Header("Analyzers")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float squatSpeed = 1.5f;
    [Tooltip("How fast the player accelerates/decelerates. Higher = more responsive.")]
    [SerializeField] private float momentumResponsiveness = 5.0f;

    [Header("Jump Physics")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;
    [Tooltip("Minimum run intensity required to perform a running jump. Below this, it's a stationary jump.")]
    [SerializeField] private float minRunForMomentumJump = 0.2f;
    
    [Header("Environment")]
    [Tooltip("The Y position of the floor.")]
    [SerializeField] private float groundLevel = 0f;

    // --- Private Variables ---
    private float verticalVelocity;
    private bool isGrounded = true;
    
    // Tracks the current smoothed speed for momentum
    private float currentSpeed = 0f; 
    
    // If true, the player jumped from a standstill and cannot move horizontally in mid-air
    private bool isStationaryJumpLock = false;

    private void OnEnable()
    {
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
    }

    private void Update()
    {
        if (playerRoot == null || forwardReference == null) return;

        MoveHorizontal();
        ApplyGravity();
    }

    private void MoveHorizontal()
    {
        // 1. Determine Target Speed
        float targetSpeed = 0f;

        if (squatAnalyzer != null && squatAnalyzer.IsSquatting)
        {
            // Squatting logic: Fixed slow speed, immediate response (no momentum)
            targetSpeed = squatSpeed;
            // If squatting, we treat run intensity as binary (moving or not)
            if (runAnalyzer.RunFactor < 0.1f) targetSpeed = 0f;
        }
        else
        {
            // Running logic: Speed depends on head bobbing intensity
            targetSpeed = runSpeed * runAnalyzer.RunFactor;
        }

        // 2. Handle Stationary Jump Locking
        // If we are in the air AND locked, force speed to 0 (prevent drifting)
        if (!isGrounded && isStationaryJumpLock)
        {
            targetSpeed = 0f;
            currentSpeed = 0f;
        }

        // 3. Apply Momentum (Smoothing)
        // Lerp current speed towards target speed for smooth transitions
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * momentumResponsiveness);

        // 4. Move Player
        if (currentSpeed > 0.01f)
        {
            Vector3 forwardDir = forwardReference.forward;
            forwardDir.y = 0; // Flatten direction
            forwardDir.Normalize();

            playerRoot.Translate(forwardDir * currentSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleJump()
    {
        // Block jump if in air or if squatting
        if (!isGrounded) return;
        if (squatAnalyzer != null && squatAnalyzer.IsSquatting) return;

        // Determine if this is a Running Jump or a Standing Jump
        float currentRunIntensity = runAnalyzer.RunFactor;

        if (currentRunIntensity > minRunForMomentumJump)
        {
            // Running Jump: Allow momentum to continue in air
            isStationaryJumpLock = false;
            Debug.Log("Action: Running Jump!");
        }
        else
        {
            // Standing Jump: Lock movement so player goes straight up/down
            isStationaryJumpLock = true;
            Debug.Log("Action: Standing Jump (Locked)");
        }

        verticalVelocity = jumpForce;
        isGrounded = false;
    }

    private void ApplyGravity()
    {
        // Apply gravity
        verticalVelocity -= gravity * Time.deltaTime;
        playerRoot.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);

        // Ground Check
        if (playerRoot.position.y <= groundLevel)
        {
            // Snap to floor
            Vector3 pos = playerRoot.position;
            pos.y = groundLevel;
            playerRoot.position = pos;

            isGrounded = true;
            verticalVelocity = 0;
            
            // Unlock the jump restriction when landing
            isStationaryJumpLock = false;
        }
    }
}